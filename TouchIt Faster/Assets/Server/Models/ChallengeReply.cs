using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstServ.Models.TouchItFaster
{
    public enum ChallengeReplyType
    {
        ChallengeRefused,
        ChallengeAccepted,
        GameOver
    }

    public class ChallengeReply
    {
        public int ChallengeID;
        public ChallengeReplyType Reply;
    }
}
