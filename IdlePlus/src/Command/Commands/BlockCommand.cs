using Brigadier.NET;
using Brigadier.NET.Context;
using IdlePlus.Command.ArgumentTypes;
using Player;

namespace IdlePlus.Command.Commands {
	internal static class BlockCommand {

		internal static void Register(CommandDispatcher<CommandSender> registry) {
			var block = Literal.Of("block")
				.Then(Argument.Of("player", ChatPlayerArgument.Of(false))
					.Executes(HandleBlock));
			var unblock = Literal.Of("unblock")
				.Then(Argument.Of("player", ChatPlayerArgument.Of(false))
					.Executes(HandleUnblock));

			registry.Register(block);
			registry.Register(unblock);
		}

		private static void HandleBlock(CommandContext<CommandSender> context) {
			var target = context.GetArgument<string>("player");
			PlayerData.Instance.Blocks.SendBlockRequestToServer(target);
		}

		private static void HandleUnblock(CommandContext<CommandSender> context) {
			var target = context.GetArgument<string>("player");
			PlayerData.Instance.Blocks.SendUnblockPlayerRequestToServer(target);
		}
	}
}