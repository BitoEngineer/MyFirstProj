using Assets.Server.Protocol;
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

public class TouchManagerMultiplayer : MonoBehaviour
{

    //BOMB
    public GameObject BombExplosion;
    public GameObject BombGO;

    //SQUARE
    public GameObject SpecialCircleGO;

    //DATA
    public GameObject GameOverPanel;
    public Text GameOverPointsText;
    public Canvas canvas;
    public Canvas SpawnerCanvas;
    private RectTransform CanvasRect;
    private RectTransform SpawnerCanvasRect;

    public GameObject CountDownGO;

    public AudioClip CircleSound;
    public AudioClip SquareSound;
    public AudioClip BombSound;
    private AudioSource source { get { return GetComponent<AudioSource>(); } }
    private bool AdSeen = false;
    private Vector3 CircleSize, BombSize;

    public GameObject[] Circles = new GameObject[3]; /*Black, Red, Blue*/

    private Dictionary<int, GameObject> AliveObjects = new Dictionary<int, GameObject>();

    private Queue<NewObject> objectsToAdd = new Queue<NewObject>();
    private Queue<int> objectsToDelete = new Queue<int>();

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

    // Update is called once per frame
    void Update()
    {
        while(objectsToAdd.Count > 0)
        {
            SpawnObject(objectsToAdd.Dequeue());
        }
        while (objectsToDelete.Count > 0)
        {
            DeleteById(objectsToDelete.Dequeue());
        }
    }

    private void OnApplicationQuit()
    {
        var dto = new PlayerLeftGame() { Reason = "Arrrsh, that coward left the game", ChallengeID = GameContainer.CurrentGameId, OpponentID = GameContainer.OpponentID };
        ServerManager.Instance.Client.Send(URI.PlayerLeft, dto);
    }

    void OnObjectDeletion(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var obj = p.DeserializeContent<DeleteObject>();
            objectsToDelete.Enqueue(obj.ObjectID);
        }
    }

    void OnObjectReceived(JsonPacket p)
    {
        if(p.ReplyStatus == ReplyStatus.OK)
        {
            if (!MultiplayerTimerCounter.Instance.IsActive())
            {
                MultiplayerTimerCounter.Instance.ResumeTimer();
            }
            var obj = p.DeserializeContent<NewObject>();
            objectsToAdd.Enqueue(obj);
        }
    }

    public void DeleteById(int id)
    {
        GameObject go = null;
        AliveObjects.TryGetValue(id, out go);
        if (go == null)
        {
            Console.WriteLine("ID of the object must be wrong, it doesn't exists");
            /*TODO*/
        }
        AliveObjects.Remove(id);
        Destroy(go);
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
                /*TODO*/
                break;
        }
    }

    private void SpawnBomb(Vector3 v, int id)
    {
        Bomb b = new Bomb { Bomb_GO = Instantiate(BombGO, v, Quaternion.identity) as GameObject, Age_s = Time.time };
        b.Bomb_GO.transform.SetParent(SpawnerCanvas.transform, false);
        b.Bomb_GO.transform.localScale = BombSize;
        b.Bomb_GO.GetComponent<BombMultiplayer>().Id = id;

        AliveObjects.Add(id, b.Bomb_GO);
    }

    private void SpawnSpecial(Vector3 v, int id)
    {
        GameObject square_go = Instantiate(SpecialCircleGO, v, Quaternion.identity) as GameObject;
        square_go.transform.SetParent(SpawnerCanvas.transform, false);
        Square s = new Square { Square_GO = square_go, Age_s = Time.time };
        Vector3 scaled = s.Square_GO.transform.localScale;
        scaled.x = CircleSize.x / 2;
        scaled.y = CircleSize.y / 2;
        s.Square_GO.transform.localScale = scaled;
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
            GameOver gameOverObj = p.DeserializeContent<GameOver>();
            GameOverUIController.Instance.GameOverUpdate(gameOverObj);
        }
    }

    private void OnPlayerLeft(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var dto = p.DeserializeContent<PlayerLeftGame>();
            //TODO 
            GameOverUIController.Instance.GameOverUpdate(new GameOver()
            {
                OpponentPoints = 0,
                OpponentTapsInARow = 0,
                OpponentTimeLeft = 0,
                OpponentTimePoints = 0,
                Points = 999,
                TapsInARow = 999,
                TimeLeft = 99,
                TimePoints = 99
            });
        }
    }

    public void JumpMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
