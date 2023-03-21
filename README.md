# Discord-Music-Bot-With-Open-AI-Integration
A music bot made with Discord.NET 3.9, YoutubeExplode, and Betalgo.OpenAI.GPT3

Because of a change made by discord to the way they handle packets this bots music functionality is broken ATM, theres already a fix in the Discord.NET Dev build, but not for the nuget. If you're having problems this is why.

this program uses FFMpeg without it, the music part of this music bot wont work. you'll need to provide a path to your exe. 
you need to place the path in the CreateStream function in the MusicPrefixModule at the top of the class.

You'll also need to provide your API keys for both discord and OpenAI. place you discord token in Program.cs Tutorials can be found for doing so in various places. Be aware that the OpenAI Api is not free.

For the AI portion of the bot, its a pretty straight forward implementation of the Betalgo wrapper. Just a little more complicated than the examples. Just make sure to put your token in the static Helpers function. Theres only 1 openai client for the whole bot so keep that in mind.


This bot also uses CliWrap to interface with FFMpeg.

Microsoft.Extensions.DependecyInjeciton

Microsoft.Extensions.Hosting

opus

sodium.Core


With all that said this is how it works

Joining and leaving is handled automatically, with only one instance being allowed in the server at a time.

GuildID's are linked to ServerModules, ServerModules store most of the data needed to play a song to an audio channel.
The server modules are also responsible for storing chat history for the !GPT command.

When a request is made with !play, the link is parsed and a SongQueueItem is made from the metadata

if it is the first time the !play command has been played connect, if currently playing, add to queue.

The Queue isnt a queue, its a List. it makes it easier to modify it and use LINQ statements without casting.
It simply does while(queue.Count > 0) with a custom implemented DeQueue function.

Heres a list of all commands

                !DALLE - 'prompt' - Returns an image based off the given prompt
                
                !Davinci 'prompt' - AI wisdom from Davinciv3
                
                !GPT 'prompt' - AI Wisdom from chat gpt (remembers message history)
                
                !ClearGPT - Clears your message history with ChatGPT
                
                /ping, ping the bot. 
                
                /time - Gives the time
                
                !play youtubelink plays a song from youtube 
                
                !search 'random message' will search youtube and return the top result
                
                !playsearch plays the last search 
                
                !skip - skips the current song
                
                !leave - if theres a bug, use this to reset your session.
                
                !queue / !Q - Shows the queue
                
                !clearQ / !clearqueue - clears the queue
                
                !remove n - n is a number, removes a song from the queue
                
                !swap n1 n2 - Swaps two songs position in the queue
