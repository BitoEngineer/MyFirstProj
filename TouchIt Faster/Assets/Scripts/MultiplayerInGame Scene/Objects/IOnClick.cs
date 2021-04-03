using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MultiplayerInGame_Scene.Objects
{
    public interface IOnClick
    {
        void PlayOpponentSound();
        void Disable();

        void ShowPoints(int pointsDiff);
    }
}
