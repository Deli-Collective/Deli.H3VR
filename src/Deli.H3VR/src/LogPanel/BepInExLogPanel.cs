using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace Deli.H3VR.LogPanel
{
	public class BepInExLogPanel : MonoBehaviour
	{
		private List<LogEventArgs>? _currentEvents;
		private Transform? _canvas;


		public void CreateWithExisting(Transform canvas, List<LogEventArgs> currentEvents)
		{
			_canvas = canvas;
			_currentEvents = currentEvents;
		}

		public void LogEvent(LogEventArgs args)
		{

		}
	}
}
