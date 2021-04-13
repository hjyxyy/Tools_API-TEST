using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WeClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {


        private static Queue<string> messages = new Queue<string>();
        private static string url = "http://127.0.0.1:8888/msg/";
        private string BASE_REDIRECT_URL = "http://abc.com/123.php";

        private HttpListener _listener;
        public MainWindow()
        {
            InitializeComponent();
           
            initHttpServer();
            this.LAB_TXT.Content = "初始化完成";

        }
       
        

        public void initHttpServer()
        {
            _listener = new HttpListener();
            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _listener.Prefixes.Add(url);
            _listener.Start();
            messages.Enqueue("启用数据监听！");
            _listener.BeginGetContext(ListenerHandle, _listener);
            messages.Enqueue("已经成功监听: " + url);

        }

        private void ListenerHandle(IAsyncResult result)
        {
            try
            {
                // _listener = result.AsyncState as HttpListener;
                if (_listener.IsListening)
                {
                    _listener.BeginGetContext(ListenerHandle, result);
                    HttpListenerContext context = _listener.EndGetContext(result);
                    //解析Request请求
                    HttpListenerRequest request = context.Request;
                    string content = "";
                    switch (request.HttpMethod)
                    {
                        case "POST":
                            {
                                Stream stream = context.Request.InputStream;
                                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                                content = reader.ReadToEnd();
                            }
                            break;
                        case "GET":
                            {
                                var data = request.QueryString;
                            }
                            break;
                    }
                    // WriteToStatus("收到数据：" + content);
                    // messages.Enqueue(content);

                    redirectMsg(content);

                    //构造Response响应
                    HttpListenerResponse response = context.Response;
                    response.StatusCode = 200;
                    response.ContentType = "application/json;charset=UTF-8";
                    response.ContentEncoding = Encoding.UTF8;
                    response.AppendHeader("Content-Type", "application/json;charset=UTF-8");

                    using (StreamWriter writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
                    {
                        writer.Write("");
                        writer.Close();
                        response.Close();
                    }
                }

            }
            catch (Exception ex)
            {

                //WriteToStatus(ex.Message);
            }
        }

        private void redirectMsg(string msg)
        {
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                bool isSuccess = false;

                var res = HttpRequestHelper.HttpPost(BASE_REDIRECT_URL,msg, ref isSuccess);
                if (!isSuccess)
                {
                    Console.WriteLine("转发失败");
                }
            }, null);
        }


        private void BTN_OK_Click(object sender, RoutedEventArgs e)
        {
            BASE_REDIRECT_URL = this.TXT_URL.Text;
        }
    }
}
