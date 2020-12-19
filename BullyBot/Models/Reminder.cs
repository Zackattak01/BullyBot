using System;
using System.ComponentModel.DataAnnotations;

namespace BullyBot
{
    public class Reminder
    {
        [Key]
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public string Value { get; set; }

        public Reminder(DateTime time, ulong userId, ulong channelId, string value)
        {
            Time = time;
            UserId = userId;
            ChannelId = channelId;
            Value = value;
        }
        //=> (time, userId, channelId, value) = (Time, UserId, ChannelId, Value); //<- this is really cool

    }
}