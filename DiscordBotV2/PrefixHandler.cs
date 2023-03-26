using Discord.Commands;
using Discord.WebSocket;

namespace DiscordMusicBot
{
    public class PrefixHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        public PrefixHandler(DiscordSocketClient client, CommandService commands)
        {
            _client = client;
            _commands = commands;
        }

        public async Task InitializeAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
        }

        public void AddModule<T>()
        {
            _commands.AddModuleAsync<T>(null);
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            var m = message as SocketUserMessage;
            if (m != null) 
            {
                int argPos = 0;
                if (!(m.HasCharPrefix('!', ref argPos)) || !m.HasMentionPrefix(_client.CurrentUser, ref argPos) || m.Author.IsBot)
                {
                    Console.WriteLine($"{message.Author} - {message.ToString()}");
                }
                var context = new SocketCommandContext(_client, m);

                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: null);
            }
        }
    }
}
