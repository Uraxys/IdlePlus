using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Brigadier.NET;
using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Builder;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Tree;
using ChatboxLogic;
using Databases;
using IdlePlus.Command.ArgumentTypes;
using IdlePlus.Command.Commands;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;

namespace IdlePlus.Command {
	public static class CommandManager {
		
		private static readonly CommandDispatcher<CommandSender> Dispatcher = new CommandDispatcher<CommandSender>();
		private static readonly string[] OverridenCommands = new[] { "/pm", "/dm" };
		
		public static void Load() {
			Dispatcher.Register(DevelopmentCommand.Register());
			PrivateMessageCommand.Register(Dispatcher);
			
			Dispatcher.Register(a =>
				a.Literal("run").Executes(context => {
					IdleLog.Info("Run executed.");
					return 1;
				})
			);

			Dispatcher.Register(a =>
				a.Literal("register").Executes(context => {
					IdleLog.Info("Register executed.");
					return 1;
				})
			);

			Dispatcher.Register(a =>
				a.Literal("reset").Executes(context => {
					IdleLog.Info("Reset executed.");
					return 1;
				})
			);

			Dispatcher.Register(a =>
				a.Literal("mod").Executes(context => {
					IdleLog.Info("Mod executed.");
					return 1;
				})
				.Then(b =>
					b.Argument("name", Arguments.Word()).Executes(context => {
						var name = context.GetArgument<string>("name");
						IdleLog.Info("Mod name {0} executed.", name);
						return 1;
					})
				)
			);

			Dispatcher.Register(a =>
				a.Literal("spawn")
					.Then(b =>
						b.Literal("item").Then(c =>
							c.Argument("item", ItemArgument.Of()).Executes(context => {
								var item = context.GetArgument<Item>("item");
								IdleLog.Info("Spawn item {0} executed.", item.Name);
								return 1;
							}).Then(d =>
								d.Argument("amount", Arguments.Integer(1, 999)).Executes(context => {
									var item = context.GetArgument<Item>("item");
									var amount = context.GetArgument<int>("amount");
									IdleLog.Info("Spawn item {0} with amount {1} executed.", item.Name, amount);
									return 1;
								})
							))
					).Then(b =>
						b.Literal("xp").Then(c =>
							c.Argument("amount", Arguments.Integer(1, 999)).Executes(context => {
								var amount = context.GetArgument<int>("amount");
								IdleLog.Info("Spawn xp with amount {0} executed.", amount);
								return 1;
							})
						)
					)
				);

			Dispatcher.Register(a =>
				a.Literal("test").Executes(context => {
					IdleLog.Info("Test executed.");
					return 1;
				}).Then(b =>
					b.Literal("run").Then(c =>
						c.Argument("name", Arguments.Word()).Executes(context => {
							var name = context.GetArgument<string>("name");
							IdleLog.Info("Test run name {0} executed.", name);
							return 1;
						})
					)
				)
			);

			Dispatcher.Register(a =>
				a.Literal("set").Then(b =>
					b.Literal("config").Then(c => 
						c.Argument("key", Arguments.Word()).Executes(context => {
							IdleLog.Info("Set config key executed.");
							return 1;
						})
					)
				).Then(b =>
					b.Literal("value").Then(c =>
						c.Argument("key", Arguments.Word()).Executes(context => {
							IdleLog.Info("Set value key executed.");
							return 1;
						})
					)
				)
			);

			Dispatcher.Register(a =>
				a.Literal("say").Then(b =>
					b.Argument("message", Arguments.String()).Executes(context => {
						IdleLog.Info("Say executed.");
						return 1;
					})
				)
			);
		}
		
		#region Internal

		internal static async Task<CommandSuggestResult> HandleSuggestion(string command, int cursor, 
			CancellationToken token) { 
			
			if (cursor < 1) return null; // No need to parse anything if we're in front of the command.
			var reader = new StringReader(command);
			if (!reader.CanRead() || reader.Peek() != '/') return null; // Make sure it's a command.
			reader.Skip(); // Skip over the slash.
			
			// TODO: Create a real CommandSender.
			var commandSender = new CommandSender();
			
			// Start parsing the command and getting suggestions.
			var parsed = Dispatcher.Parse(reader, commandSender);
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
				var usages = Dispatcher.GetSmartUsage(suggestionContext.Parent, commandSender);
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
		
		internal static CommandResult Handle(string command) {
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
				var result = Dispatcher.Execute(reader, new CommandSender());
				IdleLog.Info("Command {0} executed with result {1}.", command, result);
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