using System;
using UnityEngine;
using UnityEngine.UI;

namespace Deli.H3VR.UiWidgets
{
	/// <summary>
	///		Widget that represents a layout group (e.g. GridLayoutGroup or HorizontalLayoutGroup) that can have children widgets
	/// </summary>
	/// <typeparam name="TLayout">The type of the layout group</typeparam>
	public class LayoutWidget<TLayout> : UiWidget<TLayout> where TLayout : LayoutGroup
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
