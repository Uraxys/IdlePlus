using Brigadier.NET;
using Brigadier.NET.Context;
using Client;
using IdlePlus.Command.ArgumentTypes;
using Networking.Chat;

namespace IdlePlus.Command.Commands {
	internal static class PrivateMessageCommand {
		
		internal static void Register(CommandDispatcher<CommandSender> registry) {
			var command = Literal.Of("dm");

			command.Then(Argument.Of("name", ChatPlayerArgument.Of(false))
				.Then(Argument.Of("message", Arguments.GreedyString())
					.Executes(HandlePrivateMessage)));

			var node = registry.Register(command);
			registry.Register(Literal.Of("pm").Redirect(node));
		}

		private static void HandlePrivateMessage(CommandContext<CommandSender> context) {
			var target = context.GetArgument<string>("name");
			var message = context.GetArgument<string>("message");

			var packet = new ChatboxPrivateMessage { ReceivingPlayer = target, Message = message };
			NetworkClientChatService.Instance.SendData(packet);
		}
	}
}