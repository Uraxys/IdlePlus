using Brigadier.NET;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using IdlePlus.Utilities;
using Player;

namespace IdlePlus.Command.Commands {
	internal static class DevelopmentCommand {

		internal static LiteralArgumentBuilder<CommandSender> Register() {
			var command = Literal.Of("dev");

			command.Then(Literal.Of("export")
				.Then(Literal.Of("items").Executes(HandleExportItems))
				.Then(Literal.Of("tasks").Executes(HandleExportTasks)));

			command.Then(Literal.Of("print")
				.Then(Argument.Of("message", Arguments.GreedyString())
					.Executes(HandlePrint)));
			
			command.Then(Literal.Of("say")
				.Then(Argument.Of("message", Arguments.GreedyString())
					.Executes(HandleSay)));
			
			return command;
		}
		
		/*
		 * Export
		 */

		private static void HandleExportItems(CommandContext<CommandSender> context) {
			IdleLog.Info("// TODO: Export items.");
		}

		private static void HandleExportTasks(CommandContext<CommandSender> context) {
			IdleLog.Info("// TODO: Export tasks.");
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