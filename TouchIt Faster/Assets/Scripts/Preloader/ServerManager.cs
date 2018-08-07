using Assets.Server.Models;
using Assets.Server.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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

    public enum ReplyResult { Timedout, ServerUnavailable, Success }
    public delegate void ReplyReceived(JsonPacket p, ReplyResult result);

    private Dictionary<int, ReplyReceived> waitingReply = new Dictionary<int, ReplyReceived>();
    private Dictionary<int, Timer> waitingTimers = new Dictionary<int, Timer>();
    public ReplyReceived PacketReceivedEvent { get; set; }


    // Use this for initialization
    void Start () {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ClientId = SystemInfo.deviceUniqueIdentifier;
        ServerName = "192.168.1.6";
        ServerPort = 2222;

        socket = new TcpClient(ServerName, ServerPort);

        startReceiving();
    }

    public void SetClientID(string clientID)
    {
        this.ClientId = clientID;
    }

    DateTime lastRetry = DateTime.UtcNow;
    private void Send(object obj)
    {
        JsonPacket jpacket = (JsonPacket)obj;
        byte[] packet = jpacket.GetPacket();

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
            Process(jpacket, ReplyResult.ServerUnavailable);

            if ((DateTime.UtcNow - lastRetry).TotalSeconds > 10)
            {
                socket = new TcpClient(ServerName, ServerPort);
                socket.Connect(ServerName, ServerPort);
            }
        }
    }

    private void Send(JsonPacket p)
    {
        ThreadPool.QueueUserWorkItem(Send, p);
    }

    public void Send(TouchItFasterContentType contentType, object o, ReplyReceived callback = null, int timeoutMs = 0)
    {
        JsonPacket p = new JsonPacket(ClientId, (byte)contentType, JsonUtility.ToJson(o));

        if (callback != null)
        {
            waitingReply.Add(p.PacketID, callback);
        }
        
        if(timeoutMs > 0)
        {
            Timer t = new Timer(timeElapsed, p, timeoutMs, timeoutMs);
            waitingTimers.Add(p.PacketID, t);
        }

        ThreadPool.QueueUserWorkItem(Send, p);
    }

    private void timeElapsed(object state)
    {
        JsonPacket p = (JsonPacket)state;
        if (waitingReply.ContainsKey(p.PacketID))
        {
            ReplyReceived r = waitingReply[p.PacketID];
            waitingReply.Remove(p.PacketID);
            r(p, ReplyResult.Timedout);
        }
        waitingTimers[p.PacketID].Dispose();
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
                Task.Run(()=>Process(p));
            return true;
        }

        return false;
    }

    private void Process(JsonPacket p, ReplyResult result = ReplyResult.Success)
    {
        if (waitingReply.ContainsKey(p.PacketID))
        {
            ReplyReceived r = waitingReply[p.PacketID];
            waitingReply.Remove(p.PacketID);
            r(p, result);
            return;
        }

        if (waitingTimers.ContainsKey(p.PacketID))
        {
            waitingTimers[p.PacketID].Dispose();
        }

        if(PacketReceivedEvent!=null)
            PacketReceivedEvent(p, result);
    }
}
