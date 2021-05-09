using BepInEx.Logging;
using FistVR;

namespace Deli.H3VR.Api
{
	public partial class H3Api
	{
		private static H3Api? _instance;

		/// <summary>
		///		Gets the instance of the H3 API object.
		/// </summary>
		public static H3Api Instance
		{
			get
			{
				_instance ??= new H3Api();
				return _instance;
			}
		}

		/// <summary>
		///		Reference to the current Wrist Menu
		/// </summary>
		public FVRWristMenu? WristMenu { get; private set; }

		private ManualLogSource _logger = Logger.CreateLogSource("H3Api");

		private H3Api()
		{
			// Wrist Menu stuff
			On.FistVR.FVRWristMenu.Awake += FVRWristMenuOnAwake;
			_wristMenuButtons.ItemAdded += WristMenuButtonsItemAdded;
			_wristMenuButtons.ItemRemoved += WristMenuButtonsItemRemoved;
		}
	}
}
