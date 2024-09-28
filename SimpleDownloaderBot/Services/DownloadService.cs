
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
         * Method to download the whole public playlist
         */
        public async Task DownloadAndPostPlayListAsync(string playlistUrl, string format, SocketCommandContext context)
        {
            var playlistId = PlaylistId.Parse(playlistUrl);
            var playlist = await youtube.Playlists.GetAsync(playlistId);

            var channel = context.Channel;
            await channel.SendMessageAsync($"Playlist title: {playlist.Title}. Start Downloading...");
            Console.WriteLine($"Playlist title: {playlist.Title}");

            var videos = await youtube.Playlists.GetVideosAsync(playlistId);

            foreach ( var video in videos )
            {
                try
                {
                    await DownloadAndPostVideoAsync(video.Url, format, context);
                }
                catch( Exception ex )
                {
                    Console.WriteLine("Error: " + ex);
                    await channel.SendMessageAsync($"An error occured when downloading {video.Title} ERROR: {ex}");
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
            for (int i = 0; i < invalidChars.Length; i++) {
                videoName = videoName.Replace(invalidChars[i], ' ');
            }
            return videoName;
        }
    }
}
