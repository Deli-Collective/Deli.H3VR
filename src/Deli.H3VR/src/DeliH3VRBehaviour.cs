using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Deli.H3VR.Api;
using Deli.H3VR.Patcher;
using Deli.H3VR.UiWidgets;
using Deli.H3VR.UiWidgets.Layout;
using Deli.Runtime;
using Deli.Setup;
using Deli.VFS;
using FistVR;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Deli.H3VR
{
	public class DeliH3VRBehaviour : DeliBehaviour, ILogListener
	{
		private readonly List<LogEventArgs> _logEvents;
		private readonly LockablePanel _logPanel;
		private readonly LockablePanel _utilityPanel;
		private BepInExLogPanel? _logPanelComponent;

		public DeliH3VRBehaviour()
		{
			// Log the game build ID
			SteamAPI.Init();
			Logger.LogInfo("Deli H3VR initialized. Game build ID: " + SteamApps.GetAppBuildId());

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
			_logPanel = new LockablePanel();
			_logPanel.Configure += ConfigureLogPanel;
			WristMenu.RegisterWristMenuButton("Spawn Log Panel", SpawnLogPanel);

			// And one for the utility panel
			_utilityPanel = new LockablePanel();
			_utilityPanel.Configure += ConfigureUtilityPanel;
			WristMenu.RegisterWristMenuButton("Spawn Utility Panel", SpawnUtilityPanel);

			// Setup a callback to read and apply our texture
			Stages.Runtime += stage => StartCoroutine(OnRuntime(stage));
		}

		private IEnumerator OnRuntime(RuntimeStage stage)
		{
			// Pull the button sprite and font for our use later
			Transform button = LockablePanel.OptionsPanelPrefab!.transform.Find("OptionsCanvas_0_Main/Canvas/Label_SelectASection/Button_Option_1_Locomotion");
			WidgetDefaults defaults = new();
			defaults.ButtonSprite = button.GetComponent<Image>().sprite;
			defaults.TextFont = button.GetChild(0).GetComponent<Text>().font;

			// Load the log panel texture
			DelayedReader<Texture2D> texReader = stage.GetReader<Texture2D>();
			var op = texReader(Resources.GetFile("LogPanel.png") ?? throw new FileNotFoundException("Log panel texture is missing!"));
			yield return op;
			_logPanel.TextureOverride = op.Result;
		}

		private void ConfigureUtilityPanel(GameObject panel)
		{
			GameObject canvas = panel.transform.Find("OptionsCanvas_0_Main/Canvas").gameObject;
			UiWidget.CreateAndConfigureWidget(canvas, (GridLayoutWidget widget) =>
			{
				// Fill our parent and set pivot to top middle
				widget.RectTransform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
				widget.RectTransform.localPosition = Vector3.zero;
				widget.RectTransform.anchoredPosition = Vector2.zero;
				widget.RectTransform.sizeDelta = new Vector2(37f / 0.07f, 24f / 0.07f);
				widget.RectTransform.pivot = new Vector2(0.5f, 1f);

				// Adjust our grid settings
				widget.GridLayoutGroup.cellSize = new Vector2(171, 50);
				widget.GridLayoutGroup.spacing = Vector2.one * 4;
				widget.GridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
				widget.GridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
				widget.GridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
				widget.GridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
				widget.GridLayoutGroup.constraintCount = 3;

				widget.AddChild((ButtonWidget button) => button.ButtonText.text = "Button 1!");
				widget.AddChild((ButtonWidget button) => button.ButtonText.text = "Mod Configs");
				widget.AddChild((ButtonWidget button) => button.ButtonText.text = "Another button");
				widget.AddChild((ButtonWidget button) => button.ButtonText.text = "Another button 2");
				widget.AddChild((ButtonWidget button) => button.ButtonText.text = "Another button 3");
			});
		}

		private void SpawnUtilityPanel(FVRWristMenu wristMenu)
		{
			GameObject panel = _utilityPanel.GetOrCreatePanel();
			wristMenu.m_currentHand.RetrieveObject(panel.GetComponent<FVRPhysicalObject>());
		}

		#region Log Panel Stuffs

		// Wrist menu button callback. Gets our panel instance and makes the hand retrieve it.
		private void SpawnLogPanel(FVRWristMenu wristMenu)
		{
			GameObject panel = _logPanel.GetOrCreatePanel();
			wristMenu.m_currentHand.RetrieveObject(panel.GetComponent<FVRPhysicalObject>());
		}

		private void ConfigureLogPanel(GameObject panel)
		{
			Transform canvasTransform = panel.transform.Find("OptionsCanvas_0_Main/Canvas");
			_logPanelComponent = panel.AddComponent<BepInExLogPanel>();
			_logPanelComponent.CreateWithExisting(Source, canvasTransform, _logEvents);
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

		#endregion
	}
}
