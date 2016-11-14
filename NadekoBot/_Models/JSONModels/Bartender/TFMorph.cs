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
             string code, string name, int[] eyec, int[] haic, int[] skit, int[] skio, int[] sknc, int[] ornc,
             int[] skic, int[] coco, int[] winc, int[] taic, int[] horc, int mwin, int mtai, int meye, int mhai, int mear,
             int mton, int cton, int marm, int mleg, int mhor, TFData trans, TFHead head, TFBody body, TFAppendages appe
            )
        {
            Code = code;
            Name = name;
            EyeColor = eyec;
            HairColor = haic;
            SkinType = skit;
            Ornaments = skio;
            OrnamentColor = ornc;
            SkinColor = sknc;
            SkinCovering = skic;
            CoveringColor = coco;
            WingColor = winc;
            TailColor = taic;
            HornColor = horc;

            MaxWings = mwin;
            MaxTails = mtai;
            MaxEyes = meye;
            MaxHair = mhai;
            MaxEars = mear;
            MaxTongueSize = mton;
            MaxTongueCount = cton;
            MaxArms = marm;
            MaxLegs = mleg;
            MaxHorns = mhor;

            Transform = trans;
            Head = head;
            Body = body;
            Appendages = appe;
        }
        public String Code { get; set; }
        public String Name { get; set; }

        public int[] SkinType { get; set; }
        public int[] Ornaments { get; set; }
        public int[] OrnamentColor { get; set; }
        public int[] SkinCovering { get; set; }
        public int[] CoveringColor { get; set; }

        public int[] WingColor { get; set; }
        public int[] TailColor { get; set; }

        public int[] HairColor { get; set; }
        public int[] EyeColor { get; set; }
        public int[] SkinColor { get; set; }
        public int[] LipColor { get; set; }
        public int[] HornColor { get; set; }

        public int MaxWings { get; set; }
        public int MaxTails { get; set; }
        public int MaxEyes { get; set; }
        public int MaxHair { get; set; }
        public int MaxEars { get; set; }
        public int MaxTongueSize { get; set; }
        public int MaxTongueCount { get; set; }
        public int MaxArms { get; set; }
        public int MaxLegs { get; set; }
        public int MaxHorns { get; set; }

        public TFData Transform { get; set; }
        public TFHead Head { get; set; }
        public TFBody Body { get; set; }
        public TFAppendages Appendages { get; set; }
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
