using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;

namespace ONet
{
    public class Client
    {
        Socket _socket;
        Timer timer;
        int attempts = 0;

        IPEndPoint endPoint;
        
        byte[] buffer;

        #region accessors

        public int Attempts
        {
            get
            {
                return attempts;
            }
        }
        public IPEndPoint EndPoint
        {
            get
            {
                return endPoint;
            }
        }
        public bool Connected
        {
            get
            {
                return _socket.Connected;
            }
        }

        #endregion

        void Receive(IAsyncResult result)
        {   
            if (BitConverter.ToUInt16(buffer, 0) == 0)
            {
                if (disconnect != null)
                    disconnect(new GameMessage());
                Die();
            }
            else
            {
                GameMessage msg = new GameMessage();
                msg.fromBytes(buffer);
                if (message != null)
                    message(msg);
                _socket.BeginReceive(buffer, 0, 512, SocketFlags.None, new AsyncCallback(Receive), this);
            }
        }

        void Connect(IAsyncResult result)
        {
            if (connect != null)
                connect(new GameMessage());
            _socket.BeginReceive(buffer, 0, 512, SocketFlags.None, new AsyncCallback(Receive), this);
        }

        public void Send(GameMessage dataChunk)
        {
            _socket.Send(dataChunk.toBytes());
        }

        public Client(IPEndPoint endPoint)
        {
            buffer = new byte[512];
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.endPoint = endPoint;
            //TryConnect();
        }
        void Retry(object state)
        {
            if (_socket.Connected == false)
            {
                ++attempts;
                TryConnect();
            }
        }
        public void TryConnect()
        {
            _socket.BeginConnect(endPoint, new AsyncCallback(Connect), _socket);
            if (timer == null)
            {
                timer = new Timer(new TimerCallback(Retry), timer, 1500, 0);
            }
            else if (attempts > 10)
            {
                if (timeout != null)
                    timeout(new GameMessage());
            }
            else
            {
                timer.Change(1500, 0);
            }
        }
        public void Disconnect()
        {
            if (_socket.Connected)
            {
                attempts = 0;
                _socket.Send(GameMessage.disconnectMessage());
                _socket.Disconnect(true);
            }
        }
        void Die()
        {
            if (_socket.Connected)
            {
                _socket.Disconnect(true);
            }
        }

        #region callbacks

        public delegate void Callback(GameMessage message);
        Callback connect;
        public Callback OnConnect
        {
            set
            {
                connect = value;
            }
        }
        Callback disconnect;
        public Callback OnDisconnect
        {
            set
            {
                disconnect = value;
            }
        }
        Callback message;
        public Callback OnMessage
        {
            set
            {
                message = value;
            }
        }
        Callback timeout;
        public Callback OnTimeout
        {
            set
            {
                timeout = value;
            }
        }

        #endregion

    }
}
