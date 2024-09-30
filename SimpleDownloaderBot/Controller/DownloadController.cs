using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SimpleDownloaderBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDownloaderBot.Controller
{
    internal class DownloadController : ModuleBase<SocketCommandContext>
    {
        [Command("download")]
        public async Task downloadVideoAsync(string youtubeUrl, bool sendPrivate = true)
        {
            DownloadService downloadService = new DownloadService();
            downloadService.CheckURL(youtubeUrl, Context, sendPrivate);
        }

    }
}
