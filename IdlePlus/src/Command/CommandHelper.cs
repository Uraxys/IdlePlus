using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Builder;

namespace IdlePlus.Command {
	public static class Literal {

		public static LiteralArgumentBuilder<CommandSender> Of(string name) {
			return new LiteralArgumentBuilder<CommandSender>(name);
		}
		
	}

	public static class Argument {

		public static RequiredArgumentBuilder<CommandSender, T> Of<T>(string name, ArgumentType<T> type) {
			return RequiredArgumentBuilder<CommandSender, T>.RequiredArgument(name, type);
		}
		
	}
}