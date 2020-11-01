using Assets.Scripts.Multiplayer_Scene;
using Assets.Server.Protocol;
using MyFirstServ.Models.TouchItFaster;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.MultiplayerInGame_Scene;
using Assets.Scripts.Utils;
using Assets.Server.Models;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class BombMultiplayer : MonoBehaviour
{
    public GameObject BombExplosion;
    public AudioClip BombSound;
    public int Id { get; set; }
    //private Text PatxauText;

    private string[] BOMB_TOUCH_TEXT = new string[] { "Ooops", "Arrssh", "Duck" };
    private System.Random random = new System.Random();
    private AudioSource source { get { return GetComponent<AudioSource>(); } }

    void Start()
    {
        gameObject.AddComponent<AudioSource>();
        //PatxauText = GameObject.Find("PatxauText").GetComponent<Text>();
    }

    private bool playSoundOnDestroy = false;


    private void OnMouseDown()
    {
        playSoundOnDestroy = true;
        TouchManagerMultiplayer.Instance.DeleteById(Id);

        //PatxauText.fontSize = 25;
        //PatxauText.gameObject.transform.position = transform.position;
        //StartCoroutine(UIUtils.ShowMessageInText(BOMB_TOUCH_TEXT[random.Next(0, 3)], 0.5f, PatxauText));

        ServerManager.Instance.Client.Send(URI.DeleteObject, new DeleteObject() { ChallengeID = GameContainer.CurrentGameId, ObjectID = Id }, OnObjectDeletion);
    }

    private void OnObjectDeletion(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            OnDeletedObject deletedObj = p.DeserializeContent<OnDeletedObject>();
            PlayerInGameContainer.Instance.UpdateGameStats(deletedObj);
        }
    }

    private void OnDestroy()
    {
        if (playSoundOnDestroy)
        {
            var audioSource = GetComponent<AudioSource>();
            audioSource.volume = 1; //TODO está a dar bué baixinho madafaker
            AudioSource.PlayClipAtPoint(audioSource.clip, transform.position, 1f);

            Vector3 v = gameObject.transform.position;
            GameObject anim = Instantiate(BombExplosion, v, Quaternion.identity);
            anim.transform.SetParent(transform.parent);
            Destroy(anim, 0.8f);
            //source.PlayOneShot(BombSound);
        }
    }
}


