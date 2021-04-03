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
    public GameObject PointsTextGO;
    private GameObject PointsTextNewGO;

    void Start()
    {
        PointsTextNewGO = Instantiate(PointsTextGO, gameObject.transform.parent);
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
    }

    private void OnObjectDeletion(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            OnDeletedObject deletedObj = p.DeserializeContent<OnDeletedObject>();
            var pointsDiff = PlayerInGameContainer.Instance.UpdateGameStats(deletedObj);
            
            ShowPoints(pointsDiff);
        }

        Destroy(gameObject);
    }

    public void Disable()
    {
        isDead = true;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void ShowPoints(int pointsDiff)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            PointsTextNewGO.transform.position = transform.position;
            var text = PointsTextNewGO.GetComponent<Text>();
            var pointsText = "";
            if (pointsDiff > 0)
            {
                pointsText = "+" + pointsDiff;
                text.color = Color.green;
            }
            else
            {
                pointsText = "" + pointsDiff;
                text.color = Color.red;
            }
            StartCoroutine(UIUtils.ShowMessageInText(pointsText, 1.0f, text));
        });
    }
}
