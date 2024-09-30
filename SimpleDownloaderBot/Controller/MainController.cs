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
                .AddField("!download [Youtube URL] [send as PM true/false (optional | default true)]", "Downloads the specific Video as mp3 and send it" + 
                            "as PM or, if 'send as PM' is set to false, send it in the channel where the command was called. Playlists gets recognised" +
                            " automatically and the videos will be downloaded one by one")
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
