using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Server.Models
{
    public class PlayerLeftGame
    {
        public string Reason { get; set; }
        public int ChallengeID { get; set; }
        public long OpponentID { get; set; }

        public GameOverDTO GameOverDto { get; set; }
    }
}
