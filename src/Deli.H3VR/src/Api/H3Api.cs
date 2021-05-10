using System.Collections.Generic;
using BepInEx.Logging;
using FistVR;
using Steamworks;

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

		private readonly HashSet<Mod> _scoreboardDisabled = new();
		private readonly ManualLogSource _logger = Logger.CreateLogSource("H3Api");

		private H3Api()
		{
			// Wrist Menu stuff
			On.FistVR.FVRWristMenu.Awake += FVRWristMenuOnAwake;
			_wristMenuButtons.ItemAdded += WristMenuButtonsItemAdded;
			_wristMenuButtons.ItemRemoved += WristMenuButtonsItemRemoved;

			// Disable any form of Steam leaderboard uploading
			On.Steamworks.SteamUserStats.UploadLeaderboardScore += (orig, leaderboard, method, score, details, count) =>
			{
				// If no mods have requested disabling leaderboards, let it pass
				if (_scoreboardDisabled.Count == 0) return orig(leaderboard, method, score, details, count);

				// Otherwise log that it's been disabled and return an invalid call
				_logger.LogInfo("Scoreboard submission is disabled as requested by " + _scoreboardDisabled.Count + " mod(s)");
				return SteamAPICall_t.Invalid;
			};
		}

		/// <summary>
		///		API method for requesting scoreboard submission to be disabled. If at the time of score submission any mod has requested submission
		///		to be disabled, it will be skipped.
		/// </summary>
		/// <param name="source">Your mod</param>
		/// <param name="disabled">If you want scoreboard submission disabled</param>
		public void RequestLeaderboardDisable(Mod source, bool disabled)
		{
			if (disabled) _scoreboardDisabled.Add(source);
			else _scoreboardDisabled.Remove(source);
		}
	}
}
