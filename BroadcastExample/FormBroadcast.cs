using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BroadcastExample
{
    public partial class FormBroadcast : Form
    {
        delegate void AppendStringCallback(string text);
        AppendStringCallback appendStringCallback;
        //使用的接收端口号
        private int port = 8001;
        private UdpClient udpClient;
        public FormBroadcast()
        {
            InitializeComponent();
            appendStringCallback = new AppendStringCallback(AppendString);
        }
        private void AppendString(string text)
        {
            if (richTextBox1.InvokeRequired == true)
            {
                this.Invoke(appendStringCallback, text);
            }
            else
            {
                richTextBox1.AppendText(text + "\r\n");
            }
        }
        /// <summary>
        /// 在后台运行的接收线程
        /// </summary>
        private void ReceiveData()
        {
            //在本机指定的端口接收
            udpClient = new UdpClient(port);
            IPEndPoint remote = null;
            //接收从远程主机发送过来的信息；
            while (true)
            {
                try
                {
                    //关闭udpClient 时此句会产生异常
                    byte[] bytes = udpClient.Receive(ref remote);
                    string str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    AppendString(string.Format("来自{0}：{1}", remote, str));
                }
                catch
                {
                    //退出循环，结束线程
                    break;
                }
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            UdpClient myUdpClient = new UdpClient();
            try
            {
                //让其自动提供子网中的IP 广播地址
                IPEndPoint iep = new IPEndPoint(IPAddress.Broadcast, 8001);
                //允许发送和接收广播数据报
                myUdpClient.EnableBroadcast = true;
                //将发送内容转换为字节数组
                byte[] bytes = Encoding.UTF8.GetBytes(textBox1.Text);
                //向子网发送信息
                myUdpClient.Send(bytes, bytes.Length, iep);
                textBox1.Clear();
                textBox1.Focus();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "发送失败");
            }
            finally
            {
                myUdpClient.Close();
            }
        }

        private void FormBroadcast_Load(object sender, EventArgs e)
        {
            Thread myThread = new Thread(new ThreadStart(ReceiveData));
            //将线程设为后台运行
            myThread.IsBackground = true;
            myThread.Start();
        }

        private void FormBroadcast_FormClosing(object sender, FormClosingEventArgs e)
        {
            udpClient.Close();
        }
    }
}
