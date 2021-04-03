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
using Assets.Scripts.Preloader;
using UnityEngine.Audio;
using Assets.Scripts.MultiplayerInGame_Scene.Objects;
using System.Threading.Tasks;

public class SpecialCircleMultiplayer : MonoBehaviour, IOnClick
{

    public int Id { get; set; }
    public AudioMixerGroup SpecialCircleAudioMixer;
    public AudioClip ScorePointsClip;
    public AudioClip OpponentScoreClip;
    //private Text PatxauText;

    void Start()
    {
        //PatxauText = GameObject.Find("PatxauText").GetComponent<Text>();
    }

    public void PlayOpponentSound()
    {
        PlaySound(OpponentScoreClip);
    }

    private void PlaySound(AudioClip clip)
    {
        var source = GetComponent<AudioSource>();

        source.outputAudioMixerGroup = SpecialCircleAudioMixer;
        source.PlayOneShot(clip);
    }


    private bool isDead = false;


    private void OnMouseDown()
    {
        if (isDead)
            return;

        PlaySound(ScorePointsClip);
        Disable();

        TouchManagerMultiplayer.Instance.DeleteById(Id, false, false);

        ServerManager.Instance.Client.Send(URI.DeleteObject, new DeleteObject() { ChallengeID = GameContainer.CurrentGameId, ObjectID = Id }, OnObjectDeletion);

        //if (PlayerInGameContainer.Instance.CurrTapsInARow > 5)
        //{
        //    UnityMainThreadDispatcher.Instance().Enqueue(() => 
        //    {
        //        PatxauText.fontSize = 20 + ((PlayerInGameContainer.Instance.CurrTapsInARow - 6) * 2);
        //        PatxauText.gameObject.transform.position = transform.position;
        //        StartCoroutine(UIUtils.ShowMessageInText("Hell Yeah!!", 0.5f, PatxauText));
        //    });
        //}
    }

    private void OnObjectDeletion(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            OnDeletedObject deletedObj = p.DeserializeContent<OnDeletedObject>();
            PlayerInGameContainer.Instance.UpdateGameStats(deletedObj);
        }

        Destroy(gameObject);
    }

    public void Disable()
    {
        isDead = true;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
}
