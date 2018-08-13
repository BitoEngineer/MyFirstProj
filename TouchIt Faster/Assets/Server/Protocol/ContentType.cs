using System;

namespace Assets.Server.Protocol
{
	public enum URI
	{
        Handshake,
        CreateUser,
        Request,
        Friends,
        ChallengeRequest,
        ChallengeReply,
        ChallengeUpdate,
        NewObject,
        DeleteObject,
        SearchPlayer,
        Start
    }
}

