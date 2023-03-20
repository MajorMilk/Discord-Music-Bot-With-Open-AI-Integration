using Discord.Commands;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace DiscordBotV2.Modules
{
    internal class AIPrefixModule : ModuleBase
    {
        [Command("DALLE", RunMode = RunMode.Async)]
        public async Task DALLECommandAsync(string prompt)
        {
            var authorname = Context.Message.Author.Username;

            var result = await Helpers.DALLEAsync(prompt, authorname);

            await ReplyAsync(result);
        }

        [Command("ClearGPT")]
        public async Task ClearGPTHistory()
        {
            var aID = Context.Message.Author.Id;

            if (Helpers.Servers.TryGetValue(Context.Guild.Id, out var Server))
            {
                if (Server.UserGptHistory.ContainsKey(aID))
                {
                    Server.UserGptHistory.Remove(aID);
                }
            }
        }


        [Command("GPT", RunMode = RunMode.Async)]
        public async Task GPTAsync(string prompt)
        {
            var guildId = Context.Guild.Id;
            if (!Helpers.Servers.TryGetValue(guildId, out var Server))
            {
                Server = new(guildId);
                Helpers.Servers[guildId] = Server;
            }
            var authorId = Context.Message.Author.Id;
            if (!Server.UserGptHistory.TryGetValue(authorId, out List<ChatMessage> messages))
            {
                messages = new List<ChatMessage>();
                messages.Add(ChatMessage.FromSystem("You are Chat GPT, however you are also deployed in a discord server. " +
                                                    "Try not to respond with too long of messages. You should also format your messages using the tools discord offers, " +
                                                    "for code, do three back ticks (```) followed by the language name (```csharp) then end the code sample with another three back ticks. " +
                                                    "you should also use discord emojis and similar features as much as possible."));

                messages.Add(ChatMessage.FromAssistant("I understand completley. :thumbsup:"));
            }
            messages.Add(ChatMessage.FromUser(prompt));
            Server.UserGptHistory[authorId] = messages;
            var result = await Helpers.GPTAsync(messages, guildId, authorId);
            await ReplyAsync(result);
        }


        [Command("Davinci", RunMode = RunMode.Async)]
        public async Task DavinciAsync(string str)
        {
            var result = await Helpers.DavinciAsync(str);
            if (result != "")
            {
                await ReplyAsync(result);
            }
            var aid = Context.Message.Author.Id;
        }
    }
}
