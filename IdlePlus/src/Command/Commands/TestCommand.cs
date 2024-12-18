using Databases;
using IdlePlus.Utilities;

namespace IdlePlus.Command.Commands {
	public class TestCommand {
		
		/*
		 * Brigadier Example Sketch
		 */

		// Direct Message
		
		[IdleCommand(
			Name = "dm <target> <message>",
			Aliases = new[] { "pm", "msg", "tell" },
			Arguments = new[] { IdleArgs.CachedPlayerName, IdleArgs.String }
		)]
		private static void HandleDirectMessage(CommandSender sender, string target, string message) {
			
		}
		
		// Test

		[IdleCommand(
			Name = "test"
		)]
		private static void HandleTest(CommandSender sender) {
			
		}
		
		[IdleCommand(
			Name = "test stop"
		)]
		private static void HandleTestStop(CommandSender sender) {
			
		}
		
		[IdleCommand(
			Name = "test start <name>",
			Arguments = new[] { IdleArgs.String }
		)]
		private static void HandleTestStart(CommandSender sender, string name) {
			
		}
		
		// Spawn
		
		[IdleCommand(
			Name = "spawn item <item> [amount:1-999]",
			Arguments = new[] { IdleArgs.Item, IdleArgs.Integer }
		)]
		private static void HandleSpawnItem(CommandSender sender, Item item, int amount = -1) {
			
		}
		
		// Random test - User command
		
		[IdleCommand(
			Name = "user <player>",
			Arguments = new[] { IdleArgs.CachedPlayerName }
		)]
		private static void HandleUser(CommandSender sender, string player) {
			
		}
		
		[IdleCommand(
			Name = "user <player> reset",
			Arguments = new[] { IdleArgs.CachedPlayerName }
		)]
		private static void HandleUserReset(CommandSender sender, string player) {
			
		}


		
		/*
		 * Legacy tests
		 */
		
		[CommandInfo(Name = "test")]
		private static void Test(string[] args) {
			IdleLog.Info($"Test command executed, number of arguments: {args.Length}");
		}
		
		[CommandInfo(Name = "test2", Aliases = new[] { "t2" }, RequiredArguments = 1)]
		private static void Test2(string[] args) {
			IdleLog.Info($"Test2 command executed, number of arguments: {args.Length}");
		}
	}
}