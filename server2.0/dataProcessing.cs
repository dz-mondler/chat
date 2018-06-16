using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Data;

namespace server2._0
{
    public class dataProcessing
    {
        public  Socket socket;
        public string userName;
        //构造函数，初始化socket
        public dataProcessing(Socket socket)
        {
            this.socket = socket;
        }
        //type:发送类型(发送给自己\发送给另一个账号\广播) data:发送数据(包括完整的类型头及数据) receiver:另一个账号名
        public bool sendData(string type, string data,string receiver=null)//对发送数据进行处理,num:数据类型;data:发送的数据
        {
            try
            {
                switch (type)
                {
                        //发送给自己(获取配置信息等)
                    case "MYSELF":
                        socket.Send(UTF8Encoding.UTF8.GetBytes(data));
                        break;
                        //发送给另一个账号(私聊)
                    case "ANOTHERUSER":
                        foreach (dataProcessing item in business.getDictionary().Values)
                        {
                            if (item.userName == receiver)
                            {
                                item.socket.Send(UTF8Encoding.UTF8.GetBytes(data));
                                socket.Send(UTF8Encoding.UTF8.GetBytes(data));
                                return true;
                            }
                        }
                        break;
                        //广播
                    case "BROADCAST":
                        foreach (Socket item in business.getDictionary().Keys)
                        {
                            item.Send(UTF8Encoding.UTF8.GetBytes(data));
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (SocketException)
            {return false;}
            return true;
        }
        public  bool receiveData()//对接收的数据进行处理
        {
            if (socket == null) return false;
            byte[] receiveByte = new byte[1024];
            try
            {
                socket.Receive(receiveByte);
            }
            catch (Exception)
            {
                return false;
            }
            string[] data = null;
            string receiveString = UTF8Encoding.UTF8.GetString(receiveByte);
            //拆分消息
            data = receiveString.Split('$');
            switch (data[0])
            {
                //消息类型0:申请登录/注册账号
                case "0":
                    string user = data[1];
                    string password = data[2];
                    //检查用户是否已登录
                    if (!business.existName(data[1]))
                    {
                        sendData("MYSELF", "0$NAMEONLINE$");
                        return false;
                    }
                    //调用方法连接数据库查看用户名是否正确
                    if (business.isConnectSQL())//返回true:需要连接数据库验证账号及密码
                    {
                        DataTable da = sqlserver.SQLselect("SELECT password FROM account WHERE name='" + user + "'");
                        if (da.Rows.Count <= 0)
                        {
                            sendData("MYSELF", "0$NOTEXISTNAME$");
                            return false;
                        }
                        if (da.Rows[0]["password"].ToString() != password)
                        {
                            sendData("MYSELF", "0$WRONGPASSWORD$");
                            return false;
                        }
                        
                    }
                    //=================================
                    if (!sendData("MYSELF", "0$SUCCESS$")) return false;
                    this.userName = user;
                    sendData("BROADCAST","5$"+userName+"$");
                    business.addText(user + "[" + DateTime.Now.ToString() + "]已登录...");
                    business.addDictionary(this);
                    Thread thread = new Thread(Receive);
                    thread.IsBackground = true;
                    thread.Start();
                    break;
                //消息类型1:发送公共消息(广播)
                case "1"://数据类型1$sender$消息长度$消息内容$
                    sendData("BROADCAST", receiveString);
                    break;
                //消息类型2:私聊
                case "2"://数据类型2$sender$receiver$消息长度$消息内容$
                    sendData("ANOTHERUSER", receiveString,data[2]);
                    break;
                //申请账号列表    
                case "3":
                     string s = "3$";
                    foreach (dataProcessing item in business.getDictionary().Values)
	                {
                        s += item.userName;
                        s += "$"; 
	                }
                    sendData("MYSELF",s);
                    break;
                //申请注册    
                case "4":
                    user = data[1];
                    password = data[2];
                    DataTable dt = sqlserver.SQLselect("SELECT password FROM account WHERE name='" + user + "'");
                    if (dt.Rows.Count > 0)
                    {
                        sendData("MYSELF", "0$SAMENAME$");
                        return false;
                    }
                    sqlserver.SQLupdate("INSERT INTO account VALUES('" + user + "','" + password + "')");
                    sendData("MYSELF", "0$REGISTERSUCCESS$");
                    break;
                //退出登录
                case "404":
                    return false;
                default:
                    break;
            }
            return true;
        }
        //开始对该端口进行监听，当receiveData返回false时，则断开连接
        private void Receive()
        {
            while (true)
            {
                if (!receiveData())
                {
                    string s = "6$"+userName+"$";
                    sendData("BROADCAST",s);
                    business.addText(userName + "[" + DateTime.Now.ToString() + "]:退出登录");
                    business.removeDictionary(this);
                    return;
                } 
            }
        }
    }
}
