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
        public UInt16 index;

        byte[] _message;
        public const ushort Disconnect = 65535;
        public const ushort Initialise = 65534;
        public const ushort Bundle = 65533;

        public GameMessage()
        {

        }

        public static GameMessage MessageBundle(List<GameMessage> messages)
        {
            return MessageBundle(messages.ToArray());
        }

        public static GameMessage MessageBundle(GameMessage[] messages)
        {
            ushort length = 0;            
            for (int i = 0; i < messages.Length; ++i)
            {
                length += messages[i].MessageSize;
                length += 6;
            }
            byte[] array = new byte[length + 6];
            BitConverter.GetBytes(Bundle).CopyTo(array, 0);
            BitConverter.GetBytes(length).CopyTo(array, 2);
            BitConverter.GetBytes((ushort)messages.Length).CopyTo(array, 4);
            length = 6;
            for (int i = 0; i < messages.Length; ++i)
            {
                messages[i].toBytes().CopyTo(array, length);
                length += messages[i].MessageSize;
                length += 6;
            }
            return new GameMessage(array);
        }

        public static List<GameMessage> SplitBundle(GameMessage bundleMessage)
        {
            List<GameMessage> retList = new List<GameMessage>();
            int offset = 0;
            for (int i = 0; i < bundleMessage.index; ++i)
            {
                retList.Add(new GameMessage());
                retList[i].fromBytes(bundleMessage.Message, offset);
                offset += retList[i].MessageSize;
                offset += 6;
            }
            return retList;
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

        public void fromBytes(byte[] array)
        {
            fromBytes(array, 0);
        }
        public void fromBytes(byte[] array, int startIndex)
        {
            DataType = BitConverter.ToUInt16(array, startIndex);
            MessageSize = BitConverter.ToUInt16(array, startIndex + 2);
            index = BitConverter.ToUInt16(array, startIndex + 4);
            if (MessageSize > 0)
            {
                _message = new byte[MessageSize];
                for (int i = 0; i < MessageSize; ++i)
                {
                    _message[i] = array[i + 6 + startIndex];
                }
            }
        }
        public void SetMessage(byte[] array)
        {
            MessageSize = (ushort) array.Length; 
            _message = array;
        }

        public byte[] toBytes()
        {
            byte[] array = new byte[6 + _message.Length];
            BitConverter.GetBytes(DataType).CopyTo(array, 0);
            BitConverter.GetBytes(MessageSize).CopyTo(array, 2);
            BitConverter.GetBytes(index).CopyTo(array, 4);
            _message.CopyTo(array, 6);
            return array;
        }

        public static byte[] CreateMessage(UInt16 dataType, byte[] array)
        {
            byte[] msgArray = new byte[6 + array.Length];
            BitConverter.GetBytes(dataType).CopyTo(msgArray, 0);
            BitConverter.GetBytes((UInt16)array.Length).CopyTo(msgArray, 2);
            BitConverter.GetBytes((UInt16)0).CopyTo(msgArray, 4);
            array.CopyTo(msgArray, 6);
            return msgArray;
        }

        public static byte[] disconnectMessage(string reasonForDisconnect)
        {
            byte[] array = new byte[6 + reasonForDisconnect.Length];            
            BitConverter.GetBytes((ushort)Disconnect).CopyTo(array, 0);
            BitConverter.GetBytes((ushort)reasonForDisconnect.Length).CopyTo(array, 2);            
            ASCIIEncoding.ASCII.GetBytes(reasonForDisconnect).CopyTo(array, 6);
            return array;
        }
        public static byte[] initialisationMessage(int idNumber)
        {
            byte[] array = new byte[6];
            BitConverter.GetBytes(Initialise).CopyTo(array, 0);
            BitConverter.GetBytes((ushort)0).CopyTo(array, 2);
            BitConverter.GetBytes((ushort)idNumber).CopyTo(array, 4);
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
