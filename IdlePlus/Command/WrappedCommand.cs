using System;
using System.Reflection;
using IdlePlus.Utilities;

namespace IdlePlus.Command {
	public class WrappedCommand {

		public readonly string Command;
		public readonly string[] Aliases;
		public readonly int RequiredArguments;
		
		private readonly MethodInfo _method;
		
		public WrappedCommand(MethodInfo method) {
			this._method = method;
			
			var info = method.GetCustomAttribute<CommandInfo>();
			this.Command = info.Name.ToLower();
			this.Aliases = Array.ConvertAll(info.Aliases, alias => alias.ToLower());
			this.RequiredArguments = info.RequiredArguments;
		}
		
		public void Invoke(string[] args) {
			if (this.RequiredArguments > args.Length) {
				// TODO: Send feedback to the user that they didn't provide enough arguments.
				return;
			}

			try {
				this._method.Invoke(null, new object[] { args });
			} catch (Exception e) {
				IdleLog.Error($"Error running command /{this.Command}", e);
			}
		}
	}
}