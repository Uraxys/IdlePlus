using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brigadier.NET;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Tree;
using ChatboxLogic;
using IdlePlus.API.Event;
using IdlePlus.API.Event.Contexts.IdlePlus;
using IdlePlus.API.Utility;
using IdlePlus.Attributes;
using IdlePlus.Command.Commands;
using IdlePlus.Settings;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;

namespace IdlePlus.Command {
	public static class CommandManager {
		
		private static readonly CommandDispatcher<CommandSender> Dispatcher = new CommandDispatcher<CommandSender>();
		private static readonly string[] OverridenCommands = { "/pm", "/dm", "/block", "/unblock", "/commands" };

		[InitializeOnce(OnSceneLoad = Scenes.MainMenu)]
		private static void InitializeOnce() {
			if (ModSettings.Miscellaneous.DeveloperTools.Value) {
				DevelopmentCommand.Register(Dispatcher);
			}
			
			PrivateMessageCommand.Register(Dispatcher);
			BlockCommand.Register(Dispatcher);
			CommandsCommand.Register(Dispatcher);

			Events.IdlePlus.OnRegisterCommand.Call(new CommandRegisterContext(builder => Dispatcher.Register(builder)));
		}
		
		#region Internal

		internal static async Task<CommandSuggestResult> HandleSuggestion(CommandSender sender, string command,
			int cursor, CancellationToken token) { 
			
			if (cursor < 1) return null; // No need to parse anything if we're in front of the command.
			var reader = new StringReader(command);
			if (!reader.CanRead() || reader.Peek() != '/') return null; // Make sure it's a command.
			reader.Skip(); // Skip over the slash.
			
			// Start parsing the command and getting suggestions.
			var parsed = Dispatcher.Parse(reader, sender);
			token.ThrowIfCancellationRequested();
			var suggestions = await Dispatcher.GetCompletionSuggestions(parsed, cursor);
			token.ThrowIfCancellationRequested();
			
			var resultUsageStartIndex = 0;
			var resultUsage = new List<string>();
			
			// Only check for exceptions if we're at the end of the command.
			var parserChecked = cursor == command.Length;
			if (parserChecked && !parsed.Exceptions.IsEmpty()) {
				var foundUnknownCommand = false;
				foreach (var entry in parsed.Exceptions) {
					var exception = entry.Value;
					if (exception.Type == CommandSyntaxException.BuiltInExceptions.LiteralIncorrect()) {
						foundUnknownCommand = true;
						continue;
					}
					resultUsage.Add(exception.Message);
				}

				if (foundUnknownCommand) {
					resultUsage.Add(CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand().Create().Message);
				}
			}

			// If we haven't added any usage information, then try to find command suggestions.
			if (resultUsage.IsEmpty()) {
				var suggestionContext = parsed.Context.FindSuggestionContext(cursor);
				var usages = Dispatcher.GetSmartUsage(suggestionContext.Parent, sender);
				token.ThrowIfCancellationRequested();
				
				var foundUsage = false;
				if (!usages.IsEmpty()) {
					resultUsageStartIndex = suggestionContext.StartPos;
					foreach (var entry in usages) {
						if (entry.Key is LiteralCommandNode<CommandSender>) continue;
						foundUsage = true;
						resultUsage.Add(entry.Value);
					}
				}
				
				// If we didn't have any usage information, no suggestions were found,
				// and the parser has checked the command while we can still read, then
				// add an unknown command or argument exception to the usage list.
				if (!foundUsage && parserChecked && parsed.Reader.CanRead()) {
					if (parsed.Context.Range.IsEmpty) {
						resultUsage.Add(CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand()
							.CreateWithContext(parsed.Reader).Message);
					} else {
						resultUsage.Add(CommandSyntaxException.BuiltInExceptions.DispatcherUnknownArgument()
							.CreateWithContext(parsed.Reader).Message);
					}
				}
			}
			
			return new CommandSuggestResult { RequestedCommand = command, Suggestions = suggestions, 
				UsageStartIndex = resultUsageStartIndex, Usage = resultUsage };
		}
		
		internal static CommandResult Handle(CommandSender sender, string command) {
			var reader = new StringReader(command);
			if (!reader.CanRead() || reader.Peek() != '/') return null; // Make sure it's a command.
			reader.Skip(); // Skip over the slash.
			
			// Check if it's a vanilla command, if it is, don't do anything.
			var commandName = command.ToLower().Split(' ')[0];
			if (ChatboxManager.Instance._chatboxPopup.Chatbox._messagePrefixes.Contains(commandName) &&
			    !OverridenCommands.Contains(commandName)) {
				return null;
			}
			
			// Custom handling.
			
			try {
				var result = Dispatcher.Execute(reader, sender);
				IdleLog.Debug("Command {0} executed with result {1}.", command, result);
				return new CommandResult { Success = result == 1, Response = null };
			} catch (CommandSyntaxException e) {
				var unknownCommand = e.Message.StartsWith("Unknown command at position");
				return new CommandResult { Success = false, UnknownCommand = unknownCommand, Response = e.Message };
			}
		}
		
		#endregion
	}

	internal class CommandResult {
		public bool Success;
		public bool UnknownCommand;
		public string Response;
	}

	internal class CommandSuggestResult {
		public string RequestedCommand;
		
		public int UsageStartIndex;
		public List<string> Usage;
		public Suggestions Suggestions;
	}
}