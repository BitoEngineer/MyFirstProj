
using Assets.Scripts.Data;
using Assets.Scripts.Single_Player_Scene.Models;
using Assets.Scripts.Utils;
using Assets.Server.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchManager : MonoBehaviour
{
    //CIRCLES
    public GameObject Circle_Black_GO;
    public GameObject Circle_Red_GO;
    public GameObject Circle_Blue_GO;
    public GameObject AliveCirclesText;
    public float ScaleCircle = 10;
    public Text PointsText;
    public Text HighestPointsText;
    private Dictionary<GameObject, Circle> AliveCircles = new Dictionary<GameObject, Circle>();

    //BOMB
    public GameObject BombExplosion;
    public float ScaleBomb = 10;
    public GameObject BombGO;
    private Dictionary<GameObject, Bomb> AliveBombs = new Dictionary<GameObject, Bomb>();

    //SQUARE
    public GameObject SpecialCircleGO;
    private Dictionary<GameObject, SpecialCircle> AliveSpecialCircles = new Dictionary<GameObject, SpecialCircle>();

    //DATA
    public int Lifes = 3;
    public GameObject Life1;
    public GameObject Die1;
    public GameObject Life2;
    public GameObject Die2;
    public GameObject Life3;
    public GameObject Die3;
    public GameObject GameOverPanel;
    public bool OnPause = false;
    public Canvas canvas;
    public Canvas SpawnerCanvas;
    private RectTransform CanvasRect;
    private RectTransform SpawnerCanvasRect;
    private float X_length;
    private float Y_length;
    private Vector3 stageDimensions;
    public AudioClip BombSound;
    public AudioMixerGroup BombAudioMixer;
    private AudioSource source { get { return GetComponent<AudioSource>(); } }

    public bool AdSeen = false;
    private Vector3 CircleSize, BombSize;

    private readonly float MAX_POINTS_CIRCLE = 15F;
    private readonly float POINTS_SQUARE = 30F;
    private GameObject[] Circles = new GameObject[3];
    public LayerMask mask;

    public GameObject CountDownGO;
    public GameObject TimerCounterGO;

    private Game Game = new Game();
    public Game GetGame() => Game;

    private float LAST_CIRCLE_SPAWNED_AGE_s = 0;
    private float LAST_BOMB_SPAWN_AGE_s = 0f;

    //Time between spawns
    private float CIRCLE_SPAWN_s = 2f;
    private float SPAWN_CIRCLE_DECREMENT_s = 0.1f;
    private float BOMB_SPAWN_s = 2.5f;

    //Minimum time between circle spawns
    private float CIRCLE_SPAWN_LIMIT_s = 0.3f;

    //Lifetime
    private float SPECIAL_CIRCLE_LIFETIME_s = 1f;
    private float CIRCLE_LIFETIME_s = 1.5f;
    private float BOMB_LIFE_TIME_s = 2f;

    //Probability to spawn
    private float SPAWN_BOMB_PROBABILITY = 0.6f;
    private float SPAWN_BOMB_PROBABILITY_INCREMENT = 0.2f;
    private float SPAWN_SPECIAL_CIRCLE_PROBABILITY = 0.3f;

    //Maximum circles in game
    private float MAX_CIRCLES = 10;

    void Start()
    {
        HighestPointsText.GetComponent<Text>().text = PlayerPrefs.GetFloat(PlayerPrefsKeys.SINGLE_HIGHEST_SCORE_KEY).ToString("f0"); //TODO PlayerContainer.Instance.Info?.SinglePlayerHighestScore.ToString() ?? "-";

        Circles[0] = Circle_Black_GO;
        Circles[1] = Circle_Red_GO;
        Circles[2] = Circle_Blue_GO;
        CountDownGO.GetComponent<CountDown>().StartCountDown(false);
        gameObject.AddComponent<AudioSource>();

        CanvasRect = canvas.GetComponent<RectTransform>();
        SpawnerCanvasRect = SpawnerCanvas.GetComponent<RectTransform>();

        stageDimensions = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        X_length = stageDimensions.x - (Circle_Black_GO.transform.localScale.x);
        Y_length = stageDimensions.y - (Circle_Black_GO.transform.localScale.y * 4);

        CircleSize = new Vector3(CanvasRect.rect.width / 32, CanvasRect.rect.width / 32, 0);
        BombSize = new Vector3(CanvasRect.rect.width / 30, CanvasRect.rect.width / 30, 0);

    }

    void Update()
    {
        if (!OnPause && !CountDownGO.GetComponent<CountDown>().Counting)
        {
            if (CheckGameOver())
            {
                SetAliveCirclesText();
                return;
            }

            float time = Time.time;
            SpawnObjects(time);
            CheckSpecialCirclesToDestroy(time); //TODO BUG- destroying instantly
            CheckBombsToDestroy(time);
            CheckCirclesToDestroy(time);
            SetAliveCirclesText();
        }
    }

    private void SetAliveCirclesText()
    {
        var totalCircles = AliveCircles.Values.Count();
        var aliveCirclesTextComp = AliveCirclesText.GetComponent<Text>();
        aliveCirclesTextComp.text = totalCircles + "";
        if (totalCircles > MAX_CIRCLES * 0.8)
        {
            aliveCirclesTextComp.color = Constants.RED;
        }
        else if (totalCircles >= MAX_CIRCLES * 0.5 && totalCircles <= MAX_CIRCLES * 0.8)
        {
            aliveCirclesTextComp.color = Constants.ORANGE;
        }
        else
        {
            aliveCirclesTextComp.color = Color.black;
        }
    }

    private void OnDestroy()
    {
        Game.End();
    }

    private bool IsToSpawnCircle(float time) => (time - LAST_CIRCLE_SPAWNED_AGE_s) > CIRCLE_SPAWN_s;
    private bool IsToSpawnBomb(float time) => (time - LAST_BOMB_SPAWN_AGE_s) > BOMB_SPAWN_s;

    private void SpawnObjects(float time)
    {
        if (IsToSpawnCircle(time))
        {
            GenerateCircles();
            GenerateSpecialCircle();
        }

        if (IsToSpawnBomb(time))
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

    private void CheckSpecialCirclesToDestroy(float time)
    {
        var circlesToRemove = new List<GameObject>();
        foreach (SpecialCircle s in AliveSpecialCircles.Values)
        {
            if ((time - s.Age_s) > SPECIAL_CIRCLE_LIFETIME_s)
            {
                GameObject specialCircle = s.Special_GO;
                circlesToRemove.Add(specialCircle);
                Destroy(specialCircle);
                GenerateBomb(specialCircle.transform.position); 
            }
        }

        foreach(var specialCircle in circlesToRemove)
            AliveSpecialCircles.Remove(specialCircle);
    }

    private bool IsToGenerateBombWith20Probability() => (Random.Range(0f, 100f) / 100f) <= SPAWN_BOMB_PROBABILITY;
    public void CheckCirclesToDestroy(float time)
    {
        var circlesToRemove = new List<GameObject>();
        foreach (Circle c in AliveCircles.Values)
        {
            if (time - c.Age_s > CIRCLE_LIFETIME_s)
            {
                GameObject circle = c.Circle_Prefab;
                circlesToRemove.Add(circle);
                Destroy(circle);
                GenerateBomb(c.Position);
            }
        }

        foreach(var circle in circlesToRemove)
            AliveCircles.Remove(circle);
    }


    private void CheckBombsToDestroy(float time)
    {
        var bombsToRemove = new List<GameObject>();

        foreach (Bomb b in AliveBombs.Values)
        {
            if (time - b.Age_s > BOMB_LIFE_TIME_s)
            {
                GameObject bomb = b.Bomb_GO;
                bombsToRemove.Add(bomb);
                Destroy(bomb);
            }
        }

        foreach(var bomb in bombsToRemove)
            AliveBombs.Remove(bomb);
    }


    private bool GenerateBomb(Vector3? v = null)
    {
        if (IsToGenerateBombWith20Probability())
        {
            BoxCollider2D bc = BombGO.GetComponent<BoxCollider2D>();
            v = v ?? GetVallidCoords(bc.size.x * 100);
            InstantiateBomb(v.Value);
            return true;
        }
        return false;
    }

    private void InstantiateBomb(Vector3 v)
    {
        Bomb b = new Bomb { Bomb_GO = Instantiate(BombGO, v, Quaternion.identity) as GameObject, Age_s = Time.time };
        b.Bomb_GO.transform.SetParent(SpawnerCanvas.transform, false);
        b.Bomb_GO.transform.localScale = BombSize;
        LAST_BOMB_SPAWN_AGE_s = b.Age_s;
        AliveBombs.Add(b.Bomb_GO, b);
    }

    public void BombTouched(GameObject gameObject)
    {
        Vector3 v = gameObject.transform.position;

        Game.UpdateTapsInRow();

        AliveBombs.Remove(gameObject);
        Destroy(gameObject);
        GameObject anim = Instantiate(BombExplosion, v, Quaternion.identity);
        anim.transform.SetParent(SpawnerCanvasRect);
        Destroy(anim, 0.8f);
            
        SPAWN_BOMB_PROBABILITY += SPAWN_BOMB_PROBABILITY_INCREMENT;

        source.outputAudioMixerGroup = BombAudioMixer;
        source.PlayOneShot(BombSound);
        switch (Lifes--)
        {
            case 3:
                Life1.SetActive(false);
                Die1.SetActive(true);
                break;
            case 2:
                Life2.SetActive(false);
                Die2.SetActive(true);
                break;
            case 1:
                Life3.SetActive(false);
                Die3.SetActive(true);
                GameOver();
                break;
        }
    }

    private bool GenerateSpecialCircle()
    {
        float randomSquare = Random.Range(0f, 100f) / 100f;
        if (randomSquare <= SPAWN_SPECIAL_CIRCLE_PROBABILITY)
        {
            BoxCollider2D bc = SpecialCircleGO.GetComponent<BoxCollider2D>();
            Vector3 v = GetVallidCoords(bc.size.x * 100);
            GameObject square_go = Instantiate(SpecialCircleGO, v, Quaternion.identity) as GameObject;
            square_go.transform.SetParent(SpawnerCanvas.transform, false);
            SpecialCircle s = new SpecialCircle { Special_GO = square_go, Age_s = Time.time };
            
            Vector3 scaled = s.Special_GO.transform.localScale;
            scaled.x = CircleSize.x * 0.7f;
            scaled.y = CircleSize.y * 0.7f;
            s.Special_GO.transform.localScale = scaled;

            AliveSpecialCircles.Add(square_go, s);
            return true;
        }

        return false;
    }

    public void SquareTouched(GameObject go)
    {
        Vector3 v = WorldToCanvasCoords(go.transform.position, SpawnerCanvasRect);
        AliveSpecialCircles.Remove(go);

        Game.IncreasePoints(POINTS_SQUARE);
        PointsText.text = Game.Points.ToString("f0");
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
        LAST_CIRCLE_SPAWNED_AGE_s = Time.time;
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
        v.z = -100;
        return v;
    }

    public void PointsUpdate(GameObject destroyedCircle)
    {
        Game.IncreasePoints(MAX_POINTS_CIRCLE);

        PointsText.text = Game.Points.ToString("f0");
        Destroy(AliveCircles[destroyedCircle].counter);
        AliveCircles.Remove(destroyedCircle);

        if (CIRCLE_SPAWN_s >= CIRCLE_SPAWN_LIMIT_s)
        {
            CIRCLE_SPAWN_s -= SPAWN_CIRCLE_DECREMENT_s;
            SPAWN_CIRCLE_DECREMENT_s = SPAWN_CIRCLE_DECREMENT_s * 0.95f;
        }

    }

    public Vector3 WorldToCanvasCoords(Vector3 v, RectTransform canvas)
    {
        v.x = ((v.x * canvas.sizeDelta.x) / (stageDimensions.x * 2));
        v.y = ((v.y * canvas.sizeDelta.y) / (stageDimensions.y * 2));
        return v;
    }

    private void GameOver()
    {
        Game.End();
        Pause();

        GameOverPanel.SetActive(true);
    }

    public void Resume()
    {
        TimerCounterGO.GetComponent<TimerCounter>().ResumeTimer();
        OnPause = false;
    }

    public void Pause()
    {
        TimerCounterGO.GetComponent<TimerCounter>().StopTimer();
        OnPause = true;
    }

    public void ContinueOnClick()
    {
        if (!AdSeen)
        {
            //AdManager.Instance.showInterstitialAd();
            AdSeen = true;
            GameOverPanel.SetActive(false);

            CleanUpCircles();
            DisableAllLifes();

            OnPause = false;
            Lifes = 3;
            CountDownGO.GetComponent<CountDown>().StartCountDown(false);
        }
        else
        {
            //Generate text saying that AD already was used
        }

    }

    public void DisableAllLifes()
    {
        Life3.SetActive(false);
        Die3.SetActive(true);
        Life2.SetActive(false);
        Die2.SetActive(true);
        Life1.SetActive(false);
        Die1.SetActive(true);
    }

    public void CleanUpCircles()
    {
        foreach (GameObject c in AliveCircles.Keys)
        {
            Destroy(c);
        }

        AliveCircles.Clear();
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void Restart()
    {
        SceneManager.LoadScene("Single player");
    }
}
