using System;
using BullyBot;
using Microsoft.EntityFrameworkCore;

namespace BullyBot
{
    public class BullyBotDbContext : DbContext, IDisposable
    {
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //I would love to connect this to the ConfigService but I cant be bothered to research it
            var connectionString = Environment.GetEnvironmentVariable("BullyBotConnectionString");
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        }

        public override void Dispose()
        {
            System.Console.WriteLine("disposed!");
            base.Dispose();
        }
    }
}