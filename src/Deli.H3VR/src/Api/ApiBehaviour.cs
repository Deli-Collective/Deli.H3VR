using Deli.Setup;
using Steamworks;

namespace Deli.H3VR.Api
{
	public class ApiBehaviour : DeliBehaviour
	{
		public ApiBehaviour()
		{
			SteamAPI.Init();
			Logger.LogInfo("Deli H3VR initialized. Game build ID: " + SteamApps.GetAppBuildId());
		}
	}
}
