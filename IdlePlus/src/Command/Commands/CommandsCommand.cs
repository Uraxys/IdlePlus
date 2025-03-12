using Brigadier.NET;
using Popups;

namespace IdlePlus.Command.Commands {
	internal static class CommandsCommand {

		internal static void Register(CommandDispatcher<CommandSender> registry) {
			var command = Literal.Of("commands")
				.Executes(context => {
					var popup = PopupManager.Instance.SetupHardPopup(HardPopup.ChatCommandsPopup);
					popup.Show();
				});

			registry.Register(command);
		}
	}
}