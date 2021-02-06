using System;
using BullyBot;
using Microsoft.EntityFrameworkCore;

namespace BullyBot
{
    public class BullyBotDbContext : DbContext
    {
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //I would love to connect this to the ConfigService but I cant be bothered to research it
            var connectionString = Environment.GetEnvironmentVariable("BullyBotConnectionString");
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        }
    }
}