using Brigadier.NET.Builder;

namespace IdlePlus.Command {
	public static class Literal {

		public static LiteralArgumentBuilder<CommandSender> Of(string name) {
			return new LiteralArgumentBuilder<CommandSender>(name);
		}
		
	}
}