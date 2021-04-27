using Deli.H3VR.Api;
using Deli.Setup;

namespace Deli.H3VR.LogPanel
{
	public class BepInExLogPanel : DeliBehaviour
	{
		public BepInExLogPanel()
		{
			Stages.Setup += OnSetup;
		}

		private void OnSetup(SetupStage stage)
		{
			WristMenuButtons.AddWristMenuButton("BepInEx Log", () =>
			{
				Logger.LogInfo("Maybe open the log panel or something idk");
			});
		}
	}
}
