using System.Diagnostics;
using System.Net.Sockets;
namespace EmailClient
{
    public partial class LoginForm : Form
    {
        public string name;
        public string pass;
        public MainForm form;
        public TcpClient client;
        public BinaryReader Reader;
        public BinaryWriter Writer;
        public int port = 25535;
        public string ip = "127.0.0.1";
        public LoginForm()
        {
            InitializeComponent();
            client = new TcpClient();
        }
        public async Task Connect()
        {
            try
            {
                await client.ConnectAsync(ip,port);//尝试连接
                Reader = new BinaryReader(client.GetStream());
                Writer = new BinaryWriter(client.GetStream());
            }
            catch
            {

            }
        }
        public async Task RecvInfo()
        {
            try
            {
                while (client.Connected)
                {
                    string information = await Task.Run(()=> Reader.ReadString());
                    if (!string.IsNullOrEmpty(information))
                    {
                        DealInforamtion(information);
                    }
                }
            }
            catch
            {
                client.Close();
                Reader.Close();
                Writer.Close();
                Reader.Dispose();
                Writer.Dispose();
            }
        }
        public void DealInforamtion(string information)
        {
            string operation = information[..5];
            string info = information[5..];
            if(operation == "ACCEP")//服务器接收到邮箱名
            {
                Send("SMTP", "PASSW", pass);
            }
            else if(operation == "ALLOW")//允许登录
            {
                this.Invoke(() =>
                {
                    form = new MainForm(name, pass);
                    form.Show();
                    this.Hide();
                    form.FormClosed += (s, args) => this.Close();
                });
                Send("SMTP", "CLOSE", "close");
            }
            else if(operation == "REFUS")//不允许登录
            {

            }
        }
        public bool Check()
        {
            if(textBox1.Text == "" || textBox2.Text == "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Send(string server,string oper,string info)
        {
            var mainfo = server + oper + info;
            Writer.Write(mainfo);
            Writer.Flush();
        }
        private async void button1_Click(object sender, EventArgs e)  // 登录按钮
        {
            if (Check())
            {
                MessageBox.Show("邮箱或密码不能为空");
            }
            else
            {
                name = textBox1.Text;
                pass = textBox2.Text;
                await Connect();
                Task.Run(() => RecvInfo());
                Send("SMTP", "LOGIN", name);
            }
        }
        private void button2_Click(object sender, EventArgs e)//测试按钮
        {
            MainForm s = new MainForm(name,pass);
            s.Show();
            this.Hide();
            s.FormClosed += (s, args) => this.Close();
        }
    }
}
