using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ONet
{
    public class GameMessage
    {
        public UInt16 DataType;
        public UInt16 MessageSize;
        byte[] _message;
        public const ushort Disconnect = 255;
        public const ushort Initialise = 254;

        public GameMessage()
        {

        }

        public GameMessage(byte[] array)
        {
            fromBytes(array);
        }

        public byte[] Message
        {
            get
            {
                return _message;
            }
        }

        public virtual void fromBytes(byte[] array)
        {
            DataType = BitConverter.ToUInt16(array, 0);
            MessageSize = BitConverter.ToUInt16(array, 2);
            if (array.Length > 4)
            {
                _message = new byte[array.Length - 4];
                for (int i = 0; i < array.Length - 4; ++i)
                {
                    _message[i] = array[i + 4];
                }
            }
        }

        public byte[] toBytes()
        {
            byte[] array = new byte[4 + _message.Length];
            BitConverter.GetBytes(DataType).CopyTo(array, 0);
            BitConverter.GetBytes(MessageSize).CopyTo(array, 2);
            _message.CopyTo(array, 4);
            return array;
        }

        public static byte[] CreateMessage(UInt16 dataType, byte[] array)
        {
            byte[] msgArray = new byte[4 + array.Length];
            BitConverter.GetBytes(dataType).CopyTo(msgArray, 0);
            BitConverter.GetBytes((UInt16)array.Length).CopyTo(msgArray, 2);
            array.CopyTo(msgArray, 4);
            return msgArray;
        }

        public static byte[] disconnectMessage(string reasonForDisconnect)
        {
            byte[] array = new byte[4 + reasonForDisconnect.Length];            
            BitConverter.GetBytes((ushort)Disconnect).CopyTo(array, 0);
            BitConverter.GetBytes((ushort)reasonForDisconnect.Length).CopyTo(array, 2);
            ASCIIEncoding.ASCII.GetBytes(reasonForDisconnect).CopyTo(array, 4);
            return array;
        }
        public static byte[] initialisationMessage(int idNumber)
        {
            byte[] array = new byte[4];
            BitConverter.GetBytes(Initialise).CopyTo(array, 0);
            BitConverter.GetBytes((ushort)idNumber).CopyTo(array, 2);
            return array;
        }
        public string messageAsString()
        {
            bool done = false;
            byte[] array = new byte[MessageSize];
            int i = 0;
            while (!done)
            {
                if (_message[i] != 0)
                    array[i] = _message[i];
                else
                {
                    done = true;
                }
                ++i;
            }
            return new String(ASCIIEncoding.ASCII.GetChars(array));
        }
    }
}
