using UnityEngine.UI;

namespace Deli.H3VR.UiWidgets
{
	public class TextWidget : UiWidget
	{
		protected override void Awake()
		{
			base.Awake();
			gameObject.AddComponent<Text>();
		}
	}
}
