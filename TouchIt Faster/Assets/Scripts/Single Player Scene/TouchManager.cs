
using Assets.Scripts.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchManager : MonoBehaviour
{
    //CIRCLES
    public GameObject Circle_Black_GO;
    public GameObject Circle_Red_GO;
    public GameObject Circle_Blue_GO;
    public float ScaleCircle = 10;
    public Text PointsText;
    private Dictionary<GameObject, Circle> AliveCircles = new Dictionary<GameObject, Circle>();
    private float LastCircleSpawnAge_s = 0;
    public float CIRCLE_SPAWN_s = 2f;
    public float CIRCLE_SPAWN_LIMIT_s = 2f;
    public float DEC_CIRCLE_SPAWN_s = 0.2f;
    public float MAX_CIRCLES = 5;
    public float CIRCLE_LIFETIME_s = 2.0f;

    //BOMB
    public GameObject BombExplosion;
    public float ScaleBomb = 10;
    public GameObject BombGO;
    public float Bomb_Spawn_s = 1f;
    public float HitBombDamage_percentage = 0.2f;
    public float BombLifeTime_s = 5f;
    public float Prob_Bomb = 0.2f;
    public float Bomb_Spawn_Inc_probability = 0.01f;
    public float MAX_BOMB_SPAWN_probability = 0.5f;
    private float LastBombSpawn_s = 0f;
    private Dictionary<GameObject, Bomb> AliveBombs = new Dictionary<GameObject, Bomb>();

    //SQUARE
    public GameObject SpecialCircleGO;
    public float SpecialCircleLifeTime_s = 3f;
    public float Prob_SpecialCircle = 0.5f;
    private Dictionary<GameObject, Square> AliveSpecialCircles = new Dictionary<GameObject, Square>();
    List<TextTimer> TextDestroyedSpecialCircles = new List<TextTimer>();

    //DATA
    private int Lifes = 3;
    public Image Life1;
    public Image Life2;
    public Image Life3;
    public GameObject GameOverPanel;
    public Text GameOverPointsText;
    public bool OnPause = false;
    public Canvas canvas;
    public Canvas SpawnerCanvas;
    private RectTransform CanvasRect;
    private RectTransform SpawnerCanvasRect;
    public Text SpecialCircleHittedText;
    public float SpecialCircleTextLifeTime_s = 1f;
    private float X_length;
    private float Y_length;
    private Vector3 stageDimensions;
    private float Points;
    public AudioClip CircleSound;
    public AudioClip SquareSound;
    public AudioClip BombSound;
    private AudioSource source { get { return GetComponent<AudioSource>(); } }

    private bool AdSeen = false;
    private Vector3 CircleSize, BombSize;

    private readonly float MAX_POINTS_CIRCLE = 15F;
    private readonly float POINTS_SQUARE = 30F;
    private GameObject[] Circles = new GameObject[3];
    public LayerMask mask;



    // Use this for initialization
    void Start()
    {
        Circles[0] = Circle_Black_GO;
        Circles[1] = Circle_Red_GO;
        Circles[2] = Circle_Blue_GO;
        CountDown.Instance.StartCountDown(false);
        gameObject.AddComponent<AudioSource>();
        source.clip = CircleSound;
        source.playOnAwake = false;

        CanvasRect = canvas.GetComponent<RectTransform>();
        SpawnerCanvasRect = SpawnerCanvas.GetComponent<RectTransform>();

        Points = 0;
      
        stageDimensions = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        X_length = stageDimensions.x - (Circle_Black_GO.transform.localScale.x);
        Y_length = stageDimensions.y - (Circle_Black_GO.transform.localScale.y * 4 );

        CircleSize = new Vector3(CanvasRect.rect.width/32, CanvasRect.rect.width / 32, 0);
        BombSize = new Vector3(CanvasRect.rect.width/30, CanvasRect.rect.width / 30, 0);
        
    }



    // Update is called once per frame
    void Update()
    {
        if (!OnPause && !CountDown.Instance.Counting)
        {
            if(CheckGameOver()) return;

            float time = Time.time;
            SpawnObjects(time);
            CheckSquaresToDestroy(time);
            CheckBombsToDestroy(time);
            CheckCirclesToDestroy(time);
        }
        
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat("points", Points);
    }

    private void SpawnObjects(float time)
    {
        if (time - LastCircleSpawnAge_s > CIRCLE_SPAWN_s)
        {
            GenerateCircles();
            GenerateSquare();
        }
        if(time - LastBombSpawn_s > Bomb_Spawn_s)
        {
            GenerateBomb();
        }
    }

    private bool CheckGameOver()
    {
        if (AliveCircles.Count > MAX_CIRCLES)
        {
            GameOver();
            return true;
        }
        return false;
    }

    private void CheckSquaresToDestroy(float time){
        foreach (Square s in AliveSpecialCircles.Values)
        {
            if (time - s.Age_s > SpecialCircleLifeTime_s)
            {
                GameObject square = s.Square_GO;
                AliveBombs.Remove(square);
                Destroy(square);
            }
        }
        foreach (TextTimer t in TextDestroyedSpecialCircles)
        {
            if (time - t.Age_s > SpecialCircleTextLifeTime_s)
            {
                TextDestroyedSpecialCircles.Remove(t);
                Destroy(t.text);
            }
        }


    }

    public void CheckCirclesToDestroy(float time)
    {
        foreach(Circle c in AliveCircles.Values)
        {
            if(time - c.Age_s > CIRCLE_LIFETIME_s)
            {
                GameObject circle = c.Circle_Prefab;
                AliveCircles.Remove(circle);
                Destroy(circle);           
                GenerateBomb(c.Position);
            }
        }
    }


    private void CheckBombsToDestroy(float time)
    {
        foreach (Bomb b in AliveBombs.Values)
        {
            if (time - b.Age_s > BombLifeTime_s)
            {
                GameObject bomb = b.Bomb_GO;
                AliveBombs.Remove(bomb);
                Destroy(bomb);
            }
        }
    }


    private bool GenerateBomb()
    {
        float randomBomb = Random.Range(0f, 100f) / 100f;
        if (randomBomb <= Prob_Bomb)
        {
            BoxCollider2D bc = BombGO.GetComponent<BoxCollider2D>();
            Vector3 v = GetVallidCoords(bc.size.x*100);
            GenerateBomb(v);
            return true;
        }
        return false;
    }

    private void GenerateBomb(Vector3 v)
    {
        Bomb b = new Bomb { Bomb_GO = Instantiate(BombGO, v, Quaternion.identity) as GameObject, Age_s = Time.time };
        b.Bomb_GO.transform.SetParent(SpawnerCanvas.transform, false);
        b.Bomb_GO.transform.localScale = BombSize;
        LastBombSpawn_s = b.Age_s;
        AliveBombs.Add(b.Bomb_GO, b);
    }

    public void BombTouched(GameObject gameObject)
    {
        Vector3 v = gameObject.transform.position;
        Points = Points * (1 - HitBombDamage_percentage);
        PointsText.text = Points.ToString("f0");
        AliveBombs.Remove(gameObject);
        Destroy(gameObject);
        GameObject anim = Instantiate(BombExplosion, v, Quaternion.identity);
        anim.transform.SetParent(SpawnerCanvasRect);
        Destroy(anim, 0.8f);

        if (Prob_Bomb < MAX_BOMB_SPAWN_probability)
            Prob_Bomb += Bomb_Spawn_Inc_probability;
        source.PlayOneShot(BombSound);
        switch (Lifes--)
        {
            case 3:
                Life1.enabled = true;
                break;
            case 2:
                Life2.enabled = true;
                break;
            case 1:
                Life3.enabled = true;
                GameOver();
                break;
        }


    }

    private bool GenerateSquare()
    {
        float randomSquare = Random.Range(0f, 100f) / 100f;
        if (randomSquare <= Prob_SpecialCircle)
        {
            BoxCollider2D bc = SpecialCircleGO.GetComponent<BoxCollider2D>();
            Vector3 v = GetVallidCoords(bc.size.x*100);
            GameObject square_go = Instantiate(SpecialCircleGO, v, Quaternion.identity) as GameObject;
            square_go.transform.SetParent(SpawnerCanvas.transform, false);
            Square s = new Square { Square_GO = square_go, Age_s = Time.time };
            Vector3 scaled = s.Square_GO.transform.localScale;
            scaled.x = CircleSize.x /2;
            scaled.y = CircleSize.y / 2;
            s.Square_GO.transform.localScale = scaled;
            AliveSpecialCircles.Add(square_go, s);
            return true;
        }
        return false;
    }

    public void SquareTouched(GameObject go)
    {
        source.PlayOneShot(SquareSound);
        Vector3 v = WorldToCanvasCoords(go.transform.position, SpawnerCanvasRect);
        AliveSpecialCircles.Remove(go);
        Text t = Instantiate(SpecialCircleHittedText, v, Quaternion.identity) as Text;
        t.transform.SetParent(canvas.transform, false);
        TextDestroyedSpecialCircles.Add(new TextTimer { text = t, Age_s = Time.time });

        Points += POINTS_SQUARE;
        PointsText.text = Points.ToString("f0");
    }


    private void GenerateCircles()
    {
        int index = Random.Range(0, 3);
        GameObject circle = Circles[index];
        BoxCollider2D b = circle.GetComponent<BoxCollider2D>();
        Vector3 v = GetVallidCoords(b.size.x);
        Circle add = new Circle
        {
            Circle_Prefab = Instantiate(circle, v, Quaternion.identity) as GameObject,
            Age_s = Time.time,
            counter = null,
            Position = v
        };
        add.Circle_Prefab.transform.SetParent(SpawnerCanvas.transform, false);
        add.Circle_Prefab.transform.localScale = CircleSize;

        AliveCircles.Add(add.Circle_Prefab, add);
        LastCircleSpawnAge_s = Time.time;
        
    }

    private Vector3 GetVallidCoords(float halfWidth)
    {
        Vector3 v = SpawnerCanvasRect.position;
        bool valid = false;
        while (!valid)
        {
            v.x += Random.Range(-SpawnerCanvasRect.rect.width / 2, SpawnerCanvasRect.rect.width / 2);
            v.y += Random.Range(-SpawnerCanvasRect.rect.height / 2, SpawnerCanvasRect.rect.height / 2);
            valid = Physics2D.OverlapCircle(v, halfWidth * 0.0092f) == null;
        }

        return v;
    }

    public void PointsUpdate(GameObject destroyedCircle)
    {
        Points += MAX_POINTS_CIRCLE;

        source.PlayOneShot(CircleSound);

        PointsText.text = Points.ToString("f0");
        Destroy(AliveCircles[destroyedCircle].counter);
        AliveCircles.Remove(destroyedCircle);

        if(CIRCLE_SPAWN_s > CIRCLE_SPAWN_LIMIT_s)
        {
            CIRCLE_SPAWN_s -= DEC_CIRCLE_SPAWN_s;
            DEC_CIRCLE_SPAWN_s -= DEC_CIRCLE_SPAWN_s/20;
        }
        
    }

    public Vector3 WorldToCanvasCoords(Vector3 v, RectTransform canvas)
    {
        v.x= ((v.x * canvas.sizeDelta.x) / (stageDimensions.x * 2));
        v.y= ((v.y * canvas.sizeDelta.y) / (stageDimensions.y * 2));
        return v;
    }

    private void GameOver()
    {
        TimerCounter.Instance.StopTimer();
        OnPause = true;
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

    public void ContinueOnClick()
    {
        if (!AdSeen)
        {
            //AdManager.Instance.showInterstitialAd();
            AdSeen = true;
            GameOverPanel.SetActive(false);

            CleanUpCircles();
            Life1.enabled = false;
            Life2.enabled = false;
            Life3.enabled = false;
            OnPause = false;
            Lifes = 3;
            CountDown.Instance.StartCountDown(false);
        }
        else
        {
            //Generate text saying that AD already was used
        }
        
    }

    private void CleanUpCircles()
    {
        foreach(GameObject c in AliveCircles.Keys)
        {
            Destroy(c);
        }

        AliveCircles.Clear();
    }
}
