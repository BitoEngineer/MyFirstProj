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
using Assets.Scripts.MultiplayerInGame_Scene.Objects;
using Assets.Scripts.Preloader;

public class BombMultiplayer : MonoBehaviour
{
    public GameObject BombExplosion;
    public int Id { get; set; }
    public GameObject PointsTextGO;
    private GameObject PointsTextNewGO;


    void Start()
    {
        PointsTextNewGO = Instantiate(PointsTextGO, gameObject.transform.parent);
        gameObject.AddComponent<AudioSource>();
    }

    private void OnMouseDown()
    {
        TouchManagerMultiplayer.Instance.DeleteById(Id, false, false);

        ServerManager.Instance.Client.Send(URI.DeleteObject, new DeleteObject() { ChallengeID = GameContainer.CurrentGameId, ObjectID = Id }, OnObjectDeletion);
    }

    private int? points = null;
    private void OnObjectDeletion(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            OnDeletedObject deletedObj = p.DeserializeContent<OnDeletedObject>();
            points = PlayerInGameContainer.Instance.UpdateGameStats(deletedObj);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GetComponent<SpriteRenderer>().enabled = false;

                PlayExplosion();

                if (points.HasValue)
                    ShowPoints(points.Value);

                Task.Delay(5000).ContinueWith((t) => Destroy(gameObject));
            });
        }
    }

    private void PlayExplosion()
    {
        Vector3 v = gameObject.transform.position;
        GameObject anim = Instantiate(BombExplosion, v, Quaternion.identity);
        anim.transform.SetParent(transform.parent);

        Destroy(anim, 0.8f);
    }

    public void ShowPoints(int pointsDiff)
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;

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

        StartCoroutine(UIUtils.ShowMessageInText(pointsText, 2.0f, text));
    }
}

