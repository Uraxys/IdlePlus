using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brigadier.NET;
using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;
using IdlePlus.API.Event;
using IdlePlus.API.Utility;
using IdlePlus.API.Utility.Game;
using IdlePlus.Attributes;

namespace IdlePlus.Command.ArgumentTypes {
	public class ChatPlayerArgument : ArgumentType<string> {

		private static readonly DynamicCommandExceptionType ExceptionInvalid = new DynamicCommandExceptionType(o => new LiteralMessage($"Invalid name {o}"));
		private static readonly SimpleCommandExceptionType ExceptionSelf = new SimpleCommandExceptionType(new LiteralMessage("Can't target self"));
		private static readonly IEnumerable<string> ExampleValues = new[] { "utesty1", "utesty2", "utesty3" };

		private static string _currentUsername;
		private static readonly ReaderWriterLockSlim ReadWriteLock = new ReaderWriterLockSlim();
		private static readonly HashSet<string> UsernameIndex = new HashSet<string>();
		private static readonly LinkedList<string> Usernames = new LinkedList<string>();
		private const int Capacity = 25;

		[InitializeOnce(OnSceneLoad = Scenes.MainMenu)]
		internal static void InitializeOnce() {
			// Reset known usernames when the player login.
			Events.Player.OnLogin.Register(context => {
				ResetKnownUsernames(context.PlayerData.Username);
			});
			
			// Add usernames to the known names list.
			Events.Chat.OnPublicMessage.Register(context => {
				if (context.Sender == null) return;
				AddKnownUsername(context.Sender);
			});
		}

		private static void AddKnownUsername(string username) {
			if (username.Equals(_currentUsername, StringComparison.OrdinalIgnoreCase)) return;
			
			ReadWriteLock.EnterWriteLock();
			try {
				if (UsernameIndex.Contains(username)) {
					Usernames.Remove(username);
					Usernames.AddFirst(username);
					return;
				}

				if (Usernames.Count >= Capacity) {
					var last = Usernames.Last.Value;
					Usernames.RemoveLast();
					UsernameIndex.Remove(last);
				}
				
				Usernames.AddFirst(username);
				UsernameIndex.Add(username);
			} finally {
				ReadWriteLock.ExitWriteLock();
			}
		}

		private static void ResetKnownUsernames(string def = null) {
			ReadWriteLock.EnterWriteLock();
			try {
				Usernames.Clear();
				UsernameIndex.Clear();
				_currentUsername = def;
			} finally {
				ReadWriteLock.ExitWriteLock();
			}
		}

		public static ChatPlayerArgument Of(bool allowSelf = true) {
			return new ChatPlayerArgument(allowSelf);
		}

		private readonly bool _allowSelf;
		
		public ChatPlayerArgument(bool allowSelf = true) {
			this._allowSelf = allowSelf;
		}

		public override string Parse(IStringReader reader) {
			ReadWriteLock.EnterReadLock();
			try {
				var name = reader.ReadUnquotedString();
				if (!PlayerUtils.IsValidUsername(name)) throw ExceptionInvalid.CreateWithContext(reader, name);
				if (_currentUsername != null && !_allowSelf &&
				    name.Equals(_currentUsername, StringComparison.OrdinalIgnoreCase)) {
					throw ExceptionSelf.CreateWithContext(reader);
				}
				return name;
			} finally {
				ReadWriteLock.ExitReadLock();
			}
		}

		public override Task<Suggestions> ListSuggestions<TSource>(CommandContext<TSource> context, SuggestionsBuilder builder) {
			return Task.Run(() => {
				ReadWriteLock.EnterReadLock();
				try {
					var current = builder.Remaining;
					if (_currentUsername != null && this._allowSelf && _currentUsername.StartsWith(current)) 
						builder.Suggest(current);
					foreach (var name in Usernames.Where(name => name.StartsWith(current))) {
						builder.Suggest(name);
					}
				} finally {
					ReadWriteLock.ExitReadLock();
				}

				return builder.Build();
			});
		}

		public override IEnumerable<string> Examples => ExampleValues;
	}
}