using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Deli.H3VR.Api;
using Deli.H3VR.Patcher;
using Deli.Setup;
using FistVR;
using UnityEngine;

namespace Deli.H3VR.LogPanel
{
	public class LogPanelBehaviour : DeliBehaviour, ILogListener
	{
		private readonly List<LogEventArgs> _logEvents;
		private BepInExLogPanel? _logPanel;

		public LogPanelBehaviour()
		{
			// Register a new wrist menu button
			WristMenuButtons.RegisterWristMenuButton("BepInEx Log", SpawnLogPanel);

			// Register ourselves as the new log listener and try to grab what's already been captured
			BepInEx.Logging.Logger.Listeners.Add(this);
			if (LogBuffer.Instance is not null)
			{
				// Grab the captured logs from the buffer and dispose it.
				_logEvents = LogBuffer.Instance.LogEvents;
				LogBuffer.Instance.Dispose();
				Logger.LogInfo($"Captured logs from the patching stage: {_logEvents.Count}");
			}
			else
			{
				// If the instance was somehow null we can still continue, just without the previous logs.
				_logEvents = new List<LogEventArgs>();
				Logger.LogError("LogBuffer instance was null! Captured logs will start from here instead.");
			}
		}

		private void SpawnLogPanel(FVRWristMenu wristMenu)
		{
			// If the log panel doesn't exist (Not yet created or scene switched and it got deleted) make a new one
			if (_logPanel is null || !_logPanel)
			{
				// Make a copy of the panel, clean it and add our own component
				GameObject panel = Instantiate(wristMenu.OptionsPanelPrefab);
				Transform canvasTransform = CleanPanel(panel);
				_logPanel = panel.AddComponent<BepInExLogPanel>();
				_logPanel.CreateWithExisting(canvasTransform, _logEvents);
			}

			// Then we just make the hand pick up the panel
			wristMenu.m_currentHand.RetrieveObject(_logPanel.GetComponent<FVRPhysicalObject>());
		}

		private Transform CleanPanel(GameObject panel)
		{
			Transform panelTransform = panel.transform;

			// This proto object has a bunch of hidden stuff we don't want, but it does also contain the actual panel model
			// So just move it up and delete the proto
			Transform proto = panelTransform.Find("OptionsPanelProto");
			proto.Find("Tablet").SetParent(panelTransform);
			Destroy(proto.gameObject);

			// Then, everything else we want to delete in the main object is disabled so use that as a filter
			foreach (Transform child in panelTransform)
			{
				if (!child.gameObject.activeSelf)
					Destroy(child.gameObject);
			}

			// Lastly we just want to clear out the main canvas
			Transform canvas = panelTransform.Find("OptionsCanvas_0_Main/Canvas");
			foreach (Transform child in canvas)
			{
				Destroy(child.gameObject);
			}

			// Then we return the canvas for later use
			return canvas;
		}

		void ILogListener.LogEvent(object sender, LogEventArgs eventArgs) => _logEvents.Add(eventArgs);

		void IDisposable.Dispose()
		{
			BepInEx.Logging.Logger.Listeners.Remove(this);
			_logEvents.Clear();
		}
	}
}
