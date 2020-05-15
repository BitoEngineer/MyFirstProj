using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Server.Models;

namespace Assets.Scripts.MultiplayerInGame_Scene
{
    public class PlayerInGameContainer
    {
        private int _CurrentPoints = 0;

        public int CurrentPoints
        {
            get { return _CurrentPoints; }
            set
            {
                _CurrentPoints = value;
                PlayerStatsUIController.Instance.UpdatePoints(_CurrentPoints);
            }
        }

        private float _Time;
        public float Time
        {
            get { return _Time; }
            set
            {
                _Time = value;
                MultiplayerTimerCounter.Instance.UpdateTimer(_Time);
            }
        }

        private float _ExtraTime;
        public float ExtraTime
        {
            get { return _ExtraTime; }
            set
            {
                _ExtraTime = value;
                if (_ExtraTime != 0)
                {
                    MultiplayerTimerCounter.Instance.ExtraTime(_ExtraTime);
                }
            }
        }

        public int MaxTapsInARow;
        public int CurrTapsInARow;


        private static PlayerInGameContainer _instance = null;

        public static PlayerInGameContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerInGameContainer();
                }
                return _instance;
            }
        }

        public void UpdateGameStats(OnDeletedObject deletedObj)
        {
            CurrentPoints = deletedObj.CurrentPoints;
            MaxTapsInARow = deletedObj.MaxTapsInARow;
            CurrTapsInARow = deletedObj.CurrTapsInARow;
            Time = deletedObj.Time;
            ExtraTime = deletedObj.ExtraTime;
        }
    }
}
