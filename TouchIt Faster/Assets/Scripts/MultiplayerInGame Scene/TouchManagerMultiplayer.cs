﻿using Assets.Server.Protocol;
using MyFirstServ.Models.TouchItFaster;
using System;
using System.Collections.Generic;
using Assets.Server.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.Multiplayer_Scene;
using Assets.Scripts.Preloader;
using Assets.Scripts.Utils;
using System.Linq;
using Assets.Scripts.MultiplayerInGame_Scene.Objects;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class TouchManagerMultiplayer : MonoBehaviour
{

    public const int BOMB_TIMELIFE_ms = 3000;

    //BOMB
    public GameObject BombGO;

    //SQUARE
    public GameObject SpecialCircleGO;

    //DATA
    public GameObject GameOverPanel;
    public GameObject PlayerLeftPanel;
    public Canvas canvas;
    public Canvas SpawnerCanvas;
    public GameObject OpponentNameText;
    public GameObject OpponentPointsText;
    public GameObject OnTouchOpponentPointsReceivedText;
    private RectTransform CanvasRect;
    private RectTransform SpawnerCanvasRect;

    public GameObject CountDownGO;
    public GameObject TimerCounterGO;

    public AudioClip CircleSound;
    public AudioClip SquareSound;
    public AudioClip BombSound;
    private AudioSource source { get { return GetComponent<AudioSource>(); } }
    private bool AdSeen = false;
    private Vector3 CircleSize, BombSize;

    public GameObject[] Circles = new GameObject[3]; /*Black, Red, Blue*/

    private Dictionary<int, GameObject> AliveObjects = new Dictionary<int, GameObject>();

    private ConcurrentQueue<NewObject> objectsToAdd = new ConcurrentQueue<NewObject>();
    private ConcurrentQueue<int> objectsToDelete = new ConcurrentQueue<int>();
    private bool gameEnded = false;

    public static TouchManagerMultiplayer Instance;

    // Use this for initialization
    void Start()
    {
        Instance = this;

        CountDownGO.GetComponent<MultiplayerCountDown>().StartCountDown(true);
        gameObject.AddComponent<AudioSource>();
        source.clip = CircleSound;
        source.playOnAwake = false;

        CanvasRect = canvas.GetComponent<RectTransform>();
        SpawnerCanvasRect = SpawnerCanvas.GetComponent<RectTransform>();

        CircleSize = new Vector3(CanvasRect.rect.width/32, CanvasRect.rect.width / 32, 0);
        BombSize = new Vector3(CanvasRect.rect.width/30, CanvasRect.rect.width / 30, 0);

        ServerManager.Instance.Client.AddCallback(URI.NewObject, OnObjectReceived);
        ServerManager.Instance.Client.AddCallback(URI.DeleteObject, OnObjectDeletion);
        ServerManager.Instance.Client.AddCallback(URI.PlayerLeft, OnPlayerLeft);
        ServerManager.Instance.Client.AddCallback(URI.GameOver, OnGameOver);
    }

    private bool _isOpponentNameFilled = false;
    void Update()
    {
        if(!_isOpponentNameFilled && GameContainer.HaveStarted)
        {
            OpponentNameText.GetComponent<Text>().text = GameContainer.OpponentName;
            _isOpponentNameFilled = false;
        }

        while (objectsToAdd.Count > 0)
        {
            if(objectsToAdd.TryDequeue(out NewObject obj))
                SpawnObject(obj);
        }

        while (objectsToDelete.Count > 0)
        {
            if (objectsToDelete.TryDequeue(out int id))
                DeleteById(id);
        }

        var bombsToDestroy = _bombsAlive.Where(bomb => BombIsAliveForMoreThan2Seconds(bomb)).ToList();
        foreach (var bomb in bombsToDestroy)
        {
            DeleteById(bomb.Id, true);
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void OnApplicationQuit()
    {
        var dto = new PlayerLeftGame() { Reason = "Arrrsh, that coward left the game", ChallengeID = GameContainer.CurrentGameId, OpponentID = GameContainer.OpponentID };
        ServerManager.Instance.Client.Send(URI.PlayerLeft, dto);
    }

    bool BombIsAliveForMoreThan2Seconds(BombSpawned bomb) => (DateTime.UtcNow - bomb.BornAt).TotalMilliseconds >= BOMB_TIMELIFE_ms;
    void OnObjectDeletion(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var obj = p.DeserializeContent<OnDeletedObject>();
            objectsToDelete.Enqueue(obj.ObjectID);
            UpdateOpponentPointsOnUI(obj);
        }
    }

    private void UpdateOpponentPointsOnUI(OnDeletedObject obj)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            var opponentPointsText = OpponentPointsText.GetComponent<Text>();

            var pointsDifference = obj.CurrentPoints - int.Parse(opponentPointsText.text);
            string pointsText = "";
            var pointsReceivedText = OnTouchOpponentPointsReceivedText.GetComponent<Text>();
            if (pointsDifference > 0)
            {
                pointsText = "+" + pointsDifference;
                pointsReceivedText.color = Color.green;
            }
            else
            {
                pointsText = "" + pointsDifference;
                pointsReceivedText.color = Color.red;
            }

            StartCoroutine(UIUtils.ShowMessageInText(pointsText, 1f, pointsReceivedText));
            opponentPointsText.text = "" + obj.CurrentPoints;
        });
    }

    void OnObjectReceived(JsonPacket p)
    {
        if(p.ReplyStatus == ReplyStatus.OK)
        {
            if (!MultiplayerTimerCounter.Instance.IsActive() && !gameEnded)
            {
                MultiplayerTimerCounter.Instance.ResumeTimer();
            }
            var obj = p.DeserializeContent<NewObject>();

            objectsToAdd.Enqueue(obj);
        }
    }

    public void DeleteById(int id, bool setInactive=false, bool destoy = true)
    {
        GameObject go = null;
        AliveObjects.TryGetValue(id, out go);
        if (go == null)
        {
            Debug.Log("ID of the object must be wrong, it doesn't exists");
        }

        _bombsAlive.RemoveAll(p => p.Id == id);
        AliveObjects.Remove(id);

        if (setInactive)
        {
            go.SetActive(false);
        }
        else if(destoy && go != null)
        {
            var onClick = go.GetComponent<IOnClick>();
            if(onClick != null)
            {
                onClick.PlayOpponentSound();
                onClick.Disable();

                Task.Delay(5000).ContinueWith(t =>
                {
                    Destroy(go);
                });
            }
            else
            {
                Destroy(go);
            }
        }
    }

    private void SpawnObject(NewObject obj)
    {
        Vector3 v = transform.position;
        v.x = obj.X * SpawnerCanvasRect.rect.width;
        v.y = obj.Y * SpawnerCanvasRect.rect.height * -2;
        v.z = -100;
        switch (obj.Type)
        {
            case ObjectType.Circle1:
                SpawnCircle(v, 0, obj.ID);
                break;
            case ObjectType.Circle2:
                SpawnCircle(v, 1, obj.ID);
                break;
            case ObjectType.Circle3:
                SpawnCircle(v, 2, obj.ID);
                break;
            case ObjectType.Special:
                SpawnSpecial(v, obj.ID);
                break;
            case ObjectType.Bomb:
                SpawnBomb(v, obj.ID);
                break;
            default:
                Console.WriteLine("Multiplayer ERROR: Object doesn't exists");
                break;
        }
    }

    private class BombSpawned 
    { 
        public int Id { get; set; } 
        public DateTime BornAt { get; set; } 
    }
    private List<BombSpawned> _bombsAlive = new List<BombSpawned>();
    private void SpawnBomb(Vector3 v, int id)
    {
        Bomb b = new Bomb { Bomb_GO = Instantiate(BombGO, v, Quaternion.identity) as GameObject, Age_s = Time.time };
        b.Bomb_GO.transform.SetParent(SpawnerCanvas.transform, false);
        b.Bomb_GO.transform.localScale = BombSize;
        b.Bomb_GO.GetComponent<BombMultiplayer>().Id = id;

        AliveObjects.Add(id, b.Bomb_GO);
        _bombsAlive.Add(new BombSpawned() { Id = id, BornAt = DateTime.UtcNow });
    }

    private void SpawnSpecial(Vector3 v, int id)
    {
        GameObject square_go = Instantiate(SpecialCircleGO, v, Quaternion.identity) as GameObject;
        square_go.transform.SetParent(SpawnerCanvas.transform, false);
        SpecialCircle s = new SpecialCircle { Special_GO = square_go, Age_s = Time.time };
        Vector3 scaled = s.Special_GO.transform.localScale;
        scaled.x = CircleSize.x * 0.7f;
        scaled.y = CircleSize.y * 0.7f;
        s.Special_GO.transform.localScale = scaled;
        square_go.GetComponent<SpecialCircleMultiplayer>().Id = id;

        AliveObjects.Add(id, square_go);
    }

    private void SpawnCircle(Vector3 v, int index, int id)
    {
        Circle add = new Circle
        {
            Circle_Prefab = Instantiate(Circles[index], v, Quaternion.identity) as GameObject,
            Age_s = Time.time,
            counter = null
        };
        add.Circle_Prefab.transform.SetParent(SpawnerCanvas.transform, false);
        add.Circle_Prefab.transform.localScale = CircleSize;
        add.Circle_Prefab.GetComponent<CircleMultiplayer>().Id = id;

        AliveObjects.Add(id, add.Circle_Prefab);
    }

    private void OnGameOver(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var gameOverObj = p.DeserializeContent<GameOverDTO>();

            MyPlayer.Instance.UpdateMultiplayerStats(gameOverObj.MultiplayerHighestScore, gameOverObj.MaxHitsInRowMultiplayer, gameOverObj.TotalWins, gameOverObj.TotalLoses);
            gameEnded = true;

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                TimerCounterGO.GetComponent<MultiplayerTimerCounter>().StopTimer();
                GameOverPanel.SetActive(true);
                GameOverPanel.GetComponent<GameOverUIController>().GameOverUpdate(gameOverObj);
            });
        }
    }

    private void OnPlayerLeft(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var dto = p.DeserializeContent<PlayerLeftGame>();
            var gameOverObj = dto.GameOverDto;

            MyPlayer.Instance.UpdateMultiplayerStats(gameOverObj.MultiplayerHighestScore, gameOverObj.MaxHitsInRowMultiplayer, gameOverObj.TotalWins, gameOverObj.TotalLoses);
            gameEnded = true;

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                TimerCounterGO.GetComponent<MultiplayerTimerCounter>().StopTimer();
                PlayerLeftPanel.SetActive(true);
                PlayerLeftPanel.GetComponent<PlayerLeftUIController>().PlayerLeftUpdate(gameOverObj);
            });
        }
    }

    public void JumpMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
