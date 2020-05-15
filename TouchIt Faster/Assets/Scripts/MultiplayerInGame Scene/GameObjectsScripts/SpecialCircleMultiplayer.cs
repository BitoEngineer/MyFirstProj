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

public class SpecialCircleMultiplayer : MonoBehaviour {

    public int Id { get; set; }
    //private Text PatxauText;

    void Start()
    {
        //PatxauText = GameObject.Find("PatxauText").GetComponent<Text>();
    }

    private void OnMouseDown()
    {
        TouchManagerMultiplayer.Instance.DeleteById(Id);
        //if (PlayerInGameContainer.Instance.CurrTapsInARow > 5)
        //{
        //    UnityMainThreadDispatcher.Instance().Enqueue(() => 
        //    {
        //        PatxauText.fontSize = 20 + ((PlayerInGameContainer.Instance.CurrTapsInARow - 6) * 2);
        //        PatxauText.gameObject.transform.position = transform.position;
        //        StartCoroutine(UIUtils.ShowMessageInText("Hell Yeah!!", 0.5f, PatxauText));
        //    });
        //}
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
}
