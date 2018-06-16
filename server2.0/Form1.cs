using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace server2._0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //是否选择连接数据库
        public bool isConnectSQL()
        {
            if (radioButton1.Checked)
            {
                return false;
            }
            return true;
        }
        //添加日志
        public void addText(string s)//location:对应窗口
        {
            //通过等待异步，在不发生跨线程调用异常的情况下完成多线程对winform多线程控件的控制
            richTextBox1.BeginInvoke(new Action(() =>
            {
                richTextBox1.BeginInvoke(new Action(() =>
                {
                    richTextBox1.AppendText(s);
                    richTextBox1.AppendText("\n");
                }));
            }));
        }
        //添加在线成员列表
        public void addListItem(string s)
        {
            listBox1.BeginInvoke(new Action(() =>
            {
                listBox1.Items.Add(s);
            }));
        }
        //删除在线成员列表
        public void delListItem(string s)
        {
            listBox1.BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    if (listBox1.Items[i].ToString() == s)
                    {
                        listBox1.Items.RemoveAt(i);
                        break;
                    }
                }
            }));
        }
        //消息提示
        public void showMessageBox(string s)
        {MessageBox.Show(s);}

        //启动服务器
        private void button1_Click(object sender, EventArgs e)
        {
            if (business.startServer())
            {
                richTextBox1.Text = "服务启动成功...\n";
                button1.Enabled = false;
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
                if (button3.Text == "返回")
                {
                    button3_Click(sender,e);
                }
                button3.Enabled = false;
            }
            else
            {
                richTextBox1.Text = "服务启动失败...\n";
            }
            //获取当前数据库数据
            button5_Click(sender,e);
        }
        //强制下线
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {
                return;
            }
            business.logout(listBox1.SelectedItem.ToString());
        }
        //刷新数据库列表记录
        private void button5_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            DataTable dt = sqlserver.SQLselect("SELECT name FROM account");
            if (dt == null)
            {
                richTextBox1.AppendText("数据库连接错误\n");
                button5.Enabled = false;
                MessageBox.Show("数据库连接错误！");
                return;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                listBox2.Items.Add(dt.Rows[i]["name"].ToString());
            }
        }
        //退出
        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "返回")
            {
                this.Height -= 230;
                button3.Text = "修改数据库地址及账号";
            }
            else
            {
                this.Height += 230;
                button3.Text = "返回";
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                business.port = int.Parse(textBox4.Text.Trim());
            }
            catch (Exception)
            {
                MessageBox.Show("端口错误！");
                this.richTextBox1.AppendText("修改端口及数据库信息失败......\n");
            }
            sqlserver.changeAddress(textBox1.Text.Trim(), textBox2.Text.Trim(), textBox3.Text.Trim());
            button3_Click(sender,e);
            this.richTextBox1.AppendText("修改端口及数据库信息成功......\n");
        }


        
    }
}
