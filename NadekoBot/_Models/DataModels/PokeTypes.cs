﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NadekoBot.Classes.JSONModels;

namespace NadekoBot.DataModels
{
    class UserPokeTypes : IDataModel
    {
        public long UserId { get; set; }
        public string type { get; set; }
        public string moves {get; set; }
    }
}