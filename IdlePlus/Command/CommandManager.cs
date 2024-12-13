using System;
using System.Collections.Generic;
using System.Reflection;
using IdlePlus.Command.Commands;
using IdlePlus.Utilities;

namespace IdlePlus.Command {
	public class CommandManager {

		private static Dictionary<string, WrappedCommand> _commands = new Dictionary<string, WrappedCommand>();

		public static void Load() {
			Register(typeof(TestCommand));
		}

		public static void Register(Type type) {
			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance |
			                                       BindingFlags.Static | BindingFlags.Public)) {
				var info = method.GetCustomAttribute<CommandInfo>();
				if (info == null) continue;

				if (!method.IsStatic) {
					IdleLog.Warn($"Couldn't register command {info.Name}, method isn't static!");
					continue;
				}

				if (method.GetParameters().Length != 1 || method.GetParameters()[0].ParameterType != typeof(string[])) {
					IdleLog.Warn($"Couldn't register command {info.Name}, method has invalid parameters!");
					continue;
				}

				// "Register" the command.
				var command = new WrappedCommand(method);

				if (_commands.ContainsKey(command.Command))
					IdleLog.Warn("Command {0} is already registered, overriding.", command.Command);
				_commands.Add(command.Command, command);

				foreach (var alias in command.Aliases) {
					if (_commands.ContainsKey(alias))
						IdleLog.Warn("Alias {0} is already registered, overriding.", alias);
					_commands.Add(alias, command);
				}
			}
		}

		public static bool Handle(string command) {
			if (!command.StartsWith("/")) return false;
			var args = command.Substring(1).Split(' ');

			var cmd = args[0].ToLower();
			if (!_commands.ContainsKey(cmd)) return false;
			
			args = args.Length == 1
				? Array.Empty<string>()
				: new List<string>(args).GetRange(1, args.Length - 1).ToArray();
			
			_commands[cmd].Invoke(args);
			return true;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class CommandInfo : Attribute {
		
		/// <summary>
		/// The name of the command, this is what the user will type to execute the command.
		/// </summary>
		public string Name;
		
		/// <summary>
		/// The aliases this command has, these are shortcuts for the command.
		/// </summary>
		public string[] Aliases = new string[0];
		
		/// <summary>
		/// The required arguments of the command, more is allowed, but not less.
		/// </summary>
		public int RequiredArguments = 0;
	}
}