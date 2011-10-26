using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Microsoft.Xna.Framework;

namespace ONet
{
    public class GameServer : IDisposable
    {
        public static GameServer currentInstance;

        int portNumber;
        Socket socket;
        public bool isActive = false;
        List<Socket> sockets = new List<Socket>();
        public Dictionary<int, Connection> Connections = new Dictionary<int, Connection>();
        int lastClientNumber = 0;

        void Accept(IAsyncResult result)
        {
            Socket s = (Socket)result.AsyncState;
            Connections.Add(lastClientNumber, new Connection(this, s.EndAccept(result), lastClientNumber, new Callback(clientDisconnect), new Callback(message)));
            clientConnect(lastClientNumber, new GameMessage());
            ++lastClientNumber;
            s.BeginAccept(new AsyncCallback(Accept), s);
        }
        public void End(int connectionNumber)
        {
            Connections[connectionNumber].Disconnect();
            Connections.Remove(connectionNumber);
        }
        public GameServer(int port = 8024)
        {            
            if (currentInstance == null)
            {
                currentInstance = this;
            }
            else
            {
                throw new Exception("only one instance of GameServer is allowed!");
            }
            portNumber = port;
        }
        public void Listen()
        {
            isActive = true;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 8024));
            socket.Listen(10);
            socket.BeginAccept(new AsyncCallback(Accept), socket);
        }
        public void Dispose()
        {
            currentInstance = null;
        }

        #region callbacks

        public delegate void Callback(int clientNumber, GameMessage message);
        Callback clientConnect;
        Callback clientDisconnect;
        Callback clientMessage;

        void connectMessage(int clientNumber, GameMessage message)
        {
            if (clientConnect != null)
                clientConnect(clientNumber, message);
        }
        void disconnectMessage(int clientNumber, GameMessage message)
        {
            if (clientDisconnect != null)
                clientDisconnect(clientNumber, message);
        }
        void message(int clientNumber, GameMessage message)
        {
            if (clientMessage != null)
                clientMessage(clientNumber, message);
        }
        public Callback OnClientConnect
        {
            set
            {
                clientConnect = value;
            }
        }
        public Callback OnClientDisconnect
        {
            set
            {
                clientDisconnect = value;
            }
        }
        public Callback OnClientMessage
        {
            set
            {
                clientMessage = value;
            }
        }
        #endregion
    }
}
