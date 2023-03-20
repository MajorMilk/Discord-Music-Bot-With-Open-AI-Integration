using Discord.Interactions;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace DiscordBot.Modules
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




        






        /*[SlashCommand("sqrt", "Returns the square root of a number")]
        public Task SqrtCommand(
            [Option("Num", "The number you want the square root of")] double num)
        {

        }*/

    }
}
