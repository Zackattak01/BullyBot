using System;
using System.Collections.Generic;
using System.Linq;

namespace BullyBot
{
    public class FFmpegArguments
    {
        public string CommandLineArguments { get { return BuildArgString(); } }

        private bool Report { get; }

        public bool PipedInput { get; }

        public bool PipedOutput { get; }

        public string InputFile { get; }

        public string OutputFormat { get; }

        public FFmpegArguments()
        {
            Report = false;
            PipedInput = false;
            PipedOutput = false;
            InputFile = string.Empty;
            OutputFormat = string.Empty;
        }

        public FFmpegArguments(bool report, bool pipedInput, bool pipedOutput, string inputFile, string outputFormat)
        {
            this.Report = report;
            this.PipedInput = pipedInput;
            this.PipedOutput = pipedOutput;
            this.InputFile = inputFile;
            this.OutputFormat = outputFormat;
        }

        public FFmpegArguments WithReporting()
        {
            return new FFmpegArguments(true, PipedInput, PipedOutput, InputFile, OutputFormat);
        }

        public FFmpegArguments WithPipedInput()
        {
            return new FFmpegArguments(Report, true, PipedOutput, string.Empty, OutputFormat);
        }

        public FFmpegArguments WithPipedOutput()
        {
            return new FFmpegArguments(Report, PipedInput, true, InputFile, OutputFormat);
        }

        public FFmpegArguments WithPipedIO()
        {
            return new FFmpegArguments(Report, true, true, string.Empty, OutputFormat);
        }

        public FFmpegArguments WithInputFile(string input)
        {
            return new FFmpegArguments(Report, false, PipedOutput, input, OutputFormat);
        }

        public FFmpegArguments WithOutputFormat(string format)
        {
            return new FFmpegArguments(Report, PipedInput, PipedOutput, InputFile, format);
        }



        private string BuildArgString()
        {
            List<string> arguments = new List<string>();

            if (Report)
                arguments.Add("-v verbose -report ");

            //build input param
            if (string.IsNullOrEmpty(InputFile) && PipedInput == false)
                throw new ArgumentException("FFmpegArguments: Input type must be specified!");

            string inputParam = PipedInput ? "pipe:0" : InputFile;

            arguments.Add($"-i {inputParam}");

            //add universal args
            arguments.Add("-ac 2");
            arguments.Add("-ar 48000");

            //build output param

            if (string.IsNullOrEmpty(OutputFormat))
                throw new ArgumentException("FFmpegArguments: Output format must be specified!");

            arguments.Add($"-f {OutputFormat}");

            if (PipedOutput == false)
                throw new ArgumentException("FFmpegArguments: Output type must be specified!");

            arguments.Add("pipe:1");



            return string.Join(' ', arguments);
        }

    }
}