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
		private H3API _api;

		public LogPanelBehaviour()
		{
			// Register a new wrist menu button
			_api = H3API.Instance;
			_api.WristMenu.RegisterWristMenuButton("Spawn Log Panel", SpawnLogPanel);

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
		}

		private void SpawnLogPanel(FVRWristMenu wristMenu)
		{
			// If the log panel doesn't exist (Not yet created or scene switched and it got deleted) make a new one
			if (_logPanel is null || !_logPanel)
			{
				// Make a copy of the panel, clean it and add our own component
				GameObject panel = _api.LockablePanel.GetCleanLockablePanel();
				Transform canvasTransform = panel.transform.Find("OptionsCanvas_0_Main/Canvas");
				_logPanel = panel.AddComponent<BepInExLogPanel>();
				_logPanel.CreateWithExisting(Source, canvasTransform, _logEvents);
			}

			// Then we just make the hand pick up the panel
			wristMenu.m_currentHand.RetrieveObject(_logPanel.GetComponent<FVRPhysicalObject>());
		}

		void ILogListener.LogEvent(object sender, LogEventArgs eventArgs)
		{
			_logEvents.Add(eventArgs);

			// If we have a log panel active let it update too
			if (_logPanel && _logPanel is not null) _logPanel.LogEvent();
		}

		void IDisposable.Dispose()
		{
			BepInEx.Logging.Logger.Listeners.Remove(this);
			_logEvents.Clear();
		}
	}
}
