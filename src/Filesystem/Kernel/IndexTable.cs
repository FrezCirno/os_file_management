using System;
using System.Collections.Generic;

namespace TinyFileSystem
{
    // 三级索引结构
    // 10个一级索引, 1个二级索引, 1个三级索引
    // 每个二级索引包含128个一级索引
    // 每个三级索引包含128个二级索引
    // 最多10+128+128*128个数据块
    [Serializable]
    public class IndexTable
    {
        public static int capacity = 10;
        private readonly int[] index = new int[capacity];
        private int size = 0;
        private PrimaryIndex pIndex;
        private SecondaryIndex sIndex;

        public IndexTable()
        {
        }

        public bool Isfull()
        {
            return size >= capacity;
        }

        public bool AddIndex(int block)
        {
            if (!Isfull())
            {
                index[size] = block;
                size++;
                if(size == capacity)
                {
                    pIndex = new PrimaryIndex();
                }
            }
            else if (!pIndex.Isfull())
            {
                pIndex.AddIndex(block);
                if (pIndex.Isfull())
                {
                    sIndex = new SecondaryIndex();
                }
            }
            else if(!sIndex.Isfull())
            {
                sIndex.AddIndex(block);
            }
            else
            {
                return false;
            }
            return true;
        }

        public List<int> GetBlockList()
        {
            List<int> blockList = new List<int>();

            for(int i = 0; i < size; i++)
            {
                blockList.Add(index[i]);
            }
            if(size == 100)
            {
                for(int j = 0;j < pIndex.size; j++)
                {
                    blockList.Add(pIndex.index[j]);
                }
            }
            if (pIndex != null && pIndex.Isfull())
            {
                foreach (PrimaryIndex pIndex in sIndex.pIndex)
                {
                    for(int k = 0; k < pIndex.size; k++)
                    {
                        blockList.Add(pIndex.index[k]);
                    }
                }
            }

            return blockList;
        }
    }

    [Serializable]
    public class PrimaryIndex
    {
        public static int capacity = 128;
        public int[] index = new int[capacity];
        public int size = 0;

        public PrimaryIndex()
        {
        }

        public void AddIndex(int data)
        {
            index[size] = data;
            size++;
        }

        public bool Isfull()
        {
            return size >= capacity;
        }
    }

    [Serializable]
    public class SecondaryIndex
    {
        public static int capacity = 128;
        public List<PrimaryIndex> pIndex = new List<PrimaryIndex>{ new PrimaryIndex() };
        public int size = 0;

        public SecondaryIndex()
        {
        }

        public bool Isfull()
        {
            return size >= capacity;
        }

        public void AddIndex(int data)
        {
            PrimaryIndex temp = pIndex[size];
            if (temp.Isfull())
            {
                pIndex.Add(new PrimaryIndex());
                size++;
                temp = pIndex[size];
            }
            temp.AddIndex(data);
        }
    }
}
