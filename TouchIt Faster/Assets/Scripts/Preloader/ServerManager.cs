using Assets.Server.Protocol;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerManager : MonoBehaviour {

    public static ServerManager Instance { set; get; }

    public string ServerName { get; set; }
    public int ServerPort { get; set; }
    public string ClientId { get; set; }

    private TcpClient socket;
    private const int MAX_BYTES = 1024 * 5;
    private byte[] bufferCallback = new byte[MAX_BYTES];
    private List<byte> buffer = new List<byte>();



    // Use this for initialization
    void Start () {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ClientId = "1234565";
        ServerName = "192.168.1.3";
        ServerPort = 2222;

        socket = new TcpClient(ServerName, ServerPort);
        socket.Connect(ServerName, ServerPort);

        startReceiving();
    }


    DateTime lastRetry = DateTime.UtcNow;
    private void Send(object obj)
    {
        byte[] packet = (byte[])obj;

        bool retryConnect = false;

        try
        {
            if ((socket != null) && socket.Connected)
                socket.Client.Send(packet, 0, packet.Length, SocketFlags.None);
            else
                retryConnect = true;
        }
        catch (SocketException e)
        {
            Console.Write("ERROR: " + e.Message);
            retryConnect = true;
        }

        if (retryConnect)
        {
            if ((DateTime.UtcNow - lastRetry).TotalSeconds > 10)
            {
                socket = new TcpClient(ServerName, ServerPort);
                socket.Connect(ServerName, ServerPort);
            }
        }
    }

    public void Send(JsonPacket p)
    {
        ThreadPool.QueueUserWorkItem(Send, p.GetPacket());
    }

    public void Send(byte contentType, object o)
    {
        ThreadPool.QueueUserWorkItem(Send, (new JsonPacket(ClientId, contentType, JsonMapper.ToJson(o))).GetPacket());
    }
    
    private void startReceiving()
    {
        socket.Client.BeginReceive(bufferCallback, 0, bufferCallback.Length, SocketFlags.None, new AsyncCallback(receiveCallback), this);
    }

    private void receiveCallback(IAsyncResult ar)
    {
        int bytesRead = socket.Client.EndReceive(ar);

        if (bytesRead > 0)
        {
            byte[] data = new byte[bytesRead];

            Array.Copy(bufferCallback, data, bytesRead);

            buffer.AddRange(data);

            while (ProcessReceivedData()) ;

        }

        startReceiving();
    }

    private bool ProcessReceivedData()
    {
        JsonPacket p;

        if (JsonPacket.DeserializePacket(buffer, out p))
        {
            if (!string.IsNullOrEmpty(p.ContentJson))
                Process(p);
            return true;
        }

        return false;
    }

    private void Process(JsonPacket p)
    {
        switch (p.ContentType)
        {
            default:
                break;
                
        }
    }
}
