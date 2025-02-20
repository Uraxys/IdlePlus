namespace IdlePlus.API.Event.Contexts {
	public class ChatMessageEventContext : EventContext {
		public string Sender { get; }
		public string Message { get; }
		public ChatMessageEventContext(string sender, string message) {
			this.Sender = sender;
			this.Message = message;
		}
	}
}