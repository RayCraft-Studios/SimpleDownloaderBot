using Newtonsoft.Json;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using SimpleDownloaderBot.Bot;
using SimpleDownloaderBot.Data;

namespace SimpleDownloaderBot;
class Program
{
    static Config config;

    /**
     * Entry point
     * 
     * Checks token 
     * Init and start bot
     */ 
    static async Task Main(string[] args)
    {
        InitAPIKeys();

        var discordBot = new DiscordBot(config.discordBotToken);
        await discordBot.StartAsync();
    }

    /**
     * Method to check if the tokens in config.json are empty
     */
    static void InitAPIKeys()
    {
        bool isValid = true;
        try
        {
            using (StreamReader reader = new StreamReader("Data/config.json"))
            {
                string json = reader.ReadToEnd();
                config = JsonConvert.DeserializeObject<Config>(json);
            }
            if (string.IsNullOrEmpty(config.discordBotToken)) { isValid = false; Console.WriteLine("Discord Bot Token is Missing. Please insert it in config.json"); }
        }
        catch (Exception ex)
        {
            isValid = false;
            Console.WriteLine(ex);
        }

        if (!isValid) { Environment.Exit(1); }
    }
}
