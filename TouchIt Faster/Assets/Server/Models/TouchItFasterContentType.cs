using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Server.Models
{
    public enum TouchItFasterContentType
    {
        Handshake = 1,
        PlayerInfo = 2,
        Request = 3,
        Friends = 4,
        ChallengeRequest=5,
        ChallengeReply = 6,
        ChallengeUpdate = 7,
        NewObject = 8,
        DeleteObject = 9

    }
}
