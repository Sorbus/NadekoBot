using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Classes.JSONModels
{
    public class PlantModel
    {
        public PlantModel(string picc, string picm, string plac, string plam, string picp, string plap )
        {
            PickCommand = picc;
            PickMessage = picm;
            PlantCommand = plac;
            PlantMessage = plam;
            PlantPast = picp;
            PickPast = plap;
        }
        public string PickCommand { get; set; }
        public string PickMessage { get; set; }
        public string PlantCommand { get; set; }
        public string PlantMessage { get; set; }
        public string PickPast { get; set; }
        public string PlantPast { get; set; }
    }
}
