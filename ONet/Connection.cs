using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace ONet
{
    public class Connection
    {
        GameServer _server;
        Socket _socket;
        byte[] buffer;
        public String lastText = "";
        public bool newChunk = false;

        public void ReceiveData(IAsyncResult result)
        {
            DataChunk chunk = new DataChunk();
            chunk.fromBytes(buffer);
            newChunk = true;
            if (chunk.DataType == 0)
            {  
                _server.End(this);
            }
            else
            {
                _socket.BeginReceive(buffer, 0, 256, SocketFlags.None, new AsyncCallback(ReceiveData), _socket);
            }
        }

        public Connection(GameServer server, Socket socket, DataChunk chunk)
        {
            _socket = socket;
            _server = server;
            _socket.BeginReceive(buffer, 0, chunk.Size, SocketFlags.None, new AsyncCallback(ReceiveData), _socket);
        }
        public void Disconnect()
        {
            _socket.Disconnect(false);
        }
    }
}
