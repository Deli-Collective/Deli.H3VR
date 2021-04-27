using System.Collections.Generic;
using FistVR;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Deli.H3VR.Api
{
	public static class WristMenuButtons
	{
		private struct ButtonOptions
		{
			public string Text { get; set; }
			public UnityAction OnClick { get; set; }
		}

		private static List<ButtonOptions> _buttons = new();

		static WristMenuButtons()
		{
			On.FistVR.FVRWristMenu.Awake += FVRWristMenuOnAwake;
		}

		private static void FVRWristMenuOnAwake(On.FistVR.FVRWristMenu.orig_Awake orig, FVRWristMenu self)
		{
			// We want to place our new button below the spectator panel button (idx 16)
			Button spectatorButton = self.Buttons[16];
			RectTransform spectatorButtonRt = spectatorButton.GetComponent<RectTransform>();

			// Expand the canvas by the height of this button
			RectTransform canvas = self.transform.Find("MenuGo/Canvas").GetComponent<RectTransform>();
			OptionsPanel_ButtonSet buttonSet = canvas.GetComponent<OptionsPanel_ButtonSet>();
			Vector2 size = canvas.sizeDelta;
			size.y += spectatorButtonRt.sizeDelta.y * _buttons.Count;
			canvas.sizeDelta = size;

			// So for any UI elements that are LOWER than this button, move them down by the height of the button
			foreach (RectTransform child in canvas)
			{
				if (!(child.anchoredPosition.y < spectatorButtonRt.anchoredPosition.y)) continue;
				Vector2 pos = child.anchoredPosition;
				pos.y -= spectatorButtonRt.sizeDelta.y * _buttons.Count;
				child.anchoredPosition = pos;
			}

			// Make all the buttons
			for (int i = 0; i < _buttons.Count; i++)
			{
				// Copy the spectator button and place it where it should be
				Button newButton = Object.Instantiate(spectatorButton, canvas);
				RectTransform newButtonRt = newButton.GetComponent<RectTransform>();
				Vector2 pos = newButtonRt.anchoredPosition;
				pos.y -= spectatorButtonRt.sizeDelta.y * (i + 1);
				newButtonRt.anchoredPosition = pos;

				// Apply the options
				ButtonOptions options = _buttons[i];
				newButton.GetComponentInChildren<Text>().text = options.Text;
				newButton.onClick = new Button.ButtonClickedEvent();
				newButton.onClick.AddListener(() =>
				{
					self.Aud.PlayOneShot(self.AudClip_Engage);
					options.OnClick();
				});

				// Now we need to modify some things to accomodate this new button
				FVRWristMenuPointableButton pointable = newButton.GetComponent<FVRWristMenuPointableButton>();
				pointable.ButtonIndex = self.Buttons.Count;
				buttonSet.ButtonImagesInSet = buttonSet.ButtonImagesInSet.AddToArray(newButton.GetComponent<Image>());
				self.Buttons.Add(newButton);
			}
		}

		public static void AddWristMenuButton(string text, UnityAction onClick)
		{
			_buttons.Add(new ButtonOptions
			{
				Text = text,
				OnClick = onClick
			});
		}
	}
}
