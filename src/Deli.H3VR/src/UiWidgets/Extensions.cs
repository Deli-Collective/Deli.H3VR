﻿using UnityEngine;

namespace Deli.H3VR.UiWidgets
{
	public static class Extensions
	{
		/// <summary>
		///		Anchors this element so that it completely fills it's parent
		/// </summary>
		/// <param name="rt"></param>
		public static void FillParent(this RectTransform rt)
		{
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.sizeDelta = Vector2.zero;
		}
	}
}