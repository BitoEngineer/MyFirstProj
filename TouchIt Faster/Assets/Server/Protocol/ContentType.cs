using System;

namespace Assets.Server.Protocol
{
	public enum URI
	{
        Login,
        Request,
        Friends,
        FriendConnectivityChanged,
        ChangeName,
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
        PlayerLeft,
        GetOnlinePlayers,
        GetTopFive,
    }
}

