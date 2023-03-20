using DiscordBotV2.ServerInstance;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System.Text.RegularExpressions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DiscordBotV2
{
    public static class Helpers
    {

        public static Dictionary<ulong, ServerModule> Servers = new();

        public static bool IsValidYoutubeUrl(string videoUrl)
        {
            Regex regex = new(@"(?:v=|\/)([a-zA-Z0-9_-]{11}).*");
            var match = regex.Match(videoUrl);
            return match.Success;
        }



        public static OpenAIService gpt3 = new(new OpenAiOptions()
        {
            ApiKey = "TOKEN"
        });

        public static bool GPTInUse = false;

        //Should never really be used. Just for in case of Client freeezing requests to DALLE
        public static void ResetGPTClient()
        {
            gpt3 = new(new OpenAiOptions()
            {
                ApiKey = "TOKEN"
            });
            GPTInUse = false;
        }


        public static async Task<string> DALLEAsync(string prompt, string authorName)
        {
            if (GPTInUse) return "GPT Client is in use";
            else
            {
                GPTInUse = true;
                var imageResult = await gpt3.Image.CreateImage(new ImageCreateRequest
                {
                    Prompt = prompt,
                    //     Note that this is 2 cents per image 512 and 255 are way cheaper
                    Size = StaticValues.ImageStatics.Size.Size1024,
                    ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
                    User = authorName
                });
                GPTInUse = false;
                if(imageResult.Successful)
                {
                    return imageResult.Results[0].Url;
                }
                return "Failed";
            }
        }
        public static async Task<string> DavinciAsync(string prompt)
        {
            if(!GPTInUse)
            {
                GPTInUse = true;
                var completionResult = await gpt3.Completions.CreateCompletion(new CompletionCreateRequest()
                {
                    Prompt = prompt,
                    Model = Models.TextDavinciV3,
                    Temperature = 0.5F,
                    MaxTokens = 333
                }) ;
                GPTInUse = false;
                if (completionResult.Successful)
                {
                    return completionResult.Choices[0].Text;
                }
                else
                {
                    return completionResult.Error.ToString();
                }
            }
            return "The GPT Client is in Use";
        }

        public static async Task<string> GPTAsync(List<ChatMessage> messages,ulong GuildID, ulong AuthorID)
        {
            if (!GPTInUse)
            {
                GPTInUse = true;
                var completionResult = await gpt3.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = messages,
                    Model = Models.ChatGpt3_5Turbo,
                    MaxTokens = 1000
                });
                GPTInUse = false;
                if (completionResult.Successful)
                {
                    Servers[GuildID].UserGptHistory[AuthorID].Add(ChatMessage.FromAssistant(completionResult.Choices.First().ToString()));
                    return completionResult.Choices.First().Message.Content;
                }
                else
                {
                    return completionResult.Error.ToString();
                }
            }
            return "The GPT Client is in Use";

        }


        public static async Task<Video> GetVideoMetaDataAsync(string videoUrl, ulong GuildID)
        {
            try
            {
                if (!IsValidYoutubeUrl(videoUrl))
                {
                    return null;
                }

                var regex = new Regex(@"(?:v=|\/)([a-zA-Z0-9_-]{11}).*");
                var match = regex.Match(videoUrl);
                var videoId = match.Success ? match.Groups[1].Value : null;

                if (Servers[GuildID].YoutubeClient == null) Servers[GuildID].YoutubeClient = new();
                var video = await Servers[GuildID].YoutubeClient.Videos.GetAsync(videoId);
                return video;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<Stream> AudioStreamInfoAsync(ulong GuildID, VideoId vId, CancellationToken token)
        {
            var cli = Servers[GuildID].YoutubeClient;

            var streamInfSet = await cli.Videos.Streams.GetManifestAsync(vId);

            var streamInfo = streamInfSet.GetAudioStreams().GetWithHighestBitrate();
            return await cli.Videos.Streams.GetAsync(streamInfo, token);


        }

    }
}
