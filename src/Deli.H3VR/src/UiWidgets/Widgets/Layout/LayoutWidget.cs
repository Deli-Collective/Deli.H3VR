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
			T widget = CreateAndConfigureWidget(gameObject, configure);
			widget.RectTransform.localPosition = Vector3.zero;
			widget.RectTransform.localScale = Vector3.one;
		}
	}
}
