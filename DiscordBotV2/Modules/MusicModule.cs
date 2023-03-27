using CliWrap;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using DiscordMusicBot.ServerInstance;
using YoutubeExplode.Videos;
using RunMode = Discord.Interactions.RunMode;

namespace DiscordMusicBot.Modules
{
    public class MusicModule : InteractionModuleBase<SocketInteractionContext>
    {
        public async Task<MemoryStream> CreateStream(Stream strean, CancellationToken token)
        {
            var memoryStream = new MemoryStream();
            await Cli.Wrap("Path-to-FFMpeg")
            .WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
            .WithStandardInputPipe(PipeSource.FromStream(strean))
            .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
            .ExecuteAsync(token);
            return memoryStream;
        }

        //Big nope on reducing memory usage unfortunatley
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

        [SlashCommand("swap", "Swaps two songs position in the queue")]
        public async Task MoveSongAsync(int i, int j)
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out ServerModule Server))
            {
                await RespondAsync("You must have a queue", ephemeral:true);
                return;
            }
            else if (Server.MoveSong(i - 1, j - 1))
            {
                var eb = new EmbedBuilder()
                {
                    Title = "Swapped",
                    Description = $"[{Server.QueueItems[j - 1].Title}]({Server.QueueItems[j - 1].Url})\n[{Server.QueueItems[i - 1].Title}]({Server.QueueItems[i - 1].Url})",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    }
                };


                await RespondAsync(embed:eb.Build());
            }
            else await RespondAsync("Failed", ephemeral: true);
        }


        [SlashCommand("remove","Removes a song from the queue")]
        public async Task ReamoveSongAsync(int i)
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out ServerModule Server))
            {
                await RespondAsync("You must have a queue", ephemeral: true);
                return;
            }
            if(i > Server.QueueItems.Count || i < 1)
            {
                await RespondAsync("Invalid index", ephemeral: true);
                return;
            }
            var item = Server.QueueItems[i - 1];
            if (Server.RemoveSong(i - 1))
            {
                var eb = new EmbedBuilder()
                {
                    Title = "Removed",
                    Description = $"[{item.Title}]({item.Url})",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    }
                };
                await RespondAsync(embed: eb.Build());
            }
            else await RespondAsync("Failed", ephemeral: true);
        }



        [SlashCommand("clearqueue","Clears the queue")]
        public async Task ClearQAsync()
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out ServerModule Server))
            {
                await RespondAsync("You must have a queue", ephemeral: true);
                return;
            }
            Server.QueueItems.Clear();

            await RespondAsync("the queue has been cleared", ephemeral: true);
        }

        [SlashCommand("queue","Shows the queue")]
        public async Task ShowQueueAsync()
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out ServerModule Server))
            {
                await RespondAsync("You must have a queue", ephemeral:true);
                return;
            }
            var l = Server.GetQueue();

            string t = "";
            int it = 1;
            foreach (var item in l)
            {
                t += $"{it++}: " +  $"[{item.songname}]({item.Url})" + " - Requested by: " + item.User.Username + '\n';
            }
            if (t.Length > 0)
            {
                await RespondAsync(t, ephemeral: true);
            }
        }

        [SlashCommand("playsearch","Plays the most recent search in the server", runMode:RunMode.Async)]
        public async Task PlaySearchAsync()
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out var Server))
            {
                Helpers.Servers[Context.Guild.Id] = new(Context.Guild.Id);
                Server = Helpers.Servers[Context.Guild.Id];
            }
            else if (Server.LastSearch == "")
            {
                await RespondAsync("You Need to Search for something first!", ephemeral:true);
                return;
            }

            await PlayAsync(Server.LastSearch);
        }


        [SlashCommand("play", "Plays a given youtube URL in the server", runMode: RunMode.Async)]
        public async Task PlayAsync(string URL)
        {
            var guildId = Context.Guild.Id;
            if (!Helpers.Servers.TryGetValue(guildId, out var Server))
            {
                Helpers.Servers.Add(guildId, new(guildId));
                Server = Helpers.Servers[guildId];
            }

            //Spam protection
            if (Server.QueueItems.Where(x => x.User == Context.User).Count() >= 3)
            {
                await RespondAsync("Only 3 songs at a time please", ephemeral:true);
                return;
            }

            if (string.IsNullOrEmpty(URL))
            {
                await RespondAsync("Please provide a YouTube URL to play.", ephemeral:true);
                return;
            }

            if (!Helpers.IsValidYoutubeUrl(URL))
            {
                await RespondAsync("Invalid URL.", ephemeral: true);
                return;
            }

            var voiceChannel = (Context.User as IVoiceState)?.VoiceChannel;
            if (voiceChannel == null)
            {
                await RespondAsync("You must be in a voice channel to use this command.", ephemeral: true);
                return;
            }


            //Grab metedata of video provided
            var video = await Helpers.GetVideoMetaDataAsync(URL, guildId);
            if (video == null)
            {
                await RespondAsync("Failed to get video metadata.", ephemeral: true);
                return;
            }

            
            if (video.Duration.Value.TotalMinutes > 10)
            {
                await RespondAsync("The max length of a video is 10 minutes", ephemeral: true);
                return;
            }


            SongQueueItem t = new();
            t.User = Context.User;
            t.Url = URL;
            t.songname = video.Title;
            t.VID = video.Id;
            Server.CurrentSong = t.songname;

            EmbedBuilder eb = new()
            {
                Title = "Now Playing",
                Description = $"[{t.songname}]({t.Url})",
                Color = Color.Red,
                ThumbnailUrl = video.Thumbnails[0].Url,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {t.User.Username}",
                    IconUrl = t.User.GetAvatarUrl()
                }
            };
            t.EmbedMessage = eb;

            if (Server.AudioClient != null)
            {
                Server.QueueItems.Add(t);
                await RespondAsync($"Added {video.Title} to the queue. Position in queue: {Server.QueueItems.Count()}.", ephemeral:true);
                return;
            }
            else
            {
                await RespondAsync(embed:t.EmbedMessage.Build(), ephemeral:true);
                Server.AudioClient = await voiceChannel.ConnectAsync();

                Server.QueueItems.Add(t);

                while (Server.QueueItems.Count > 0)
                {
                    await PlayNextSong(guildId);
                }

                await LeaveAsync();
            }

        }


        public async Task PlayNextSong(ulong GuildID)
        {
            //If in the server, grab the audio client
            if (Helpers.Servers.TryGetValue(GuildID, out var Server))
            {
                var item = Server.DeQueue();

                Server.CurrentSong = item.Value.songname;
                CancellationTokenSource token = Server.ServerCancelToken;
                if (token == null || token.IsCancellationRequested)
                {
                    Server.ServerCancelToken = new();
                }
                try
                {
                    await ReplyAsync(embed: item.Value.EmbedMessage.Build());
                    await SendAsync(Server.AudioClient, item.Value.VID, Server.ServerCancelToken.Token);
                }
                catch (OperationCanceledException)
                {
                    // The song was skipped
                    var eb = new EmbedBuilder()
                    {
                        Title = "Skipped",
                        Description = $"[{item.Value.songname}]({item.Value.Url})",
                        Color = Color.Red,
                        ThumbnailUrl = item.Value.EmbedMessage.ThumbnailUrl,
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Requested by {item.Value.User.Username}",
                            IconUrl = item.Value.User.GetAvatarUrl()
                        }
                    };
                    await ReplyAsync(embed:eb.Build());
                }
            }
            else
                Console.WriteLine("ERROR");
        }


        [SlashCommand("search","Searches for a song on youtube")]
        public async Task SearchAsync(string Phrase)
        {
            if (!Helpers.Servers.TryGetValue(Context.Guild.Id, out var Server))
            {
                Helpers.Servers[Context.Guild.Id] = new(Context.Guild.Id);
                Server = Helpers.Servers[Context.Guild.Id];
            }
            var client = Server.YoutubeClient;
            try
            {
                var SearchClient = client.Search;

                //This is an enumerable, you can grab as many searche results as you want.
                var vids = SearchClient.GetVideosAsync(Phrase);
                var search = vids.FirstAsync();


                var result = search.Result;
                Server.LastSearch = result.Url;

                EmbedBuilder eb = new()
                {
                    Title = "Search Result",
                    Description = $"[{result.Title}]({result.Url})",
                    Color = Color.Red,
                    ThumbnailUrl = result.Thumbnails[0].Url,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Requested by {Context.User.Username}",
                        IconUrl = Context.User.GetAvatarUrl()
                    }
                };

                await ReplyAsync(embed: eb.Build());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }


        [SlashCommand("skip", "Skips the currently playing song")]
        public async Task SkipAsync()
        {   //                                                     Although this is a bool it cancels the token
            if (!Helpers.Servers.ContainsKey(Context.Guild.Id) || !Helpers.Servers[Context.Guild.Id].SkipSong())
            {
                await RespondAsync("I'm not playing anything?", ephemeral:true);
                return;
            }
            //in order to make this a slash command without refactoring i've made this concession.
            //The command needs to respond with something, so I'm just going to respond with "Done" epehemeraly
            await RespondAsync("Done", ephemeral: true);
        }

        [SlashCommand("leave","Attempts to make the bot leave")]
        public async Task LeaveAsync()
        {
            var guildId = Context.Guild.Id;

            if (!Helpers.Servers.TryGetValue(guildId, out var Server) || Server.AudioClient == null)
            {
                await RespondAsync("But Im not in the server?", ephemeral:true);
                return;
            }


            await Server.AudioClient.StopAsync();
            Server.Dispose();
            Helpers.Servers.Remove(guildId);
        }
    }

}
