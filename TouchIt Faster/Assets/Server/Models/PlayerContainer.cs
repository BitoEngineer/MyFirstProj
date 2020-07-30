using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Server.Models
{
    public class PlayerContainer
    {
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



        public PlayerInfo Info { get; set; }

        internal void UpdateMultiplayerStats(int multiplayerHighestScore, int maxHitsInRowMultiplayer, int totalWins, int totalLoses)
        {
            Info.Wins = totalWins;
            Info.Loses = totalLoses;
            Info.MultiplayerHighestScore = multiplayerHighestScore;
            Info.MaxHitsInRowMultiplayer = maxHitsInRowMultiplayer;
        }
    }
}
