using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Deli.H3VR.Api;
using Deli.H3VR.Patcher;
using Deli.Setup;

namespace Deli.H3VR.LogPanel
{
	public class BepInExLogPanel : DeliBehaviour, ILogListener
	{
		private List<LogEventArgs> _logEvents;

		public BepInExLogPanel()
		{
			Stages.Setup += OnSetup;

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

		private void OnSetup(SetupStage stage)
		{
			WristMenuButtons.AddWristMenuButton("BepInEx Log", () =>
			{
				Logger.LogInfo("Maybe open the log panel or something idk");
			});
		}

		void ILogListener.LogEvent(object sender, LogEventArgs eventArgs)
		{
			_logEvents.Add(eventArgs);
		}

		void IDisposable.Dispose()
		{
			BepInEx.Logging.Logger.Listeners.Remove(this);
			_logEvents.Clear();
		}
	}
}
