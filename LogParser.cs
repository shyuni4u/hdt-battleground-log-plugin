using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Web.Script.Serialization;

using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using System.Threading.Tasks;
using System.IO;

namespace BGLogPlugin
{
    public class LogParser : IPlugin
    {
        private enum SaveState { Ready, Used };

        private const string CHECK_PLACE_TAG = "tag=PLAYER_LEADERBOARD_PLACE";
        private const string CHECK_PLACE_HERO = "cardId=";
        private const string CHECK_PLACE_VALUE = "value=";

        private SaveState LogState = SaveState.Ready;
        private ActivePlayer TurnState = ActivePlayer.Player;
        private ParamJson LogJson = new ParamJson();

        private PlacementFromPowerlog plc = new PlacementFromPowerlog();

        private List<string> ResultLog = new List<string>();

        private void SetInit()
        {
            plc = new PlacementFromPowerlog();
            LogState = SaveState.Ready;
            TurnState = ActivePlayer.Player;

            LogJson.PlayerID = "";
            LogJson.HeroID = "";
            LogJson.Version = "1.0.0";
            LogJson.Placement = 1;
            LogJson.MMR = 0;
            LogJson.LeaderBoard = new List<string>();
            LogJson.UsedCard = new List<string>();
            LogJson.TurnBoard = new List<string>();
            LogJson.TurnCount = 0;

            ResultLog.Clear();
        }

        private void OnTurnStart(ActivePlayer player)
        {
            if (TurnState != player && player == ActivePlayer.Player)
            {
                LogJson.TurnCount++;
                foreach (Entity ent in Core.Game.Player.Board)
                {
                    if (ent.Card.Type == "Minion")
                    {
                        LogJson.TurnBoard.Add(String.Format("{0}@#{1}@#{2}@#{3}@#{4}@#{5}", LogJson.TurnCount, ent.Card.Id, ent.Card.Name, ent.Card.Type, ent.Card.Attack, ent.Card.Health));
                    }
                }
            }
            TurnState = player;
        }

        private void OnGameStart()
        {
            SetInit();
            LogState = SaveState.Used;
        }

        private void OnGameWon()
        {
            Save();
            SetInit();
        }

        private void OnGameLost()
        {
            Save();
            SetInit();
        }

        private void OnGameEnd()
        {
            Save();
            SetInit();
        }

        private void Save()
        {
            if (LogState == SaveState.Used && Core.Game.CurrentGameMode == GameMode.Battlegrounds)
            {
                LogJson.PlayerID = Core.Game.Player.Name;
                foreach (Entity ent in Core.Game.Player.Board)
                {
                    if (ent.Card.Type == "Minion")
                    {
                        LogJson.LeaderBoard.Add(String.Format("{0}@#{1}@#{2}@#{3}@#{4}", ent.Card.Id, ent.Card.Name, ent.Card.Type, ent.Card.Attack, ent.Card.Health));
                    }
                }

                LogJson.Placement = plc.GetPlaement();

                if (LogJson.HeroID == "")
                {
                    LogJson.HeroID = plc.GetTempHeroID();
                }

                if (Core.Game.BattlegroundsRatingInfo != null)
                    LogJson.MMR = HearthMirror.Reflection.GetBattlegroundRatingInfo().Rating;

                JavaScriptSerializer jSer = new JavaScriptSerializer();
                string payload = jSer.Serialize(LogJson);
                Task.Run(() => Api.Post(ApiServer, payload)).Wait(500);

                //ResultLog.Add(payload);
                //FileHelper.Write(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PowerLog.txt"), ResultLog);

                LogJson.UsedCard.Clear();
                LogJson.LeaderBoard.Clear();
                LogJson.TurnBoard.Clear();
            }
        }

        //  Save placement
        private void DoInPowerLog(string line)
        {
            //ResultLog.Add(line);
            plc.CalcPlacement(line);
        }

        //  Save hero
        private void OnPlayerPlay(Card card)
        {
            if (card.Type == "Hero")
            {
                LogJson.HeroID = card.Id;
            }
            else if (card.Type == "Minion")
            {
                LogJson.UsedCard.Add(String.Format("{0}@#{1}@#{2}@#{3}@#{4}@#{5}", LogJson.TurnCount, card.Id, card.Name, card.Type, card.Attack, card.Health));
            }
            else if (card.Type == "Spell")  // triple
            {
                LogJson.UsedCard.Add(String.Format("{0}@#{1}@#{2}@#{3}@#{4}@#{5}", LogJson.TurnCount, card.Id, card.Name, card.Type, -1, -1));
            }
        }

        private void OnPlayerHeroPower()
        {
            LogJson.UsedCard.Add(String.Format("{0}@#{1}@#{2}@#{3}@#{4}@#{5}", LogJson.TurnCount, "HeroPower", "HeroPower", "HeroPower", 0, 0));
        }

        private void OnPlayerCreateInPlay(Card card)
        {
            if (card.Type == "Game_Mode_Button")
            {
                //  level up, freeze, refresh
                LogJson.UsedCard.Add(String.Format("{0}@#{1}@#{2}@#{3}@#{4}@#{5}", LogJson.TurnCount, card.Id, card.Name, card.Type, card.Attack, card.Health));
            }
        }

        public void OnLoad()
        {
            LogEvents.OnPowerLogLine.Add(DoInPowerLog);

            GameEvents.OnGameStart.Add(OnGameStart);
            GameEvents.OnTurnStart.Add(OnTurnStart);
            GameEvents.OnPlayerHeroPower.Add(OnPlayerHeroPower);

            if (TurnState == ActivePlayer.Player)
            {
                GameEvents.OnPlayerPlay.Add(OnPlayerPlay);
                GameEvents.OnPlayerCreateInPlay.Add(OnPlayerCreateInPlay);
            }

            GameEvents.OnGameWon.Add(OnGameWon);
            GameEvents.OnGameLost.Add(OnGameLost);
            GameEvents.OnGameEnd.Add(OnGameEnd);
        }
        public void OnButtonPress() { }
        public void OnUnload() { }
        public void OnUpdate() { }

        public string ApiServer = "http://battlegroundlab.com/api/set_log.php";
        public string Name => "Save Battleground Log";
        public string Description => "Upload Battleground Log to <Battleground-Lab Server>";
        public string ButtonText => "DO NOT PUSH THIS BUTTON!";
        public string Author => "shyuniz";
        public Version Version => new Version(1, 0, 0);
        public MenuItem MenuItem => null;
    }
}
