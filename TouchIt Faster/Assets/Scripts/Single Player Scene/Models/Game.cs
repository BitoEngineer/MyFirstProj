using Assets.Scripts.Preloader;
using Assets.Scripts.Utils;
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

        public Game(float points = 0, int tapsInRow = 0)
        {
            Points = points;
            TapsInRow = tapsInRow;

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _highestScore = PlayerPrefs.GetFloat(PlayerPrefsKeys.SINGLE_HIGHEST_SCORE_KEY);
                _maxTapsInRow = PlayerPrefs.GetFloat(PlayerPrefsKeys.SINGLE_MAX_TAPS_KEY);
            });
        }

        public float Points { get; private set; }
        public int TapsInRow { get; private set; }
        public float MaxTapsInRow => _maxTapsInRow;
        private float _maxTapsInRow = -1;

        public float HighestScore => _highestScore;
        private float _highestScore = -1;

        private int _currentTapsInRow = 0;

        public void IncreasePoints(float pointsToAdd)
        {
            Points += pointsToAdd;
            _currentTapsInRow++;
        }

        public void UpdateTapsInRow()
        {
            if(_currentTapsInRow > TapsInRow)
            {
                TapsInRow = _currentTapsInRow;
            }

            _currentTapsInRow = 0;
        }

        public void End()
        {
            SetScoreOnPlayerPrefs();
            SetMaxTapsOnPlayerPrefs();
        }

        private void SetMaxTapsOnPlayerPrefs()
        {
            if (TapsInRow > MaxTapsInRow)
            {
                _maxTapsInRow = TapsInRow;
                UnityMainThreadDispatcher.Instance().Enqueue(() => PlayerPrefs.SetFloat(PlayerPrefsKeys.SINGLE_MAX_TAPS_KEY, TapsInRow));
            }
        }

        private void SetScoreOnPlayerPrefs()
        {
            if(Points > HighestScore)
            {
                _highestScore = Points;
                UnityMainThreadDispatcher.Instance().Enqueue(() => PlayerPrefs.SetFloat(PlayerPrefsKeys.SINGLE_HIGHEST_SCORE_KEY, Points));
            }
        }
    }
}
