
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

        /**
         * Methode that use the Youtube-Explode Libary to download the specific
         * Video through the specific link
         * 
         * videoUrl = url from the video you want to download
         * context = the SocketCommandContext you want to send the responce
         */ 
        public async Task DownloadAndPostVideoAsync(string videoUrl, string format, SocketCommandContext context)
        {
            var channel = context.Channel;
            var videoId = VideoId.Parse(videoUrl);
            var video = await youtube.Videos.GetAsync(videoId);

            string validName = CheckValidName(video.Title);
            await channel.SendMessageAsync($"Downloading {validName}...");
            Console.WriteLine($"Downloading {validName}...");

            string tempPath = Path.GetTempPath();

            try
            {
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
                var videoStreamInfo = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                var videoFilePath = Path.Combine(tempPath, $"{validName}.mp4");
                var audioFilePath = Path.Combine(tempPath, $"{validName}.mp3");

                await youtube.Videos.Streams.DownloadAsync(videoStreamInfo, videoFilePath);
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath);

                await channel.SendFileAsync(audioFilePath, $"Here is the audio from {video.Title}!");

                if (File.Exists(audioFilePath)) File.Delete(audioFilePath);
                if (File.Exists(videoFilePath)) File.Delete(videoFilePath);
                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download error: {ex.Message}");
                throw;
            }
        }

        /**
         * Method to remove invalid char out
         * of the string 
         */
        private string CheckValidName(string videoName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidChars.Length; i++) {
                videoName = videoName.Replace(invalidChars[i], ' ');
            }
            return videoName;
        }
    }
}
