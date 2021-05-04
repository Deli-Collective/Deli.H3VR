using FistVR;
using UnityEngine;
using UnityEngine.UI;

namespace Deli.H3VR.UiWidgets
{
	public class ButtonWidget : UiWidget
	{
		private static readonly Color ColorUnselected = new Color(27 / 255f, 73 / 255f, 155 / 255f, 160 / 255f);
		private static readonly Color ColorSelected = new Color(192 / 255f, 202 / 255f, 222 / 255f, 216 / 255f);

		public Button Button = null!;
		public Text ButtonText = null!;
		public Image ButtonImage = null!;
		public FVRPointableButton PointableButton = null!;

		protected override void Awake()
		{
			base.Awake();
			ButtonImage = gameObject.AddComponent<Image>();
			Button = gameObject.AddComponent<Button>();
			ButtonImage.sprite = Defaults.ButtonSprite;
			ButtonImage.color = ColorUnselected;

			// Get the text stuff setup
			GameObject child = new("Text");
			child.transform.SetParent(transform);
			ButtonText = child.AddComponent<Text>();
			((RectTransform) child.transform).FillParent();
			ButtonText.alignment = TextAnchor.MiddleCenter;
			ButtonText.color = Defaults.TextColor;
			ButtonText.font = Defaults.TextFont;

			// Now we need the Pointable Button component
			PointableButton = gameObject.AddComponent<FVRPointableButton>();
			PointableButton.MaxPointingRange = 2;
			PointableButton.Button = Button;
			PointableButton.Image = ButtonImage;
			PointableButton.Text = ButtonText;

			// I can't be bothered to convert these from 0.0 to 1.0
			PointableButton.ColorUnselected = ColorUnselected;
			PointableButton.ColorSelected = ColorSelected;

			// Lastly we need a collider for the buttons
			Vector2 sizeDelta = RectTransform.sizeDelta;
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.center = Vector3.zero;
			boxCollider.size = new Vector3(sizeDelta.x, sizeDelta.y, 5f);
		}
	}
}
