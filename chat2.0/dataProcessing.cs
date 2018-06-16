using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

//处理传输的数据(包括发送与接收)
namespace chat2._0
{
    static class dataProcessing
    {
        //连接服务器套接字
        private static Socket server;
        //服务器地址
        private static IPAddress IP = IPAddress.Parse("127.0.0.1");//服务器地址
        private static int port = 8081;//服务器端口号
        private static chat myChat = null;//提供一些公共方法
        private static login myLogin = null;
        //窗口初始化时初始化该静态成员
        public static void setChat(chat s)
        {myChat = s;}
        public static void setlogin(login l)
        {myLogin = l;}
        //修改服务器地址及端口号
        public static void address(string ip,int por)
        {
            IP = IPAddress.Parse(ip);
            port = por;
        }
        //发送数据（不同于服务端的sendData）
        public static bool sendData(int num, string[] data)//对发送数据进行处理,num:数据类型;data:发送的数据
        {
            if (server == null) return false;
            string sendData = "";

            switch (num)
            {
                //登录时验证账号密码:
                //data[0]:账号    data[1]:密码
                case 0://格式:数据类型0$用户名$密码$
                    sendData = num.ToString() + "$" + 
                          data[0] + "$" + data[1] + "$";
                    break;
                //发送公共消息
                //data[0]:消息内容
                case 1://格式:数据类型1$sender$消息长度$消息内容$
                    sendData = num.ToString() + "$" + myChat.getUserName() +"$" + data[0].Length + "$" + data[0] + "$";
                    break;
                //私聊
                //data[0]:receiver    data[1]:消息内容
                case 2://格式:数据类型2$sender$receiver$消息长度$消息内容$
                    sendData = num.ToString() + "$" + myChat.getUserName() +
                        "$" + data[0] + "$" + data[1].Length.ToString() + "$" + data[1] + "$";
                    break;
                //获取在线用户列表
                //data[0]:代码
                case 3://格式:数据类型3$代码("GETONLINE":获取在线用户列表)	
                    sendData = num.ToString() + "$";
                    break;
                case 4:
                    sendData = num.ToString() + "$" + data[0] + "$" + data[1] + "$";
                    break;
                case 5:
                    myChat.addText("公共聊天室",data[1]+"已登录");
                    myChat.addListBox(data[1]);
                    break;
                case 404:
                    sendData = "404$";
                    break;
                default:
                    return false;
            }
            try
            {
                server.Send(UTF8Encoding.UTF8.GetBytes(sendData));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        //对接收的数据进行处理
        public static string[] receiveData()
        {
            string[] data = null;
            if (server == null) return data;
            
            byte[] receiveByte = new byte[1024];
            try
            {
                server.Receive(receiveByte);
            }
            catch (Exception)
            {
                data = null;
                return data;
            }
            string receiveString = UTF8Encoding.UTF8.GetString(receiveByte);
            //拆分消息
            data = receiveString.Split('$');
            //选择对应消息种类进行处理
            switch (data[0])
            {
                    //消息类型0:是否允许登录账号
                case "0":
                    //直接返回已分段的消息
                    break;
                case "1"://1$sender$textLength$text$
                    string sender = data[1];
                    int textLength = int.Parse(data[2]);
                    string text = receiveString.Substring(receiveString.IndexOf('$', data[0].Length + data[1].Length + 2) + 1, textLength);
                    string result = sender+"["+DateTime.Now.ToString()+"]:\n"+text;
                    myChat.addText("公共聊天室",result);
                    break;
                    //私聊
                case "2"://数据类型2$sender$receiver$消息长度$消息内容$
                    result = data[1]+"["+DateTime.Now.ToString()+"]:\n"+receiveString.Substring(data[0].Length + data[1].Length + data[2].Length + data[3].Length + 4, int.Parse(data[3]));
                    if (data[1] == myChat.getUserName())
                    {
                        myChat.addText(data[2], result);
                    }
                    else
                    {
                        myChat.addText(data[1], result);
                    }
                    
                    break;
                case "3":
                    for (int i = 1; i < data.Length-1; i++)
                    {
                        myChat.addListBox(data[i]);
                    }
                    break;
                case "5":
                    myChat.addListBox(data[1]);
                    break;
                case "6":
                    myChat.delListBox(data[1]);
                    break;
                case "404":
                    sendData(404, null);
                    myChat.showMessageBox("您已被强制下线");
                    data = null;
                    break;
                default:
                    break;
            }
            return data;
        }
        //开始工作
        //login:新建后台线程连接服务端并修改界面状态 
        //chat:新建后台线程与服务端传输数据
        public static bool beginWork(string choice)
        {
            if (myChat == null && myLogin == null) return false;
            Thread thread = null;
            switch (choice)
            {
                case "login"://login
                    thread = new Thread(connectServer);
                    break;
                case "chat"://chat
                    thread = new Thread(Receive);
                    break;
                default:
                    return false;
            }
            thread.IsBackground = true;
            thread.Start();
            return true;
        }
        //连接服务端并修改界面状态
        private static void connectServer()
        {
            if (myLogin == null) return;
            try
            {
                server = new Socket(SocketType.Stream, ProtocolType.Tcp);
                server.Connect(IP, port);
            }
            catch (SocketException)
            {
                myLogin.setConnect(false);
                myLogin.setFooterSituation("无法连接到服务器.");
                return;
            }
            myLogin.setConnect(true);
            myLogin.setFooterSituation("已连接到服务器,请进行操作.");
        }
        //与服务端传输数据，当receiveData返回值为null时，则跳出循环，停止接收数据
        private static void Receive()
        {
            if (myChat == null) return;
            while (true)
            {
                string[] data = dataProcessing.receiveData();
                if (data == null)
                {
                    myChat.setBreakSituation();
                    return;//404
                }
            }
        }
    }
}
