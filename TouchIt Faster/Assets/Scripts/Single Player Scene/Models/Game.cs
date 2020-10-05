using Assets.Scripts.Preloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Single_Player_Scene.Models
{
    public class Game
    {
        private const string HIGHEST_SCORE_KEY = "highest_score";

        public Game(float points = 0, int tapsInRow = 0)
        {
            Points = points;
            TapsInRow = tapsInRow;

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _highestScore = PlayerPrefs.GetFloat(HIGHEST_SCORE_KEY);
            });
        }

        public float Points { get; private set; }
        public int TapsInRow { get; private set; }
        public float HighestScore => _highestScore;
        private float _highestScore = -1;

        private int _currentTapsInRow = 0;

        public void IncreasePoints(float pointsToAdd)
        {
            Points += pointsToAdd;
            _currentTapsInRow++;
        }

        public void ResetTapsInRow()
        {
            if(_currentTapsInRow > TapsInRow)
            {
                TapsInRow = _currentTapsInRow;
            }

            _currentTapsInRow = 0;
        }

        public void SetScoreOnPlayerPrefs()
        {
            if(Points > HighestScore)
            {
                _highestScore = Points;
                UnityMainThreadDispatcher.Instance().Enqueue(() => PlayerPrefs.SetFloat(HIGHEST_SCORE_KEY, Points));
            }
        }
    }
}
