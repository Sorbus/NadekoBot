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
            string cod, string nam, string bodt, string bodu, string bodl, string legt, string armt, string fact, string[] eyec,
            string hait, string[] haic, string eart, string tont, string teet, string skit, string[] skio, string skic, string feet,
            string hant, string hanm, string feem, string wint, string tait, int mwin, int mtai, int meye, int mhai, int mear,
            int mton, int marm, int mleg, string[] wins, string[] tais, string[] legp, string earp, string[] winp, string[] taip,
            int perm
            )
        {
            Code = cod;
            Name = nam;
            BodyType = bodt;
            UpperType = bodu;
            LowerType = bodl;
            LegType = legt;
            ArmType = armt;
            FaceType = fact;
            EyeColor = eyec;
            HairType = hait;
            HairColor = haic;
            EarType = eart;
            TongueType = tont;
            TeethType = teet;
            SkinType = skit;
            SkinOrnaments = skio;
            SkinCovering = skic;
            FeetType = feet;
            HandType = hant;
            HandModification = hanm;
            FeetModification = feem;
            WingType = wint;
            WingSize = wins;
            TailType = tait;
            TailSize = tais;
            LegPosition = legp;
            EarPosition = earp;
            WingPosition = winp;
            TailPosition = taip;

            MaxWings = mwin;
            MaxTails = mtai;
            MaxEyes = meye;
            MaxHair = mhai;
            MaxEars = mear;
            MaxTongue = mton;
            MaxArms = marm;
            MaxLegs = mleg;

            Permanence = perm;
        }
        public String Code { get; set; }
        public String Name { get; set; }
        public String BodyType { get; set; }
        public String UpperType { get; set; }
        public String LowerType { get; set; }
        public String LegType { get; set; }
        public String ArmType { get; set; }
        public String FaceType { get; set; }
        public String[] EyeColor { get; set; }
        public String HairType { get; set; }
        public String[] HairColor { get; set; }
        public String EarType { get; set; }
        public String TongueType { get; set; }
        public String TeethType { get; set; }
        public String SkinType { get; set; }
        public String[] SkinOrnaments { get; set; }
        public String SkinCovering { get; set; }
        public String FeetType { get; set; }
        public String HandType { get; set; }
        public String HandModification { get; set; }
        public String FeetModification { get; set; }
        public String WingType { get; set; }
        public String TailType { get; set; }
        public String[] WingSize { get; set; }
        public String[] TailSize { get; set; }
        public String[] LegPosition { get; set; }
        public String EarPosition { get; set; }
        public String[] WingPosition { get; set; }
        public String[] TailPosition { get; set; }

        public int MaxWings { get; set; }
        public int MaxTails { get; set; }
        public int MaxEyes { get; set; }
        public int MaxHair { get; set; }
        public int MaxEars { get; set; }
        public int MaxTongue { get; set; }
        public int MaxArms { get; set; }
        public int MaxLegs { get; set; }

        public int Permanence { get; set; }
    }
}
