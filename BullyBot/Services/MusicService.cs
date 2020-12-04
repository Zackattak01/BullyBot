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
                Arguments = "-v verbose -report -probesize 2147483647 -i test -ac 2 -f s16le -ar 48000 pipe:1",
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


                try
                {
                    while (true)
                    {
                        await output.CopyToAsync(discord);
                        System.Console.WriteLine("done copying");
                    }

                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                }
                finally { await discord.FlushAsync(); }

                System.Console.WriteLine("operation finishes");
            }
        }

        private async Task<Stream> GetAudioStreamAsync(Video video)
        {
            StreamManifest manifest = await GetManifestAsync(video);

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