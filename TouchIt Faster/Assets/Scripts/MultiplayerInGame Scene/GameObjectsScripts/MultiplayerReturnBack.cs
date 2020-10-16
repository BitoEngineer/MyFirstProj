using Assets.Scripts.Multiplayer_Scene;
using Assets.Scripts.Preloader;
using Assets.Server.Models;
using Assets.Server.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerReturnBack : MonoBehaviour {

    public GameObject SpawnerCanvas;
    public GameObject ExitPopup;
    private TouchManagerMultiplayer touchM;
    public GameObject TimerCounterGO;

    private void Start()
    {
        touchM = GameObject.Find("Touch").GetComponent<TouchManagerMultiplayer>();

    }


    public void OnMouseDown()
    {
        ExitPopup.SetActive(true);
        SpawnerCanvas.SetActive(false);
    }

    public void Leave()
    {
        var dto = new PlayerLeftGame() { Reason = "Arrrsh, that coward left the game", ChallengeID = GameContainer.CurrentGameId, OpponentID = GameContainer.OpponentID };
        ServerManager.Instance.Client.Send(URI.PlayerLeft, dto, (p) => 
        {
            var gameOverObj = p.DeserializeContent<GameOverDTO>();
            MyPlayer.Instance.UpdateMultiplayerStats(gameOverObj.MultiplayerHighestScore, gameOverObj.MaxHitsInRowMultiplayer, gameOverObj.TotalWins, gameOverObj.TotalLoses);

            UnityMainThreadDispatcher.Instance().Enqueue(() => SceneManager.LoadScene("Multiplayer"));
        });
    }

    public void Resume()
    {
        SpawnerCanvas.SetActive(true);
        ExitPopup.SetActive(false);
        //touchM.OnPause = false;

    }
}
