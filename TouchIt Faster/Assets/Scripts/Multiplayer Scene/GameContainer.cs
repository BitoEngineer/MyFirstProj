using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Multiplayer_Scene
{
    public static class GameContainer
    {
        public static int CurrentGameId { get; set; }
        public static long OpponentID { get; set; }
        public static string OpponentName { get; set; }
        public static bool HaveStarted => OpponentName != null;
    }
}
