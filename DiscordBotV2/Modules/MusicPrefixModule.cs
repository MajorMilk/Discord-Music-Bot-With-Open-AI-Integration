using CliWrap;
using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBotV2.ServerInstance;
using YoutubeExplode.Videos;

namespace DiscordBotV2.Modules
{
    internal class MusicPrefixModule : ModuleBase
    {
		public async Task<MemoryStream> CreateStream(Stream strean, CancellationToken token)
        {
            var memoryStream = new MemoryStream();
            await Cli.Wrap("PATH-TO-FFMPEG")
        .WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
        .WithStandardInputPipe(PipeSource.FromStream(strean))
        .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
        .ExecuteAsync(token);
            return memoryStream;
        }




        public async Task SendAsync(IAudioClient client, VideoId vidId, CancellationToken token)
        {
            using (var audioStream = await Helpers.AudioStreamInfoAsync(Context.Guild.Id, vidId, token))
            {
                using (var FFStream = await CreateStream(audioStream, token))
                {
                    using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
                    {
                        try { await discord.WriteAsync(FFStream.ToArray(), 0, (int)FFStream.Length, token); }
                        finally { await discord.FlushAsync(); }
                    }
                    FFStream.Dispose();
                }
            }
        }
		

        [Command("swap")]
        public async Task MoveSongAsync(int i, int j)
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out ServerModule Server))
            {
                await ReplyAsync("You must have a queue");
                return;
            }
            else if (Server.MoveSong(i - 1, j - 1))
            {
                await ReplyAsync($"Moved {i} to {j} and {j} to {i}");
            }
            else await ReplyAsync("Failed");
        }


        [Command("remove")]
        public async Task ReamoveSongAsync(int i)
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out ServerModule Server))
            {
                await ReplyAsync("You must have a queue");
                return;
            }

            if (Server.RemoveSong(i - 1))
            {
                await ReplyAsync("Removed song number " + i);
            }
            else await ReplyAsync("Failed");
        }



        [Command("clearqueue")]
        public async Task ClearQueueAsync()
        {
            await ClearQAsync();
        }

        [Command("clearQ")]
        public async Task ClearQAsync()
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out ServerModule Server))
            {
                await ReplyAsync("You must have a queue");
                return;
            }
            Server.QueueItems.Clear();

            await ReplyAsync("the queue has been cleared");
        }

        [Command("Q")]
        public async Task ShowQ()
        {
            await ShowQueueAsync();
        }


        [Command("queue")]
        public async Task ShowQueueAsync()
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out ServerModule Server))
            {
                await ReplyAsync("You must have a queue");
                return;
            }
            var l = Server.GetQueue();

            string t = "";
            int it = 1;
            foreach (var item in l)
            {
                t += $"{it++}: " + item.songname + " - Requested by: " + item.User.Username + '\n';
            }
            if (t.Length > 0)
            {
                await ReplyAsync(t);
            }
        }

        [Command("playsearch", RunMode = RunMode.Async)]
        public async Task PlaySearchAsync()
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out var Server))
            {
                Helpers.Servers[Context.Guild.Id] = new(Context.Guild.Id);
                Server = Helpers.Servers[Context.Guild.Id];
            }
            else if (Server.LastSearch == "")
            {
                await ReplyAsync("You Need to Search for something first!");
                return;
            }

            await PlayAsync(Server.LastSearch);
        }


        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayAsync(string URL)
        {
            var guildId = Context.Guild.Id;
            if (!Helpers.Servers.TryGetValue(guildId, out var Server))
            {
                Helpers.Servers.Add(guildId, new(guildId));
                Server = Helpers.Servers[guildId];
            }

            //Spam protection
            if (Server.QueueItems.Where(x => x.User == Context.Message.Author).Count() >= 3)
            {
                await ReplyAsync("Only 3 songs at a time please");
                return;
            }

            if (string.IsNullOrEmpty(URL))
            {
                await ReplyAsync("Please provide a YouTube URL to play.");
                return;
            }

            if (!Helpers.IsValidYoutubeUrl(URL))
            {
                await ReplyAsync("Invalid URL.");
                return;
            }

            var voiceChannel = (Context.User as IVoiceState)?.VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyAsync("You must be in a voice channel to use this command.");
                return;
            }


            //Grab metedata of video provided
            var video = await Helpers.GetVideoMetaDataAsync(URL, guildId);



            //This is because RAM usage is directly linked to a videos length.
            //A 10 minute vid takes about 600-700 MB of ram to play on a single server.
            if (video.Duration.Value.TotalMinutes > 10)
            {
                await ReplyAsync("The max length of a video is 10 minutes");
                return;
            }


            SongQueueItem t = new SongQueueItem();
            t.User = Context.Message.Author;
            t.Url = URL;
            t.songname = video.Title;
            t.VID = video.Id;
            Server.CurrentSong = t.songname;

            if (Server.AudioClient != null)
            {
                Server.QueueItems.Add(t);
                await ReplyAsync($"Added {video.Title} to the queue. Position in queue: {Server.QueueItems.Count()}.");
                return;
            }
            else
            {
                Server.AudioClient = await voiceChannel.ConnectAsync();

                Server.QueueItems.Add(t);

                while (Server.QueueItems.Count > 0)
                {
                    await PlayNextSong(guildId);
                }

                await LeaveAsync();
            }

        }


        private async Task PlayNextSong(ulong GuildID)
        {
            //If in the server, grab the audio client
            if (Helpers.Servers.TryGetValue(GuildID, out var Server))
            {
                var item = Server.DeQueue();
                Server.CurrentSong = item.songname;
                if (item == null) return;

                Server.CurrentSong = item.songname;
                CancellationTokenSource token = Server.ServerCancelToken;
                if (token == null || token.IsCancellationRequested)
                {
                    Server.ServerCancelToken = new();
                }
                try
                {
                    await ReplyAsync($"Playing: {Server.CurrentSong}");
                    await SendAsync(Server.AudioClient, item.VID, Server.ServerCancelToken.Token);
                }
                catch (OperationCanceledException)
                {
                    // The song was skipped
                    await ReplyAsync($"Skipped: {item.songname}");
                }
            }
        }

        /// <summary>
        /// If theres no YoutubeClient yet for a sevrer, make one for it
        /// 
        /// then return top search result
        /// </summary>
        /// <param name="Phrase"></param>
        /// <returns></returns>
        [Command("search")]
        public async Task SearchAsync(string Phrase)
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out var Server))
            {
                Helpers.Servers[Context.Guild.Id] = new(Context.Guild.Id);
                Server = Helpers.Servers[Context.Guild.Id];
            }
            var client = Server.YoutubeClient;
            if (client == null)
            {
                Console.WriteLine("Making a YoutubeClient");
                client = new();
                Server.YoutubeClient = client;
            }
            try
            {
                var SearchClient = client.Search;

                var vids = SearchClient.GetVideosAsync(Phrase);

                var test = vids.FirstAsync();
                var result = test.Result;
                Server.LastSearch = result.Url;
                string response = $"Heres what I found: {result.Author} - {result.Title} - {result.Url}";

                await ReplyAsync(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }


        [Command("skip")]
        public async Task SkipAsync()
        {   //                                                     Although this is a bool it cancels the token
            if (!Helpers.Servers.ContainsKey(Context.Guild.Id) || !Helpers.Servers[Context.Guild.Id].SkipSong())
            {
                await ReplyAsync("I'm not playing anything?");
                return;
            }
        }

        

        //Never used because its kinda pointless to have the bot just join out of no where and do nothing
        public async Task JoinAsync()
        {
            var voiceChannel = (Context.User as IVoiceState)?.VoiceChannel;
            if (voiceChannel == null)
            {
                await Context.Channel.SendMessageAsync("You need to be in a voice channel to use this command.");
                return;
            }
            await Context.Channel.SendMessageAsync($"Joined voice channel **{voiceChannel.Name}**.");
            await voiceChannel.ConnectAsync();
        }

        [Command("leave")]
        public async Task LeaveAsync()
        {
            var guildId = Context.Guild.Id;
            IAudioClient client;

            if (!Helpers.Servers.TryGetValue(guildId, out var Server) || Server.AudioClient == null)
            {
                await ReplyAsync("But Im not in the server?");
                return;
            }
            client = Server.AudioClient;


            await client.StopAsync();
            Server.Dispose();
            Helpers.Servers.Remove(guildId);
        }


    }
}
