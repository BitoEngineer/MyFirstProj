using Assets.Scripts.Preloader;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Server.Models
{
    public class MyPlayer
    {

        private static MyPlayer _instance = null;

        public static MyPlayer Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new MyPlayer();
                }
                return _instance;
            }
        }

        private PlayerInfo _info = null;
        public PlayerInfo Info 
        { 
            get
            {
                if (_info == null)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        var pInfoStr = PlayerPrefs.GetString(PlayerPrefsKeys.PLAYER_INFO_KEY);
                        if (!string.IsNullOrEmpty(pInfoStr))
                        {
                            _info = JsonConvert.DeserializeObject<PlayerInfo>(pInfoStr);
                        }
                        else
                        {
                            //TODO
                        }
                    });

                    Thread.Sleep(10);
                }
                    
                return _info;
            }
        }

        private List<long> friendsIds = null;

        internal void UpdateMultiplayerStats(int multiplayerHighestScore, int maxHitsInRowMultiplayer, int totalWins, int totalLoses)
        {
            Info.Wins = totalWins;
            Info.Loses = totalLoses;
            Info.MultiplayerHighestScore = multiplayerHighestScore;
            Info.MaxHitsInRowMultiplayer = maxHitsInRowMultiplayer;
        }

        internal void SetPlayerInfo(PlayerInfo pInfo)
        {
            _info = pInfo;

            UnityMainThreadDispatcher.Instance().Enqueue(() => PlayerPrefs.SetString(PlayerPrefsKeys.PLAYER_INFO_KEY, JsonConvert.SerializeObject(pInfo)));
        }

        public void SetFriends(List<long> ids)
        {
            friendsIds = ids;
        }

        public bool IsFriendOf(long userId) => friendsIds == null ? false : friendsIds.Contains(userId);

        public void AddFriend(long userId)
        {
            if(friendsIds == null)
            {
                friendsIds = new List<long>() { userId };
            }
            else if(!friendsIds.Contains(userId))
            {
                friendsIds.Add(userId);
            }
        }

        internal void RemoveFriend(long iD)
        {
            if(friendsIds != null)
            {
                friendsIds.Remove(iD);
            }
        }
    }
}
