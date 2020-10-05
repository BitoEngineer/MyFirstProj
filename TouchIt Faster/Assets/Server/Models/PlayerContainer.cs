using Assets.Scripts.Preloader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Server.Models
{
    public class PlayerContainer
    {
        private const string PLAYER_INFO_KEY = "player_info";

        private static PlayerContainer _instance = null;

        public static PlayerContainer Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerContainer();
                }
                return _instance;
            }
        }

        private PlayerInfo _info = null;
        public PlayerInfo Info 
        { 
            get
            {
                if (_info == null)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        var pInfoStr = PlayerPrefs.GetString(PLAYER_INFO_KEY);
                        if (!string.IsNullOrEmpty(pInfoStr))
                        {
                            _info = JsonConvert.DeserializeObject<PlayerInfo>(pInfoStr);
                        }
                        else
                        {
                            //TODO
                        }
                    });

                    Thread.Sleep(10);
                }
                    
                return _info;
            }
        }

        internal void UpdateMultiplayerStats(int multiplayerHighestScore, int maxHitsInRowMultiplayer, int totalWins, int totalLoses)
        {
            Info.Wins = totalWins;
            Info.Loses = totalLoses;
            Info.MultiplayerHighestScore = multiplayerHighestScore;
            Info.MaxHitsInRowMultiplayer = maxHitsInRowMultiplayer;
        }

        internal void SetPlayerInfo(PlayerInfo pInfo)
        {
            _info = pInfo;

            UnityMainThreadDispatcher.Instance().Enqueue(() => PlayerPrefs.SetString(PLAYER_INFO_KEY, JsonConvert.SerializeObject(pInfo)));
        }
    }
}
