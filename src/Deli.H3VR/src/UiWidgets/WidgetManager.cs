using Deli.Runtime;
using Deli.Setup;
using UnityEngine;

namespace Deli.H3VR.UiWidgets
{
	/// <summary>
	///		This exist pretty much just to contain the default values for a bunch of widgets
	/// </summary>
	public class WidgetManager : DeliBehaviour
	{
		public WidgetDefaults Defaults = new();

		internal static WidgetManager Instance { get; private set; }

		internal WidgetManager()
		{
			Instance = this;
			Stages.Runtime += OnRuntime;
		}

		private void OnRuntime(RuntimeStage stage)
		{
			throw new System.NotImplementedException();
		}
	}

	public class WidgetDefaults
	{
		// Text
		public Font TextFont { get; internal set; }
		public Color TextColor { get; internal set; } = Color.white;

		// Button
		public Sprite ButtonSprite { get; internal set; }
	}
}
