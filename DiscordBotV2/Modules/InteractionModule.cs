﻿using Discord.Interactions;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace DiscordMusicBot.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "ping pong")]
        public async Task HandlePingCommand()
        {
            await RespondAsync("Pong");
        }

        [SlashCommand("time", "return the time")]
        public async Task HandleTimeCommand()
        {
            string time = System.DateTime.Now.ToString();
            await RespondAsync(time);
        }

    }
}
