using CliWrap;
using Discord.Commands;

namespace DiscordBot.Modules
{
    internal partial class PrefixModule : ModuleBase
    {

        [Command("help")]
        public async Task HandleHelpCommand()
        {
            //var perms = (Context.User as IGuildUser)?.GetPermissions((Context.User as IVoiceChannel));

            await ReplyAsync("The program uses the following commands\n\n" +
                "!DALLE - 'prompt' - Returns an image based off the given prompt\n" +
                "!Davinci 'prompt' - AI wisdom from Davinciv3\n" +
                "!GPT 'prompt' - AI Wisdom from chat gpt (remembers message history)\n" +
                "!ClearGPT - Clears your message history with ChatGPT\n"+
                "/ping, ping the bot. \n" +
                "/time - Gives the time\n" +
                "!play youtubelink plays a song from youtube  \n" +
                "!search 'random message' will search youtube and return the top result \n" +
                "!playsearch plays the last search \n" +
                "!skip - skips the current song\n" +
                "!leave - if theres a bug, use this to reset your session.\n" +
                "!queue / !Q - Shows the queue\n" +
                "!clearQ / !clearqueue - clears the queue\n" +
                "!remove n - n is a number, removes a song from the queue\n" +
                "!swap n1 n2 - Swaps two songs position in the queue\n\n" +
                "Joining is handled automatically by the bot, only 1 instance per server, 3 requests per user at a time.\n\n" +
                "If you're having trouble searching for things or doing similar things that requie an input try wrapping your input in quoutes.");
        }
        
    }
}
