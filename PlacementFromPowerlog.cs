using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGLogPlugin
{
    class PlacementFromPowerlog
    {
        private const string token_1 = "FULL_ENTITY - Updating [entityName=";
        private const string token_2 = "zone=SETASIDE ";
        private const string token_3 = "cardId=";
        private const string token_4 = " player=";

        private const string health_token = "tag=HEALTH value=";

        private const string except_1 = "UNKNOWN ENTITY";
        private const string except_2 = "TB_BaconShop_HERO_PH";
        private const string except_3 = "TB_BaconShop_HERO_KelThuzad";

        private const string place_token_1 = "tag=PLAYER_LEADERBOARD_PLACE";
        private const string place_token_2 = "cardId=";
        private const string place_token_3 = "value=";
        private const string place_token_4 = " player=";

        private const string damage_token_1 = "TAG_CHANGE Entity=[entityName=";
        private const string damage_token_2 = "zone=SETASIDE";
        private const string damage_token_3 = "tag=DAMAGE value=";
        private const string damage_token_4 = "cardId=";
        private const string damage_token_5 = " player=";

        private bool isEnabled = false;

        private Hero[] heros = new Hero[8];
        private int idxHero = 0;

        public PlacementFromPowerlog()
        {
            idxHero = 0;
            for (int i = 0; i < heros.Length; i++) heros[i] = new Hero();
            heros[idxHero++].HeroID = "me";
        }

        public void CalcPlacement(string line)
        {
            if (!isEnabled
                && line.IndexOf(token_1) > -1 && line.IndexOf(token_2) > -1 && line.IndexOf(token_3) > -1
                && line.IndexOf(except_1) == -1 && line.IndexOf(except_2) == -1 && line.IndexOf(except_3) == -1
                && idxHero < 8)
            {
                string t = line.Substring(line.IndexOf(token_3) + token_3.Length, (line.IndexOf(token_4) - line.IndexOf(token_3) - token_4.Length + 1));
                heros[idxHero].HeroID = t;
                isEnabled = true;
            }

            if (isEnabled
                && line.IndexOf(health_token) > -1)
            {
                int cutStart = line.IndexOf(health_token) + health_token.Length;
                string h = line.Substring(cutStart, line.Length - cutStart);
                heros[idxHero].Health = Int32.Parse(h);
                idxHero++;
                isEnabled = false;
            }

            if (line.IndexOf(place_token_1) > -1 && line.IndexOf(place_token_2) > -1 && line.IndexOf(place_token_3) > -1)
            {
                int cutStart = line.IndexOf(place_token_2) + place_token_2.Length;
                int cutEnd = line.IndexOf(place_token_4) - cutStart;
                int valueStart = line.IndexOf(place_token_3) + place_token_3.Length;
                string cardId = line.Substring(cutStart, cutEnd);
                int placement = Int32.Parse(line.Substring(valueStart, line.Length - valueStart));

                bool isMe = true;
                foreach (Hero temp in heros)
                {
                    if (temp.HeroID == cardId)
                    {
                        temp.Placement = placement;
                        isMe = false;
                    }
                }
                if (isMe)
                {
                    heros[0].Placement = placement;
                    heros[0].HeroID = cardId;
                }
            }

            if (line.IndexOf(damage_token_1) > -1 && line.IndexOf(damage_token_2) > -1 && line.IndexOf(damage_token_3) > -1)
            {
                int cutStart = line.IndexOf(damage_token_4) + damage_token_4.Length;
                int cutEnd = line.IndexOf(damage_token_5) - cutStart;
                int valueStart = line.IndexOf(damage_token_3) + damage_token_3.Length;
                string cardId = line.Substring(cutStart, cutEnd);
                int damaged = Int32.Parse(line.Substring(valueStart, line.Length - valueStart));

                bool isMe = true;
                foreach (Hero temp in heros)
                {
                    if (temp.HeroID == cardId)
                    {
                        temp.Damaged = damaged;
                        isMe = false;
                    }
                }
                if (isMe) heros[0].Damaged = damaged;
            }
        }

        public int GetPlaement()
        {
            int prev_placement = heros[0].Placement;
            foreach (Hero hr in heros)
            {
                if (hr.HeroID == "me") continue;
                if (prev_placement < hr.Placement && (hr.Health - hr.Damaged > 0))
                {
                    heros[0].Placement++;
                }
            }
            return heros[0].Placement;
        }

        public string GetTempHeroID()
        {
            return heros[0].HeroID;
        }

        public void PrintHeros()
        {
            foreach (Hero hr in heros)
            {
                Console.WriteLine(hr.HeroID + " , " + hr.Health + " , " + hr.Damaged + " , " + hr.Placement);
            }
            Console.WriteLine();
            Console.WriteLine();
            int prev_placement = heros[0].Placement;
            foreach (Hero hr in heros)
            {
                if (hr.HeroID == "me") continue;
                if (prev_placement < hr.Placement && (hr.Health - hr.Damaged > 0))
                {
                    heros[0].Placement++;
                }
            }
            Console.WriteLine(heros[0].Placement);
        }

        public void Clear()
        {
            idxHero = 0;
            for (int i = 0; i < heros.Length; i++) heros[i] = new Hero();
            heros[idxHero].HeroID = "me";
        }
    }
}
