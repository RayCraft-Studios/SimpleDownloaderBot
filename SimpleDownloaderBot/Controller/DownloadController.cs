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
            await ReplyAsync($"Start downloading...");

            try
            {
                await downloadService.DownloadVideoAsMusic(youtubeUrl, Context);
                await ReplyAsync("Download completed and file sent.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }

        [Command("download_playlist")]
        public async Task downloadPlaylistAsync(string playlistUrl)
        {
            await ReplyAsync($"Start downloading whole Playlist...");

            try
            {
                await downloadService.DownloadPlaylistAsMusic(playlistUrl, Context);
                await ReplyAsync("Download completed!");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }

    }
}
