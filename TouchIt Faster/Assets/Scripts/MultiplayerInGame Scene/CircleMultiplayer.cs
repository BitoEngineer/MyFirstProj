using Assets.Server.Protocol;
using MyFirstServ.Models.TouchItFaster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMultiplayer : MonoBehaviour {

    public int Id { get; set; }

    void Start () {
		
	}

    private void OnMouseDown()
    {
        TouchManagerMultiplayer.Instance.DeleteById(Id);
        ServerManager.Instance.Client.Send(URI.DeleteObject, new DeleteObject() { ID = Id });
    }
}
