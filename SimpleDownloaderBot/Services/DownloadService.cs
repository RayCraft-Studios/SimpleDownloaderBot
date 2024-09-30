
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using Discord.Commands;
using System.IO.Compression;
using System.Reflection.Metadata.Ecma335;
using System.Web;
using AngleSharp.Dom;

namespace SimpleDownloaderBot.Services
{
    internal class DownloadService
    {
        private YoutubeClient youtube = new YoutubeClient();
        private int minutesMax = 10;
        private const int batchSize = 5;
        private string tempPath = Path.GetTempPath();

        public async Task CheckURL(string url, SocketCommandContext context)
        {
            if (IsYoutubePlaylist(url)){
                await DownloadPlaylistAsMusic(url, context);
            }
            else{
                await DownloadVideoAsMusic(url, context);
            }
        }

        /**
         * Methode that use the Youtube-Explode Libary to download the specific
         * Video through the specific link
         * 
         * videoUrl = url from the video you want to download
         * context = the SocketCommandContext you want to send the responce
         */
        private async Task DownloadVideoAsMusic(string videoUrl, SocketCommandContext context)
        {
            var channel = context.Channel;
            try
            {
                var videoId = VideoId.Parse(videoUrl);
                var video = await youtube.Videos.GetAsync(videoId);

                string validName = CheckValidName(video.Title);
                await channel.SendMessageAsync($"Start downloading {validName}...");
                Console.WriteLine($"Downloading {validName}...");

                if (video.Duration <= TimeSpan.FromMinutes(minutesMax))
                {
                    try
                    {
                        var videoFilePath = Path.Combine(tempPath, $"{validName}.mp4");
                        var audioFilePath = Path.Combine(tempPath, $"{validName}.mp3");

                        await DownloadVideo(video, videoFilePath, audioFilePath);

                        if (File.Exists(audioFilePath))
                        {
                            await channel.SendFileAsync(audioFilePath);
                            File.Delete(audioFilePath);
                        }

                        if (File.Exists(videoFilePath)) File.Delete(videoFilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Download error: {ex.Message}");
                        throw;
                    }
                }
                else
                {
                    await channel.SendMessageAsync($"{video.Title} is to long: Downloads cannot have more than {minutesMax} minutes. This data has a length of {video.Duration}");
                }
            }
            catch (Exception ex)
            {
                await channel.SendMessageAsync($"An error occurred: {ex.Message}");
            }
        }

        /**
         * Method to download the whole public playlist
         */
        private async Task DownloadPlaylistAsMusic(string playlistUrl, SocketCommandContext context)
        {
            var channel = context.Channel;
            try
            {
                var playlistId = PlaylistId.Parse(playlistUrl);
                var playlist = await youtube.Playlists.GetAsync(playlistId);

                await channel.SendMessageAsync($"Playlist title: {playlist.Title}. Start Downloading...");
                Console.WriteLine($"Playlist title: {playlist.Title}");

                var videos = await youtube.Playlists.GetVideosAsync(playlistId);
                var videoList = videos.ToList();

                for (int i = 0; i < videoList.Count; i += batchSize)
                {
                    List<string> pathList = new List<string>();
                    var currentBatch = videoList.Skip(i).Take(batchSize).ToList();
                    var downloadTasks = currentBatch.Select(async video =>
                    {
                        try
                        {
                            var videoId = VideoId.Parse(video.Url);
                            var ytvideo = await youtube.Videos.GetAsync(videoId);

                            string validName = CheckValidName(ytvideo.Title);

                            var videoFilePath = Path.Combine(tempPath, $"{validName}.mp4");
                            var audioFilePath = Path.Combine(tempPath, $"{validName}.mp3");

                            await DownloadVideo(ytvideo, videoFilePath, audioFilePath);

                            if (File.Exists(audioFilePath))
                            {
                                pathList.Add(audioFilePath);
                            }
                            if (File.Exists(videoFilePath)) File.Delete(videoFilePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex);
                            await channel.SendMessageAsync($"An error occurred when downloading {video.Title} ERROR: {ex.Message}");
                        }
                    });

                    await Task.WhenAll(downloadTasks);
                    await channel.SendMessageAsync($"Batch {i / batchSize + 1} completed.");

                    string zipFile = filesToZip(pathList);
                    if (File.Exists(zipFile))
                    {
                        await channel.SendFileAsync(zipFile);
                        File.Delete(zipFile);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
                await channel.SendMessageAsync("All videos have been processed.");
            }
            catch (Exception ex)
            {
                await channel.SendMessageAsync($"An error occurred: {ex.Message}");
            }
        }

        /**
         * Helper methode to convert files to zip
         */
        private string filesToZip(List<string> fileList)
        {
            string zipFilePath = Path.Combine(tempPath, "DownloadedVideos.zip");

            using (var zipFileStream = new FileStream(zipFilePath, FileMode.Create))
            {
                using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    foreach (var file in fileList)
                    {
                        if (File.Exists(file)) { 
                            zipArchive.CreateEntryFromFile(file, Path.GetFileName(file));
                            File.Delete(file);
                        }
                    }
                }
            }
            return zipFilePath;
        }

        /**
         * Method to download the specific video
         */
        private async Task DownloadVideo(Video video, string videoFilePath, string audioFilePath)
        {
            if (video.Duration <= TimeSpan.FromMinutes(minutesMax))
            {
                try
                {
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                    var videoStreamInfo = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                    var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                    await youtube.Videos.Streams.DownloadAsync(videoStreamInfo, videoFilePath);
                    await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath);

                    Console.WriteLine("Done!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Download error: {ex.Message}");
                    throw;
                }
            }
        }

        /**
         * Helper Method to check if the link is for a playlist
         */
        private static bool IsYoutubePlaylist(string url)
        {
            Uri uri;
            if (Uri.TryCreate(url,UriKind.Absolute, out uri))
            {
                var query = HttpUtility.ParseQueryString(uri.Query);
                return query["list"] != null;
            }
            return false;
        }

        /**
         * Method to remove invalid char out
         * of the string 
         */
        private string CheckValidName(string videoName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidChars.Length; i++)
            {
                videoName = videoName.Replace(invalidChars[i], ' ');
            }
            return videoName;
        }
    }
}
