﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordGameServerManager
{
    //To be Implemented logic for user methods will be migrated to here
    class User
    {
    }
    public struct userdata
    {
        public string username { get; set; }
        public ulong userID { get; set; }
        public string discriminator { get; set; }
        public string role { get; set; }
        public int trust_level { get; set; }
        public bool registered { get; set; }
        public permissions perms { get; set; }
    }
}
