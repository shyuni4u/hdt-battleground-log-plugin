using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BGLogPlugin
{
    class BattlegroundPowerLog
    {
        public List<string> GameLog { get; set; }
        private int lineCount;

        private const string tokenBaseTag = "TAG_CHANGE";
        private const string tokenEntityTag = "Entity=";
        private const string tokenTagTag = "tag=";
        private const string tokenValueTag = "value=";

        private const string tokenBaseBlockType = "BlockType=";
        private const string tokenEntityBlockType = "Entity=";
        private const string tokenEffectCardIdBlockType = "EffectCardId=";
        private const string tokenTargetBlockType = "Target=";
        private const string tokenSubOptionBlockType = "SubOption=";

        private const string tokenBaseReload = "SUB_SPELL_START - ";
        private const string tokenSpellPrefabGUIDReload = "SpellPrefabGUID=";
        private const string tokenSourceReload = "Source=";

        private readonly Regex regTag = new Regex(@"- *tag=");

        //private const string tokenBaseChoiceHero = "BlockType=TRIGGER Entity=GameEntity EffectCardId= EffectIndex=11";

        private const string tokenEntityNameBob = "entityName=";
        private const string tokenIdBob = "id=";
        //private const string tokenZoneBob = "zone=";
        //private const string tokenZonePosBob = "zonePos=";
        private const string tokenCardIdBob = "cardId=";
        private const string tokenPlayerBob = "player=";
        //private const string tokenAfterBob = "] EffectCardId=";

        private const string tokenBaseCard = "FULL_ENTITY - Updating [";
        private const string tokenEntityNameCard = "entityName=";
        private const string tokenIdCard = "id=";
        private const string tokenZoneCard = "zone=";
        private const string tokenZonePosCard = "zonePos=";
        private const string tokenCardIdCard = "cardId=";
        private const string tokenPlayerCard = "player=";
        private const string tokenAfterCard = "] CardID=";

        private CardTag dump = null;

        private readonly bool TEST_MODE = false;

        public BattlegroundPowerLog()
        {
            GameLog = new List<string>();
            lineCount = 0;
        }

        public void ParsePowerLog(string line)
        {
            lineCount++;
            //  TAG_CHANGE (WITH TEST)
            if (line.IndexOf(tokenBaseTag) > -1)
            {
                CardCheck();
                string entity = ExportValueInLine(line, tokenEntityTag, tokenTagTag);
                string tag = ExportValueInLine(line, tokenTagTag, tokenValueTag);
                string value = ExportValueInLine(line, tokenValueTag);

                if (!TEST_MODE)
                {
                    if (!Int32.TryParse(tag, out _))
                    {
                        AddLog(String.Format("TAG_TYPE={0}@#{1} TARGET={2}", tag.Trim(), value.Trim(), entity));
                    }
                }
                else
                {
                    if (entity.Trim() == "GameEntity")
                    {
                        if (tag.Trim() == "NUM_TURNS_IN_PLAY")
                        {
                            AddLog(String.Format("  # TurnStart: {0}", Int32.Parse(value) % 2 == 1 ? "Enemy" : "Me"));
                        }
                        else if (tag.Trim() == "BACON_COIN_ON_ENEMY_MINIONS")
                        {
                            AddLog(String.Format("  # {0} Info", Int32.Parse(value) == 0 ? "Enemy" : "Bob"));
                        }
                        else
                        {
                            //addLog(String.Format("   - TAG tag={0}, value={1}", tag, value));
                        }
                    }
                    if (tag.Trim() == "ATK")
                    {
                        AddLog(String.Format("  # ATK {0}, target: {1}", value, entity));
                    }
                    if (tag.Trim() == "HEALTH")
                    {
                        AddLog(String.Format("  # HEALTH {0}, target: {1}", value, entity));
                    }
                }
            }

            //  BLOCK TYPE (WITH TEST)
            else if (line.IndexOf(tokenBaseBlockType) > -1)
            {
                CardCheck();
                string blockType = ExportValueInLine(line, tokenBaseBlockType, tokenEntityBlockType);
                if (!TEST_MODE)
                {
                    string entity = ExportValueInLine(line, tokenEntityBlockType);
                    AddLog(String.Format("BLOCK_TYPE@#{0} TARGET={1}", blockType.Trim(), entity));
                }
                else
                {
                    if (blockType.Trim() == "PLAY")
                    {
                        string entity = ExportValueInLine(line, tokenEntityBlockType, tokenEffectCardIdBlockType);
                        string cardId = ExportValueInLine(entity, tokenCardIdBob, tokenPlayerBob);
                        string target = ExportValueInLine(line, tokenTargetBlockType, tokenSubOptionBlockType);
                        if (cardId.Trim() == "TB_BaconShop_DragBuy")
                        {
                            string targetEntityName = ExportValueInLine(target, tokenEntityNameBob, tokenIdBob);
                            //string targetCardId = exportValueInLine(target, tokenCardIdBob, tokenPlayerBob);
                            AddLog("  # Buy: " + targetEntityName);
                        }
                        if (cardId.Trim() == "TB_BaconShop_DragSell")
                        {
                            string targetEntityName = ExportValueInLine(target, tokenEntityNameBob, tokenIdBob);
                            //string targetCardId = exportValueInLine(target, tokenCardIdBob, tokenPlayerBob);
                            AddLog("  # Sell: " + targetEntityName);
                        }
                        if (cardId.Trim() == "TB_BaconShop_8p_Reroll_Button")
                        {
                            AddLog("  # [RELOAD] by BlockType.TB_BaconShop_8p_Reroll_Button ");
                        }
                    }
                }
            }

            //  CARD
            else if (line.IndexOf(tokenBaseCard) > -1)
            {
                CardCheck();
                string entityName = ExportValueInLine(line, tokenEntityNameCard, tokenIdCard).Trim();
                string zone = ExportValueInLine(line, tokenZoneCard, tokenZonePosCard).Trim();
                string zonePos = ExportValueInLine(line, tokenZonePosCard, tokenCardIdCard).Trim();
                string cardId = ExportValueInLine(line, tokenCardIdCard, tokenPlayerCard).Trim();
                string player = ExportValueInLine(line, tokenPlayerCard, tokenAfterCard).Trim();

                dump = new CardTag
                {
                    entityName = entityName,
                    zone = zone,
                    zonePos = zonePos,
                    cardId = cardId,
                    player = player
                };
            }

            //  SUB SPELL (WITH TEST)
            else if (line.IndexOf(tokenBaseReload) > -1)
            {
                CardCheck();
                string spellPrefabGUID = ExportValueInLine(line, tokenSpellPrefabGUIDReload, tokenSourceReload);
                if (!TEST_MODE)
                {
                    AddLog(String.Format("SUB_SPELL@#{0}", spellPrefabGUID.Trim()));
                }
                else
                {
                    if (spellPrefabGUID.IndexOf("Bacon_MinionSwap_OverrideSpawnIn_Super") > -1)
                    {
                        AddLog("  # [RELOAD] by SUB_SPELL.Bacon_MinionSwap_OverrideSpawnIn_Super ");
                    }
                }
            }

            //  tag
            else if (regTag.IsMatch(line))
            {
                if (dump != null)
                {
                    string tag = ExportValueInLine(line, tokenTagTag, tokenValueTag);
                    string value = ExportValueInLine(line, tokenValueTag);
                    dump.tags.Add(tag.Trim(), value);
                }
            }

            else
            {
                CardCheck();
            }
        }

        private void CardCheck()
        {
            if (dump != null)
            {
                //  card parse
                if (!TEST_MODE)
                {
                    AddLog(String.Format("CARD@#entityName={1} zone={2} zonePos={3} cardId={4} player={5} tags={6}", dump.zone, dump.entityName, dump.zone, dump.zonePos, dump.cardId, dump.player, DictionaryToString(dump.tags)));
                    dump = null;
                }
                else
                {
                    AddLog(String.Format("[{0}] entityName={1} zone={2} zonePos={3} cardId={4} player={5} tags={6}", dump.zone, dump.entityName, dump.zone, dump.zonePos, dump.cardId, dump.player, DictionaryToString(dump.tags)));
                    dump = null;
                }
            }
        }

        private void AddLog(string log)
        {
            if (!TEST_MODE)
            {
                GameLog.Add(log);
            }
            else
            {
                GameLog.Add(String.Format("{0, 6} {1}", lineCount, log));
            }
        }

        private string DictionaryToString(Dictionary<string, string> dictionary)
        {
            string dictionaryString = "{";
            foreach (KeyValuePair<string, string> keyValues in dictionary)
            {
                dictionaryString += keyValues.Key + ":" + keyValues.Value + ",";
            }
            return dictionaryString.TrimEnd(',', ' ') + "}";
        }

        private string ExportValueInLine(string line, string token1, string token2 = null)
        {
            try
            {
                if (token2 == null)
                {
                    return line.Substring(line.IndexOf(token1) + token1.Length, line.Length - line.IndexOf(token1) - token1.Length);
                }
                else
                {
                    return line.Substring(line.IndexOf(token1) + token1.Length, (line.IndexOf(token2) - (line.IndexOf(token1) + token1.Length)));
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return ex.ToString();
            }
        }
    }
}
