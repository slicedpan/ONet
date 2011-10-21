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
        public List<Connection> connections = new List<Connection>();
        Dictionary<int, DataChunk> _chunks;
        int lastClientNumber = 0;

        void Accept(IAsyncResult result)
        {
            Socket s = (Socket)result.AsyncState;
            _chunks.Add(lastClientNumber, new DataChunk());
            connections.Add(new Connection(this, s.EndAccept(result), _chunks[lastClientNumber]));
            ++lastClientNumber;
            s.BeginAccept(new AsyncCallback(Accept), s);
        }
        public void End(Connection connectionToEnd)
        {
            connectionToEnd.Disconnect();
            connections.Remove(connectionToEnd);
        }
        public GameServer(List<DataChunk> receiveChunks, int port = 8024)
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
        public String LastText = "";

        public void Dispose()
        {
            currentInstance = null;
        }
    }
}
