using System;
using System.Collections.Generic;

namespace TinyFileSystem
{
    // File Control Block
    // 文件控制块, 包含文件基本信息, 用于表示文件或目录
    // 不包含块分配表, 放在File中
    // 使用双向长子-兄弟表示法表示树形结构, 
    [Serializable]
    public class FCB
    {
        public enum FileType { directory, regular };
        public static int fileCounter = 0; // 文件总数

        public int fileNo; // 文件号
        public string name; // 文件名
        public FileType type; // 文件类型
        public int size; // 文件大小(字节)
        public DateTime createTime; // 创建时间
        public IndexTable blockTable; // 块分配图

        public FCB child = null, parent = null; // 指向父目录和子目录的指针
        public FCB next = null, pre = null; // 指向左右兄弟的指针
        
        public string Path {
            get
            {
                string path = this.name;
                FCB fcb = this.parent;
                while (fcb != null)
                {
                    path = fcb.name + (fcb.parent == null ? "://" : "/") + path;
                    fcb = fcb.parent;
                }
                return path;
            }
        }

        public FCB(string fileName, FileType fileType)
        {
            this.fileNo = fileCounter++;
            this.name = fileName;
            this.type = fileType;
            this.size = 0;
            this.createTime = DateTime.Now;

            if (fileType == FileType.regular)
            {
                this.blockTable = new IndexTable();
            }
        }

        public FCB(FCB origin)
        {
            this.fileNo = fileCounter++;
            this.name = origin.name;
            this.type = origin.type;
            this.size = origin.size;
            this.createTime = origin.createTime;

            if (origin.type == FileType.regular)
            {
                this.blockTable = new IndexTable();
                List<int> indexs = origin.blockTable.GetBlockList();
                string content = "";
                foreach (int index in indexs)
                {
                    content += MainWindows.bitMap.GetBlock(index);
                }
                this.blockTable = MainWindows.bitMap.CreateBlockTable(content);
            }
            else
            {
                FCB child = origin.child;
                while (child!=null)
                {
                    this.AppendChild(new FCB(child));
                    child = child.next;
                }
            }
        }

        public void AppendChild(FCB child)
        {
            if (this.child == null)
            {
                this.child = child;
                child.parent = this;
            }
            else
            {
                FCB temp = this.child;
                while (temp.next != null)
                {
                    temp = temp.next;
                }
                temp.next = child;
                child.pre = temp;
                child.parent = this;
            }
        }

        // 从链表中删除自己
        public void Remove()
        {
            if (parent != null && parent.child == this)
            {
                parent.child = next;
            }
            else if (pre != null)
            {
                pre.next = next;
            }
        }
    }
}
