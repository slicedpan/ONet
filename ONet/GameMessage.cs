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
        public int index;

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
                length += 8;
            }
            byte[] array = new byte[length + 8];
            BitConverter.GetBytes(Bundle).CopyTo(array, 0);
            BitConverter.GetBytes(length).CopyTo(array, 2);
            BitConverter.GetBytes(messages.Length).CopyTo(array, 4);
            length = 8;
            for (int i = 0; i < messages.Length; ++i)
            {
                messages[i].toBytes().CopyTo(array, length);
                length += messages[i].MessageSize;
                length += 8;
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
                offset += 8;
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
            index = BitConverter.ToInt32(array, startIndex + 4);
            if (MessageSize > 0)
            {
                _message = new byte[MessageSize];
                for (int i = 0; i < MessageSize; ++i)
                {
                    _message[i] = array[i + 8 + startIndex];
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
            byte[] array;
            if (_message != null)
            {
                array = new byte[8 + _message.Length];
                _message.CopyTo(array, 8);
            }
            else
            {
                array = new byte[8];
            }
            BitConverter.GetBytes(DataType).CopyTo(array, 0);
            BitConverter.GetBytes(MessageSize).CopyTo(array, 2);
            BitConverter.GetBytes(index).CopyTo(array, 4);           
            return array;
        }

        public static byte[] CreateMessage(UInt16 dataType, byte[] array)
        {
            byte[] msgArray = new byte[8 + array.Length];
            BitConverter.GetBytes(dataType).CopyTo(msgArray, 0);
            BitConverter.GetBytes((UInt16)array.Length).CopyTo(msgArray, 2);
            BitConverter.GetBytes((int)0).CopyTo(msgArray, 4);
            array.CopyTo(msgArray, 8);
            return msgArray;
        }

        public static byte[] disconnectMessage(string reasonForDisconnect)
        {
            byte[] array = new byte[8 + reasonForDisconnect.Length];            
            BitConverter.GetBytes((ushort)Disconnect).CopyTo(array, 0);
            BitConverter.GetBytes((ushort)reasonForDisconnect.Length).CopyTo(array, 2);            
            ASCIIEncoding.ASCII.GetBytes(reasonForDisconnect).CopyTo(array, 8);
            return array;
        }
        public static byte[] initialisationMessage(int idNumber)
        {
            byte[] array = new byte[8];
            BitConverter.GetBytes(Initialise).CopyTo(array, 0);
            BitConverter.GetBytes((ushort)0).CopyTo(array, 2);
            BitConverter.GetBytes(idNumber).CopyTo(array, 4);
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
        public static bool IsEquivalent(GameMessage m1, GameMessage m2)
        {
            if (m1.MessageSize != m2.MessageSize)
                return false;
            if (m1.MessageSize > 0)
            {
                for (int i = 0; i < m1.MessageSize; ++i)
                {
                    if (m1.Message[i] != m2.Message[i])
                        return false;
                }
            }
            return (m1.DataType == m2.DataType && m1.index == m2.index);
        }
        public bool IsEquivalent(GameMessage other)
        {
            return IsEquivalent(this, other);
        }
    }
    public class GameMessageComparer : IEqualityComparer<GameMessage>
    {
        public bool Equals(GameMessage x, GameMessage y)
        {
            return GameMessage.IsEquivalent(x, y);
        }

        public int GetHashCode(GameMessage obj)
        {
            throw new NotImplementedException();
        }
    }
}
