using UnityEngine;

public class CircleClick : MonoBehaviour {

    private TouchManager touchM;

    //Use this for initialization
    void Start () {
          touchM = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    private void OnMouseDown()
    {
        if (!touchM.OnPause)
        {
            touchM.PointsUpdate(gameObject);
            Destroy(gameObject); 
        }
              
    }
}
