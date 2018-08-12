using Assets.Server.Protocol;
using MyFirstServ.Models.TouchItFaster;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
    private float Points;
    public AudioClip CircleSound;
    public AudioClip SquareSound;
    public AudioClip BombSound;
    private AudioSource source { get { return GetComponent<AudioSource>(); } }
    public Text CountDown;
    public CountDown CD;
    private bool AdSeen = false;
    private Vector3 CircleSize, BombSize;

    private readonly float MAX_POINTS_CIRCLE = 15F;
    private readonly float POINTS_SQUARE = 30F;
    public GameObject[] Circles = new GameObject[3]; /*Black, Red, Blue*/

    private Dictionary<int, GameObject> AliveObjects = new Dictionary<int, GameObject>();

    public static TouchManagerMultiplayer Instance;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        CD = CountDown.GetComponent<CountDown>();
        CD.StartCountDown();
        gameObject.AddComponent<AudioSource>();
        source.clip = CircleSound;
        source.playOnAwake = false;

        CanvasRect = canvas.GetComponent<RectTransform>();
        SpawnerCanvasRect = SpawnerCanvas.GetComponent<RectTransform>();

        Points = 0;

        CircleSize = new Vector3(CanvasRect.rect.width/32, CanvasRect.rect.width / 32, 0);
        BombSize = new Vector3(CanvasRect.rect.width/30, CanvasRect.rect.width / 30, 0);

        ServerManager.Instance.Client.AddCallback(URI.NewObject, OnObjectReceived);
        ServerManager.Instance.Client.AddCallback(URI.DeleteObject, OnObjectDeletion);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnObjectDeletion(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var obj = p.DeserializeContent<DeleteObject>();
            DeleteById(obj.ID);
        }
    }

    void OnObjectReceived(JsonPacket p)
    {
        if(p.ReplyStatus == ReplyStatus.OK)
        {
            var obj = p.DeserializeContent<NewObject>();
            SpawnObject(obj);
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat("points", Points);
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
        v.x = obj.X;
        v.y = obj.Y;
        switch (obj.Type)
        {
            case ObjectType.Circle1:
                SpawnCircle(v, 1, obj.ID);
                break;
            case ObjectType.Circle2:
                SpawnCircle(v, 2, obj.ID);
                break;
            case ObjectType.Circle3:
                SpawnCircle(v, 3, obj.ID);
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
        square_go.GetComponent<SpecialMultiplayer>().Id = id;

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


    private void GameOver()
    {
        TimerCounter.Instance.StopTimer();
        GameOverPanel.SetActive(true);
        GameOverPointsText.text = "Points: "+Points.ToString("f0");

    }

    public void Restart()
    {
        SceneManager.LoadScene("Single Player");
    }

    public void JumpMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
