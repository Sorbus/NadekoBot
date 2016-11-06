﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NadekoBot.Classes.JSONModels;

namespace NadekoBot.DataModels
{
    class UserMorph : IDataModel
    {
        public long UserId { get; set; }

        public int Gender { get; set; } // 0 = they, 1 = he, 2 = she

        // unless noted these numbers refer to locations in the UserMorphs.JSON array
        public int BodyType { get; set; }
        public int UpperType{ get; set; }
        public int LowerType{ get; set; }

        public int LegType { get; set; }
        public int ArmType{ get; set; }

        public int FaceType { get; set; }
        public int EyeColor { get; set; } // draws from list inside whichever morph facetype is
        public int HairType { get; set; }
        public int HairColor { get; set; } // draws from hairtype
        public int EarType { get; set; }
       
        public int TongueType { get; set; }
        public int TeethType { get; set; }

        public int SkinType { get; set; }
        public int SkinOrnamentsMorph { get; set; }
        public int SkinOrnmanets { get; set; } // draws from SkinOrnamentsMorph

        public int SkinCovering { get; set; }
        public int ArmCovering { get; set; }
        public int TorsoCovering{ get; set; }
        public int LegCovering{ get; set; }

        public int HandModification { get; set; }
        public int FeetModification { get; set; }

        public int WingType { get; set; }
        public int TailType { get; set; }

        // following values are purely numeric
        public int LegCount { get; set; }
        public int ArmCount { get; set; }
        public int WingCount { get; set; }
        public int TailCount { get; set; }
        public int WingSize { get; set; }
        public int TailSize { get; set; }
        public int HairLength { get; set; }
        public int EarCount { get; set; }
        public int TongueLength { get; set; }
        public int EyeCount { get; set; }

    }
}
