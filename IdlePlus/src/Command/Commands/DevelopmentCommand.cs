using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using IdlePlus.Utilities;

namespace IdlePlus.Command.Commands {
	internal static class DevelopmentCommand {

		internal static LiteralArgumentBuilder<CommandSender> Register() {
			var command = Literal.Of("dev");

			command.Then(Literal.Of("export")
				.Then(Literal.Of("items").Executes(HandleExportItems))
				.Then(Literal.Of("tasks").Executes(HandleExportTasks)));
			
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
	}
}