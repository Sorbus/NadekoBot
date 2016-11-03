using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class PokemonType
    {
        public PokemonType(string n, string i, List<PokemonMove> moves, List<PokemonMultiplier> multi)
        {
            Name = n;
            Icon = i;
            Moves = moves;
            Multipliers = multi;
        }
        public string Name { get; set; }
        public List<PokemonMultiplier> Multipliers { get; set; }
        public string Icon { get; set; }
        public List<PokemonMove> Moves { get; set; }
    }
    public class PokemonMultiplier
    {
        public PokemonMultiplier(string t, double m)
        {
            Type = t;
            Multiplication = m;
        }
        public string Type { get; set; }
        public double Multiplication { get; set; }
    }
    public class PokemonMove
    {
        public PokemonMove(string n, string t)
        {
            Name = n;
            Type = t;
        }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
