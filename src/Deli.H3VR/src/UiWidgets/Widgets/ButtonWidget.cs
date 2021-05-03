using UnityEngine;
using UnityEngine.UI;

namespace Deli.H3VR.UiWidgets
{
	public class ButtonWidget : UiWidget
	{
		public Button Button = null!;
		public Text ButtonText = null!;
		public Image ButtonImage = null!;

		protected override void Awake()
		{
			base.Awake();
			ButtonImage = gameObject.AddComponent<Image>();
			Button = gameObject.AddComponent<Button>();
			ButtonImage.sprite = Manager.Defaults.ButtonSprite;


			GameObject child = new("Text");
			child.transform.SetParent(transform);
			ButtonText = child.AddComponent<Text>();
			((RectTransform) child.transform).FillParent();
			ButtonText.alignment = TextAnchor.MiddleCenter;
			ButtonText.color = Manager.Defaults.TextColor;
			ButtonText.font = Manager.Defaults.TextFont;
		}
	}
}
