using Discord;
using YoutubeExplode.Videos;

namespace DiscordMusicBot
{
    public struct SongQueueItem
    {
        public string Url { get; set; } = "";

        public string Title { get; set; } = "";

        /// <summary>
        /// Video ID
        /// </summary>
        public VideoId VID { get; set; }

        public string songname { get; set; } = "";

        public IUser? User { get; set; }

        public EmbedBuilder? EmbedMessage { get; set; }


        public SongQueueItem()
        {
        }
    }
}
