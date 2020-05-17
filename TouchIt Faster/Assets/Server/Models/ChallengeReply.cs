using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstServ.Models.TouchItFaster
{
    public enum ChallengeReplyType
    {
        RequestedOffline,
        Waiting,
        ChallengeRefused,
        ChallengeAccepted,
        Start,
        GameOver
    }

    public class ChallengeReply
    {
        public int ChallengeID;
        public long OpponentID;
        public ChallengeReplyType Reply;
        public string OpponentName;
    }
}
