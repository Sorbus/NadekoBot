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
            string hait, string[] haic, string eart, string tont, string teet, string skit, string skio, string skic, string hanm,
            string feem, string wint, string tait, int mwin, int mtai, int meye, int mhai, int mear, int mton
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
            HandModification = hanm;
            FeetModification = feem;
            WingType = wint;
            TailType = tait;

            MaxWings = mwin;
            MaxTails = mtai;
            MaxEyes = meye;
            MaxHair = mhai;
            MaxEars = mear;
            MaxTongue = mton;
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
        public String SkinOrnaments { get; set; }
        public String SkinCovering { get; set; }
        public String HandModification { get; set; }
        public String FeetModification { get; set; }
        public String WingType { get; set; }
        public String TailType { get; set; }

        public int MaxWings { get; set; }
        public int MaxTails { get; set; }
        public int MaxEyes { get; set; }
        public int MaxHair { get; set; }
        public int MaxEars { get; set; }
        public int MaxTongue { get; set; }
    }
}
