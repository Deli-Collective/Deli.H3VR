using UnityEngine.UI;

namespace Deli.H3VR.UiWidgets.Layout
{
	public class GridLayoutWidget : LayoutWidget
	{
		public GridLayoutGroup GridLayoutGroup = null!;

		protected override void Awake()
		{
			base.Awake();
			GridLayoutGroup = gameObject.AddComponent<GridLayoutGroup>();
		}
	}
}
