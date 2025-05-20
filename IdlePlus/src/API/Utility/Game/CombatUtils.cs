using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Combat;
using Navigation;

namespace IdlePlus.API.Utility.Game {
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CombatUtils {
		
		private static readonly HashSet<CombatState> InLobbyState = new HashSet<CombatState> {
			CombatState.InLobbyGroup,
			CombatState.InLobbySolo
		};
		
		private static readonly HashSet<CombatState> InCombatState = new HashSet<CombatState> {
			CombatState.InGroupCombat,
			CombatState.InSoloCombat,
			CombatState.OutOfCombatViewGroup,
			CombatState.OutOfCombatViewSolo
		};

		/// <summary>
		/// Retrieves the current combat state of the player.
		/// </summary>
		/// <returns>The current combat state of the player.</returns>
		public static CombatState GetCombatState() => CombatStateManager.Instance.State;

		/// <summary>
		/// Determines if the player is currently in a combat lobby.
		/// </summary>
		/// <returns>True if the player is in a combat lobby, false otherwise.</returns>
		public static bool IsInLobby() => InLobbyState.Contains(GetCombatState());

		/// <summary>
		/// Determines if the player is currently in combat.
		/// </summary>
		/// <returns>True if the player is currently in combat, false otherwise.</returns>
		public static bool IsInCombat() => InCombatState.Contains(GetCombatState());

		/// <summary>
		/// Determines if the player is currently in a group combat lobby.
		/// </summary>
		/// <returns>True if the player is in a group combat lobby, false otherwise.</returns>
		public static bool IsInGroupLobby() => GetCombatState() == CombatState.InLobbyGroup;

		/// <summary>
		/// Determines if the player is currently in group combat.
		/// </summary>
		/// <returns>True if the player is in group combat, false otherwise.</returns>
		public static bool IsInGroupCombat() {
			var state = GetCombatState();
			return state == CombatState.InGroupCombat || state == CombatState.OutOfCombatViewGroup;
		}

		/// <summary>
		/// Determines if the player is currently in a solo combat lobby.
		/// </summary>
		/// <returns>True if the player is in a solo combat lobby, false otherwise.</returns>
		public static bool IsInSoloLobby() => GetCombatState() == CombatState.InLobbySolo;

		/// <summary>
		/// Determines if the player is currently in solo combat.
		/// </summary>
		/// <returns>True if the player is in solo combat, false otherwise.</returns>
		public static bool IsInSoloCombat() {
			var state = GetCombatState();
			return state == CombatState.InSoloCombat || state == CombatState.OutOfCombatViewSolo;
		}

		/*
		 * TODO: Refactor Group?
		 * 
		 * Not sure if I like that we're using static methods for something
		 * that should be instance based.
		 *
		 * Instead, it might be better to return the CombatTeam and add extensions
		 * methods. Or we could go full out and just wrap the group in our own
		 * class.
		 */
		
		public static class Group {

			public static readonly int MaxSize = CombatTeamManager.MAX_MEMBERS;
			
			private static void EnsureGroupState() {
				var state = GetCombatState();
				switch (state) {
					case CombatState.InGroupCombat:
					case CombatState.InLobbyGroup:
					case CombatState.OutOfCombatViewGroup:
						return;
					default: 
						throw new InvalidOperationException("Player is not in group combat.");
				}
			}
			
			/// <summary>
			/// Determines if the local player is the group leader of the current combat group.
			/// </summary>
			/// <returns>True if the local player is the group leader, false otherwise.</returns>
			/// <exception cref="InvalidOperationException">Thrown when the player is not in group combat.</exception>
			public static bool IsLocalPlayerGroupLeader() {
				EnsureGroupState();
				var teamManager = NavigationManager.Instance._combatTeamManager;
				return teamManager.LocalPlayerIsTeamLeader;
			}

			/// <summary>
			/// Gets the size of the current combat group.
			/// </summary>
			/// <returns>The number of members in the group.</returns>
			/// <exception cref="InvalidOperationException">Thrown when the player is not in group combat.</exception>
			public static int GetGroupSize() {
				EnsureGroupState();
				var teamManager = NavigationManager.Instance._combatTeamManager;
				return teamManager.Team.Members.Count;
			}

			/// <summary>
			/// Determines if the current combat group is full.
			/// </summary>
			/// <returns>True if the group is full, false otherwise.</returns>
			/// <exception cref="InvalidOperationException">Thrown when the player is not in group combat.</exception>
			public static bool IsGroupFull() => GetGroupSize() >= CombatTeamManager.MAX_MEMBERS;

			/// <summary>
			/// Determines if the specified player is in the current combat group.
			/// </summary>
			/// <param name="username">The username of the player to check.</param>
			/// <returns>True if the player is in the group, false otherwise.</returns>
			/// <exception cref="InvalidOperationException">Thrown when the player is not in group combat.</exception>
			public static bool IsPlayerInGroup(string username) {
				EnsureGroupState();
				foreach (var member in NavigationManager.Instance._combatTeamManager.Team.Members) {
					if (!string.Equals(member.Username, username, StringComparison.CurrentCultureIgnoreCase)) continue;
					return true;
				}
				
				return false;
			}
		}
	}
}