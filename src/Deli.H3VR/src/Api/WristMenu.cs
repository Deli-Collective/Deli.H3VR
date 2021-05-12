using System.Collections.Generic;
using System.Linq;
using FistVR;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Deli.H3VR.Api
{
	public partial class H3Api
	{
		/// <summary>
		///		Collection of wrist menu buttons. Add to this collection to register a button and remove from the collection to unregister.
		/// </summary>
		public ICollection<WristMenuButton> WristMenuButtons => _wristMenuButtons;

		private readonly ObservableHashSet<WristMenuButton> _wristMenuButtons = new();
		private readonly Dictionary<WristMenuButton, Button> _currentButtons = new();

		private void WristMenuButtonsItemAdded(WristMenuButton button)
		{
			if (WristMenu is null || !WristMenu) return;
			AddWristMenuButton(WristMenu, button);
		}

		private void WristMenuButtonsItemRemoved(WristMenuButton button)
		{
			if (WristMenu is null || !WristMenu) return;
			RemoveWristMenuButton(WristMenu, button);
		}

		/// <summary>
		///		Hook for the wrist menu awake. This adds all the buttons to the wrist menu
		/// </summary>
		private void FVRWristMenuOnAwake(On.FistVR.FVRWristMenu.orig_Awake orig, FVRWristMenu self)
		{
			// Note to self; this is required and very important.
			orig(self);

			// Keep our reference to the wrist menu up to date
			WristMenu = self;

			// Clear the list of existing buttons
			_currentButtons.Clear();

			// For all the registered buttons, add them
			foreach (WristMenuButton button in _wristMenuButtons)
				AddWristMenuButton(self, button);
		}

		/// <summary>
		///		Adds a wrist menu button to the wrist menu
		/// </summary>
		private void AddWristMenuButton(FVRWristMenu wristMenu, WristMenuButton button)
		{
			// The button we want to use as a reference is either the spectator button (wristMenu.Buttons[16])
			// or the button just above where this one should go according to the priority
			WristMenuButton? aboveButton = _wristMenuButtons
				.OrderByDescending(x => x.Priority)
				.LastOrDefault(x => x.Priority > button.Priority);
			Button referenceButton = aboveButton is null ? wristMenu.Buttons[16] : _currentButtons[aboveButton];
			RectTransform referenceRt = referenceButton.GetComponent<RectTransform>();

			// Expand the canvas by the height of this button
			RectTransform canvas = wristMenu.transform.Find("MenuGo/Canvas").GetComponent<RectTransform>();
			OptionsPanel_ButtonSet buttonSet = canvas.GetComponent<OptionsPanel_ButtonSet>();
			Vector2 size = canvas.sizeDelta;
			size.y += referenceRt.sizeDelta.y;
			canvas.sizeDelta = size;

			// So for any UI elements that are LOWER than this button, move them down by the height of the button
			foreach (RectTransform child in canvas)
			{
				if (!(child.anchoredPosition.y < referenceRt.anchoredPosition.y)) continue;
				Vector2 pos1 = child.anchoredPosition;
				pos1.y -= referenceRt.sizeDelta.y;
				child.anchoredPosition = pos1;
			}

			// Copy the spectator button and place it where it should be
			Button newButton = Object.Instantiate(referenceButton, canvas);
			RectTransform newButtonRt = newButton.GetComponent<RectTransform>();
			Vector2 pos = newButtonRt.anchoredPosition;
			pos.y -= referenceRt.sizeDelta.y;
			newButtonRt.anchoredPosition = pos;

			// Apply the options
			newButton.GetComponentInChildren<Text>().text = button.Text;
			newButton.onClick = new Button.ButtonClickedEvent();
			newButton.onClick.AddListener(() =>
			{
				wristMenu.Aud.PlayOneShot(wristMenu.AudClip_Engage);
				button.CallOnClick(this);
			});

			// Now we need to modify some things to accomodate this new button
			FVRWristMenuPointableButton pointable = newButton.GetComponent<FVRWristMenuPointableButton>();
			pointable.ButtonIndex = wristMenu.Buttons.Count;
			buttonSet.ButtonImagesInSet = buttonSet.ButtonImagesInSet.AddToArray(newButton.GetComponent<Image>());
			wristMenu.Buttons.Add(newButton);

			// Finally add it to the dict and call the create event
			_currentButtons.Add(button, newButton);
			button.CurrentButton = pointable;
			button.CallOnCreate(this, pointable);
		}

		/// <summary>
		///		Removes a wrist menu button from the wrist menu
		/// </summary>
		private void RemoveWristMenuButton(FVRWristMenu wristMenu, WristMenuButton button)
		{
			// This time our reference is the current button
			Button referenceButton = _currentButtons[button];
			RectTransform referenceRt = referenceButton.GetComponent<RectTransform>();

			// Shrink the canvas by the height of this button
			RectTransform canvas = wristMenu.transform.Find("MenuGo/Canvas").GetComponent<RectTransform>();
			OptionsPanel_ButtonSet buttonSet = canvas.GetComponent<OptionsPanel_ButtonSet>();
			Vector2 size = canvas.sizeDelta;
			size.y -= referenceRt.sizeDelta.y;
			canvas.sizeDelta = size;

			// So for any UI elements that are LOWER than this button, move them up by the height of the button
			foreach (RectTransform child in canvas)
			{
				if (!(child.anchoredPosition.y < referenceRt.anchoredPosition.y)) continue;
				Vector2 pos1 = child.anchoredPosition;
				pos1.y += referenceRt.sizeDelta.y;
				child.anchoredPosition = pos1;
			}

			// Then remove it from the internal stuff.
			// Unfortunately, removing a button requires us to re-assign the index values of all the buttons on the wrist menu :P
			wristMenu.Buttons.Remove(_currentButtons[button]);
			buttonSet.ButtonImagesInSet = wristMenu.Buttons.Select(x => x.GetComponent<Image>()).ToArray();
			for (int i = 0; i < buttonSet.ButtonImagesInSet.Length; i++)
			{
				FVRWristMenuPointableButton pointable = buttonSet.ButtonImagesInSet[i].GetComponent<FVRWristMenuPointableButton>();
				pointable.ButtonIndex = i;
			}

			// Destroy the object and remove the button from the dict
			Object.Destroy(referenceButton.gameObject);
			_currentButtons.Remove(button);
		}
	}

	/// <summary>
	///		Represents a wrist menu button
	/// </summary>
	public class WristMenuButton
	{
		public string Text { get; }
		public int Priority { get; }

		/// <summary>
		///		Reference to this button's actual wrist menu button instance. This may be null when the button is not on the wrist menu
		/// </summary>
		public FVRWristMenuPointableButton? CurrentButton { get; internal set; }

		/// <summary>
		///		Called when this button is added to the wrist menu and provides a reference to the game object that was created
		/// </summary>
		public event WristMenuButtonOnCreate? OnCreate;

		/// <summary>
		///		Called when this button is selected by the player
		/// </summary>
		public event WristMenuButtonOnClick? OnClick;

		public WristMenuButton(string text, WristMenuButtonOnClick? clickAction = null)
		{
			Text = text;
			Priority = 0;
			if (clickAction is not null) OnClick += clickAction;
		}

		public WristMenuButton(string text, int priority, WristMenuButtonOnClick? clickAction = null)
		{
			Text = text;
			Priority = priority;
			if (clickAction is not null) OnClick += clickAction;
		}

		public delegate void WristMenuButtonOnClick(H3Api api, WristMenuButton caller);
		public delegate void WristMenuButtonOnCreate(H3Api api, FVRWristMenuPointableButton button);

		internal void CallOnCreate(H3Api api, FVRWristMenuPointableButton button) => OnCreate?.Invoke(api, button);
		internal void CallOnClick(H3Api api) => OnClick?.Invoke(api, this);
	}
}
