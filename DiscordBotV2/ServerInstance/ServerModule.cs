using Discord.Audio;
using OpenAI.GPT3.ObjectModels.RequestModels;
using YoutubeExplode;

namespace DiscordBotV2.ServerInstance
{
    public class ServerModule : IServerInstance
    {
        private bool disposedValue;

        public ulong GuildId { get; private set; }
        public string? LastSearch { get; set; }
        public string? CurrentSong { get; set; }
        public YoutubeClient? YoutubeClient { get; set; } = new();
        public IAudioClient? AudioClient { get; set; }
        public CancellationTokenSource? ServerCancelToken { get; set; }

        public List<SongQueueItem>? QueueItems { get; set; } = new();


        public Dictionary<ulong, List<ChatMessage>>? UserGptHistory { get; set; } = new();

        public ServerModule(ulong guildId)
        {
            ServerCancelToken = new CancellationTokenSource();
            LastSearch = string.Empty;
            AudioClient = null;
            GuildId = guildId;
        }

        public void ClearQueue()
        {
            QueueItems.Clear();
        }

        public IReadOnlyList<SongQueueItem> GetQueue()
        {
            return QueueItems;
        }

        public bool MoveSong(int fromIndex, int toIndex)
        {
            try
            {
                var temp = QueueItems[toIndex];
                QueueItems[toIndex] = QueueItems[fromIndex];
                QueueItems[fromIndex] = temp;
                return true;
            }
            catch
            {
                return false;
            }

        }


        public bool RemoveSong(int index)
        {
            try
            {
                QueueItems.RemoveAt(index);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SkipSong()
        {
            //If not playing anything
            if (AudioClient == null)
            {

                return false;
            }

            //DONT TOUCH - COMPLETE BLACK MAGIC
            ServerCancelToken.Cancel();
            ServerCancelToken.Dispose();
            ServerCancelToken = new();
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (AudioClient != null)
                {
                    AudioClient.Dispose();
                    AudioClient = null;
                }
                if (YoutubeClient != null)
                {
                    YoutubeClient = null;
                }
                if (ServerCancelToken != null)
                {
                    ServerCancelToken.Dispose();
                    ServerCancelToken = null;
                }
                QueueItems = null;
                UserGptHistory = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public SongQueueItem? DeQueue()
        {
            try
            {
                var t = QueueItems[0];
                QueueItems.RemoveAt(0);
                return t;
            }
            catch
            { return null; }
        }
    }
}
