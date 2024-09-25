
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
            var videoId = VideoId.Parse(videoUrl);
            var video = await youtube.Videos.GetAsync(videoId);
            Console.WriteLine($"Downloading {video.Title}...");

            string tempPath = Path.GetTempPath();
            string mp4FilePath = Path.Combine(tempPath, $"{video.Title}.mp4");
            string mp3FilePath = Path.Combine(tempPath, $"{video.Title}.mp3");

            try
            {
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
                var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

                await youtube.Videos.Streams.DownloadAsync(streamInfo, mp4FilePath);
                //ConvertMp4ToMp3(mp4FilePath, mp3FilePath);

                //var channel = context.Channel;
                //await channel.SendFileAsync(mp3FilePath, $"Here is the audio from {video.Title}!");

                if (File.Exists(mp4FilePath)) File.Delete(mp4FilePath);
                if (File.Exists(mp3FilePath)) File.Delete(mp3FilePath);
                Console.WriteLine("Download und Post abgeschlossen, temporäre Dateien gelöscht.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Download: {ex.Message}");
                throw;
            }
        }

        static void ConvertMp4ToMp3(string inputFilePath, string outputFilePath)
        {
            try
            {
                // Extrahiere das Audio aus der MP4-Datei
                using (var mediaFile = System.IO.File.OpenRead(inputFilePath))
                {
                    var outputWaveFile = Path.ChangeExtension(outputFilePath, ".wav");
                    using (var reader = new MediaFoundationReader(inputFilePath))
                    {
                        WaveFileWriter.CreateWaveFile(outputWaveFile, reader);
                    }

                    // Konvertiere die WAV-Datei in eine MP3-Datei
                    using (var reader = new AudioFileReader(outputWaveFile))
                    using (var writer = new LameMP3FileWriter(outputFilePath, reader.WaveFormat, LAMEPreset.VBR_90))
                    {
                        reader.CopyTo(writer);
                    }

                    // Lösche die temporäre WAV-Datei
                    System.IO.File.Delete(outputWaveFile);
                }

                Console.WriteLine("Umwandlung abgeschlossen.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler bei der Umwandlung: {ex.Message}");
            }
        }
    }
}
