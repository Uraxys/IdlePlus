using IdlePlus.Utilities;

namespace IdlePlus.Command.Commands {
	public class TestCommand {
		
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