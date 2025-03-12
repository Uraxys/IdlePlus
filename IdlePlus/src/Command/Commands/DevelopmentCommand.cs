using System;
using System.Diagnostics;
using System.IO;
using Brigadier.NET;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Databases;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Player;
using Tasks;

namespace IdlePlus.Command.Commands {
	internal static class DevelopmentCommand {

		internal static void Register(CommandDispatcher<CommandSender> registry) {
			var command = Literal.Of("dev");

			command.Then(Literal.Of("export")
				.Then(Literal.Of("items").Executes(HandleExportItems)));
				//.Then(Literal.Of("tasks").Executes(HandleExportTasks)));

			command.Then(Literal.Of("print")
				.Then(Argument.Of("message", Arguments.GreedyString())
					.Executes(HandlePrint)));
			
			command.Then(Literal.Of("say")
				.Then(Argument.Of("message", Arguments.GreedyString())
					.Executes(HandleSay)));

			registry.Register(command);
		}
		
		/*
		 * Export
		 */

		private static void HandleExportItems(CommandContext<CommandSender> context) {
			var path = Path.Combine(BepInEx.Paths.PluginPath, "IdlePlus", "export");
			Directory.CreateDirectory(path);

			var root = new JObject();
			root.Add("exported_at", new JValue($"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.FFFZ}"));
			root.Add("version", new JObject(
				new JProperty("config", SettingsDatabase.SharedSettings.ConfigVersion),
				new JProperty("latest_build", SettingsDatabase.SharedSettings.LatestBuildVersion),
				new JProperty("required_build", SettingsDatabase.SharedSettings.RequiredBuildVersion))
			);
			root.Add("items", new JArray().Do(arr => {
				foreach (var item in ItemDatabase.ItemList._values) {
					arr.Add(item.ToJson());
				}
			}));

			var json = root.ToString(Formatting.Indented);
			File.WriteAllText(Path.Combine(path, "items.json"), json);
			
			context.Source.SendMessage("Exported item data to 'IdlePlus/export/items.json'.");
		}

		private static void HandleExportTasks(CommandContext<CommandSender> context) {
			if (true) return;
			var path = Path.Combine(BepInEx.Paths.PluginPath, "IdlePlus", "export");
			Directory.CreateDirectory(path);

			var root = new JObject();
			root.Add("exported_at", new JValue($"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.FFFZ}"));
			root.Add("version", new JObject(
				new JProperty("config", SettingsDatabase.SharedSettings.ConfigVersion),
				new JProperty("latest_build", SettingsDatabase.SharedSettings.LatestBuildVersion),
				new JProperty("required_build", SettingsDatabase.SharedSettings.RequiredBuildVersion))
			);
			
			root.Add("tasks", new JArray().Do(arr => {
				foreach (var entry in TaskDatabase.Tasks) {
					var type = entry.key;
				}
			}));
		}
		
		/*
		 * Say
		 */

		private static void HandlePrint(CommandContext<CommandSender> context) {
			var sender = context.Source;
			var message = context.GetArgument<string>("message");
			sender.SendMessage(message);
		}
		
		private static void HandleSay(CommandContext<CommandSender> context) {
			var sender = context.Source;
			var message = context.GetArgument<string>("message");
			message = $"[00:00:00] [TAG] {PlayerData.Instance.Username}: {message}";
			sender.SendMessage(message, mode: GameMode.Default, premium: true, moderator: true);
		}
	}
}