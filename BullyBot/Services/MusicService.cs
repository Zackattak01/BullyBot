using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace BullyBot
{
    public class MusicService
    {
        YoutubeClient youtube;

        public const string ffmpegSaveAndConvert = "-v verbose -report -i test -ac 2 -f s16le -ar 48000 pipe:1";
        public const string ffmpegPlay = "-v verbose -report -i test2 -ac 2 -f s16le -ar 48000 pipe:1";

        public MusicService()
        {
            youtube = new YoutubeClient();
        }

        //add queue logic here
        //in future return video info for nice looking embed
        public async Task PlayAsync(string songName, IVoiceChannel channel)
        {
            Video video = await SearchVideoAsync(songName);




            IAudioClient audioClient = await channel.ConnectAsync();
            _ = Task.Run(async () => await SendAsync(audioClient, video));
        }



        private Process CreateStream()
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-v verbose -report -i test -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,

            });
        }

        private async Task SendAsync(IAudioClient client, Video video)
        {

            var a = await GetManifestAsync(video);
            IStreamInfo info = a.GetAudioOnly().WithHighestBitrate();
            await youtube.Videos.Streams.DownloadAsync(info, "test");


            //using (var ytVideo = await youtube.Videos.Streams.GetAsync(streamInfo))
            using (var ffmpeg = CreateStream())
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var input = ffmpeg.StandardInput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                System.Console.WriteLine(video.Title);
                /*
                _ = Task.Run(async () =>
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Stream videoStream;
                    long currentPos = -1;

                    var a = await GetManifestAsync(video);
                    IStreamInfo info = a.GetMuxed().WithHighestVideoQuality();
                    await youtube.Videos.Streams.DownloadAsync(info, "test");

                    while (stopwatch.Elapsed < video.Duration)
                    {
                        System.Console.WriteLine("Looped");
                        videoStream = await GetAudioStreamAsync(video);
                        try
                        {
                            if (currentPos != -1)
                                videoStream.Seek(currentPos, SeekOrigin.Begin);

                            //await videoStream.CopyToAsync(input);
                        }
                        catch (Exception e)
                        {
                            // System.Console.WriteLine("ERROR " + e.Message);
                            // System.Console.WriteLine("INPUT: " + youtubeVideo.Length);
                            // System.Console.WriteLine("POSITION: " + youtubeVideo.Position);

                            // var youtubeVideo2 = await youtube.Videos.Streams.GetAsync(streamInfo);
                            // await youtubeVideo2.CopyToAsync(input);
                            System.Console.WriteLine(e.Message);
                        }
                        finally
                        {
                            currentPos = videoStream.Position;
                            //await videoStream.FlushAsync();
                            //await videoStream.DisposeAsync();
                        }


                    }


                    System.Console.WriteLine(stopwatch.Elapsed);
                    System.Console.WriteLine("function exits");
                });*/



                Console.WriteLine("done");
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
                Console.WriteLine("done done");
            }
        }

        private async Task<Stream> GetAudioStreamAsync(Video video)
        {
            StreamManifest manifest = await GetManifestAsync(video);

            // System.Console.WriteLine(video.Duration);
            // foreach (var stream in manifest.GetAudio())
            // {
            //     System.Console.WriteLine(stream.Size);
            //     System.Console.WriteLine(stream.AudioCodec);
            //     System.Console.WriteLine(stream.Bitrate);
            //     System.Console.WriteLine();
            // }

            IStreamInfo streamInfo = manifest.GetAudioOnly().WithHighestBitrate();
            System.Console.WriteLine(streamInfo.Bitrate);
            return await youtube.Videos.Streams.GetAsync(streamInfo);
        }

        private async Task<Video> SearchVideoAsync(string songName)
            => await youtube.Search.GetVideosAsync(songName).FirstAsync();


        private async Task<StreamManifest> GetManifestAsync(Video video)
            => await youtube.Videos.Streams.GetManifestAsync(video.Id);
    }
}