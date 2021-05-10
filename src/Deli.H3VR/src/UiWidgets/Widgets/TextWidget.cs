using UnityEngine.UI;

namespace Deli.H3VR.UiWidgets
{
	public class TextWidget : UiWidget<Text>
	{
		protected override void Awake()
		{
			base.Awake();
			MainComponent.font = Style.TextFont;
			MainComponent.color = Style.TextColor;
		}
	}
}
