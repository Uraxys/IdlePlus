using ChatboxLogic;

namespace IdlePlus.API.Utility.Data {
	/// <summary>
	/// A copy of <see cref="ChannelId"/>, which can be used with the Idle Plus
	/// API.
	/// </summary>
	public enum ChannelType {
		None = 0,
		/// <summary>
		/// General
		/// </summary>
		General = 1,
		// Help
		Help = 2,
		/// <summary>
		/// Trade
		/// </summary>
		Trade = 3,
		/// <summary>
		/// Clan recruiting
		/// </summary>
		ClanRecruitment = 10,
		/// <summary>
		/// Combat-LFG
		/// </summary>
		CombatLookingForGroup = 8,
		/// <summary>
		/// Raids-LFG
		/// </summary>
		RaidLookingForGroup = 9,

		/// <summary>
		/// Private messages
		/// </summary>
		PrivateMessage = 4,
		/// <summary>
		/// Clan chat
		/// </summary>
		ClanPrivate = 5,

		/// <summary>
		/// The chat used when the player is in a combat group.
		/// </summary>
		CombatTeam = 6,
		/// <summary>
		/// The chat used when the player is in a raid group.
		/// </summary>
		RaidTeam = 7
	}

	public static class ChannelTypeExtensions {
		public static ChannelId ToChannelId(this ChannelType type) {
			return (ChannelId)type;
		}
	}
}