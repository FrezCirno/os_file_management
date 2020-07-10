using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace TinyFileSystem
{
    public partial class MainWindows : Form
    {
        public FCB root; // 根目录
        public FCB currentDirectory; // 当前目录
        public static BitMap bitMap; // 全局位图
        public FCBTable fcbTable;
        public List<FCB> clipBoard = new List<FCB>();

        private readonly Stack<FCB> pastStack = new Stack<FCB>(); // 路径栈
        private readonly Stack<FCB> furtStack = new Stack<FCB>(); // 路径栈

        public MainWindows()
        {
            InitializeComponent();
            try
            {
                Deserialize();
            }
            catch (FileNotFoundException)
            {
                root = new FCB("我的文件", FCB.FileType.directory);
                fcbTable = new FCBTable();
                bitMap = new BitMap();
            }
            currentDirectory = root;
            InitializeView();
            UpdateView();
        }

        public void InitializeView()
        {
            listView1.Items.Clear();
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(new TreeNode("我的文件"));
            toolStripStatusLabel1.Text = "系统总空间：" + (BitMap.capacity * Block.blockSize / 1024).ToString() + "KB";
        }

        public void UpdateView()
        {
            comboBox1.Text = currentDirectory.Path;

            UpdateTreeView();
            UpdateListView(currentDirectory);

            int used = bitMap.size * Block.blockSize;
            string usedString = (used > 1024 ? (used / 1024).ToString() + "KB" : used.ToString() + "B");
            toolStripStatusLabel2.Text = "已用空间：" + usedString;

            int rest = (BitMap.capacity - bitMap.size) * Block.blockSize;
            string restString = (rest > 1024 ? (rest / 1024).ToString() + "KB" : rest.ToString() + "B");
            toolStripStatusLabel3.Text = "剩余空间：" + restString;
        }

        // 更新左边的树形视图
        public void UpdateTreeView()
        {
            treeView1.Nodes.Clear();
            TreeNode rootNode = new TreeNode("我的文件");
            rootNode.Tag = root;
            NodeDFS(rootNode, root);
            rootNode.Expand();
            treeView1.Nodes.Add(rootNode);
        }

        // 更新右边的列表视图
        public void UpdateListView(FCB fcb)
        {
            listView1.Items.Clear();
            FCB child = fcb.child;
            while (child != null)
            {
                ListViewItem listViewItem = new ListViewItem(new string[] {
                        child.name,
                        child.size.ToString() + " 字节",
                        (child.type == FCB.FileType.regular ? "普通文件" : "文件夹"),
                        child.createTime.ToString()
                    });
                listViewItem.Tag = child;
                listViewItem.ImageIndex = (child.type == FCB.FileType.directory ? 0 : 1);
                listView1.Items.Add(listViewItem);
                child = child.next;
            }
        }

        // DFS所有目录节点
        private void NodeDFS(TreeNode treeNode, FCB fcb)
        {
            FCB child = fcb.child;
            while (child != null)
            {
                if (child.type == FCB.FileType.directory)
                {
                    TreeNode newNode = new TreeNode(child.name, 0, 0);
                    newNode.Tag = child;
                    NodeDFS(newNode, child);
                    newNode.Expand();
                    treeNode.Nodes.Add(newNode);
                }
                else
                {
                    TreeNode newNode = new TreeNode(child.name, 1, 1);
                    newNode.Tag = child;
                    treeNode.Nodes.Add(newNode);
                }
                child = child.next;
            }
        }

        // 检查文件是否有重名, 如果有的话返回添加序号后的文件名
        private string NameCheck(string s)
        {
            string[] nameExt = s.Split('.');

            string name = nameExt[0]; // "新建文件夹(1)" "新建文本文件(1)"
            string ext = nameExt.ElementAtOrDefault(1); // "txt"

            int counter = 0;
            FCB child = currentDirectory.child;
            while (child != null)
            {
                int lastP = child.name.LastIndexOf('(');
                if (lastP == -1) lastP = child.name.Length;
                string childBaseName= child.name.Substring(0, lastP); // "新建文件夹(1)" "新建文本文件(1)"
                if (childBaseName != child.name && childBaseName == name)
                {
                    counter++;
                }
                else if (child.name == s)
                {
                    counter++;
                }
                child = child.next;
            }
            if (counter > 0)
            {
                name += "(" + counter.ToString() + ")";
            }
            if (ext != null)
            {
                name += "." + ext;
            }
            return name;
        }

        private void 打开ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ListViewItem currentItem;
            if (listView1.SelectedItems.Count != 0)
            {
                currentItem = listView1.SelectedItems[0];
            }
            else
            {
                MessageBox.Show("请首先选择一个文件或文件夹！");
                return;
            }

            FCB fcb = (FCB)currentItem.Tag;

            Open(fcb);
        }

        public void Serialize()
        {
            FileStream fileStream;
            BinaryFormatter b = new BinaryFormatter();
            string dir = Directory.GetCurrentDirectory();

            fileStream = new FileStream(Path.Combine(dir, "root.dat"), FileMode.Create);
            b.Serialize(fileStream, root);
            fileStream.Close();

            fileStream = new FileStream(Path.Combine(dir, "fcbTable.dat"), FileMode.Create);
            b.Serialize(fileStream, fcbTable);
            fileStream.Close();

            fileStream = new FileStream(Path.Combine(dir, "bitMap.dat"), FileMode.Create);
            b.Serialize(fileStream, bitMap);
            fileStream.Close();
        }

        public void Deserialize()
        {
            FileStream fileStream;
            BinaryFormatter b = new BinaryFormatter();
            string dir = Directory.GetCurrentDirectory();

            fileStream = new FileStream(Path.Combine(dir, "root.dat"), FileMode.Open, FileAccess.Read, FileShare.Read);
            root = b.Deserialize(fileStream) as FCB;
            fileStream.Close();

            fileStream = new FileStream(Path.Combine(dir, "fcbTable.dat"), FileMode.Open, FileAccess.Read, FileShare.Read);
            fcbTable = b.Deserialize(fileStream) as FCBTable;
            fileStream.Close();

            fileStream = new FileStream(Path.Combine(dir, "bitMap.dat"), FileMode.Open, FileAccess.Read, FileShare.Read);
            bitMap = b.Deserialize(fileStream) as BitMap;
            fileStream.Close();
        }

        public void MainWindows_Closing(object sender, EventArgs e)
        {
            Serialize();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Code by 谭梓煊 (1853434)", "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void 使用帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请参阅项目文档", "使用帮助", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file_name = NameCheck("新建文本文件.txt");

            // 创建一个FCB
            FCB fcb = new FCB(file_name, FCB.FileType.regular);
            currentDirectory.AppendChild(fcb);

            // 将FCB加入到FCB表中
            fcbTable.Add(fcb);

            UpdateView();
        }

        private void 文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file_name = NameCheck("新建文件夹");

            // 创建一个FCB
            FCB fcb = new FCB(file_name, FCB.FileType.directory);
            currentDirectory.AppendChild(fcb);

            // 将FCB加入到FCB表中
            fcbTable.Add(fcb);

            UpdateView();
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请首先选择一个文件或文件夹！");
                return;
            }
            foreach (ListViewItem currentItem in listView1.SelectedItems)
            {
                FCB fcb = (FCB)currentItem.Tag;

                if (fcb.blockTable != null)
                {
                    List<int> indexs = fcb.blockTable.GetBlockList();
                    bitMap.FreeBlock(indexs);
                }

                fcb.Remove();
                fcbTable.Remove(fcb);
            }

            UpdateView();
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请首先选择一个文件或文件夹！");
                return;
            }

            ListViewItem currentItem = listView1.SelectedItems[0];
            FCB fcb = (FCB)currentItem.Tag;
            if (fcb != null)
            {
                Open(fcb);
            }
        }

        private void Open(FCB fcb, bool isBack = false)
        {
            switch (fcb.type)
            {
                case FCB.FileType.directory:
                    (isBack ? furtStack : pastStack).Push(currentDirectory);
                    currentDirectory = fcb;
                    comboBox1.Text = currentDirectory.Path;
                    UpdateListView(fcb);
                    break;
                case FCB.FileType.regular:
                    Editor fileEditor = new Editor(fcb);
                    fileEditor.FormClosed += (s, e) => { this.UpdateView(); };
                    fileEditor.Show();
                    break;
                default:
                    break;
            }
        }

        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem currentItem;
            if (listView1.SelectedItems.Count != 0)
            {
                currentItem = listView1.SelectedItems[0];
            }
            else
            {
                MessageBox.Show("请首先选择一个文件或文件夹！");
                return;
            }

            FCB fcb = (FCB)currentItem.Tag;

            String newfilename = Interaction.InputBox("请输入文件名", "填写文件名");
            if (newfilename != "")
            {
                fcb.name = newfilename;
                UpdateView();
            }
        }

        private void 格式化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentDirectory = root = new FCB("我的文件", FCB.FileType.directory);
            bitMap = new BitMap();
            fcbTable = new FCBTable();
            UpdateView();
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Serialize();
        }

        // 后退
        private void Button1_Click(object sender, EventArgs e)
        {
            if (pastStack.Count > 0)
            {
                Open(pastStack.Pop(), true); 
                UpdateView();
            }
        }

        // 前进
        private void Button2_Click(object sender, EventArgs e)
        {
            if (furtStack.Count > 0)
            {
                Open(furtStack.Pop());
                UpdateView();
            }
        }

        // 向上
        private void Button4_Click(object sender, EventArgs e)
        {
            if (currentDirectory.parent == null)
                return;
            Open(currentDirectory.parent);
            UpdateView();
        }

        // 搜索
        private void Button3_Click(object sender, EventArgs e)
        {
            string search = textBox1.Text;
            foreach (ListViewItem item in this.listView1.Items)
            {
                if (item.SubItems[0].Text == search)
                {
                    item.Selected = true;
                    return;
                }
            }
        }

        private void TreeView1_NodeMouseClick_1(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode treeNode = e.Node;
                FCB fcb = (FCB)treeNode.Tag;
                if (fcb.type == FCB.FileType.directory)
                {
                    Open(fcb);
                }
            }
        }

        private void TreeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode treeNode = e.Node;
                FCB fcb = (FCB)treeNode.Tag;
                if (fcb.type == FCB.FileType.regular)
                {
                    Open(fcb);
                }
            }
        }

        private void 剪切ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clipBoard.Clear();
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请首先选择一个文件或文件夹！");
                return;
            }
            foreach (ListViewItem currentItem in listView1.SelectedItems)
            {
                FCB fcb = (FCB)currentItem.Tag;
                clipBoard.Add(new FCB(fcb));

                if (fcb.blockTable != null)
                {
                    List<int> indexs = fcb.blockTable.GetBlockList();
                    bitMap.FreeBlock(indexs);
                }

                fcb.Remove();
                fcbTable.Remove(fcb);
            }

            UpdateView();
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clipBoard.Clear();
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请首先选择一个文件或文件夹！");
                return;
            }
            foreach (ListViewItem currentItem in listView1.SelectedItems)
            {
                FCB fcb = (FCB)currentItem.Tag;
                clipBoard.Add(new FCB(fcb));
            }

            UpdateView();
        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (FCB fcb in clipBoard)
            {
                FCB newFCB = new FCB(fcb);
                newFCB.name = NameCheck(newFCB.name);
                currentDirectory.AppendChild(newFCB);
            }
            UpdateView();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
