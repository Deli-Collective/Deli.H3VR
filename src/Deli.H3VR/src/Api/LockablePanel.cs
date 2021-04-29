using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deli.H3VR.Api
{
	public class LockablePanel
	{
		private GameObject? _optionsPanelPrefab;

		internal LockablePanel()
		{
			On.FistVR.FVRWristMenu.Awake += (_, self) => _optionsPanelPrefab = self.OptionsPanelPrefab;
		}

		public GameObject GetCleanLockablePanel()
		{
			if (_optionsPanelPrefab is null || !_optionsPanelPrefab)
				throw new InvalidOperationException("You're trying to create a lockable panel too early! Please wait until the runtime phase.");

			GameObject panel = Object.Instantiate(_optionsPanelPrefab);
			CleanPanel(panel);
			return panel;
		}

		private static void CleanPanel(GameObject panel)
		{
			Transform panelTransform = panel.transform;

			// This proto object has a bunch of hidden stuff we don't want, but it does also contain the actual panel model
			// So just move it up and delete the proto
			Transform proto = panelTransform.Find("OptionsPanelProto");
			proto.Find("Tablet").SetParent(panelTransform);
			Object.Destroy(proto.gameObject);

			// Then, everything else we want to delete in the main object is disabled so use that as a filter
			foreach (Transform child in panelTransform)
			{
				if (!child.gameObject.activeSelf)
					Object.Destroy(child.gameObject);
			}

			// Lastly we just want to clear out the main canvas
			Transform canvas = panelTransform.Find("OptionsCanvas_0_Main/Canvas");
			foreach (Transform child in canvas)
			{
				Object.Destroy(child.gameObject);
			}
		}
	}
}
