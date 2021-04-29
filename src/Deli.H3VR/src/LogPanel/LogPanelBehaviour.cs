using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Deli.H3VR.Api;
using Deli.H3VR.Patcher;
using Deli.Runtime;
using Deli.Setup;
using Deli.VFS;
using FistVR;
using UnityEngine;

namespace Deli.H3VR.LogPanel
{
	public class LogPanelBehaviour : DeliBehaviour, ILogListener
	{
		private readonly List<LogEventArgs> _logEvents;
		private readonly LockablePanel _panel;
		private BepInExLogPanel? _logPanelComponent;

		public LogPanelBehaviour()
		{
			// Register a new wrist menu button
			H3API.GetOrInit(Source).WristMenu.RegisterWristMenuButton("Spawn Log Panel", SpawnLogPanel);

			// Register ourselves as the new log listener and try to grab what's already been captured
			BepInEx.Logging.Logger.Listeners.Add(this);
			if (LogBuffer.Instance is not null)
			{
				// Grab the captured logs from the buffer and dispose it.
				_logEvents = LogBuffer.Instance.LogEvents;
				LogBuffer.Instance.Dispose();
			}
			else
			{
				// If the instance was somehow null we can still continue, just without the previous logs.
				_logEvents = new List<LogEventArgs>();
				Logger.LogError("LogBuffer instance was null! Captured logs will start from here instead.");
			}

			// Make a new LockablePanel
			_panel = new LockablePanel();
			_panel.Configure += panel =>
			{
				Transform canvasTransform = panel.transform.Find("OptionsCanvas_0_Main/Canvas");
				_logPanelComponent = panel.AddComponent<BepInExLogPanel>();
				_logPanelComponent.CreateWithExisting(Source, canvasTransform, _logEvents);
			};

			// Setup a callback to read and apply our texture
			Stages.Runtime += stage => StartCoroutine(OnRuntime(stage));
		}

		private IEnumerator OnRuntime(RuntimeStage stage)
		{
			DelayedReader<Texture2D> texReader = stage.GetReader<Texture2D>();
			var op = texReader(Resources.GetFile("LogPanel.png")
			                   ?? throw new FileNotFoundException("Log panel texture is missing!"));
			yield return op;
			_panel.TextureOverride = op.Result;
		}

		// Wrist menu button callback. Gets our panel instance and makes the hand retrieve it.
		private void SpawnLogPanel(FVRWristMenu wristMenu)
		{
			GameObject panel = _panel.GetOrCreatePanel();
			wristMenu.m_currentHand.RetrieveObject(panel.GetComponent<FVRPhysicalObject>());
		}

		void ILogListener.LogEvent(object sender, LogEventArgs eventArgs)
		{
			_logEvents.Add(eventArgs);
			if (_logPanelComponent && _logPanelComponent is not null) _logPanelComponent.LogEvent();
		}

		void IDisposable.Dispose()
		{
			BepInEx.Logging.Logger.Listeners.Remove(this);
			_logEvents.Clear();
		}
	}
}
