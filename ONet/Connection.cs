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
        GameServer.ErrorCallback _error;

        Socket _socket;
        public Socket Socket
        {
            get
            {
                return _socket;
            }
        }
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
            if (BitConverter.ToUInt16(buffer, 0) == GameMessage.Disconnect)
            {                
                try
                {
                    _disconnect(idNumber, getMessage());
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close(2);                    
                }
                catch (Exception e)
                {
                    _error(e.Message);
                }
                Connection junk;
                _server.Connections.TryRemove(idNumber, out junk);
            }
            else
            {
                _message(idNumber, getMessage());
                if (_socket.Connected)
                {
                    try
                    {
                        _socket.BeginReceive(buffer, 0, 2048, SocketFlags.None, new AsyncCallback(ReceiveData), _socket);
                    }
                    catch (Exception se)
                    {
                        reportError(se.Message);
                    }
                }
            }
        }

        public Connection(GameServer server, Socket socket, int number, GameServer.Callback disconnect, GameServer.Callback message, GameServer.ErrorCallback error)
        {
            _socket = socket;
            _server = server;
            _error = error;
            _message = message;
            _disconnect = disconnect;
            idNumber = number;
            buffer = new byte[2048];
            try
            {
                _socket.BeginReceive(buffer, 0, 2048, SocketFlags.None, new AsyncCallback(ReceiveData), _socket);
                _socket.Send(GameMessage.initialisationMessage(number));
            }
            catch (Exception se)
            {
                reportError(se.Message);
            }
        }
        public void Send(GameMessage message)
        {
            Send(message.toBytes());
        }
        public void Send(byte[] array)
        {
            try
            {
                _socket.Send(array);
            }
            catch (Exception se)
            {
                reportError(se.Message);
            }
        }
        public void Disconnect()
        {
            GameMessage msg = new GameMessage();
            msg.fromBytes(GameMessage.disconnectMessage(String.Format("Removing Client {0}", idNumber)));
            _disconnect(idNumber, msg);            
            try
            {                
                _socket.Send(GameMessage.disconnectMessage("disconnected by server"));
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close(2);
            }
            catch (Exception se)
            {
                reportError(se.Message);
            }
        }
        void reportError(string message)
        {
            _error(String.Format("Client {0}: {1}", idNumber, message));
        }
    }
}
