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
             string code, string name, int[] skit, int[] skio, int[] skic,
             MorphData transform, MorphColors color, MorphMax max, TFHead head, TFBody body, TFAppendages appendages
            )
        {
            Code = code;
            Name = name;

            SkinType = skit;
            Ornaments = skio;
            SkinCovering = skic;

            Transform = transform;
            Head = head;
            Body = body;
            Appendages = appendages;
            Color = color;
            Max = max;
        }
        public String Code { get; set; }
        public String Name { get; set; }

        public int[] SkinType { get; set; }
        public int[] Ornaments { get; set; }
        public int[] SkinCovering { get; set; }

        public MorphData Transform { get; set; }
        public MorphMax Max { get; set; }
        public MorphColors Color { get; set; }

        public TFHead Head { get; set; }
        public TFBody Body { get; set; }
        public TFAppendages Appendages { get; set; }
    }

    public class MorphColors
    {
        public MorphColors(int[] eyec, int[] haic, int[] ornc, int[] sknc, int[] coco, int[] winc,
            int[] taic, int[] horc, int[] necc, int[] armc, int[] legc, int[] tong)
        {
            Covering = coco;
            Wing = winc;
            Tail = taic;
            Horn = horc;
            Neck = necc;
            Arm = armc;
            Leg = legc;
            Ornament = ornc;
            Skin = sknc;
            Eye = eyec;
            Hair = haic;
            Tongue = tong;
        }
        public int[] Ornament { get; set; }
        public int[] Covering { get; set; }
        public int[] Wing { get; set; }
        public int[] Tail { get; set; }
        public int[] Hair { get; set; }
        public int[] Eye { get; set; }
        public int[] Skin { get; set; }
        public int[] Lip { get; set; }
        public int[] Horn { get; set; }
        public int[] Neck { get; set; }
        public int[] Leg { get; set; }
        public int[] Arm { get; set; }
        public int[] Tongue { get; set; }
    }

    public class MorphMax
    {
        public MorphMax(int mwin, int mtai, int meye, int mhai, int mear, int mton, int cton,
            int marm, int mleg, int mhor)
        {
            Wings = mwin;
            Tails = mtai;
            Eyes = meye;
            Hair = mhai;
            Ears = mear;
            TongueSize = mton;
            TongueCount = cton;
            Arms = marm;
            Legs = mleg;
            Horns = mhor;
        }
        public int Wings { get; set; }
        public int Tails { get; set; }
        public int Eyes { get; set; }
        public int Hair { get; set; }
        public int Ears { get; set; }
        public int TongueSize { get; set; }
        public int TongueCount { get; set; }
        public int Arms { get; set; }
        public int Legs { get; set; }
        public int Horns { get; set; }
    }

    public class MorphData
    {
        public MorphData(int perm)
        {
            Permanence = perm;
        }
        public int Permanence { get; set; }
    }
}
