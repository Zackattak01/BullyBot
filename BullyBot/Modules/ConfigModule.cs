using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ConfigurableServices;
using Discord.Commands;

namespace BullyBot
{
    [Group("config")]
    [RequireOwner]
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        public readonly ConfigService configService;

        public ConfigModule(IConfigService configService)
        {
            this.configService = configService as ConfigService;
        }

        [Command("reload")]
        public async Task ReloadAsync()
        {
            configService.Reload();
            await ReplyAsync("Config Reloaded");
        }

        [Command("backup")]
        public async Task BackupAsync()
        {
            var configPath = configService.ConfigPath;
            var zipName = $"Config-Backup-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff")}.zip";


            ZipFile.CreateFromDirectory(configService.ConfigPath, Path.Combine(configService.BackupPath, zipName));

            await ReplyAsync($"Backup success!  Backup name: \"{zipName}\"");
        }

        [Command("backup")]
        public async Task BackupAsync(string filename)
        {
            var configPath = configService.ConfigPath;
            var zipName = filename.EndsWith(".zip") ? filename : filename += ".zip";


            ZipFile.CreateFromDirectory(configService.ConfigPath, Path.Combine(configService.BackupPath, zipName));

            await ReplyAsync($"Backup success!  Backup name: \"{zipName}\"");
        }

        [Command("backup list")]
        [Alias("backups")]
        [Priority(1)]
        public async Task BackupListAsync()
        {
            var backupDir = new DirectoryInfo(configService.BackupPath);

            System.Console.WriteLine(backupDir.ToString());

            var backups = backupDir.GetFiles();

            var replyString = $"There are {backups.Count()} backups:\n\n";
            replyString += string.Join('\n', (backups.Select(x => Path.GetFileNameWithoutExtension(x.ToString()))));

            await ReplyAsync(replyString);
        }

        [Command("backup remove")]
        [Priority(1)]
        public async Task BackupRemoveAsync(string filename)
        {
            var zipName = filename.EndsWith(".zip") ? filename : string.Concat(filename, ".zip");
            var backup = new FileInfo(Path.Combine(configService.BackupPath, zipName));

            if (backup is null)
            {
                await ReplyAsync("Backup not found!");
                return;
            }
            else
            {
                backup.Delete();
                await ReplyAsync($"Backup \"{filename}\" removed");
            }
        }

    }
}