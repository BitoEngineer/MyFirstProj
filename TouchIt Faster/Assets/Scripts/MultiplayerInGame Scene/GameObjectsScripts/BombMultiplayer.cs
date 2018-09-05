using Assets.Scripts.Multiplayer_Scene;
using Assets.Server.Protocol;
using MyFirstServ.Models.TouchItFaster;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.MultiplayerInGame_Scene;
using Assets.Server.Models;
using UnityEngine;

public class BombMultiplayer : MonoBehaviour
{
    public int Id { get; set; }

    void Start()
    {
    }

    private void OnMouseDown()
    {
        TouchManagerMultiplayer.Instance.DeleteById(Id);
        ServerManager.Instance.Client.Send(URI.DeleteObject, new DeleteObject() { ChallengeID = GameContainer.CurrentGameId, ObjectID = Id }, OnObjectDeletion);
    }

    private void OnObjectDeletion(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            OnDeletedObject deletedObj = p.DeserializeContent<OnDeletedObject>();
            PlayerInGameContainer.Instance.Build(deletedObj);
        }
    }
}


