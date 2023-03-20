using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot;
using DiscordBot.Modules;
using DiscordBotV2.Modules;
using DiscordBotV2.ServerInstance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordBotV2
{
    public class Program
    {
        private readonly string AuthToken = "TOKEN";

        public static Task Main() => new Program().MainAsync();


        public async Task MainAsync()
        {

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                services
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
                    AlwaysDownloadUsers = true

                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton(x => new CommandService())
                .AddSingleton<PrefixHandler>()
                .AddSingleton<ServerModule>()
                .AddSingleton<MusicPrefixModule>()
                .AddSingleton<AIPrefixModule>()
                )
                .Build();


            await RunAsync(host);

        }


        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var _client = provider.GetRequiredService<DiscordSocketClient>();
            var sCommands = provider.GetRequiredService<InteractionService>();
            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
            var pCommands = provider.GetRequiredService<PrefixHandler>();
            pCommands.AddModule<PrefixModule>();
            pCommands.AddModule<MusicPrefixModule>();
            pCommands.AddModule<AIPrefixModule>();

            await pCommands.InitializeAsync();


            _client.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); };
            sCommands.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); };

            _client.Ready += async () =>
            {
                await sCommands.RegisterCommandsGloballyAsync();
                Console.WriteLine("Bot ready");
            };

            await _client.LoginAsync(TokenType.Bot, AuthToken);
            await _client.StartAsync();

            await Task.Delay(-1);



        }
    }
}






