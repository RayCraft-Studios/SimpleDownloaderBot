using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SimpleDownloaderBot.Services;

namespace SimpleDownloaderBot.Controller
{
    internal class MainController : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task getInfos()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Command Overview")
                .WithColor(Color.Blue)
                .WithDescription("List of all Commands:")
                .AddField("!help", "Shows all exiting Commands")
                .AddField("!hello", "Nice greeding for nice people")
                .AddField("!download [Youtube URL]", "Downloads the specific Video and send it the channel as mp3")
                .WithFooter(footer => footer.Text = "SimpleDownloaderBot | ©RayCraft Studios 2024")
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("hello")]
        public async Task greedings()
        {
            await ReplyAsync($"Howdy {Context.User}!");
        }
    }
}
