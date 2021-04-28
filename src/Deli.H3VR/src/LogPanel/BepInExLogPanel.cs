using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Deli.H3VR.LogPanel
{
	public class BepInExLogPanel : MonoBehaviour
	{
		private List<LogEventArgs>? _currentEvents;
		private Transform? _canvas;
		private Text? _logText;

		private ConfigEntry<int>? _fontSize;
		private ConfigEntry<int>? _maxLines;
		private ConfigEntry<string>? _fontName;

		private static readonly Dictionary<LogLevel, string> LogColors = new()
		{
			[LogLevel.Fatal] = "#962c2c", // Dark red
			[LogLevel.Error] = "red",
			[LogLevel.Warning] = "#e0dd10", // Slightly muted yellow
			[LogLevel.Message] = "white",
			[LogLevel.Info] = "white",
			[LogLevel.Debug] = "grey"
		};

		public void CreateWithExisting(Mod source, Transform canvas, List<LogEventArgs> currentEvents)
		{
			// Set variables and bind configs
			_canvas = canvas;
			_currentEvents = currentEvents;
			_fontSize = source.Config.Bind("LogPanel", "FontSize", 10, "The size of the font in the log panel.");
			_maxLines = source.Config.Bind("LogPanel", "MaxLines", 30,
				"The maximum number of lines to render in the log panel at any given time. If you change some of the other settings and find that the log isn't filling the panel completely, try increasing this value.");
			_fontName = source.Config.Bind("LogPanel", "FontName", "Consolas",
				"The name of the font used on the log panel. This can be any font you have installed, but Consolas is pretty good and comes with Windows so.");

			// Create the new game object
			GameObject textObj = new("LogText", typeof(RectTransform), typeof(Text));
			textObj.transform.SetParent(_canvas);

			// Place a mask around the canvas to prevent the log from overflowing
			canvas.gameObject.AddComponent<RectMask2D>();

			// Set the size and position
			RectTransform rt = textObj.GetComponent<RectTransform>();
			rt.localScale = new Vector3(0.07f, 0.07f, 0.07f);
			rt.localPosition = Vector3.zero;
			rt.anchoredPosition = Vector2.zero;
			rt.sizeDelta = new Vector2(37f / 0.07f, 24f / 0.07f);

			// Add the text component and assign the proper values
			_logText = textObj.GetComponent<Text>();
			_logText.fontSize = _fontSize.Value;
			_logText.alignment = TextAnchor.LowerLeft;
			_logText.verticalOverflow = VerticalWrapMode.Overflow;

			// Make sure the user has the requested font installed
			if (Font.GetOSInstalledFontNames().Contains(_fontName.Value))
			{
				_logText.font = Font.CreateDynamicFontFromOSFont(_fontName.Value, _fontSize.Value);
			}
			else
			{
				_logText.font = Font.CreateDynamicFontFromOSFont("Consolas", _fontSize.Value);
				source.Logger.LogError($"Log panel font is set to '{_fontName.Value}' in the config but that font is not installed!");
			}

			UpdateText();
		}

		public void UpdateText()
		{
			if (_currentEvents is null || _maxLines is null || _logText is null) return;

			StringBuilder sb = new();
			int startIndex = Math.Max(0, _currentEvents.Count - _maxLines.Value);
			for (int i = startIndex; i < _currentEvents.Count; i++)
			{
				LogEventArgs evt = _currentEvents[i];
				sb.AppendLine($"<color={LogColors[evt.Level]}>{evt}</color>");
			}

			sb.Append($" -- Showing lines {startIndex + 1} to {startIndex + _maxLines.Value} (of {_currentEvents.Count})");
			_logText.text = sb.ToString();
		}
	}
}
