
using Assets.Scripts.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchManagerMultiplayer : MonoBehaviour
{

    //CIRCLES
    public GameObject Circle_Black_GO;
    public GameObject Circle_Red_GO;
    public GameObject Circle_Blue_GO;

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
    private GameObject[] Circles = new GameObject[3];



    // Use this for initialization
    void Start()
    {
        Circles[0] = Circle_Black_GO;
        Circles[1] = Circle_Red_GO;
        Circles[2] = Circle_Blue_GO;
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
        
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat("points", Points);
    }

    private void GenerateBomb(Vector3 v)
    {
        Bomb b = new Bomb { Bomb_GO = Instantiate(BombGO, v, Quaternion.identity) as GameObject, Age_s = Time.time };
        b.Bomb_GO.transform.SetParent(SpawnerCanvas.transform, false);
        b.Bomb_GO.transform.localScale = BombSize;
    }

    private void GenerateSquare(Vector3 v)
    {
        GameObject square_go = Instantiate(SpecialCircleGO, v, Quaternion.identity) as GameObject;
        square_go.transform.SetParent(SpawnerCanvas.transform, false);
        Square s = new Square { Square_GO = square_go, Age_s = Time.time };
        Vector3 scaled = s.Square_GO.transform.localScale;
        scaled.x = CircleSize.x / 2;
        scaled.y = CircleSize.y / 2;
        s.Square_GO.transform.localScale = scaled;
    }


    private void GenerateCircles(Vector3 v, int index)
    {
        Circle add = new Circle
        {
            Circle_Prefab = Instantiate(Circles[index], v, Quaternion.identity) as GameObject,
            Age_s = Time.time,
            counter = null
        };
        add.Circle_Prefab.transform.SetParent(SpawnerCanvas.transform, false);
        add.Circle_Prefab.transform.localScale = CircleSize;       
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
