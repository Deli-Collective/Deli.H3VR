using FistVR;
using UnityEngine;

namespace Deli.H3VR.Api
{
	public static class H3InputExtensions
	{
		/// <summary>
		///		Matches the game's code to determine if the touchpad has been pressed in the given direction
		/// </summary>
		/// <param name="hand">The hand</param>
		/// <param name="direction">The center of the direction you want to check</param>
		/// <param name="width">The number of degrees away from the center that is allowed</param>
		/// <returns>If the touchpad is pressed down in the given direction</returns>
		/// <example>
		///		IsTouchpadPressedWithinRange(hand, Vector2.up, 45f);
		/// </example>
		public static bool IsTouchpadPressedInDirection(this HandInput hand, Vector2 direction, float width=45f)
		{
			return hand.TouchpadDown && hand.TouchpadAxes.magnitude > .25f && Vector2.Angle(hand.TouchpadAxes, direction) <= width;
		}
	}
}
