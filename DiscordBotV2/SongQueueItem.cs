using Discord;
using YoutubeExplode.Videos;

namespace DiscordBotV2
{
    public class SongQueueItem
    {
        public string Url { get; set; } = "";

        public string Title { get; set; } = "";

        /// <summary>
        /// Video ID
        /// </summary>
        public VideoId VID { get; set; }

        public string songname { get; set; } = "";

        public IUser? User { get; set; }


        public SongQueueItem()
        {
        }
    }
}
