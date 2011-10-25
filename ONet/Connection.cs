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

        GameServer.Callback _disconnect;
        GameServer.Callback _message;

        Socket _socket;
        byte[] buffer;
        bool newChunk = false;
        int idNumber;

        public bool NewChunk
        {
            get
            {
                return newChunk;
            }
        }

        public GameMessage getMessage()
        {
            newChunk = false;
            GameMessage msg = new GameMessage();
            msg.fromBytes(buffer);
            return msg;
        }

        public void ReceiveData(IAsyncResult result)
        {
            newChunk = true;
            _message(idNumber, getMessage());
            if (BitConverter.ToUInt16(buffer, 0) == 0)
            {  
                _server.End(idNumber);
            }
            else
            {
                _socket.BeginReceive(buffer, 0, 512, SocketFlags.None, new AsyncCallback(ReceiveData), _socket);
            }
        }

        public Connection(GameServer server, Socket socket, int number, GameServer.Callback disconnect, GameServer.Callback message)
        {
            _socket = socket;
            _server = server;
            _message = message;
            _disconnect = disconnect;
            idNumber = number;
            buffer = new byte[512];
            _socket.BeginReceive(buffer, 0, 512, SocketFlags.None, new AsyncCallback(ReceiveData), _socket);
        }
        public void Disconnect()
        {
            _disconnect(idNumber, new GameMessage());
            _socket.Disconnect(false);
        }
    }
}
