using CliWrap;
using Discord;
using Discord.Commands;

namespace DiscordMusicBot.Modules
{
    public class PrefixModule : ModuleBase
    {

        [Command("help")]
        public async Task HandleHelpCommand()
        {
            await ReplyAsync("The program uses the following commands\n\n" +
                "!DALLE - 'prompt' - Returns an image based off the given prompt\n" +
                "!Davinci 'prompt' - AI wisdom from Davinciv3\n" +
                "!GPT 'prompt' - AI Wisdom from chat gpt (remembers message history)\n" +
                "!ClearGPT - Clears your message history with ChatGPT\n"+
                "/ping, ping the bot. \n" +
                "/time - Gives the time\n" +
                "/play youtubelink plays a song from youtube  \n" +
                "/search 'random message' will search youtube and return the top result \n" +
                "/playsearch plays the last search \n" +
                "/skip - skips the current song\n" +
                "/leave - if theres a bug, use this to reset your session.\n" +
                "/queue / !Q - Shows the queue\n" +
                "/remove n - n is a number, removes a song from the queue\n" +
                "/swap n1 n2 - Swaps two songs position in the queue\n\n" +
                "Joining is handled automatically by the bot, only 1 instance per server, 3 requests per user at a time.\n\n" +
                "If you're having trouble searching for things or doing similar things that requie an input try wrapping your input in quoutes.");
        }

        [Command("ClearBotMessages", RunMode = RunMode.Async)]
        public async Task ClearBotMessages()
        {
            var user = Context.User as IGuildUser;

            if (!user.GuildPermissions.Administrator)
            {
                await ReplyAsync("You do not have permission to use this command");
                return;
            }


            var message = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
            var botMessages = message.Where(x => x.Author.Id == Context.Client.CurrentUser.Id || x.Content.StartsWith("!"));
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(botMessages);
        }

    }
}
