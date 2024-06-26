using System;
using System.Text.RegularExpressions;
using BepInEx.Logging;

namespace IdlePlus.Utilities {
	public static class IdleLog {

		public static ManualLogSource Logger;
		private static readonly Regex PlaceholderRegex = new Regex(@"\{\}");
		
		/*public static void Error(object message, params object[] args) {
			Log(LogLevel.Error, message, args);
		}*/
		
		public static void Error(object message) {
			Log(LogLevel.Error, message);
		}
		
		public static void Error(object message, Il2CppSystem.Exception exception) {
			Log(LogLevel.Error, "{}\n{}", message, exception.ToString());
		}
		
		public static void Error(object message, Exception exception) {
			Log(LogLevel.Error, "{}\n{}", message, exception.ToString());
		}
		
		public static void Warn(object message, params object[] args) {
			Log(LogLevel.Warning, message, args);
		}
		
		public static void Message(object message, params object[] args) {
			Log(LogLevel.Message, message, args);
		}
		
		public static void Info(object message, params object[] args) {
			Log(LogLevel.Info, message, args);
		}
		
		public static void Debug(object message, params object[] args) {
			Log(LogLevel.Debug, message, args);
		}
		
		public static void Log(LogLevel level, object message, params object[] args) {
			var msg = message.ToString();
			
			// Insert the arguments, supporting both {0} and {} as placeholders.
			for (var i = 0; i < args.Length; i++) {
				// Replace {i} and the first {} with the argument.
				msg = msg.Replace("{" + i + "}", args[i].ToString());
				msg = PlaceholderRegex.Replace(msg, args[i].ToString(), 1);
			}
			
			Logger.Log(level, msg);
		}
	}
}