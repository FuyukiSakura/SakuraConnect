
namespace Sakura.Live.Connect.Dreamer.Models.OneComme
{
    /// <summary>
    /// Events from OneComme
    /// </summary>
    public class OneCommeEvent
    {
        /// <summary>
        /// The type of the event
        /// </summary>
        public string Type { get; set; } = "";
        
        /// <summary>
        /// This can be anything according to <see cref="Type"/>
        /// </summary>
        public OneCommeData Data { get; set; } = new();
    }

    /// <summary>
    /// Data of the event
    /// </summary>
    public class OneCommeData
    {
        /// <summary>
        /// List of comments received
        /// </summary>
        public List<Comment> Comments { get; set; } = new();
    }

    /// <summary>
    /// The comments received
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Unique ID of the comment from OneComme
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// The metadata of the comment
        /// </summary>
        public ServiceData Data { get; set; } = new();
    }

    /// <summary>
    /// Comment data from different streaming services
    /// </summary>
    public class ServiceData
    {
        /// <summary>
        /// The user name
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The comment text
        /// </summary>
        public string Comment { get; set; } = "";
    }
}
