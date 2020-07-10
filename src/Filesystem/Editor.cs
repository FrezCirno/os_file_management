using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TinyFileSystem
{
    public partial class Editor : Form
    {
        private readonly FCB fcb;
        private readonly BitMap bitMap = MainWindows.bitMap;
        private bool dirt = false;

        public Editor()
        {
            InitializeComponent();
        }

        public Editor(FCB fcb)
        {
            InitializeComponent();
            this.fcb = fcb;
            this.Text = fcb.name;
            ShowContent();
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteDisk();
            dirt = false;
            this.Text = fcb.name;
        }

        private void ShowContent()
        {
            List<int> indexs = fcb.blockTable.GetBlockList();
            string content = "";
            foreach (int i in indexs)
            {
                content += bitMap.GetBlock(i);
            }
            richTextBox1.Text = content;
        }

        private void WriteDisk()
        {
            string content = richTextBox1.Text;
            fcb.size = content.Length;

            // 先清空原来的所有数据块
            List<int> indexs = fcb.blockTable.GetBlockList();
            bitMap.FreeBlock(indexs);
            
            // 重新写入
            fcb.blockTable = bitMap.CreateBlockTable(content);
        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {
            dirt = true;
            Text = "*" + fcb.name;
            toolStripStatusLabel2.Text = "第" + (1 + richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart)).ToString() + "行";
            toolStripStatusLabel3.Text = "第" + (1 + richTextBox1.SelectionStart - richTextBox1.GetFirstCharIndexFromLine(richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart))).ToString() + "列";
            toolStripStatusLabel4.Text = "文件大小：" + richTextBox1.Text.Length.ToString() + " 字节";
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dirt && MessageBox.Show("是否保存文件？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                WriteDisk();
            }
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Code by 谭梓煊 (1853434)", "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void 粗体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Font.Style.HasFlag(FontStyle.Bold))
            {
                richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Regular);
            }
            else
            {
                richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Bold);
            }
        }

        private void 斜体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Font.Style.HasFlag(FontStyle.Italic))
            {
                richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Regular);
            }
            else
            {
                richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Italic);
            }
        }

        private void 增大字号ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float nowSize = richTextBox1.Font.Size;
            richTextBox1.Font = new Font(richTextBox1.Font.FontFamily, nowSize + 2);
        }

        private void 减小字号ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float nowSize = richTextBox1.Font.Size;
            richTextBox1.Font = new Font(richTextBox1.Font.FontFamily, nowSize - 2);
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void richTextBox1_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = "第" + (1 + richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart)).ToString() + "行";
            toolStripStatusLabel3.Text = "第" + (1 + richTextBox1.SelectionStart - richTextBox1.GetFirstCharIndexFromLine(richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart))).ToString() + "列";
        }
    }
}
