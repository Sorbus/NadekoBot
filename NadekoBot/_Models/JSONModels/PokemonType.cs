using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class PokemonType
    {
        public PokemonType(string n, string i, List<PokemonMove> move, List<PokemonMultiplier> multi, String[] valid)
        {
            Name = n;
            Icon = i;
            Moves = move;
            Multipliers = multi;
            ValidMoves = valid;
        }
        public string Name { get; set; }
        public List<PokemonMultiplier> Multipliers { get; set; }
        public string Icon { get; set; }
        public List<PokemonMove> Moves { get; set; }
        public String[] ValidMoves { get; set; }
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
}
