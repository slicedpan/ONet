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
    public class Client : IDisposable
    {
        Socket _socket;
        Timer timer;
        int attempts = 0;
        public string socketMessage = "";
        IPEndPoint endPoint;
        int id = -1;
        public int IDNumber
        {
            get
            {
                return id;
            }
        }
        
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
            set
            {
                endPoint = value;
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
            if (BitConverter.ToUInt16(buffer, 0) == GameMessage.Disconnect)
            {
                GameMessage msg = new GameMessage();
                msg.fromBytes(buffer);
                if (disconnect != null)
                    disconnect(msg);
                Die();
            }
            else
            {
                if (BitConverter.ToUInt16(buffer, 0) == GameMessage.Initialise)
                {
                    id = BitConverter.ToUInt16(buffer, 2);
                }
                else
                {
                    GameMessage msg = new GameMessage();
                    msg.fromBytes(buffer);
                    if (message != null)
                        message(msg);
                }
                if (_socket.Connected)
                {
                    try
                    {
                        _socket.BeginReceive(buffer, 0, 2048, SocketFlags.None, new AsyncCallback(Receive), this);
                    }
                    catch (Exception se)
                    {
                        Error(se.Message);
                    }
                }
            }

        }

        void Connect(IAsyncResult result)
        {

            if (connect != null)
            {
                connect(new GameMessage());
            }
            try
            {
                _socket.EndConnect(result);
                _socket.BeginReceive(buffer, 0, 2048, SocketFlags.None, new AsyncCallback(Receive), this);
            }
            catch (Exception se)
            {
                error(se.Message);
            }
            timer.Dispose();
            timer = null;
        }

        public void Send(GameMessage dataChunk)
        {
            try
            {
                _socket.Send(dataChunk.toBytes());
            }
            catch (Exception se)
            {
                Error(se.Message);
            }
        }

        public Client(IPEndPoint endPoint)
        {
            buffer = new byte[2048];
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
            try
            {
                _socket.BeginConnect(endPoint, new AsyncCallback(Connect), _socket);
            }
            catch (Exception se)
            {
                Error(se.Message);
            }
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
        public void Disconnect(string reasonForDisconnect)
        {
            if (_socket.Connected)
            {
                attempts = 0;
                try
                {
                    _socket.Send(GameMessage.disconnectMessage(reasonForDisconnect));
                }
                catch (Exception se)
                {
                    Error(se.Message);
                }
                    /*
                catch (ObjectDisposedException oe)
                {
                    Error("Object Disposed" + oe.Message);
                }
                catch (Exception e)
                {
                    Error("Exception" + e.Message);
                }*/
                Die();
            }
        }
        void Die()
        {
            _socket.Shutdown(SocketShutdown.Both);
            if (_socket.Connected)
            {
                _socket.Disconnect(true);
            }
        }
        void Error(string errorMsg)
        {
            if (error != null)
            {
                error(errorMsg);
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
        public delegate void ErrorCallback(String errorMessage);
        ErrorCallback error;
        public ErrorCallback OnError
        {
            set
            {
                error = value;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _socket.Dispose();
        }

        #endregion

    }
}
