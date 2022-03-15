using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DiscordGameServerManager
{
    public class ThreadNotifications
    {
        public static void Notify(string notification, DSharpPlus.Entities.DiscordChannel channel) 
        { 
                notifications n = new notifications();
                n.notification = notification;
                n.channel = channel;
            DiscordFunctions.messageSend(n.notification, n.channel).ConfigureAwait(true).GetAwaiter().GetResult();
        }
        public static void Send(object data) 
        {
            notifications notification = (notifications)data;

        }
        private struct notifications
        {
            public string notification { get; set; }
            public DSharpPlus.Entities.DiscordChannel channel{get;set;}
        }
    }
}
