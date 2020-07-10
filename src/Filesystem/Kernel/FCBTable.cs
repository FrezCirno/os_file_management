using System;
using System.Collections.Generic;

namespace TinyFileSystem
{
    // 并不是目录
    // 存储fileNo和FCB的对照关系
    [Serializable]
    public class FCBTable
    {
        public Dictionary<int, FCB> fileTable = new Dictionary<int, FCB>();

        public void Add(FCB fcb)
        {
            fileTable[fcb.fileNo] = fcb;
        }

        public void Remove(FCB fcb)
        {
            fileTable.Remove(fcb.fileNo);
        }

        public FCB GetFCB(int fileNo)
        {
            if (fileTable.ContainsKey(fileNo))
            {
                return fileTable[fileNo];
            }
            else
                return null;
        }


    }
}
