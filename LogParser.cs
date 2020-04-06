using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Web.Script.Serialization;

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

        private SaveState LogState = SaveState.Ready;
        private readonly ParamJson LogJson = new ParamJson();

        private BattlegroundPowerLog bpl = new BattlegroundPowerLog();

        private readonly List<string> ResultLog = new List<string>();

        private void SetInit()
        {
            bpl = new BattlegroundPowerLog();
            LogState = SaveState.Ready;

            LogJson.Version = "1.1.0";
            LogJson.MMR = 0;
            LogJson.PowerLog = new List<string>();

            ResultLog.Clear();
        }

        private void OnGameStart()
        {
            SetInit();
            LogState = SaveState.Used;
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
                if (Core.Game.BattlegroundsRatingInfo != null)
                    LogJson.MMR = HearthMirror.Reflection.GetBattlegroundRatingInfo().Rating;

                LogJson.PowerLog = bpl.GameLog;

                JavaScriptSerializer jSer = new JavaScriptSerializer
                {
                    MaxJsonLength = 2147483647
                };
                string payload = jSer.Serialize(LogJson);
                Task.Run(() => Api.Post(ApiServer, payload)).Wait(500);

                ResultLog.Add(payload);
                //FileHelper.Write(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PowerLog.txt"), ResultLog);
                //FileHelper.Write(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BPL.txt"), ResultLog);
            }
        }

        //  Save placement
        private void DoInPowerLog(string line)
        {
            bpl.ParsePowerLog(line);
        }

        public void OnLoad()
        {
            LogEvents.OnPowerLogLine.Add(DoInPowerLog);

            GameEvents.OnGameStart.Add(OnGameStart);
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
        public Version Version => new Version(1, 1, 0);
        public MenuItem MenuItem => null;
    }
}
