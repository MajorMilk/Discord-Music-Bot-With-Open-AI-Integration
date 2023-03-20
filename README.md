# Discord-Music-Bot-With-Open-AI-Integration
A music bot made with Discord.NET 3.9, YoutubeExplode, and Betalgo.OpenAI.GPT3

this program uses FFMpeg without it, the music part of this music bot wont work. you'll need to provide a path to your exe. 
you need to place the path in the CreateStream function in the MusicPrefixModule at the top of the class.

You'll also need to provide your API keys for both discord and OpenAI. Tutorials can be found for doing so in various places. Be aware that the OpenAI Api is not free.

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

For the AI portion of the bot, its a pretty straight forward implementation of the Betalgo wrapper. Just a little more complicated than the examples.
