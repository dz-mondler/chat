using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;


namespace server2._0
{
    static class business
    {
        private static Dictionary<Socket, dataProcessing> clientList;
        private static Socket server;
        private static Form1 form;
        public static int port = 8081;//提供修改操作
        //初始化变量及窗口
        public static void init()
        {
            clientList = new Dictionary<Socket, dataProcessing>();
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Application.Run(form);
        }
        //显示消息框
        public static void showMessage(string s)
        { form.showMessageBox(s); }
        //判断是否选择连接数据库
        public static bool isConnectSQL()
        { return form.isConnectSQL(); }
        //启动服务器
        public static bool startServer()
        {
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(50);
            Thread thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Start();
            return true;
        }
        //初始化连接
        private static void Listen()
        {
            while (true)
            {
                Thread thread = new Thread(login);
                Socket client = server.Accept();
                thread.Start(client);
            }
        }
        //后台程序监听该端口的登录情况
        private static void login(object obj)
        {
            Socket client = obj as Socket;
            dataProcessing listenData = new dataProcessing(client);
            listenData.receiveData();
        }
        //添加日志消息
        public static void addText(string s)
        {
            form.addText(s);
        }
        //强制下线
        public static void logout(string user)
        {
            
            foreach (dataProcessing item in clientList.Values)
            {
                if (item.userName == user)
                {
                    item.sendData("MYSELF","404$");
                }
            }
        }
        //返回字典
        public static Dictionary<Socket, dataProcessing> getDictionary()
        {
            Dictionary<Socket, dataProcessing> copyDic = new Dictionary<Socket, dataProcessing>(clientList);
            return copyDic; 
        }
        //添加在线账号
        public static void addDictionary(dataProcessing dataprocessing)
        {
            lock (new object())
            {
                clientList.Add(dataprocessing.socket, dataprocessing);
            }
            form.addListItem(dataprocessing.userName);
        }
        //删除在线账号
        public static void removeDictionary(dataProcessing s)
        {
            lock (new object())
            {
                clientList.Remove(s.socket);
            }
            form.delListItem(s.userName);
        }
        //查询该用户是否已登录，如果是，则返回false
        public static bool existName(string s)
        {
            foreach (dataProcessing item in clientList.Values)
            {
                if (item.userName == s)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
