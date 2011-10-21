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
        int counter = 0;
        Timer timer;
        public int attempts = 0;
        IPEndPoint endPoint;
        DataChunk _dataChunk;
        byte[] buffer;
        bool newChunk = false;

        public bool NewChunk
        {
            get     
            {
                return newChunk;
            }
        }

        void Receive(IAsyncResult result)
        {

        }

        void Connect(IAsyncResult result)
        {
            _socket.BeginReceive(buffer, 0, _dataChunk.Size, SocketFlags.None, new AsyncCallback(Receive), this);
        }
        public bool Connected
        {
            get
            {
                return _socket.Connected;
            }
        }
        public void Send(DataChunk dataChunk)
        {
            _socket.Send(dataChunk.toBytes());
        }
        public DataChunk Receive()
        {
            return _dataChunk;
        }
        public Client(IPEndPoint endPoint, DataChunk receiveDataChunk)
        {
            _dataChunk = receiveDataChunk;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TryConnect();
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
                _socket.Send(DataChunk.disconnectMessage());
                _socket.Disconnect(true);
            }
        }
    }
}
