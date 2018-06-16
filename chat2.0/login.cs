using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace chat2._0
{
    public partial class login : Form
    {
        private Point mousePoint = new Point();//用于鼠标移动窗口
        private Point formLocation;//窗口位置
        bool isConnect = false;//判断是否连接上服务器
        public chat beginChat;//主界面窗口
        public login()
        {
            InitializeComponent();
            dataProcessing.setlogin(this);//提供后续数据接收与发送操作反馈结果的操作
        }
        //窗体初始化，并尝试连接服务器
        private void login_Load(object sender, EventArgs e)
        {
            //加载按钮图片
            pictureBox2.Image = imageList1.Images[0];
            //============================
            label3.Parent = pictureBox2;
            label4.Parent = panel1;
            label3.Location = new Point(pictureBox2.Width / 2 - label3.Width / 2, pictureBox2.Height / 2 - label3.Height / 2);
            label4.Location = new Point(panel1.Width - label4.Width, panel1.Height / 2 - 2 - label4.Height / 2);
            if (!dataProcessing.beginWork("login"))
            {
                label7.Text = "连接服务器失败......";
            }
        }
        //设置连接状态
        public void setConnect(bool situation)
        {
            isConnect = situation;
        }
        //设置窗体底部label状态显示
        public void setFooterSituation(string s)
        {
            label7.BeginInvoke(new Action(() =>
            {
                label7.Text = s;
            }));
        }
        //显示消息框
        public void showMessagebox(string s)
        {
            MessageBox.Show(s);
        }
        //==========按钮点击事件==========
        //注册、登录或修改按钮
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            //修改服务器信息，并调用函数重新连接服务器
            if (label3.Text == "修改")
            {
                dataProcessing.address(textBox1.Text.Trim(), int.Parse(textBox2.Text.Trim()));
                label7.Text = "正在重新连接服务器......";
                isConnect = false;
                dataProcessing.beginWork("login");
                label5_Click(sender,e);
                MessageBox.Show("修改成功，请登录");
                return;
            }
            //判断是否连接上服务器，如果没有，则取消后面操作
            if (!isConnect)
            {
                MessageBox.Show("未连接到服务器,无法进行操作");
                return;
            }
            //用户名及密码不能为空
            if (textBox1.Text.Trim() == "" || textBox2.Text.Trim() == "")
            {
                label8.Text = "请输入用户名及密码!";
                return;
            }
            //用户名及密码不能包含“$”，方便解析数据时的操作
            if (textBox1.Text.Contains('$') || textBox1.Text.Contains('$'))
            {
                MessageBox.Show("用户名或密码不能包含'$'");
                return;
            }
            //data[0]:用户名  data[1]:已加密的密码
            string[] data = new string[2];
            data[0] = textBox1.Text;//用户名
            //==============调用系统自带MD5函数进行加密====================
            byte[] result = Encoding.Default.GetBytes(textBox2.Text);    //textBox2为输入的密码
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            data[1] = BitConverter.ToString(output).Replace("-", "");  //输出加密文本赋给data[1]
            //注册账号时发送的消息
            if (label3.Text == "注册")//注册情况
            {
                if (!dataProcessing.sendData(4, data))
                {
                    MessageBox.Show("发送数据时出现错误,发送失败.");
                    return;
                }
            }
            //登录账号时发送的消息
            else if (!dataProcessing.sendData(0, data))//登录情况
            {
                MessageBox.Show("发送数据时出现错误,发送失败.");
                return;
            }
            //==========接收服务器反馈信息==========
            string[] receiveData = dataProcessing.receiveData();
            if (receiveData == null)
            {
                MessageBox.Show("接收数据时出现错误.");
                return;
            }
            //登录时接收到的代码
            if (receiveData[1]=="NOTEXISTNAME")
            {
                MessageBox.Show("不存在该用户名,请先注册");
                dataProcessing.beginWork("login");
                return;
            }
            else if(receiveData[1]=="WRONGPASSWORD")
            {
                MessageBox.Show("密码错误,请重新输入");
                dataProcessing.beginWork("login");
                return;
            }
            else if (receiveData[1]=="NAMEONLINE")
            {
                MessageBox.Show("该账号已登录，登录失败");
                return;
            }
            //登录成功
            else if (receiveData[1] == "SUCCESS")
            {
                this.Visible = false;
                beginChat = new chat(textBox1.Text.Trim());
                beginChat.Visible = true;
            }
            //注册时接收到的消息
            else if (receiveData[1]=="SAMENAME")
            {
                MessageBox.Show("该账号已存在,请输入其他账号进行注册");
                dataProcessing.beginWork("login");
                return;
            }
            else if (receiveData[1] == "REGISTERSUCCESS")
            {
                MessageBox.Show("账号注册成功,请登录");
                label3.Text = "Login";
                label6.Text = "注册";
                label5.Visible = true;
                label3.Location = new Point(pictureBox2.Width / 2 - label3.Width / 2, pictureBox2.Height / 2 - label3.Height / 2);
                dataProcessing.beginWork("login");
                return;
            }
        }
        //点击主按钮
        private void label3_Click(object sender, EventArgs e)
        {
            pictureBox2_Click(sender, e);
        }
        //右下角注册按钮
        private void label6_Click(object sender, EventArgs e)
        {
            if (label6.Text == "注册")
            {
                label3.Text = "注册";
                label6.Text = "返回";
                label5.Visible = false;
            }
            else
            {
                label3.Text = "Login";
                label6.Text = "注册";
                label5.Visible = true;
            }
            label3.Location = new Point(pictureBox2.Width / 2 - label3.Width / 2, pictureBox2.Height / 2 - label3.Height / 2);
        }
        //左下角修改服务器按钮
        private void label5_Click(object sender, EventArgs e)
        {
            if (label5.Text == "修改服务器地址")
            {
                label5.Text = "返回";
                label3.Text = "修改";
                label1.Text = "地址：";
                label2.Text = "端口：";
                textBox1.Text = "127.0.0.1";
                textBox2.Text = "8081";
                label6.Visible = false;
            }
            else
            {
                label5.Text = "修改服务器地址";
                label3.Text = "登录";
                label1.Text = "账号:";
                label2.Text = "密码：";
                textBox1.Text = "";
                textBox2.Text = "";
                label6.Visible = true;
            }
            label3.Location = new Point(pictureBox2.Width / 2 - label3.Width / 2, pictureBox2.Height / 2 - label3.Height / 2);
        }
        //====================事件处理(UI)=========================
        //登录按钮
        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Image = imageList1.Images[1];
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Image = imageList1.Images[0];
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox2.Image = imageList1.Images[2];
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox2.Image = imageList1.Images[1];
        }
       
        //登录label
        private void label3_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2_MouseEnter(sender,e);
        }

        private void label3_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox2_MouseDown(sender,e);
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2_MouseLeave(sender,e);
        }

        private void label3_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox2_MouseUp(sender,e);
        }

        //左上角退出程序按钮
        private void label4_MouseEnter(object sender, EventArgs e)
        {
            label4.ForeColor = Color.FromArgb(100, Color.Black);
        }

        private void label4_MouseLeave(object sender, EventArgs e)
        {
            label4.ForeColor = Color.FromArgb(100, Color.White);
        }

        private void label4_MouseDown(object sender, MouseEventArgs e)
        {
            label4.ForeColor = Color.FromArgb(100, 50, 90, 220);
        }

        private void label4_MouseUp(object sender, MouseEventArgs e)
        {
            label4.ForeColor = Color.FromArgb(100, 100, 180, 241);
        }

        private void label4_MouseClick(object sender, MouseEventArgs e)
        {
            System.Environment.Exit(0);//强制关闭所有窗口及线程
        }

        //顶部移动窗体事件
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            this.mousePoint = Control.MousePosition;
            this.formLocation = this.Location;
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point p = new Point(formLocation.X + Control.MousePosition.X - mousePoint.X, formLocation.Y + Control.MousePosition.Y - mousePoint.Y);
                this.Location = p;
            }
        }
        //背面移动窗体事件
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            this.mousePoint.X = e.X;
            this.mousePoint.Y = e.Y;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Top = Control.MousePosition.Y - mousePoint.Y;
                this.Left = Control.MousePosition.X - mousePoint.X;
            }
        }
        //右下角注册按钮事件
        private void label6_MouseEnter(object sender, EventArgs e)
        {
            label6.ForeColor = Color.FromArgb(100, 100, 180, 241);
        }

        private void label6_MouseDown(object sender, MouseEventArgs e)
        {
            label6.ForeColor = Color.FromArgb(100, 50, 90, 220);
        }

        private void label6_MouseUp(object sender, MouseEventArgs e)
        {
            label6.ForeColor = Color.FromArgb(100, 100, 180, 241);
        }

        private void label6_MouseLeave(object sender, EventArgs e)
        {
            label6.ForeColor = Color.FromArgb(100, Color.White);
        }

        //忘记密码按钮事件
        private void label5_MouseEnter(object sender, EventArgs e)
        {
            label5.ForeColor = Color.FromArgb(100, 100, 180, 241);
        }

        private void label5_MouseLeave(object sender, EventArgs e)
        {
            label5.ForeColor = Color.FromArgb(100, Color.White);
        }

        private void label5_MouseUp(object sender, MouseEventArgs e)
        {
            label5.ForeColor = Color.FromArgb(100, 100, 180, 241);
        }

        private void label5_MouseDown(object sender, MouseEventArgs e)
        {
            label5.ForeColor = Color.FromArgb(100, 50, 90, 220);
        }

        private void login_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                pictureBox2_Click(sender,e);
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                pictureBox2_Click(sender, e);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                textBox2.Focus();
            }
        }
    }
}
