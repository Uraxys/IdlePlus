namespace IdlePlus.Command {
	public class CommandSender {

		private const string ErrorColor = "fff";
		
		/// <summary>
		/// Sends a pre formated red message to this client.
		/// Chat colors and styling is allowed using TMP rich syntax, example:
		/// "Hello this next part is &lt;color=red&gt;RED&lt;/color&gt;!".
		/// </summary>
		/// <param name="message">The message to send in the players chat.</param>
		public void SendErrorMessage(string message) {
			this.SendMessage($"<color={ErrorColor}>{message}</color>");
		}
		
		/// <summary>
		/// Sends a message to this client.
		/// Chat colors and styling is allowed using TMP rich syntax, example:
		/// "Hello this next part is &lt;color=red&gt;RED&lt;/color&gt;!".
		/// </summary>
		/// <param name="message">The message to send in the players chat.</param>
		public void SendMessage(string message) {
			// TODO: Implement
		}
	}
}