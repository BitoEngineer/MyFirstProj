using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Server.Models
{
    public class TopFiveDto
    {
        public PlayerInfo[] TopFive { get; set; }
        public int PlayerRank { get; set; }
        public long[] FriendsIds { get; set; }
    }
}
