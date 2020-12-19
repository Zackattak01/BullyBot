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

        public string GetTimeString()
        {
            string returnString;
            if (Time.Date == DateTime.Now.Date)
                returnString = Time.ToString("a\\t h:mm tt");
            else
                returnString = Time.ToString("on MM/dd/yy a\\t h:mm tt");

            return returnString;
        }

        public string GetTimeCapitilizeFirstLetter()
        {
            string time = GetTimeString();
            return char.ToUpper(time[0]) + time.Substring(1);
        }


        public override string ToString()
        {
            return $"{GetTimeCapitilizeFirstLetter()}: {Value}  (Id: {Id})";
        }


    }
}