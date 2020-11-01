using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CircleClick : MonoBehaviour {

    private TouchManager touchM;
    public AudioClip clip;

    //Use this for initialization
    void Start () {
          touchM = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    private void PlaySound()
    {
        var audioSource = GetComponent<AudioSource>();
        AudioSource.PlayClipAtPoint(audioSource.clip, transform.position, 1f);
    }

    private void OnMouseDown()
    {
        if (!touchM.OnPause)
        {
            PlaySound();
            touchM.PointsUpdate(gameObject);
            Destroy(gameObject); 
        }
              
    }
}
