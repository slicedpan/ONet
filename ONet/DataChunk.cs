using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ONet
{
    public class DataChunk
    {
        public Vector2 pos;  
        public int DataType;
 
        public virtual int Size
        {
            get
            {
                return 12;
            }
        }

        public virtual DataChunk fromBytes(byte[] array)
        {
            DataChunk chunk = new DataChunk();
            chunk.DataType = BitConverter.ToInt32(array, 0);
            chunk.pos.X = BitConverter.ToSingle(array, 4);
            chunk.pos.X = BitConverter.ToSingle(array, 8);
            return chunk;
        }

        public virtual byte[] toBytes()
        {
            byte[] array = new byte[12];
            BitConverter.GetBytes(DataType).CopyTo(array, 0);
            BitConverter.GetBytes(pos.X).CopyTo(array, 4);
            BitConverter.GetBytes(pos.Y).CopyTo(array, 8);
            
            return array;
        }

        public static byte[] disconnectMessage()
        {
            byte[] array = new byte[1];
            BitConverter.GetBytes((int)0).CopyTo(array, 0);
            return array;
        }
    }
}
