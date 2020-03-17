using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGLogPlugin
{
    class ParamJson
    {
        public string PlayerID { get; set; }
        public string HeroID { get; set; }
        public Version Version { get; set; }
        public int Placement { get; set; }
        public int MMR { get; set; }
        public List<string> LeaderBoard { get; set; }
        public List<string> UsedCard { get; set; }
        public List<string> TurnBoard { get; set; }
        public int TurnCount { get; set; }
    }
}
