using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyFileSystem
{
    // 位图, 管理数据块
    // 10K个块
    [Serializable]
    public class BitMap
    {
        public int size = 0; // 当前使用容量
        public static int capacity = 10 * 1024; // 位图最大容量
        private readonly bool[] bitMap = new bool[capacity];
        private readonly Block[] blocks = new Block[capacity];

        public BitMap()
        {
            for (int i = 0; i < capacity; i++)
            {
                bitMap[i] = true;
            }
        }

        //Get a block's data
        public string GetBlock(int i)
        {
            return blocks[i].GetData();
        }

        // 申请一个新块, 并设置数据, 返回块号
        // 找不到新块时返回-1
        public int NewBlock(string data)
        {
            size %= capacity;
            int tempPointer = size;
            while (true)
            {
                if (bitMap[tempPointer])
                {
                    blocks[tempPointer] = new Block();
                    blocks[tempPointer].SetData(data);
                    size = tempPointer + 1;
                    return tempPointer;
                }
                else
                {
                    tempPointer += 1;
                    tempPointer %= capacity;
                }
                if (tempPointer == size)
                {
                    break;
                }
            }
            return -1;
        }

        public void FreeBlock(int index)
        {
            bitMap[index] = true;
        }

        public void FreeBlock(List<int> indexs)
        {
            foreach(int i in indexs)
            {
                bitMap[i] = true;
            }
        }

        // 将所有内容创建成一个数据块列表, 返回这个列表的blockTable
        public IndexTable CreateBlockTable(string data)
        {
            IndexTable table = new IndexTable();
            while (data.Count() > Block.blockSize)
            {
                table.AddIndex(NewBlock(data.Substring(0, Block.blockSize)));
                data = data.Remove(0, Block.blockSize);
            }
            table.AddIndex(NewBlock(data));

            return table;
        }
    }
}
