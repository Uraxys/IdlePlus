using System;
using Brigadier.NET.Builder;
using Brigadier.NET.Tree;
using IdlePlus.Command;

namespace IdlePlus.API.Event.Contexts.IdlePlus {
	
	/// <summary>
	/// <para>The event which is called when custom commands should be registered.</para>
	/// Example usage:
	/// <code>
	/// Events.IdlePlus.OnRegisterCommand.Register(context => {
	///     // Create a "/test" command.
	///     var command = Literal.Of("test")
	///         .Executes(ctx => {
	///             // Print a message in their chat.
	///             ctx.Source.SendMessage("You just ran the /test command.");
	///         });
	///
	///     // Register the command.
	///     context.Register(command);
	/// }); 
	/// </code>
	/// </summary>
	public class CommandRegisterContext : EventContext {
		
		private readonly Func<LiteralArgumentBuilder<CommandSender>, LiteralCommandNode<CommandSender>> _registry;

		public CommandRegisterContext(
			Func<LiteralArgumentBuilder<CommandSender>, LiteralCommandNode<CommandSender>> registry) {
			this._registry = registry;
		}
		
		/// <summary>
		/// Registers the provided command.
		/// </summary>
		/// <param name="command">The command to register.</param>
		/// <returns>The created command node.</returns>
		public LiteralCommandNode<CommandSender> Register(LiteralArgumentBuilder<CommandSender> command) {
			return this._registry(command);
		}
	}
}