using Deli.Setup;
using FistVR;
using UnityEngine;
using UnityEngine.UI;

namespace Deli.H3VR.UtilityPanel
{
	public class Behaviour : DeliBehaviour
	{
		// References to stuff we want
		private FVRWristMenu _wristMenu;
		private GameObject _settingsPanelPrefab;

		private void Awake()
		{
			// Hook into the wrist menu awake method to create the new button
			On.FistVR.FVRWristMenu.Awake += (orig, self) =>
			{
				// Run the original
				orig(self);

				// Get the button and make a copy of it
				var spectatorButton = self.transform.Find("Button_16_SpectatorPanel");
				var utilityPanelButton = Instantiate(spectatorButton, spectatorButton.transform.parent);

				// Take some references
				var button = utilityPanelButton.GetComponent<Button>();
				var pointable = utilityPanelButton.GetComponent<FVRWristMenuPointableButton>();
				var rectTransform = utilityPanelButton.GetComponent<RectTransform>();

				// Move the new button down
				rectTransform.Translate(0, -rectTransform.sizeDelta.y, 0);

				// Add the button to the wrist menu list and change the index of this new pointable
				self.Buttons.Add(button);
				pointable.ButtonIndex = self.Buttons.IndexOf(button);

				// Clear the current onClick action and set our own
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(SpawnUtilityPanel);
			};
		}

		public void Start()
		{
			_wristMenu = FindObjectOfType<FVRWristMenu>();
			_settingsPanelPrefab = _wristMenu.OptionsPanelPrefab;
		}

		public void SpawnUtilityPanel()
		{
			// Spawn the panel and clean it of the unwanted elements
			var utilityPanel = Instantiate(_settingsPanelPrefab, Vector3.zero, Quaternion.identity);
			foreach (GameObject child in utilityPanel.transform)
				if (child.name.Contains("Options"))
					Destroy(child);

			// Change some stuff
			utilityPanel.name = "Deli Mod Utility Panel";

			// Make the hand retrieve the object
			var currentHand = (FVRViveHand) typeof(FVRWristMenu).GetField("m_currentHand").GetValue(_wristMenu);
			currentHand.RetrieveObject(utilityPanel.GetComponent<FVRPhysicalObject>());
		}
	}
}
