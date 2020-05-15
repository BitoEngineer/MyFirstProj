using System;

namespace Assets.Server.Protocol
{
	public enum URI
	{
        Handshake,
        Login,
        Request,
        Friends,
        ChallengeRequest,
        ChallengeReply,
        ChallengeUpdate,
        NewObject,
        DeleteObject,
        SearchPlayer,
        Start,
        AddFriend,
	    Unfriend,
	    RandomChallengeRequest,
	    GameOver,
	    Revenge,
        PlayerLeft
    }
}

