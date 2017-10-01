
using Assets.Scripts.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class TouchManager : MonoBehaviour
{


    //CIRCLES
    public GameObject circle;
    public Text pointsText;
    public Dictionary<GameObject, Circle> aliveCircles = new Dictionary<GameObject, Circle>();
    public float CircleCounter = 0;
    private float lastCircleAge_s = 0;
    private BoxCollider circleCollider;
    public float CIRCLE_SPAWN_s = 2f;
    public float CIRCLE_SPAWN_LIMIT_s = 2f;
    public float DEC_CIRCLE_SPAWN_s = 0.2f;
    public float MAX_CIRCLES = 5;
    private float Circle_X_Width, Circle_Y_Height;
    private RectTransform Circle_Rect;

    //BOMB
    public GameObject BombExplosion;
    private GameObject LastBombExplosion = null;
    public GameObject bomb;
    public Text HitBombText;
    private Renderer BombRenderer;
    public float Bomb_Spawn_s = 1f;
    private float Last_Bomb_Spawn_s = 0;
    public float HitBombDamage_percentage = 0.2f;
    public float bombLifetime_s = 5f;
    public float probability_bomb = 0.2f;
    public float Bomb_Spawn_Inc_probability = 0.01f;
    public float MAX_BOMB_SPAWN_probability = 0.5f;
    public float HitBombText_LifeTime_s = 0.5f;
    private Dictionary<GameObject, Bomb> AliveBombs = new Dictionary<GameObject, Bomb>();
    List<TextTimer> TextBombsTouched = new List<TextTimer>();

    //SQUARE
    public GameObject square;
    public float squareLifetime_s = 3f;
    public float probability_square = 0.5f;
    private Dictionary<GameObject, Square> AliveSquares = new Dictionary<GameObject, Square>();
    List<TextTimer> TextDestroyedSquares = new List<TextTimer>();


    //DATA
    public int Lifes = 3;
    public Image Life1;
    public Image Life2;
    public Image Life3;
    public GameObject GameOverPanel;
    public Text GameOverPointsText;
    public bool OnPause = false;
    public Canvas canvas;
    private RectTransform CanvasRect;
    public Text HitSquare;
    public float SquareTextTime_s = 1f;
    private float X_length;
    private float Y_length;
    private Vector3 stageDimensions;
    private float points;

    private readonly float MAX_POINTS_CIRCLE = 15F;
    private readonly float POINTS_SQUARE = 30F;
    private readonly string[] Bomb_Explode_Message = { "Ooops!", "In your face :D", "WAKE UP!!", "Hiroushima", "Nagasaki" };



    // Use this for initialization
    void Start()
    {
        CanvasRect = canvas.GetComponent<RectTransform>();
        Circle_Rect = circle.GetComponent<RectTransform>();
        points = 0;
        circleCollider = circle.GetComponent<BoxCollider>();

        stageDimensions = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        Circle_X_Width = (circleCollider.size.x * (stageDimensions.x * 2)) / Screen.width;
        Circle_Y_Height = (circleCollider.size.y * (stageDimensions.y * 2)) / Screen.height;
        X_length = stageDimensions.x - (circleCollider.size.x / 1.5f);
        Y_length = stageDimensions.y - (circleCollider.size.y );
    }



    // Update is called once per frame
    void Update()
    {
        if (!OnPause)
        {
            CheckGameOver();

            float time = Time.time;
            SpawnObjects(time);
            CheckSquaresToDestroy(time);
            CheckBombsToDestroy(time);
        }
        
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat("points", points);
    }

    private void SpawnObjects(float time)
    {
        if (time - lastCircleAge_s > CIRCLE_SPAWN_s)
        {
            GenerateCircles();
            GenerateSquare();
            GenerateBomb();
        }
    }

    private void CheckGameOver()
    {
        if (aliveCircles.Count > MAX_CIRCLES)
        {
            GameOver();
        }
    }
    private void CheckSquaresToDestroy(float time){
        foreach (Square s in AliveSquares.Values)
        {
            if (time - s.Age_s > squareLifetime_s)
            {
                GameObject square = s.Square_GO;
                AliveBombs.Remove(square);
                Destroy(square);
            }
        }
        foreach (TextTimer t in TextDestroyedSquares)
        {
            if (time - t.Age_s > SquareTextTime_s)
            {
                TextDestroyedSquares.Remove(t);
                Destroy(t.text);
            }
        }


    }

    private void CheckBombsToDestroy(float time)
    {
        foreach (Bomb b in AliveBombs.Values)
        {
            if (time - b.Age_s > bombLifetime_s)
            {
                GameObject bomb = b.Bomb_GO;
                AliveBombs.Remove(bomb);
                Destroy(bomb);
            }
        }
        foreach (TextTimer t in TextBombsTouched)
        {
            if (time - t.Age_s > HitBombText_LifeTime_s)
            {
                TextBombsTouched.Remove(t);
                Destroy(t.text);
            }
        }
    }


    private bool GenerateBomb()
    {
        if (LastBombExplosion != null)
        {
            Destroy(LastBombExplosion);
            LastBombExplosion = null;
        }

        float randomBomb = Random.Range(0f, 100f) / 100f;
        if (randomBomb <= probability_bomb)
        {
            Vector3 v = GetVallidCoords();
            Bomb b = new Bomb { Bomb_GO = Instantiate(bomb, v, Quaternion.identity) as GameObject, Age_s = Time.time };
            Last_Bomb_Spawn_s = b.Age_s;
            AliveBombs.Add(b.Bomb_GO,b);
            return true;
        }
        return false;
    }

    public void BombTouched(GameObject gameObject)
    {
        switch (Lifes)
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

        Vector3 v = gameObject.transform.position;
        points = points * (1 - HitBombDamage_percentage);
        pointsText.text = points.ToString("f0");
        AliveBombs.Remove(gameObject);
        Destroy(gameObject);

        LastBombExplosion = Instantiate(BombExplosion, v, Quaternion.identity);
        --Lifes;
        if (probability_bomb < MAX_BOMB_SPAWN_probability)
            probability_bomb += Bomb_Spawn_Inc_probability;
    }

    private bool GenerateSquare()
    {
        float randomSquare = Random.Range(0f, 100f) / 100f;
        if (randomSquare <= probability_square)
        {
            Vector3 v = GetVallidCoords();
            GameObject square_go = Instantiate(square, v, Quaternion.identity) as GameObject;
            Square s = new Square { Square_GO = square_go, Age_s = Time.time };
            AliveSquares.Add(square_go, s);
            return true;
        }
        return false;
    }

    public void SquareTouched(GameObject go)
    {
        Vector3 v = WorldToCanvasCoords(go.transform.position);
        AliveSquares.Remove(go);
        Text t = Instantiate(HitSquare, v, Quaternion.identity) as Text;
        t.transform.SetParent(canvas.transform, false);
        TextDestroyedSquares.Add(new TextTimer { text = t, Age_s = Time.time });

        points += POINTS_SQUARE;
        pointsText.text = points.ToString("f0");
    }


    private void GenerateCircles()
    {
        Vector3 v = GetVallidCoords();
        Circle add = new Circle
        {
            Circle_Prefab = Instantiate(circle, v, Quaternion.identity) as GameObject,
            Age_s = Time.time,
            counter = null
        };
        aliveCircles.Add(add.Circle_Prefab, add);
        lastCircleAge_s = Time.time;
        ++CircleCounter;
        
    }

    private Vector3 GetVallidCoords()
    {
        Vector3 v =transform.position;
        float x = Random.Range(-X_length, X_length);
        float y = Random.Range(-Y_length, Y_length);
        v.x = x; v.y = y; v.z = transform.position.z - 1;
        while (Physics.CheckSphere(v, Circle_Rect.pivot.x))
        {
            v.x = Random.Range(-X_length, X_length); v.y = Random.Range(-Y_length, Y_length); ;
        }
        return v;
    }

    public void PointsUpdate(GameObject destroyedCircle)
    {
        points += MAX_POINTS_CIRCLE;
        pointsText.text = points.ToString("f0");
        Destroy(aliveCircles[destroyedCircle].counter);
        aliveCircles.Remove(destroyedCircle);

        if(CIRCLE_SPAWN_s > CIRCLE_SPAWN_LIMIT_s)
        {
            CIRCLE_SPAWN_s -= DEC_CIRCLE_SPAWN_s;
            DEC_CIRCLE_SPAWN_s -= DEC_CIRCLE_SPAWN_s/20;
        }
        
    }

    public Vector3 WorldToCanvasCoords(Vector3 v)
    {
        v.x= ((v.x * CanvasRect.sizeDelta.x) / (stageDimensions.x * 2));
        v.y= ((v.y * CanvasRect.sizeDelta.y) / (stageDimensions.y * 2));
        return v;
    }

    private void GameOver()
    {
        OnPause = true;
        GameOverPanel.SetActive(true);
        GameOverPointsText.text += points.ToString("f0");
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
