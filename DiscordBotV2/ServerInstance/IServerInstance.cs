using Discord.Audio;
using YoutubeExplode;

namespace DiscordMusicBot.ServerInstance
{
    public interface IServerInstance : IDisposable
    {
        public string? LastSearch { get; set; }

        public YoutubeClient? YoutubeClient { get; set; }

        public IAudioClient? AudioClient { get; set; }

        public CancellationTokenSource? ServerCancelToken { get; set; }

        public bool SkipSong();

        public void ClearQueue();

        public bool MoveSong(int fromIndex, int toIndex);

        public bool RemoveSong(int index);

        public SongQueueItem? DeQueue();

        public IReadOnlyList<SongQueueItem> GetQueue();



    }
}
