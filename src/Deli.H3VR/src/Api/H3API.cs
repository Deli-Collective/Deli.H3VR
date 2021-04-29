namespace Deli.H3VR.Api
{
	public class H3API
	{
		public static H3API Instance { get; private set; }
		public LockablePanel LockablePanel { get; }
		public WristMenu WristMenu { get; }

		private Mod Mod;
		internal H3API(Mod mod)
		{
			Instance = this;
			Mod = mod;
			LockablePanel = new LockablePanel();
			WristMenu = new WristMenu();
		}
	}
}
