using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NadekoBot.Classes.JSONModels;

namespace NadekoBot.DataModels.Bartender
{
    class MorphModel : IDataModel
    {
        public long UserId { get; set; }

        public int Gender { get; set; } // 0 = they, 1 = he, 2 = she

        // unless noted these numbers refer to locations in the UserMorphs.JSON array
        // public int BodyType { get; set; }
        public int UpperType { get; set; }
        public int LowerType { get; set; }

        public int LegType { get; set; }
        public int ArmType { get; set; }

        public int FaceType { get; set; }
        public int EyeType { get; set; }
        public int EyeColor { get; set; } // draws from list of colors.
        public int HairType { get; set; }
        public int HairColor { get; set; } // draws from list of colors.
        public int EarType { get; set; }
        public int LipColor { get; set; } // draws from list of colors.

        public int HornType { get; set; }
        public int HornSize { get; set; }
        public int HornColor { get; set; }

        public int NeckFeature { get; set; }
        public int NeckColor { get; set; }

        public int ArmFeature { get; set; }
        public int ArmColor { get; set; }
        public int LegFeature { get; set; }
        public int LegColor { get; set; }

        public int TongueType { get; set; }
        public int TeethType { get; set; }
        public int TongueColor { get; set; }

        public int SkinType { get; set; }
        public int SkinPattern { get; set; } // draws from list in bartender, not currently implemented
        public int SkinColor { get; set; } // draws from list of colors
        public int SkinOrnaments { get; set; } // draws from list
        public int OrnamentColor { get; set; }

        //public int SkinCovering { get; set; }
        public int ArmCovering { get; set; }
        public int TorsoCovering { get; set; }
        public int LegCovering { get; set; }
        public int CoveringColor { get; set; }

        public int HandMod { get; set; }
        public int FeetMod { get; set; }
        public int HandType { get; set; }
        public int FeetType { get; set; }

        public int WingType { get; set; }
        public int WingSize { get; set; } // draws from type of WingType
        public int WingColor { get; set; }
        public int TailType { get; set; }
        public int TailSize { get; set; } // draws from type of TailType
        public int TailColor { get; set; }

        // following values are purely numeric
        public int LegCount { get; set; }
        public int ArmCount { get; set; }
        public int WingCount { get; set; }
        public int TailCount { get; set; }
        public int HairLength { get; set; }
        public int EarCount { get; set; }
        public int TongueCount { get; set; }
        public int TongueLength { get; set; }
        public int EyeCount { get; set; }
        public int HornCount { get; set; }

        public int MorphCount { get; set; }
        public float Weight { get; set; } // bmi
        public int Height { get; set; } // in inches
        public float Musculature { get; set; } // abstract

        public MorphModel Copy()
        {
            return new MorphModel
            {
                UserId = this.UserId,

                Gender = this.Gender,

                UpperType = this.UpperType,
                LowerType = this.LowerType,

                LegType = this.LegType,
                ArmType = this.ArmType,

                FaceType = this.FaceType,
                EyeType = this.EyeType,
                EyeColor = this.EyeColor,
                HairType = this.HairType,
                HairColor = this.HairColor,
                EarType = this.EarType,
                LipColor = this.LipColor,

                HornType = this.HornType,
                HornSize = this.HornSize,
                HornColor = this.HornColor,

                NeckFeature = this.NeckFeature,
                NeckColor = this.NeckColor,

                ArmFeature = this.ArmFeature,
                ArmColor = this.ArmColor,
                LegFeature = this.LegFeature,
                LegColor = this.LegColor,

                TongueType = this.TongueType,
                TeethType = this.TeethType,
                TongueColor = this.TongueColor,

                SkinType = this.SkinType,
                SkinPattern = this.SkinPattern,
                SkinColor = this.SkinColor,
                SkinOrnaments = this.SkinOrnaments,
                OrnamentColor = this.OrnamentColor,

                ArmCovering = this.ArmCovering,
                TorsoCovering = this.TorsoCovering,
                LegCovering = this.LegCovering,
                CoveringColor = this.CoveringColor,

                HandMod = this.HandMod,
                FeetMod = this.FeetMod,
                HandType = this.HandType,
                FeetType = this.FeetType,

                WingType = this.WingType,
                WingSize = this.WingSize,
                WingColor = this.WingColor,
                TailType = this.TailType,
                TailSize = this.TailSize,
                TailColor = this.TailColor,

                LegCount = this.LegCount,
                ArmCount = this.ArmCount,
                WingCount = this.WingCount,
                TailCount = this.TailCount,
                HairLength = this.HairLength,
                EarCount = this.EarCount,
                TongueCount = this.TongueCount,
                TongueLength = this.TongueLength,
                EyeCount = this.EyeCount,
                HornCount = this.HornCount,

                MorphCount = this.MorphCount,
                Weight = this.Weight,
                Height = this.Height,
                Musculature = this.Musculature,
            };
        }
    }
}
