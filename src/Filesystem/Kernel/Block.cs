using System;

namespace TinyFileSystem
{
    // 数据块, 长度64个字节
    [Serializable]
    public class Block
    {
        public static readonly int blockSize = 64;
        private readonly char[] data = new char[blockSize];
        private int size = 0;

        public Block()
        {
        }

        public void SetData(string data)
        {
            size = (data.Length > blockSize) ? blockSize : data.Length;
            for(int i = 0; i < size; i++)
            {
                this.data[i] = data[i];
            }
        }

        public string GetData()
        {
            return new string(data);
        }
    }
}
