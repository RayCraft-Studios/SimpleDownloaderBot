using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimpleDownloaderBot.Controller;

namespace SimpleDownloaderBot.Bot
{
    internal class DiscordBot
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly string _token;

        /**
         * Constructor for the DiscordBot
         * token represents the DiscordBotToke from the Discord Developer Portal
         * in case you use this Bot for your own purpose, just insert your token in config.js
         */
        public DiscordBot(string token)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.MessageContent
            });

            _commands = new CommandService();
            _token = token;

            _client.Log += Log;
            _client.MessageReceived += HandleCommandAsync;
        }

        /**
         * Task to start your Bot
         */
        public async Task StartAsync()
        {
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        /**
         * Startup Log Message 
         */
        private Task Log(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        /**
         *  Command detector
         *  if the command is valid, the specific controller-modul 
         *  gets executed
         */

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message || message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);
            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        /**
         * Init all command-modules and makes them accessable for the
         * HandleCommandAsync
         */
        private async Task RegisterCommandsAsync()
        {
            await _commands.AddModuleAsync<DownloadController>(_services);
            await _commands.AddModuleAsync<MainController>(_services);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
