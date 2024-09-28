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
                .WithColor(Color.Blue) // Setzt eine schöne blaue Farbe für das Embed
                .WithDescription("Hier ist eine Liste aller verfügbaren Befehle:")
                .AddField("!help", "Shows all exiting Commands")
                .AddField("!download [Youtube URL]", "Downloads the specific Video and send it the channel as mp3")
                .WithFooter(footer => footer.Text = "SimpleDownloaderBot | ©RayCraft Studios 2024")
                .WithCurrentTimestamp();

            // Sende die Nachricht im aktuellen Channel
            await ReplyAsync(embed: embed.Build());
        }
    }
}
