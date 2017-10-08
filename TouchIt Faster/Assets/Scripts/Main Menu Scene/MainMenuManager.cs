
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour {

    public Button SinglePlayerButton;
    public Button MultiplayerButton;


    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {
        //AdManager.Instance.ShowBanner(); CREATE PRELOADER SCENE https://www.youtube.com/watch?v=khlROw-PfNE
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
