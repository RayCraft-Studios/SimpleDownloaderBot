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
        private readonly DownloadService downloadService = new DownloadService();

        [Command("download")]
        public async Task downloadVideoAsync(string youtubeUrl)
        {
            downloadService.DownloadVideoAsMusic(youtubeUrl, Context);
        }

        [Command("download_playlist")]
        public async Task downloadPlaylistAsync(string playlistUrl)
        {
            downloadService.DownloadPlaylistAsMusic(playlistUrl, Context);
        }

    }
}
