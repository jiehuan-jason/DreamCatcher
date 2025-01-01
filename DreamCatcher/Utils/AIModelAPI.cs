using DreamCatcher.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DreamCatcher
{
    public class AIModelAPI
    {

        private static AppSettings settings;
        private static bool _isInit = false;
        static OpenAIClientOptions openAISettings;
        static ApiKeyCredential apiKey;
        static ChatClient chatClient;

        public AIModelAPI()
        {
        }

        private static async Task init()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("appsettings.txt");
            using var reader = new StreamReader(stream);

            var contents = await reader.ReadToEndAsync();
            settings = JsonConvert.DeserializeObject<AppSettings>(contents);  // 反序列化成对象
            _isInit = true;
            openAISettings = new()
            {
                Endpoint = new Uri(await GetBaseURL())
            };
            apiKey = new ApiKeyCredential(await GetApiKey());
            chatClient = new ChatClient(model: "gpt-4o-mini", credential: apiKey, openAISettings);
        }

        private static async Task<string> GetApiKey()
        {
            if (!_isInit)
                await init();
            return settings.ApiKey;
        }

        private static async Task<string> GetBaseURL()
        {
            if (!_isInit)
                await init();
            return settings.BaseURL;
        }
        public static async Task<string> GetAIOneTagAsync(string DreamText)
        {
            return await GetAIAnswer($"从以下梦境描述中提取一个 2-4 个字的关键词：\n{DreamText}\n关键词（输出中仅包含关键词）：");
        }

        public static async Task<string> GetAITagsJSONAsync(string DreamText){
            string content = await GetAIAnswerJSON($"从以下梦境描述中提取一些场景，将场景放入一个数组，数组名为scenes，这些词中不能出现人物，且每个词仅为名词，不要出现动词以及冠词；再提取出现的人物（注意包括自己），把人物名称放入另一个数组，数组名为characters，每个数组至少有一个元素：\n{DreamText}\n关键词（输出中仅包含关键词）：");
            if(content.StartsWith("```"))
                return content[7..(content.Length-3)];
            else
                return content;
        }

        public static async Task<string> GetAIAnalyseAsync(string DreamText)
        {
            return await GetAIAnswer($"你是一个专业的梦境分析员，请你帮我从出现的场景和人物、梦境对现实的映照等方面来分析以下梦境：\n{DreamText}\n分析结果：");
        }

        public static async Task<List<Dream>> SearchDreamsAIAsync(string KeyWord){
            var prompt = $"下面会给你一些梦境Tag和对应的ID，你需要根据Tags搜索相关的梦(如果有与搜索词意思相近的Tag就符合条件)，输出相关梦的id，用\",\"(英文逗号）隔开，关键词：{KeyWord}";
            var all_dreams = await new DreamDataRepo().GetAllDreams();
            foreach(var dream in all_dreams){
                prompt += $"\nID:{dream.Id},Tags:{dream.AIDreamTags.Replace("\n","")}";
                Debug.WriteLine($"\nID:{dream.Id},Tags:{dream.AIDreamTags.Replace("\n", "")}");
            }
            prompt+= $"\n结果（输出仅包含id,ID在{all_dreams.Count}及以内,如果找不到直接输出0）：";
            var answer = await GetAIAnswer(prompt);
            if(answer == "0")
                return new List<Dream>();
            var id_list = new List<int>(answer.Split(',').Select(x => int.Parse(x)));
            
            var result = new List<Dream>();
            foreach (var id in id_list){
                var item = all_dreams.FirstOrDefault(dream => dream.Id == id);
                if (item != null)
                    result.Add(item);
            }
            return result;
        }   

        public static async Task<int> GetAIDreamTypeAsync(string DreamText)
        {
            return int.Parse( await GetAIAnswer($"从以下梦境描述中总结是一个好梦，一个噩梦还是一个光怪陆离的梦，分别输出1、2、3：\n{DreamText}\n类型（输出中仅包含梦境类型的数字）："));
        }

        public static async Task<CollectionResult<StreamingChatCompletionUpdate>> GetAIAnalyseStreamAsync(int good_dreams, int bad_dreams, int other_dreams, Dictionary<string, int> scenesCount, Dictionary<string, int> charactersCount){
            var prompt = $"你是一个专业的梦境分析员，这是客户一段时间所做的梦，请你对他这段时间的状态加以分析\n" +
                $"一共{good_dreams+bad_dreams+other_dreams}个梦,其中{good_dreams}个美梦，{bad_dreams}个噩梦，{other_dreams}个光怪陆离的梦\n " +
                $"以下是客户梦境中出现的场景和人物：\n";

            foreach (var scene in scenesCount)
            {
                prompt += $"场景：{scene.Key}，出现次数：{scene.Value}\n";
            }
            foreach (var character in charactersCount){
                prompt += $"人物：{character.Key}，出现次数：{character.Value}\n";
            }
            prompt += "请你从各种梦的次数、出现次数较多的场景和人物（不用具体列出出现几次）等方面来分析客户近期的睡眠质量以及情绪状态。如果睡眠质量不好（例如经常做噩梦），再帮助分析现实中可能遇到的问题以及解决方法。你现在正在面对客户，请你亲切又不失理性地向客户分析";
            return await GetAIAnswerStream(prompt);
            }

        private static async Task<string> GetAIAnswer(String text)
        {
            if (!_isInit)
                await AIModelAPI.init();

            ChatCompletion completion = await chatClient.CompleteChatAsync(text);
            return completion.Content[0].Text;
        }

        private static async Task<CollectionResult<StreamingChatCompletionUpdate>> GetAIAnswerStream(String text)
        {
            if (!_isInit)
                await AIModelAPI.init();

            return chatClient.CompleteChatStreaming(text);
        }

        private static async Task<string> GetAIAnswerJSON(String text)
        {
            if (!_isInit)
                await AIModelAPI.init();

            ChatCompletion completion = await chatClient.CompleteChatAsync("你是一个智能梦境助理，请你把以下问题的结果用JSON格式输出，不要输出其他任何内容：\n" + text + "\n结果（输出中仅包含JSON格式的文本）：");
            return completion.Content[0].Text;
        }

        public static async Task<byte[]> GetAIPicFromDreamTextAsync(String text){
            if                                                                            (!_isInit)
                await AIModelAPI.init();
            
            ImageClient imageClient = new ImageClient("dall-e-2", credential: apiKey, openAISettings);
            //string prompt = "你是一个专业的梦境分析员，你需要根据以下梦境描述生成一张梦境相关的图片，尽量还原梦境中真实又虚无的感觉，画面中主要以风景为主，人物尽量出现背影：\n" + text;
            string prompt = text;
            ImageGenerationOptions options = new()
            {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1024xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            GeneratedImage image = imageClient.GenerateImage(prompt, options);
            BinaryData bytes = image.ImageBytes;
            return bytes.ToArray();
        }   

    }
}
