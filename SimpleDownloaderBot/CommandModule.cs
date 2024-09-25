using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDownloaderBot
{
    internal class CommandsModule : ModuleBase<SocketCommandContext>
    {
        private readonly DownloadService downloadService = new DownloadService();

        [Command("download")]
        public async Task InitReposAsync(string youtubeUrl)
        {
            await ReplyAsync("Start downloading from {youtubeUrl}...");

            try
            {
                await downloadService.DownloadAndPostVideoAsync(youtubeUrl, "mp3", Context);
                await ReplyAsync("Download completed and file sent.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }
    }
}
