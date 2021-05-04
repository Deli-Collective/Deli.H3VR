using UnityEngine;

namespace Deli.H3VR.UiWidgets
{
	public class WidgetDefaults
	{
		internal static WidgetDefaults Instance { get; private set; }

		internal WidgetDefaults()
		{
		}

		// Text
		public Font TextFont { get; internal set; }
		public Color TextColor { get; } = Color.white;

		// Button
		public Sprite ButtonSprite { get; internal set; }
	}
}
