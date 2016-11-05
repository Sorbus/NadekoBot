using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class PokemonMove
    {
        public PokemonMove(string n, string t, int c)
        {
            Name = n;
            Type = t;
            Cost = c;
        }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Cost { get; set; }
    }
}
