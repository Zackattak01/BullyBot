using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Discord;
using Discord.Audio;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            IStreamInfo stream = await GetAudioStreamAsync(songName);




            IAudioClient audioClient = await channel.ConnectAsync();
            _ = Task.Run(async () => await SendAsync(audioClient, stream));
        }

        private async Task<IStreamInfo> GetAudioStreamAsync(string songName)
        {
            Video video = await youtube.Search.GetVideosAsync(songName).FirstAsync();

            StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);


            AudioOnlyStreamInfo streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate() as AudioOnlyStreamInfo;
            //System.Console.WriteLine(streamInfo.Bitrate);
            //System.Console.WriteLine(streamInfo.Container);
            if (streamInfo != null)
            {
                //await youtube.Videos.Streams.DownloadAsync(streamInfo, "test");
                //Stream stream = await youtube.Videos.Streams.GetAsync(streamInfo);


                return streamInfo;
            }

            throw new Exception("Something failed while getting the audio stream");
        }

        private Process CreateStream()
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-v verbose -report -acodec libopus -i pipe: -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,

            });
        }

        private async Task SendAsync(IAudioClient client, IStreamInfo streamInfo)
        {

            //using (var ytVideo = await youtube.Videos.Streams.GetAsync(streamInfo))
            using (var ffmpeg = CreateStream())
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var input = ffmpeg.StandardInput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {

                System.Console.WriteLine(streamInfo.Bitrate);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await youtube.Videos.Streams.CopyToAsync(streamInfo, input);


                    }
                    catch (IOException e)
                    {
                        System.Console.WriteLine("ERROR " + e.Message);
                        System.Console.WriteLine("INPUT: " + input.Length);
                        System.Console.WriteLine("POSITION: " + input.Position);
                    }
                });
                Console.WriteLine("done");
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }
    }
}