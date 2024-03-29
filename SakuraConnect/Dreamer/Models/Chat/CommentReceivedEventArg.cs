﻿
using Sakura.Live.Speech.Core.Models;

namespace Sakura.Live.Connect.Dreamer.Models.Chat
{
    /// <summary>
    /// Data for comment received event
    /// </summary>
    public class CommentReceivedEventArg
    {
        /// <summary>
        /// List of comments
        /// </summary>
        public List<CommentData> Comments { get; set; } = new();
    }

    /// <summary>
    /// Comment data
    /// </summary>
    public class CommentData
    {
        /// <summary>
        /// The ID of the comment
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// The user name
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// The comment text
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// The role of the commenter
        /// </summary>
        public SpeechQueueRole Role { get; set; } = SpeechQueueRole.User;

        /// <summary>
        /// Received time of the comment
        /// </summary>
        public DateTime ReceivedAt { get; set; } = DateTime.Now;
    }
}
