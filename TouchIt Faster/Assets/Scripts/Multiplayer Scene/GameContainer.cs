using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Multiplayer_Scene
{
    public static class GameContainer
    {
        public static int CurrentGameId { get; private set; }
        public static long OpponentID { get; private set; }
        public static string OpponentName { get; private set; }
        public static bool HaveStarted => OpponentName != null;

        internal static void StartGame(int challengeID, string opponentName, long opponentID)
        {
            CurrentGameId = challengeID;
            OpponentName = opponentName;
            OpponentID = opponentID;
        }

        internal static void SetChallengeId(int challengeID)
        {
            CurrentGameId = challengeID;
        }

        internal static void SetOpponentId(long opponentID)
        {
            OpponentID = opponentID;
        }

        internal static void SetOpponent(long opponentID, string opponentName)
        {
            OpponentID = opponentID;
            OpponentName = opponentName;
        }
    }
}
