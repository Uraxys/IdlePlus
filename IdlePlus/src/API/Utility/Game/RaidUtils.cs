using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Player;
using Raids;

namespace IdlePlus.API.Utility.Game {
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class RaidUtils {

		public static RaidManager GetRaidManager() => PlayerData.Instance.Quests._raidManager;
		
		public static bool IsInRaidLobby() {
			return !GetRaidManager().IsInRaid && RaidsLobbyManager.IsInRaidParty;
		}
		
		public static bool IsInRaid() {
			return GetRaidManager().IsInRaid;
		}

		public static bool IsLocalPlayerGroupLeader() {
			return GetRaidManager().LobbyManager.LocalPlayerIsRaidLeader;
		}
		
		public static RaidType GetRaidType() {
			return GetRaidManager().LobbyManager._levelInfo.RaidType;
		}

		public static int GetGroupSize() {
			return GetRaidManager().LobbyManager._memberList.GetPlayersForRaid().Count;
		}
		
		public static bool IsGroupFull() {
			return GetGroupSize() >= 6;
		}

		public static bool IsPlayerInGroup([NotNull] string username) {
			foreach (var member in GetRaidManager().LobbyManager._memberList.GetPlayersForRaid()) {
				if (!string.Equals(member.Username, username, StringComparison.CurrentCultureIgnoreCase)) continue;
				return true;
			}
			
			return false;
		}
	}
}