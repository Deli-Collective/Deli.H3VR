namespace Deli.H3VR.Api
{
	public class H3API
	{
		public static H3API? Instance { get; private set; }
		public WristMenu WristMenu { get; }

		private Mod Mod;
		private H3API(Mod mod)
		{
			Instance = this;
			Mod = mod;
			WristMenu = new WristMenu();
		}

		internal static H3API GetOrInit(Mod mod) => Instance ?? new H3API(mod);
	}
}
