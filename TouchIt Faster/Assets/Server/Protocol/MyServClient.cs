using Assets.Server.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Server.Protocol
{

    public class MyServClient
    {
        #region Log

        public delegate void LogDebug(string message, params object[] args);
        public delegate void LogError(Exception e, string message, params object[] args);

        public LogDebug LogDebugEvent { get; set; }
        public LogError LogErrorEvent { get; set; }

        #endregion

        public string ServerName { get; private set; }
        public int ServerPort { get; private set; }
        public string ClientId { get; set; }

        public enum CallResult { Timedout, ServerUnavailable, Success }
        public delegate void ReplyReceivedCallback(JsonPacket p);
        public delegate void FailureCallback(JsonPacket p, CallResult result);
        public delegate void PacketCallback(JsonPacket p);

        public bool IsConnected { get { return (socket != null) && socket.Connected; } }

        private bool? lastConnectivityState = null;
        public Action<bool> OnConnectivityChange;

        public MyServClient(string ip, int port)
        {
            ServerName = ip;
            ServerPort = port;
        }

        public void Start(string clientId)
        {
            ClientId = clientId;
            startConnect();
            if (LogDebugEvent != null) LogDebugEvent.Invoke("Starting client \"{0}\" on {1}:{2}", clientId, ServerName, ServerPort);
        }

        public void Stop()
        {
            try
            {
                socket.Close();
                if (LogDebugEvent != null) LogDebugEvent.Invoke("Client closed");
            }
            catch (Exception e)
            {
                if (LogDebugEvent != null) LogErrorEvent.Invoke(e, "Error stopping client");
            }
        }

        public void Send(URI contentType, object o, ReplyReceivedCallback callback = null, FailureCallback fail = null, int timeoutMs = 0)
        {
            JsonPacket p = new JsonPacket(ClientId, contentType.ToString(), o);

            if (callback != null || fail != null)
            {
                var handler = new ReplyHandler(callback, fail);
                waitingReply.AddOrUpdate(p.PacketID, handler, (x, y) => handler);

                if (timeoutMs > 0)
                {
                    Timer t = new Timer(timeElapsed, p, timeoutMs, timeoutMs);
                    waitingTimers.AddOrUpdate(p.PacketID, t, (x, y) => t);
                }
            }

            startSending(p);
        }

        public void AddCallback(URI contentType, PacketCallback callback)
        {
            callbacks.AddOrUpdate(contentType, callback, (ct, c) => callback);
        }

        #region Private

        private class ReplyHandler
        {
            public ReplyReceivedCallback SuccessCallback;
            public FailureCallback FailureCallback;
            public ReplyHandler(ReplyReceivedCallback SuccessCallback, FailureCallback FailureCallback)
            {
                this.SuccessCallback = SuccessCallback;
                this.FailureCallback = FailureCallback;
            }
        }

        private TcpClient socket;
        private const int MAX_BYTES = 1024 * 5;
        private byte[] bufferCallback = new byte[MAX_BYTES];
        private List<byte> buffer = new List<byte>();

        private FuckinDictionary<int, ReplyHandler> waitingReply = new FuckinDictionary<int, ReplyHandler>();
        private FuckinDictionary<int, Timer> waitingTimers = new FuckinDictionary<int, Timer>();
        private FuckinDictionary<URI, PacketCallback> callbacks = new FuckinDictionary<URI, PacketCallback>();


        private void onEndSending(object op, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                process((JsonPacket)args.UserToken, CallResult.ServerUnavailable);
            }
        }

        private void startSending(JsonPacket p)
        {
            var data = p.GetPacket();
            var args = new SocketAsyncEventArgs();

            args.SetBuffer(data, 0, data.Length);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(onEndSending);
            args.UserToken = p;

            if (!socket.Client.SendAsync(args))
            {
                onEndSending(this, args);
            }
        }

        private void timeElapsed(object op)
        {
            JsonPacket p = (JsonPacket)op;

            ReplyHandler r;

            if (waitingReply.TryRemove(p.PacketID, out r))
            {
                if (r.FailureCallback != null)
                {
                    r.FailureCallback(p, CallResult.Timedout);
                }
            }

            Timer t;

            if (waitingTimers.TryRemove(p.PacketID, out t))
            {
                t.Dispose();
            }
        }

        private void notifyConnectivityState(bool newState)
        {
            if (OnConnectivityChange != null && lastConnectivityState != newState)
            {
                OnConnectivityChange(newState);
                lastConnectivityState = newState;
            }
        }

        private void startConnect()
        {
            if (LogDebugEvent != null) LogDebugEvent.Invoke("Connecting to {0}:{1}...", ServerName, ServerPort);
            socket = new TcpClient();

            Task.Run(() =>
            {
                try
                {
                    socket.Connect(ServerName, ServerPort);

                    if (socket.Connected)
                    {
                        if (LogDebugEvent != null) LogDebugEvent.Invoke("Connected to {0}:{1}", ServerName, ServerPort);

                        notifyConnectivityState(true);

                        startReceiving();
                    }
                    else
                    {
                        notifyConnectivityState(false);
                    }
                }
                catch (Exception)
                {
                    notifyConnectivityState(false);
                    Task.Delay(TimeSpan.FromMilliseconds(1000));
                    startConnect();
                }

            });
        }

        private void startReceiving()
        {
            try
            {
                socket.Client.BeginReceive(bufferCallback, 0, bufferCallback.Length, SocketFlags.None, new AsyncCallback(receiveCallback), this);
            }
            catch (Exception e)
            {
                if (LogDebugEvent != null) LogErrorEvent.Invoke(e, "Error start receiving data - reconnecting");
                startConnect();
            }
        }

        private void receiveCallback(IAsyncResult ar)
        {
            try
            {
                int bytesRead = socket.Client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    //LogDebugEvent?.Invoke("{0} bytes received", bytesRead);

                    buffer.AddRange(bufferCallback.Take(bytesRead));

                    while (processReceivedData()) ;
                }

                startReceiving();
            }
            catch (SocketException e)
            {
                if (LogDebugEvent != null) LogErrorEvent.Invoke(e, "Error on socket - reconnecting");
                startConnect();
            }
            catch (ObjectDisposedException e)
            {
                if (LogDebugEvent != null) LogErrorEvent.Invoke(e, "Error socket closed - reconnecting");
                startConnect();
            }
            catch (Exception e)
            {
                if (LogDebugEvent != null) LogErrorEvent.Invoke(e, "Error receiving data");
                startReceiving();
            }
        }

        private bool processReceivedData()
        {
            JsonPacket p;

            if (JsonPacket.DeserializePacket(buffer, out p))
            {
                if (!string.IsNullOrEmpty(p.ContentJson))
                {
                    Task.Run(() => process(p));
                }

                return true;
            }

            return false;
        }

        private void process(JsonPacket p, CallResult result = CallResult.Success)
        {
            if (LogDebugEvent != null) LogDebugEvent.Invoke("Packet received: {0}", p.ToString());

            Timer t;
            if (waitingTimers.TryRemove(p.PacketID, out t))
            {
                t.Dispose();
            }

            ReplyHandler r;

            if (waitingReply.TryRemove(p.PacketID, out r))
            {
                if (r.SuccessCallback != null)
                {
                    r.SuccessCallback(p);
                }
                return;
            }

            PacketCallback c;

            if (callbacks.TryGetValue((URI)Enum.Parse(typeof(URI), p.ContentType), out c))
            {
                c(p);
            }
        }

        #endregion
    }
}
