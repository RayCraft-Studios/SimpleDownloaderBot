
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using Discord.Commands;

namespace SimpleDownloaderBot.Services
{
    internal class DownloadService
    {
        private YoutubeClient youtube = new YoutubeClient();
        private int minutesMax = 10;
        private const int batchSize = 10;
        private string tempPath = Path.GetTempPath();

        /**
         * Methode that use the Youtube-Explode Libary to download the specific
         * Video through the specific link
         * 
         * videoUrl = url from the video you want to download
         * context = the SocketCommandContext you want to send the responce
         */
        public async Task DownloadVideoAsMusic(string videoUrl, SocketCommandContext context)
        {
            var channel = context.Channel;
            var videoId = VideoId.Parse(videoUrl);
            var video = await youtube.Videos.GetAsync(videoId);

            string validName = CheckValidName(video.Title);
            await channel.SendMessageAsync($"Downloading {validName}...");
            Console.WriteLine($"Downloading {validName}...");

            if (video.Duration <= TimeSpan.FromMinutes(minutesMax))
            {
                try
                {
                    var videoFilePath = Path.Combine(tempPath, $"{validName}.mp4");
                    var audioFilePath = Path.Combine(tempPath, $"{validName}.mp3");

                    await downloadVideo(video, videoFilePath, audioFilePath);

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

        /**
         * Method to download the whole public playlist
         */
        public async Task DownloadPlaylistAsMusic(string playlistUrl, SocketCommandContext context)
        {
            var playlistId = PlaylistId.Parse(playlistUrl);
            var playlist = await youtube.Playlists.GetAsync(playlistId);

            var channel = context.Channel;
            await channel.SendMessageAsync($"Playlist title: {playlist.Title}. Start Downloading...");
            Console.WriteLine($"Playlist title: {playlist.Title}");

            var videos = await youtube.Playlists.GetVideosAsync(playlistId);
            var videoList = videos.ToList();

            for (int i = 0; i < videoList.Count; i += batchSize)
            {
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

                        await downloadVideo(ytvideo, videoFilePath, audioFilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                        await channel.SendMessageAsync($"An error occurred when downloading {video.Title} ERROR: {ex.Message}");
                    }
                });

                await Task.WhenAll(downloadTasks);
                await channel.SendMessageAsync($"Batch {i / batchSize + 1} completed.");
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
            await channel.SendMessageAsync("All videos have been processed.");
        }

        /**
         * Method to download the specific video
         */
        private async Task downloadVideo(Video video, string videoFilePath, string audioFilePath)
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
