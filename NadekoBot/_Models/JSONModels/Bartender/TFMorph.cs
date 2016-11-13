using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class TFMorph
    {
        public TFMorph(
            string cod, string nam, int bodu, int bodl, int legt, int armt, int fact, int eyet, int[] eyec,
            int hait, int[] haic, int eart, int tont, int teet, int skit, int[] skio, int[] ornc, int[] sknc,
            int[] skic, int feet, int hant, int[] hanm, int[] feem, int wint, int tait, int mwin, int mtai,
            int meye, int mhai, int mear, int mton, int cton, int marm, int mleg, int[] winc, int[] taic, TFData trans
            )
        {
            Code = cod;
            Name = nam;
            UpperType = bodu;
            LowerType = bodl;
            LegType = legt;
            ArmType = armt;
            FaceType = fact;
            EyeType = eyet;
            EyeColor = eyec;
            HairType = hait;
            HairColor = haic;
            EarType = eart;
            TongueType = tont;
            TeethType = teet;
            SkinType = skit;
            Ornaments = skio;
            OrnamentColor = ornc;
            SkinColor = sknc;
            SkinCovering = skic;
            FeetType = feet;
            HandType = hant;
            HandModification = hanm;
            FeetModification = feem;
            WingType = wint;
            TailType = tait;
            WingColor = winc;
            TailColor = taic;

            MaxWings = mwin;
            MaxTails = mtai;
            MaxEyes = meye;
            MaxHair = mhai;
            MaxEars = mear;
            MaxTongueSize = mton;
            MaxTongueCount = cton;
            MaxArms = marm;
            MaxLegs = mleg;

            Transform = trans;
        }
        public String Code { get; set; }
        public String Name { get; set; }
        public int UpperType { get; set; }
        public int LowerType { get; set; }
        public int LegType { get; set; }
        public int ArmType { get; set; }
        public int FaceType { get; set; }
        public int EyeType { get; set; }

        public int HairType { get; set; }

        public int EarType { get; set; }
        public int TongueType { get; set; }
        public int TeethType { get; set; }
        public int SkinType { get; set; }

        public int[] Ornaments { get; set; }
        public int[] SkinCovering { get; set; }
        public int FeetType { get; set; }
        public int HandType { get; set; }
        public int[] HandModification { get; set; }
        public int[] FeetModification { get; set; }

        public int WingType { get; set; }
        public int[] WingColor { get; set; }
        public int TailType { get; set; }
        public int[] TailColor { get; set; }

        public int[] HairColor { get; set; }
        public int[] EyeColor { get; set; }
        public int[] SkinColor { get; set; }
        public int[] OrnamentColor { get; set; }

        public int MaxWings { get; set; }
        public int MaxTails { get; set; }
        public int MaxEyes { get; set; }
        public int MaxHair { get; set; }
        public int MaxEars { get; set; }
        public int MaxTongueSize { get; set; }
        public int MaxTongueCount { get; set; }
        public int MaxArms { get; set; }
        public int MaxLegs { get; set; }

        public TFData Transform { get; set; }
    }

    public class TFData
    {
        public TFData(int perm)
        {
            Permanence = perm;
        }
        public int Permanence { get; set; }
    }
}
