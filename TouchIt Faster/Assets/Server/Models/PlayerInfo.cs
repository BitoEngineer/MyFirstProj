﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Server.Models
{
    public class PlayerInfo
    {
        public long ID;
        public string ClientID;
        public string PlayerName;
        public PlayerState CurrentState;

        public int SinglePlayerHighestScore;
        public int MaxHitsInRowSinglPlayer;

        public int MultiplayerHighestScore;
        public int Wins;
        public int Loses;
        public int MaxHitsInRowMultiplayer;

        public int BetterThan;
    }
}
