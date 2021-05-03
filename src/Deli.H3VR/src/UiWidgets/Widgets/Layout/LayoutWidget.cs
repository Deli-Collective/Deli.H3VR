using System;
using UnityEngine;

namespace Deli.H3VR.UiWidgets.Layout
{
	public class LayoutWidget : UiWidget
	{
		protected override void Awake()
		{
			base.Awake();
			RectTransform.FillParent();
		}

		public void AddChild<T>(Action<T> configure) where T : UiWidget
		{
			GameObject go = new(nameof(T));
			CreateAndConfigureWidget(go, configure);
		}
	}
}
