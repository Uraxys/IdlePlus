using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Equipment;
using IdleClansApi;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Helpers;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scripts.Shared.Data.Content.Skills;
using Upgrades;

namespace IdlePlus.API {
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public static class IdleAPI {

		private static HttpClient _client;
		private static string _baseUrl;
		private static bool _initialized;

		private static void Initialize() {
			if (_initialized) return;
			_client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
			_baseUrl = IdleClansAPIManager.Instance.BaseApiUrl;
			_initialized = true;
		}
		
		private static Task<string> GetAsyncJson(string url) {
			Initialize();
			return _client.GetAsync($"{_baseUrl}/{url}")
				.ContinueWith(task => {
					if (task.Exception != null) {
						IdleLog.Error($"Failed to fetch data from API, url {url}", task.Exception);
						throw task.Exception;
					}
					
					var response = task.Result;
					if (!response.IsSuccessStatusCode) {
						IdleLog.Error($"Failed to fetch data from API, url {url}, status code: {response.StatusCode}");
						throw new Exception($"Failed to fetch data from API, url {url}, status code: {response.StatusCode}");
					}

					return response.Content.ReadAsStringAsync().Result;
				});
		}
		
		public static class Clan {
			
		}

		public static class Player {

			public static Task<PlayerProfile> GetPlayerProfile(string username) {
				return GetAsyncJson($"Player/profile/{username}?localize=false")
					.ContinueWith(task => {
						if (task.Exception != null) throw task.Exception;
						var result = task.Result;
						
						return PlayerProfile.FromJson(result);
					});
			}
			
			public class PlayerProfile {
			
				public string Username { get; set; }
				public GameMode GameMode { get; set; }
				[CanBeNull] public string GuildName { get; set; }
			
				public double HoursOffline { get; set; }
				public int TaskTypeOnLogout { get; set; }
				public string TaskNameOnLogout { get; set; }
			
				public Dictionary<Skill, double> SkillExperiences { get; set; }
				public Dictionary<EquipmentSlot, int> Equipment { get; set; }
				public Dictionary<Skill, double> EnchantmentBoosts { get; set; }
				public Dictionary<UpgradeType, int> Upgrades { get; set; }
				public Dictionary<PvmStatType, int> PvMStats { get; set; }

				public override string ToString() {
					var experiences = string.Join(", ", SkillExperiences);
					var equipment = string.Join(", ", Equipment);
					var enchantmentBoosts = string.Join(", ", EnchantmentBoosts);
					var upgrades = string.Join(", ", Upgrades);
					var pvmStats = string.Join(", ", PvMStats);
					
					return $"PlayerProfile{{Username={Username}, GameMode={GameMode}, GuildName={GuildName}, " +
					       $"HoursOffline={HoursOffline}, TaskTypeOnLogout={TaskTypeOnLogout}, " +
					       $"TaskNameOnLogout={TaskNameOnLogout}, SkillExperiences={experiences}, " +
					       $"Equipment={equipment}, EnchantmentBoosts={enchantmentBoosts}, Upgrades={upgrades}, " +
					       $"PvMStats={pvmStats}}}";
				}

				internal static PlayerProfile FromJson(string json) {
					var root = JsonConvert.DeserializeObject(json).Cast<JObject>();
					
					return new PlayerProfile {
						Username = JsonHelper.ToValue<string>(root["username"]),
						GameMode = JsonHelper.ToValue<GameMode>(root["gameMode"]),
						GuildName = JsonHelper.ToValue<string>(root["guildName"]),
						
						HoursOffline = JsonHelper.ToValue<double>(root["hoursOffline"]),
						TaskTypeOnLogout = JsonHelper.ToValue<int>(root["taskTypeOnLogout"]),
						TaskNameOnLogout = JsonHelper.ToValue<string>(root["taskNameOnLogout"]),
						
						SkillExperiences = JsonHelper.ToSimpleDictionary<Skill, double>(root["skillExperiences"]),
						Equipment = JsonHelper.ToSimpleDictionary<EquipmentSlot, int>(root["equipment"]),
						EnchantmentBoosts = JsonHelper.ToSimpleDictionary<Skill, double>(root["enchantmentBoosts"]),
						Upgrades = JsonHelper.ToSimpleDictionary<UpgradeType, int>(root["upgrades"]),
						PvMStats = JsonHelper.ToSimpleDictionary<PvmStatType, int>(root["pvmStats"])
					};
				}
			}
		}
	}

	public static class TaskExtension {
		
		/// <summary>
		/// Executes the specified action synchronously after the completion of the input task.
		/// Most task needs to jump back to the main thread before calling any Unity related APIs,
		/// if not, the game will crash without any exceptions.
		/// </summary>
		/// <param name="task">The input task to monitor for completion.</param>
		/// <param name="action">The action to perform upon task completion.</param>
		/// <param name="exceptional">Optional action to handle exceptions that occur during task execution.</param>
		/// <typeparam name="T">The type of result produced by the task.</typeparam>
		public static void AcceptSync<T>(this Task<T> task, Action<T> action, Action<Exception> exceptional = null) {
			task.ContinueWith(t => {
				if (t.Exception != null) {
					if (exceptional != null) exceptional(t.Exception);
					else IdleLog.Error("Exception encountered in AcceptSync (System)", t.Exception);
					return;
				}
				
				IdleTasks.Run(() => action(t.Result));
			});
		}
		
		/// <summary>
		/// Executes the specified action synchronously after the completion of the input task.
		/// Most task needs to jump back to the main thread before calling any Unity related APIs,
		/// if not, the game will crash without any exceptions.
		/// </summary>
		/// <param name="task">The input task to monitor for completion.</param>
		/// <param name="action">The action to perform upon task completion.</param>
		/// <param name="exceptional">Optional action to handle exceptions that occur during task execution.</param>
		/// <typeparam name="T">The type of result produced by the task.</typeparam>
		public static void AcceptSync<T>(this Il2CppSystem.Threading.Tasks.Task<T> task, Action<T> action,
			Action<Il2CppSystem.Exception> exceptional = null) {
			task.ContinueWith((Action<Il2CppSystem.Threading.Tasks.Task>) delegate(Il2CppSystem.Threading.Tasks.Task rawTask) {
				var t = rawTask.Cast<Il2CppSystem.Threading.Tasks.Task<T>>();
				if (t.Exception != null) {
					if (exceptional != null) exceptional(t.Exception);
					else IdleLog.Error("Exception encountered in AcceptSync (IL2CPP)", t.Exception);
					return;
				}
				
				IdleTasks.Run(() => action(t.Result));
			});
		}
	}
}