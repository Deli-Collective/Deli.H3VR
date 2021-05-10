using FistVR;
using UnityEngine;
using UnityEngine.UI;

namespace Deli.H3VR.UiWidgets
{
	public class ButtonWidget : UiWidget<FVRPointableButton>
	{

		public Button Button = null!;
		public Text ButtonText = null!;
		public Image ButtonImage = null!;

		protected override void Awake()
		{
			base.Awake();
			ButtonImage = gameObject.AddComponent<Image>();
			Button = gameObject.AddComponent<Button>();
			ButtonImage.sprite = Style.ButtonSprite;
			ButtonImage.color = Style.ButtonColorUnselected;

			// Get the text stuff setup
			GameObject child = new("Text");
			child.transform.SetParent(transform);
			ButtonText = child.AddComponent<Text>();
			((RectTransform) child.transform).FillParent();
			ButtonText.alignment = TextAnchor.MiddleCenter;
			ButtonText.color = Style.TextColor;
			ButtonText.font = Style.TextFont;

			// The PointableButton component is given to us in the base UiWidgets class
			MainComponent.MaxPointingRange = 2;
			MainComponent.Button = Button;
			MainComponent.Image = ButtonImage;
			MainComponent.Text = ButtonText;
			MainComponent.ColorUnselected = Style.ButtonColorUnselected;
			MainComponent.ColorSelected = Style.ButtonColorSelected;

			// Lastly we need a collider for the buttons
			// TODO: This is clearly making them the wrong size, but it still half works
			Vector2 sizeDelta = RectTransform.sizeDelta;
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.center = Vector3.zero;
			boxCollider.size = new Vector3(sizeDelta.x, sizeDelta.y, 5f);
		}
	}
}
