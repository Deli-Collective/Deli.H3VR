using System;
using UnityEngine;
using UnityEngine.UI;

namespace Deli.H3VR.UiWidgets
{
	/// <summary>
	///		The base class for all UI Widgets.
	/// </summary>
	public class UiWidget : MonoBehaviour
	{
		public RectTransform RectTransform;
		public WidgetManager Manager;

		/// <summary>
		///		All configuration of widgets are done in their Awake() methods.
		/// </summary>
		protected virtual void Awake()
		{
			// Make sure we're a 2D UI element
			Manager = WidgetManager.Instance;
			RectTransform = gameObject.AddComponent<RectTransform>();
		}

		/// <summary>
		///		Creates a widget on the provided game object and configures it
		/// </summary>
		/// <param name="go">The game object to create the widget on</param>
		/// <param name="configure">The configuration to apply to the widget</param>
		/// <typeparam name="T">The type of widget to make</typeparam>
		/// <returns>The created widget</returns>
		public static T CreateAndConfigureWidget<T>(GameObject go, Action<T> configure) where T : UiWidget
		{
			T widget = go.AddComponent<T>();
			configure(widget);
			return widget;
		}
	}
}
