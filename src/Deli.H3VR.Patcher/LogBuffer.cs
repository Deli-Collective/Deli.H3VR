using System.Collections.Generic;
using BepInEx.Logging;
using Deli.Patcher;

namespace Deli.H3VR.Patcher
{
	/// <summary>
	///		Small class to bugger logs output in the patching stage so we can read them later in the rumtime stage
	/// </summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	public class LogBuffer : DeliModule, ILogListener
	{
		internal static LogBuffer? Instance { get; private set; }

		public LogBuffer(Mod source) : base(source)
		{
			Instance = this;
			BepInEx.Logging.Logger.Listeners.Add(this);
		}

		public void Dispose()
		{
			// When the Deli behaviour takes over we can stop listening for logs
			Instance = null;
			BepInEx.Logging.Logger.Listeners.Remove(this);
		}

		// Capture log events
		internal readonly List<LogEventArgs> LogEvents = new();
		public void LogEvent(object sender, LogEventArgs eventArgs) => LogEvents.Add(eventArgs);

	}
}
