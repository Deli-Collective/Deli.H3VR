using System.Collections.Generic;
using BepInEx.Logging;
using Deli.Patcher;

namespace Deli.H3VR.Patcher
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class LogBuffer : DeliModule, ILogListener
	{
		// We'll need a reference to this from the behaviour
		internal static LogBuffer? Instance { get; private set; }

		// Add ourselves as a listener for log events
		public LogBuffer(Mod source) : base(source)
		{
			Instance = this;
			BepInEx.Logging.Logger.Listeners.Add(this);
		}

		// When the Deli behaviour takes over we can stop listening for logs
		public void Dispose()
		{
			Instance = null;
			BepInEx.Logging.Logger.Listeners.Remove(this);
		}

		// Capture log events
		internal readonly List<LogEventArgs> LogEvents = new();
		public void LogEvent(object sender, LogEventArgs eventArgs) => LogEvents.Add(eventArgs);

	}
}
