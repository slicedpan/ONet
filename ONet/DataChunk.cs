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
                array.CopyTo(_message, 0);
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

        public static byte[] disconnectMessage()
        {
            byte[] array = new byte[1];
            BitConverter.GetBytes((int)0).CopyTo(array, 0);
            return array;
        }
    }
}
