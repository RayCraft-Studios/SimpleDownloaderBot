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

        public async Task StartAsync()
        {
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task Log(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message || message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            // Überprüfen, ob die Nachricht in einem Server-Channel oder in einer DM gesendet wurde
            if (context.Channel is SocketGuildChannel guildChannel)
            {
                Console.WriteLine($"Befehl empfangen auf Server: {context.Guild.Name} in Kanal: {context.Channel.Name} und die Nachricht ist: {message.Content}");
            }
            else if (context.Channel is SocketDMChannel)
            {
                // Befehl wurde in einer privaten Nachricht (DM) gesendet
                Console.WriteLine($"Befehl empfangen in privaten Nachrichten: {message.Content}");
            }

            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        private async Task RegisterCommandsAsync()
        {
            await _commands.AddModuleAsync<DownloadController>(_services);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
