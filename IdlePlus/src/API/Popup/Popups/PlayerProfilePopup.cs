using System;
using System.Globalization;
using ChatboxLogic;
using Client;
using Databases;
using Equipment;
using Guilds;
using IdlePlus.API.Utility.Extensions;
using IdlePlus.API.Utility.Game;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Collections;
using IdlePlus.Utilities.Extensions;
using IdlePlus.Utilities.Helpers;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Threading.Tasks;
using JetBrains.Annotations;
using Management;
using Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Player;
using Popups;
using Scripts.Content;
using Skilling;
using Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Upgrades;

namespace IdlePlus.API.Popup.Popups {
	
	[RegisterIl2Cpp]
	public class PlayerProfilePopup : CustomHardPopup {
		
		/// <summary>
		/// The <see cref="PopupKey"/> for this popup.
		/// </summary>
		public static PopupKey PopupKey { get; set; }

		/// <summary>
		/// Called when the popup should be created.
		/// </summary>
		public static void Create(GameObject obj) {
			var originalPopup = GameObjects.FindByPath("PopupManager/Canvas/HardPopups/GuildMemberProfilePopup");
			var inner = Instantiate(originalPopup, obj.transform, false);
			inner.name = "InnerProfilePopup";
			inner.SetActive(true);

			var closeButton = inner.Find("CloseButton").Use<CloseButton>();
			closeButton._uiToClose = obj;

			var guildMemberPopup = inner.Use<GuildMemberPopup>();
			var popup = obj.With<PlayerProfilePopup>();
			
			popup._skillPrefab = guildMemberPopup._skillPrefab;
			popup._skillContainer = guildMemberPopup._skillHolder;
			popup._nameText = guildMemberPopup._nameText;
			popup._uiSlots = guildMemberPopup._uiSlots;
			popup._pvmStatPrefab = guildMemberPopup._pvmStatEntry;
			popup._pvmStatContainer = guildMemberPopup._pvmStatContainer;
			popup._upgradePrefab = guildMemberPopup._upgradeEntry;
			popup._upgradeContainer = guildMemberPopup._upgradeContainer;
			popup._equipmentItemInfoHoverTransform = guildMemberPopup._equipmentItemInfoHoverTransform;
			popup._joinDateText = guildMemberPopup._joinDateText;
			popup._clanText = guildMemberPopup._creditsAccumulatedText;
			popup._taskText = guildMemberPopup._currentTaskText;
			popup._skillInstances = guildMemberPopup._skillInstances;
			popup._username = guildMemberPopup._username;
			popup._isOnline = guildMemberPopup._isOnline;

			popup._upgradeScrollRect = inner.Find("UpgradesBackground/Scroll View").Use<ScrollRect>();
			popup._clanText.gameObject.name = "ClanText";
			popup._clanText.SetText("<b>Clan:</b> <i>Not in a clan</i>");
			popup._joinDateText.gameObject.SetActive(false);

			var buttonContainer = inner.Find("ButtonsBackground/FooterButtons");
			IdleLog.Info(buttonContainer.transform.childCount);
			var button = Instantiate(buttonContainer.Find("MessageButton"), buttonContainer.transform, false);
			button.SetActive(false);
			button.name = "ButtonPrefab";
			button.transform.SetAsFirstSibling();
			button.DestroyComponent<GuildMemberInteractionButton>();
			button.Use<Button>().onClick = null;
			var textObj = button.transform.GetChild(0);
			textObj.Use<TextMeshProUGUI>().text = "Btn";
			textObj.gameObject.DestroyComponent<LocalizationText>();
			
			popup._buttonPrefab = button;
			popup._buttonPrefab.transform.SetAsFirstSibling();
			
			// Remove every other button from the container.
			for (int i = buttonContainer.transform.childCount - 1; i > 0; i--) {
				var child = buttonContainer.transform.GetChild(i).gameObject;
				if (child.GetInstanceID() == button.GetInstanceID()) continue;
				DestroyImmediate(child);
			}

			NetworkClient.OnGuildMemberProfileReceived += (Action<ReceiveMemberProfileMessage>) popup.OnReceiveMemberProfileMessage;
			DestroyImmediate(guildMemberPopup);
		}

		private static readonly ExpiringDictionary<string, FullPlayerProfile> CachedPlayerProfiles = 
			new ExpiringDictionary<string, FullPlayerProfile>(TimeSpan.FromMinutes(1));
		
		public override bool BlockingBackground => true;
		public override bool CloseWithEsc => true;
		public override bool CloseWithBackground => true;

		/// <summary>
		/// If the server has responded to our profile request or not.
		/// </summary>
		private bool _serverResponse;
		/// <summary>
		/// If the API has responded to our profile request or not.
		/// </summary>
		private bool _apiResponse;
		
		private string _username;
		private bool _isOnline;
		private bool _inGuild;
		private GameMode _gameMode;
		
		private TextMeshProUGUI _nameText;
		private TextMeshProUGUI _taskText;
		private TextMeshProUGUI _clanText;
		private TextMeshProUGUI _joinDateText;

		private GameObject _buttonPrefab;
		private PlayerProfilePopupButton[] _buttonInstances;

		private Transform _equipmentItemInfoHoverTransform;
		private RemotePlayerEquipmentSlot[] _uiSlots;
		
		private GuildMemberSkillEntry _skillPrefab;
		private Transform _skillContainer;
		private GuildMemberSkillEntry[] _skillInstances;

		private PvmStatEntry _pvmStatPrefab;
		private Transform _pvmStatContainer;
		private PvmStatEntry[] _pvmStatInstances;

		private UpgradeEntry _upgradePrefab;
		private ScrollRect _upgradeScrollRect;
		private Transform _upgradeContainer;
		private System.Collections.Generic.Dictionary<UpgradeType, UpgradeEntry> _upgradeInstances;

		public void Setup(string username, bool expectOnline = false) {
			this._serverResponse = false;
			this._apiResponse = false;
			this._isOnline = expectOnline;
			this._inGuild = false;
			
			this._username = username;
			this._nameText.SetText(username);

			// Try to request the member profile from the server.
			var packet = new RequestMemberProfileMessage { PlayerName = username };
			NetworkClient.SendData(packet);

			// Check if we have the player profile cached, and if so, use that.
			if (CachedPlayerProfiles.TryGet(username.ToLower(), out var profile)) {
				this.OnReceiveApiPlayerProfile(profile, expectOnline);
				return;
			}
			
			// We don't have the player profile, get it from the API.
			ClanApiManager.Instance.GetPlayerProfileAsync(username)
				.ContinueWith((Action<Task>)delegate(Task t) {
					var task = t.Cast<Task<FullPlayerProfile>>();
					if (task.ExceptionRecorded) {
						IdleLog.Error("Failed to fetch player profile in PlayerProfilePopup.", task.Exception);
						return;
					}

					if (task.Result == null) {
						IdleLog.Error("Failed to fetch player profile in PlayerProfilePopup, result is null.");
						return;
					}
					
					// Switch back to the main thread.
					IdleTasks.Run(() => {
						CachedPlayerProfiles.Set(username.ToLower(), task.Result);
						OnReceiveApiPlayerProfile(task.Result, expectOnline);
					});
				});
		}
		
		private void OnReceiveMemberProfileMessage(ReceiveMemberProfileMessage message) {
			IdleLog.Info($"Received member profile.");
			this._isOnline = true;
			
			var equipmentIds = JsonConvert.DeserializeObject<Il2CppStructArray<int>>(message.EquipmentJson);
			var equipment = new Dictionary<EquipmentSlot, int>();
			foreach (var itemId in equipmentIds) {
				if (!ItemDatabase.ItemList.TryGetValue(itemId, out var item)) {
					IdleLog.Error($"Failed to get item from itemId {itemId}");
					continue;
				}
				equipment[item.EquipmentSlot] = itemId;
			}
			
			var skills = new Dictionary<string, double>();
			foreach (var entry in JsonHelper.ToSimpleDictionary<string, double>(JsonConvert
				         .DeserializeObject(message.SkillExperiencesJson).Cast<JObject>())) {
				var key = entry.Key.ToLower();
				if (key == "rigour") key = "attack";
				skills.Add(key, entry.Value);
			}
			
			
			// Update the profile if it's cached.
			if (CachedPlayerProfiles.TryGet(this._username.ToLower(), out var profile)) {
				profile.Equipment = equipment;
				profile.SkillExperiences = skills;
			}

			// We always want to use the information from the server, as it's
			// the most up-to-date information we can get.
			this.SetupEquipment(equipment);
			this.SetupSkills(skills);
			
			if (!this._apiResponse) {
				this.SetupPveStats(null);
				this.SetupUpgrades(null);
			}

			this._taskText.SetText(GetTaskText(message));
			this.SetupButtons();

			this._serverResponse = true;
			if (this._apiResponse) return;
			base.Display();
			IdleTasks.Run(() => this._upgradeScrollRect.SetHorizontalNormalizedPosition(0));
		}

		private void OnReceiveApiPlayerProfile(FullPlayerProfile profile, bool expectOnline) {
			IdleLog.Info($"Received api player profile.");
			this._inGuild = !string.IsNullOrEmpty(profile.GuildName);
			this._gameMode = Enum.TryParse<GameMode>(profile.GameMode, true, out var mode) ? mode : GameMode.NotSelected;
			
			// TODO: REMOVE THIS!
			if (!this._isOnline) this._isOnline = profile.HoursOffline == 0;
			
			// Only set up the inventory if we don't expect the player to be online.
			/*if (!expectOnline) */ // Uncomment when the server always responds to our packet.
			if (!this._serverResponse) {
				this.SetupEquipment(profile.Equipment);
				this.SetupSkills(profile.SkillExperiences);
			}
			
			this.SetupPveStats(profile.PvmStats);
			this.SetupUpgrades(profile.Upgrades);
			
			this.SetupTaskText(profile.TaskNameOnLogout, profile.HoursOffline);
			this.SetupClanText(profile.GuildName);
			this.SetupButtons();
			
			this._apiResponse = true;
			if (this._serverResponse) return;
			base.Display();
			IdleTasks.Run(() => this._upgradeScrollRect.SetHorizontalNormalizedPosition(0));
		}

		private void SetupButtons() {
			if (this._buttonInstances == null) {
				this._buttonInstances = new PlayerProfilePopupButton[Enum.GetValues(typeof(ProfileButtonType)).Length];
				for (var i = 0; i < this._buttonInstances.Length; i++) {
					var type = (ProfileButtonType)i;
					var instance = Instantiate(this._buttonPrefab, this._buttonPrefab.transform.parent);
					this._buttonInstances[i] = instance.With<PlayerProfilePopupButton>(button => button.Init(type));
					instance.SetActive(true);
					instance.name = $"Button{i}";
				}
			}

			foreach (var button in this._buttonInstances) {
				button.Setup(this);
			}
		}
		
		private void SetupEquipment(Dictionary<EquipmentSlot, int> equipment) {
			if (this._serverResponse) return;
			
			foreach (var slot in this._uiSlots) {
				var equipmentSlot = slot._slot;
				if (equipmentSlot == EquipmentSlot.BootsLeft) equipmentSlot = EquipmentSlot.Boots;
				
				if (!equipment.TryGetValue(equipmentSlot, out var itemId) || itemId == -1) {
					slot.SetItemSprite(null, this._equipmentItemInfoHoverTransform, false);
					continue;
				}
				
				if (!ItemDatabase.ItemList.TryGetValue(itemId, out var item)) {
					IdleLog.Error($"Item is null for itemId {itemId} in PlayerProfilePopup#SetupEquipment");
					continue;
				}
				
				slot.SetItemSprite(item, this._equipmentItemInfoHoverTransform, false);
			}
		}

		private void SetupSkills(Dictionary<string, double> skills) {
			if (this._skillInstances == null) {
				this._skillInstances = new GuildMemberSkillEntry[Enum.GetValues(typeof(Skill)).Length];
			}
			
			// Starting at index 1, as 0 is "None".
			for (var i = 1; i < this._skillInstances.Length; i++) {
				var skillInstance = this._skillInstances[i];
				
				if (!skillInstance) {
					skillInstance = Instantiate(this._skillPrefab, this._skillContainer);
					this._skillInstances[i] = skillInstance;
				}

				var skill = (Skill) i;
				var skillName = skill == Skill.Rigour ? "attack" : skill.ToString().ToLower();
				if (!skills.TryGetValue(skillName, out var experience)) {
					IdleLog.Error($"Failed to get experience for skill {skillName} () in PlayerProfilePopup#SetupSkills");
					continue;
				}
				
				skillInstance.Setup(skill, SkillUtils.GetLevelForExperience((int) experience));
			}
		}

		private void SetupPveStats([CanBeNull] Dictionary<PvmStatType, int> stats) {
			if (this._pvmStatInstances == null) {
				this._pvmStatInstances = new PvmStatEntry[Enum.GetValues(typeof(PvmStatType)).Length];
			}
			
			// Starting at index 1, as 0 is "None".
			for (var i = 1; i < this._pvmStatInstances.Length; i++) {
				var statInstance = this._pvmStatInstances[i];
				
				if (!statInstance) {
					statInstance = Instantiate(this._pvmStatPrefab, this._pvmStatContainer);
					this._pvmStatInstances[i] = statInstance;
				}

				// If stats is null then we want to hide all the stats.
				if (stats == null) {
					statInstance.gameObject.SetActive(false);
					continue;
				}
				
				var statType = (PvmStatType) i;
				if (!stats.TryGetValue(statType, out var kills)) {
					IdleLog.Error($"Failed to get kills for stat {statType} in PlayerProfilePopup#SetupPveStats");
					continue;
				}
				
				statInstance.gameObject.SetActive(true);
				statInstance.Setup(statType, kills);
			}
		}

		private void SetupUpgrades([CanBeNull] Dictionary<string, int> upgrades) {
			if (this._upgradeInstances == null) {
				// Upgrades might be null if the API request failed.
				if (upgrades == null) return;
				// There is a ton of upgrades, some aren't even used by players,
				// so lets only create the ones that are actually needed.
				this._upgradeInstances = new System.Collections.Generic.Dictionary<UpgradeType, UpgradeEntry>();
				foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType))) {
					var upgradeName = type.ToString().ToLower();
					if (!upgrades.TryGetValue(upgradeName, out _)) continue;
					
					var upgradeEntry = Instantiate(this._upgradePrefab, this._upgradeContainer);
					this._upgradeInstances.Add(type, upgradeEntry);
				}
			}

			// If the upgrades is null then we want to hide all the upgrades.
			if (upgrades == null) {
				foreach (var entry in this._upgradeInstances) {
					entry.Value.gameObject.SetActive(false);
				}
				return;
			}
			
			foreach (var entry in upgrades) {
				if (!Enum.TryParse<UpgradeType>(entry.Key, out var type)) {
					IdleLog.Error($"Failed to parse upgrade type {entry.Key} in PlayerProfilePopup#SetupUpgrades");
					continue;
				}
				
				if (!this._upgradeInstances.TryGetValue(type, out var upgradeEntry)) {
					IdleLog.Error($"Failed to get upgrade entry for {type} in PlayerProfilePopup#SetupUpgrades");
					continue;
				}
				
				upgradeEntry.gameObject.SetActive(true);
				upgradeEntry.Setup(type, entry.Value);
			}
		}

		private void SetupTaskText(string task, double hoursOffline) {
			if (this._serverResponse) return;
			
			var currentTask = string.IsNullOrEmpty(task) ? "none" : task;
			currentTask = LocalizationManager.GetLocalizedValue(currentTask);
			var taskText = LocalizationManager.GetLocalizedValue("clan_member_offline_info", this._username,
				hoursOffline.ToString(CultureInfo.CurrentCulture), currentTask);
			this._taskText.SetText(taskText);
		}

		private void SetupClanText(string clan) {
			var clanName = string.IsNullOrEmpty(clan) ? "Not in a clan" : clan;
			this._clanText.SetText($"Clan: {clanName}");
		}

		private static string GetTaskText(ReceiveMemberProfileMessage message) {
			var taskText = LocalizationManager.GetLocalizedValue("member_current_task_empty");
			
			if (message.ActiveBossType != ClanEventBossType.None) {
				var clanBoss = ClanBossDatabase.ClanBossInfos[message.ActiveBossType];
				taskText = LocalizationManager.GetLocalizedValue(clanBoss.BossNameLocalizationKey);
				taskText = LocalizationManager.GetLocalizedValue("in_clan_boss_fight", taskText);
				taskText = LocalizationManager.GetLocalizedValue("member_current_activity", taskText);
				return taskText;
			}

			if (message.InClanEvent) {
				taskText = LocalizationManager.GetLocalizedValue("in_clan_event");
				taskText = LocalizationManager.GetLocalizedValue("member_current_activity", taskText);
				return taskText;
			}

			if (message.InRaid) {
				taskText = LocalizationManager.GetLocalizedValue("member_profile_in_raid");
				taskText = LocalizationManager.GetLocalizedValue("member_current_activity", taskText);
				return taskText;
			}

			if (message.InRaidLobby) {
				taskText = LocalizationManager.GetLocalizedValue("member_profile_in_raid_lobby");
				taskText = LocalizationManager.GetLocalizedValue("member_current_activity", taskText);
				return taskText;
			}

			if (message.TaskType == TaskType.None || message.TaskId < 0) return taskText; 
			
			var type = message.TaskType;
			var tasks = TaskManager.Instance.Tasks[type];
			if (tasks == null) {
				IdleLog.Error($"Failed to get tasks for task type {type}");
				return taskText;
			}
			var jobTask = tasks[message.TaskId];
			if (jobTask == null) {
				IdleLog.Error($"Failed to get task for task id {message.TaskId}");
				return taskText;
			}
				
			var taskName = LocalizationManager.GetLocalizedValue(type.ToString().ToLower());
			var jobName = LocalizationManager.GetLocalizedValue(jobTask.Name);
			return LocalizationManager.GetLocalizedValue("member_current_task", taskName, jobName);
		}

		[RegisterIl2Cpp]
		private class PlayerProfilePopupButton : MonoBehaviour {

			private ProfileButtonType _type;
			private Button _button;
			
			private string _username;
			private bool _online;
			private GameMode _gameMode;
			private bool _guild;
			
			[HideFromIl2Cpp]
			public void Init(ProfileButtonType type) {
				this._type = type;
				this._button = this.GetComponent<Button>();
				this._button.onClick.AddListener((UnityAction) OnButtonClicked);
				
				var text = this.transform.GetChild(0).Use<TextMeshProUGUI>();
				switch (this._type) {
					case ProfileButtonType.Message: text.text = "Message"; break;
					case ProfileButtonType.InviteToClan: text.text = "Invite to clan"; break;
					case ProfileButtonType.InviteToCombat: text.text = "Invite to combat"; break;
					case ProfileButtonType.InviteToRaid: text.text = "Invite to raid"; break;
					default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
				}
			}

			[HideFromIl2Cpp]
			public void Setup(PlayerProfilePopup popup) {
				this._username = popup._username;
				this._online = popup._isOnline;
				this._gameMode = popup._gameMode;
				this._guild = popup._inGuild;

				// Don't display any buttons if we're looking at ourselves.
				if (this._username == PlayerData.Instance.Username) {
					this.gameObject.SetActive(false);
					return;
				}
				
				// Some basic restrictions that all buttons except the message button share.
				if (this._type != ProfileButtonType.Message) {
					var ourGameMode = PlayerData.Instance.GameMode;
					if ((!this._online && this._type != ProfileButtonType.InviteToClan) || 
						ourGameMode != this._gameMode || ourGameMode == GameMode.Ironman) {
						
						this.gameObject.SetActive(false);
						return;
					}
					
					if (ourGameMode == GameMode.GroupIronman) {
						var guild = GuildManager.Instance.OurGuild;
						if (guild?.AllMembers[this._username] == null) {
							this.gameObject.SetActive(false);
							return;
						}
					}
				}
				
				switch (this._type) {
					case ProfileButtonType.Message: this.SetupMessageButton(); break;
					case ProfileButtonType.InviteToClan: this.SetupInviteToClanButton(); break;
					case ProfileButtonType.InviteToCombat: this.SetupInviteToCombatButton(); break;
					case ProfileButtonType.InviteToRaid: this.SetupInviteToRaidButton(); break;
					default: throw new ArgumentOutOfRangeException();
				}
			}
			
			private void SetupMessageButton() {
				this.gameObject.SetActive(true);
				this.Interactable(this._online);
			}
			
			private void SetupInviteToClanButton() {
				// Display if we're in a guild and the target player is not.
				var guild = GuildManager.Instance.OurGuild;
				var display = guild != null && !this._guild && !guild.IsPlayerInGuild(this._username);
				this.gameObject.SetActive(display);
				if (!display) return;
				
				// Interactable if we have permission to invite the player and
				// our guild isn't full.
				var interactable = guild.LocalPlayerHasAdminPrivileges() && guild.AllMembers.Count < 20;
				this.Interactable(interactable);
			}
			
			private void SetupInviteToCombatButton() {
				// Display if we're in a group combat lobby.
				var display = CombatUtils.IsInGroupLobby();
				this.gameObject.SetActive(display);
				if (!display) return;
				
				// Interactable if the player is the team leader, the lobby isn't full
				// and the player isn't already in the lobby.
				var interactable = CombatUtils.Group.IsLocalPlayerGroupLeader() &&
								   !CombatUtils.Group.IsPlayerInGroup(this._username) &&
								   !CombatUtils.Group.IsGroupFull();
				this.Interactable(interactable);
			}
			
			private void SetupInviteToRaidButton() {
				// Display if we're in a raid lobby.
				var display = RaidUtils.IsInRaidLobby();
				this.gameObject.SetActive(display);
				if (!display) return;
				
				// Interactable if the player is the team leader, the lobby isn't full
				// and the player isn't already in the lobby.
				var interactable = RaidUtils.IsLocalPlayerGroupLeader() &&
								   !RaidUtils.IsPlayerInGroup(this._username) &&
								   !RaidUtils.IsGroupFull();
				this.Interactable(interactable);
			}
			
			private void OnButtonClicked() {
				switch (this._type) {
					case ProfileButtonType.Message: this.OnMessageClicked(); break;
					case ProfileButtonType.InviteToClan: this.OnInviteToClanClicked(); break;
					case ProfileButtonType.InviteToCombat: this.OnInviteToCombatClicked(); break;
					case ProfileButtonType.InviteToRaid: this.OnInviteToRaidClicked(); break;
					default: throw new ArgumentOutOfRangeException();
				}
			}
			
			private void OnMessageClicked() {
				if (this._username == null || !this._online) return;
				
				ChatboxManager.Instance.SetupChannel(ChannelId.privatemessage, this._username);
				ChatboxManager.Instance.SelectPrivateChatByUsername(this._username);

				var ip = GameManager.ActiveDeploymentInfo.ChatServiceIp;
				var port = GameManager.ActiveDeploymentInfo.ChatServicePort;
				NetworkClientChatService.Instance.Connect(ip, port, true);
			}
			
			private void OnInviteToClanClicked() {
				if (this._username == null || this._guild) return;
				var guild = GuildManager.Instance.OurGuild;
				if (guild == null) return;
				
				if (guild.IsPlayerInGuild(this._username)) return;
				if (!guild.LocalPlayerHasAdminPrivileges()) return;
				if (guild.AllMembers.Count >= 20) return;
				
				NetworkClient.SendData(new SendGuildInviteMessage { PlayerReceivingInvite = this._username });
				this.Interactable(false);
			}
			
			private void OnInviteToCombatClicked() {
				if (this._username == null || !this._online) return;
				if (!CombatUtils.IsInGroupLobby()) return;
				if (!CombatUtils.Group.IsLocalPlayerGroupLeader() || CombatUtils.Group.IsGroupFull()) return;
				if (CombatUtils.Group.IsPlayerInGroup(this._username)) return;
				
				NetworkClient.SendData(new SendCombatTeamInvitationMessage { ToUsername = this._username });
				// Vanilla behavior is to show a loading popup after sending an invitation.
				PopupUtils.ShowLoadingPopup();
				this.Interactable(false);
			}
			
			private void OnInviteToRaidClicked() {
				if (this._username == null || !this._online) return;
				if (!RaidUtils.IsInRaidLobby()) return;
				if (!RaidUtils.IsLocalPlayerGroupLeader()) return;
				if (RaidUtils.IsGroupFull()) return;
				if (RaidUtils.IsPlayerInGroup(this._username)) return;
				
				NetworkClient.SendData(new InvitePlayerToRaidsMessage {
					Username = this._username,
					RaidType = RaidUtils.GetRaidType(),
					RequestResponse = true
				});
				this.Interactable(false);
			}
			
			private void Interactable(bool value) => this._button.interactable = value;
		}

		private enum ProfileButtonType {
			Message,
			InviteToClan,
			InviteToCombat,
			InviteToRaid,
		}
	}
}