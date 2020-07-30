using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Server.Models
{
    public class GameOverDTO
    {
        public int Points;
        public float TimeLeft;
        public int TimePoints;
        public int TapsInARow;

        public int OpponentPoints;
        public int OpponentTapsInARow;
        public float OpponentTimeLeft;
        public int OpponentTimePoints;

        public int MultiplayerHighestScore;
        public int MaxHitsInRowMultiplayer;
        public int TotalWins;
        public int TotalLoses;
    }
}
