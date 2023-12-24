
namespace SakuraConnect.Shared.Models.Messaging.Ai
{
	/// <summary>
	/// Is triggered when the AI has a response
	/// </summary>
	public class ThinkResultEventArgs
	{
		public List<Comment> Comments { get; set; } = new ();

		/// <summary>
		/// Gets the plain text of the comments
		/// </summary>
		public string PlainText => string.Join("\n", Comments.Select(x => x.Text));
	}
}
