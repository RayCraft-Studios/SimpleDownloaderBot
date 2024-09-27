
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using NAudio.Wave;
using NAudio.Lame;
using System;
using Discord;
using Discord.Commands;

namespace SimpleDownloaderBot
{
    internal class DownloadService
    {
        private YoutubeClient youtube = new YoutubeClient();
        public async Task DownloadAndPostVideoAsync(string videoUrl, string format, SocketCommandContext context)
        {
            var channel = context.Channel;
            var videoId = VideoId.Parse(videoUrl);
            var video = await youtube.Videos.GetAsync(videoId);
            //await channel.SendMessageAsync($"Downloading {video.Title}!");
            Console.WriteLine($"Downloading {video.Title}...");

            string tempPath = Path.GetTempPath();

            try
            {
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

                var videoStreamInfo = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                var videoFilePath = Path.Combine(tempPath, $"{video.Title}_video.mp4");
                var audioFilePath = Path.Combine(tempPath, $"{video.Title}.mp3");

                await youtube.Videos.Streams.DownloadAsync(videoStreamInfo, videoFilePath);
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath);

                await channel.SendFileAsync(audioFilePath, $"Here is the audio from {video.Title}!");

                if (File.Exists(audioFilePath)) File.Delete(audioFilePath);
                if (File.Exists(videoFilePath)) File.Delete(videoFilePath);
                Console.WriteLine("Download und Post abgeschlossen, temporäre Dateien gelöscht.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Download: {ex.Message}");
                throw;
            }
        }
    }
}
