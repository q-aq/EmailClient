﻿using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using MailKit.Net.Imap;
using MimeKit;
using MailKit;
using System.Diagnostics;
using System.Text;
using Org.BouncyCastle.Tls;
using System.Threading;
using Org.BouncyCastle.Security;
using System;

namespace EmailServer
{
    internal class Program
    {
        public static LOG log;
        static async Task Main(string[] args)
        {
            Program s = new Program();

            await s.Listener();
        }
        public Program()
        {
            log = new LOG("journal.log");//传入服务器日志文件名
        }
        public async Task Listener()//启动监听
        {
            string ip = "127.0.0.1";
            int port = 25535;
            Console.WriteLine("服务器正在监听 - 地址: " + ip + " - 端口: " + port);
            log.INFO("服务器正在监听 - 地址: " + ip + " - 端口: " + port );
            IPAddress IPAddress = IPAddress.Parse(ip);
            TcpListener TcpListener = new TcpListener(IPAddress, port);//确定监听的地址和端口号
            TcpListener.Start(4);
            try
            {
                while (true)
                {
                    TcpClient TcpClient = await TcpListener.AcceptTcpClientAsync();
                    Server s = new Server(TcpClient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误"+ex.ToString());
                log.ERROR("TCP连接失败 - 原因:" +ex.Message);
            }
        }
    }
    public class Server
    {
        public static LOG log;
        private MailMessage message;
        private MimeMessage mime;
        private string email;
        private string pass;
        private string path = "D:\\";
        private IMAP imap;
        private SMTP smtp;
        private TcpClient tcpClient;
        private BinaryReader Reader;
        private BinaryWriter Writer;
        public Server(TcpClient tcpClient)
        {
            log = Program.log;
            this.tcpClient = tcpClient;
            Reader = new BinaryReader(tcpClient.GetStream());
            Writer = new BinaryWriter(tcpClient.GetStream());
            Task.Run(()=>RecvInfo());
        }
        public async Task RecvInfo()//接收信息
        {
            try
            {
                while(true)
                {
                    string? information;
                    information = Reader.ReadString();
                    if (information != null)
                    {
                        await DealInfo(information);//处理信息
                    }
                }
            }
            catch(Exception ex)
            {
                log.ERROR(ex.Message + "line: 91");
                Reader.Close();
                Reader.Dispose();
                Writer.Close();
                Writer.Dispose();
            }
        }
        public async Task DealInfo(string information)
        {
            //发送的消息一共分为三个部分，前四比特为服务器，表示需要和那一个服务器建立通信
            //4~8比特表示什么操作，后续为信息的主体
            string server = information[..4];//选择服务器
            string operation = information.Substring(4, 5);//长度为4的操作码
            string maininfo = information[9..];//消息主体
            if (server == "IMAP")//表示需要和IMAP服务器通讯
            {
                await DealIMAP(operation, maininfo);
            }
            else if (server == "SMTP")//表示和IMAP服务器通信
            {
                await DealSMTP(operation, maininfo);
            }
        }
        public async Task DealIMAP(string operation,string maininfo)
        {
            if (operation == "LOGIN")
            {
                this.email = maininfo;
            }
            if (operation == "PASSW")
            {
                this.pass = maininfo;
                imap = new IMAP(email,pass);
                await imap.InitAsync();
                smtp = new SMTP(email,pass);
            }
            if (operation == "INBOX")//获取收件箱
            {
                Console.WriteLine("客户端请求访问收件箱");
                log.INFO("客户端请求访问收件箱 - 地址:" + email);
                Console.WriteLine("访问允许");
                log.INFO("允许访问收件箱");
                imap.Inbox.Clear();//先清空
                await imap.GetInboxAsync();//向IMAP服务器请求访问收件箱
                Console.WriteLine("尝试向用户发送中");
                log.INFO("尝试向用户发送收件箱内容");
                try
                {
                    Send("INBOX", "");//表示开始接收收件箱
                    foreach(var s in imap.Inbox)
                    {
                        SendEmail(s);//将收件箱内容逐条发送至客户端
                    }
                    Send("OVERR","");//表示接收完毕
                    Console.WriteLine("发送完毕");
                    log.INFO("发送完毕");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("发送失败");
                    log.ERROR("向用户发送收件箱过程出错 - 原因:" +ex.Message + "line: 151");
                }
            }
            if (operation == "DRAFT")//获取草稿箱
            {
                Console.WriteLine("客户端请求访问草稿箱");
                log.INFO("客户端请求访问草稿箱");
                Console.WriteLine("访问允许");
                log.INFO("允许访问草稿箱");
                await imap.GetDraftsAsync();//向IMAP服务器请求访问草稿箱
                Console.WriteLine("尝试向用户发送中");
                log.INFO("尝试向用户发送草稿箱内容");
                try
                {
                    Send("DRAFT", "");//表示开始接收收件箱
                    foreach (var s in imap.Dratfs)
                    {
                        SendEmail(s);//将草稿箱内容逐条发送到客户端
                    }
                    Send("OVERR", "");
                    Console.WriteLine("发送完毕");
                    log.INFO("发送完毕");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("发送失败");
                    log.ERROR("向用户发送草稿箱过程出错 - 原因:" + ex.Message + "line: 177");
                }
            }
            if (operation == "PATHR")//发送附件存储地址
            {
                path = maininfo;//设置文件存储位置
                Console.WriteLine("接收到文件路径");
                log.INFO($"接收到文件路径 - 地址:{path}");
            }
            if(operation == "CHECK")
            {
                bool temp = true;
                int index = Convert.ToInt32(maininfo);
                Console.WriteLine("用户请求查询附件信息，邮件编号:"+index);
                log.INFO($"邮件编号{index}");
                var message = imap.Inbox[index];//找到对应文件
                foreach (var s in message.BodyParts)
                {
                    if (s is MimePart attachment && attachment.IsAttachment)//表示有附件
                    {
                        Send("HAFIL",maininfo);//表示有附件
                        Console.WriteLine("找到附件");
                        log.INFO("将邮件编号发回 - " + maininfo);
                        log.INFO($"找到附件 - 名称:{attachment.FileName}");
                        temp = false;
                        break;
                    }
                }
                if (temp)
                {
                    Send("NOFIL", "");
                    log.INFO("没有找到任何附件");
                }
            }
            if (operation == "FILES")//发送附件
            {
                bool temp = true;
                int index = Convert.ToInt32(maininfo);
                var message = imap.Inbox[index];//找到对应文件
                foreach (var s in message.BodyParts)
                {
                    if(s is MimePart attachment &&  attachment.IsAttachment)//表示有附件
                    {
                        Console.WriteLine("准备保存附件");
                        string filename = attachment.FileName;//文件名
                        var truepath = Path.Combine(path, filename);//文件的绝对地址
                        using(var stream = File.Create(truepath))//保存文件
                        {
                            attachment.Content.DecodeTo(stream);
                        }
                        Send("FISUC", truepath);
                        Console.WriteLine("附件保存完毕");
                        log.INFO("附件保存完毕 - 地址:"+truepath);
                        temp = false;
                        break;
                    }
                }
                if(temp)
                {
                    Send("NOFIL", "");
                    Console.WriteLine("没有找到任何附件");
                    log.INFO("没有找到任何附件");
                }
            }
        }
        public async Task DealSMTP(string operation,string maininfo)
        {
            if(operation == "LOGIN")
            {
                log.INFO("用户登录,准备进行身份验证");
                Console.WriteLine("收到登录请求");
                Console.WriteLine("等待接收邮箱");
                Console.WriteLine("收到邮箱");
                log.INFO("SMTP服务器收到登录请求 - 地址:" + maininfo);
                email = maininfo;
                try
                {
                    Send("ACCEP", "");
                    Console.WriteLine("登录请求允许，请输入密码");
                    log.INFO("登录请求允许,准备接收密码");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("无法与客户端建立连接");
                    log.ERROR("与客户端连接失败 - 原因:" + ex.Message + "line: 281");
                }
            }
            else if(operation == "PASSW")
            {
                pass = maininfo;
                Console.WriteLine("收到密码");
                try
                {
                    Console.WriteLine("正在验证密码");
                    smtp = new SMTP(email, pass);
                    imap = new IMAP(email,pass);
                    bool vis = await imap.InitAsync();//验证
                    if(vis)
                    {
                        Send("ALLOW", "allow");
                        Console.WriteLine("验证通过,登录请求允许");
                    }
                    else
                    {
                        Send("REFUS", "refus");
                        Console.WriteLine("请检查密码后重试");
                    }
                }
                catch(Exception ex)
                {
                    Send("REFUS", "refus");
                    Console.WriteLine("无法与客户端建立连接");
                    log.ERROR("与客户端连接失败 - 原因:" + ex.Message + "line: 300");
                }
            }
            else if(operation == "CLOSE")//关闭
            {
                tcpClient.Close();
            }
            else if (operation == "EMAIL")//发送邮件
            {
                message = new MailMessage();//创建一个新的邮件格式
                Console.WriteLine("收到客户端邮件发送请求");
                log.INFO("服务器收到客户端邮件发送请求 - 地址 : " + email);
            }
            else if (operation == "SUBJE")//主题
            {
                message.Subject = maininfo;
            }
            else if(operation == "FROMR")//发件人
            {
                message.From = new MailAddress(maininfo);
            }
            else if(operation == "TORRR")//收件人
            {
                var list = maininfo.Split(';');
                foreach(var s in list)
                {
                    message.To.Add(new MailAddress(s));
                }
            }
            else if(operation == "BODYR")//正文
            {
                message.Body = maininfo;
            }
            else if(operation == "CCRRR")//抄送,以字符串的形式传送
            {
                if (maininfo == "") return;
                string[] cc = maininfo.Split(';');
                foreach (var s in cc)
                {
                    if(s != "")
                        message.CC.Add(s);
                }
            }
            else if(operation == "FILES")//文件
            {
                if (maininfo == "") return;
                string[] fi = maininfo.Split(";");
                foreach (var s in fi)
                {
                    if(s != "")
                    {
                        Attachment m = new Attachment(s);
                        message.Attachments.Add(m);
                    }
                }
            }
            else if(operation == "ENDRR")//表示一封邮件发送完毕
            {
                //表示从客户端接收邮件完毕
                Console.WriteLine("邮件接收完毕，正在尝试发送中");
                log.INFO("邮件接收完毕");
                bool s =  await smtp.SendMeialAsync(message);//向smtp服务器发送邮件
                if (s)
                {
                    Send("TRUER", "true");//告诉客户端邮件发送成功
                }
                else
                {
                    Send("FALSE","");
                }
            }
        }
        public void SendEmail(MimeMessage message)//向客户端发送邮件
        {
            Send("BEGIN", "");//表示一封邮件开始
            string time = message.Date.DateTime.ToString();
            Send("TIMER", time);//时间
            string subject = message.Subject;
            Send("SUBJE", subject);//主题
            string body = EnCode(message);
            Send("BODYR", body);//正文
            string from = message.From.ToString();
            Send("FROMR", from);//发件人
            string to = message.To.ToString();
            Send("TORRR", to);//收件人
            string cc = message.Cc.ToString();
            Send("CCRRR", cc);//抄送
            Send("ENDRR", "");//表示一封邮件结束
        }
        public void Send(string operation,string info)//向客户端发送信息
        {
            string maininfo = operation + info;
            Writer.Write(maininfo);
            Writer.Flush();
            log.INFO($"发送的数据:{maininfo}");
        }
        public void SendByte(byte[] buffer)//发送字节流
        {
            Writer.Write(buffer);
        }
        public void SendFile(FileStream stream,int bytesRead, byte[] buffer)//发送文件
        {
            while((bytesRead = stream.Read(buffer,0,buffer.Length)) > 0)
            {
                string heads = "FILES";
                byte[] head = Encoding.Default.GetBytes(heads);//文件头
                byte[] info = new byte[head.Length + buffer.Length];
                head.CopyTo(info, 0);
                buffer.CopyTo(info, head.Length - 1);
                Writer.Write(info,0,bytesRead);
            }
        }
        private byte[] GetBuffer(string[] buffer)//输入16进制字符串数组，转化为字节流
        //例如6E 69 68 61 6F，为GBK编码的你好，获得字节流之后将该字节流返回，使用Encoding对象的GetString方法即可获取字符串
        {
            int i = 0;
            byte[] bytes = new byte[buffer.Length];
            ASCIIEncoding asc = new ASCIIEncoding();
            foreach (var s in buffer)
            {
                if (s == "" || s == "\r\n") continue;
                byte b = asc.GetBytes(s)[0];//获取第一个字符的ascii码值
                if (b >= 0x30 && b <= 0x39)//表示为数字
                {
                    b -= 0x30;//0-9
                }
                if (b >= 0x41 && b <= 0x46)//表示字母
                {
                    b -= 0x37;//10-15;
                }
                byte c = asc.GetBytes(s)[1];
                if (c >= 0x30 && c <= 0x39)//表示为数字
                {
                    c -= 0x30;//0-9
                }
                if (c >= 0x41 && c <= 0x46)//表示字母
                {
                    c -= 0x37;//10-15;
                }
                b *= 0x10;//b表示高位数字，需要乘以10H
                b += c;//c表示低位数字
                Debug.WriteLine(s + "16->" + b);
                bytes[i++] = b;
            }
            return bytes;
        }
        private string CutHtml(string info)//删除html元素，提取核心信息
        {
            bool annonation = false;//注释
            char[] buffer = new char[info.Length];
            int index = 0;
            foreach (var s in info)
            {
                if (s == '<')//遇到左括号设置注释为true
                {
                    annonation = true;
                }
                if (s == '>')//遇到右括号设置注释为false
                {
                    annonation = false;
                    continue;
                }
                if (!annonation)//如果注释为false，则记录信息
                {
                    buffer[index] = s;
                    index++;
                }
            }
            if (index == 0)
            {
                return "";
            }
            else
            {
                string cut = new string(buffer);//转字符串输出
                return cut;
            }
        }
        private string EnCode(MimeMessage message)//对邮件正文进行解码操作
        {
            string info = message.Body.ToString();
            try
            {
                string type = "";
                string charset = "";
                string encode = "";
                int index = info.Length;
                int abs = 1;
                int one = 0;//记录第一个换行符
                int two = 0;//记录第二个换行符
                for (int i = 0; i < index; i++)
                {
                    if (info[i] == '\n')
                    {
                        if (abs == 1)
                        {
                            one = i;//记录第一个换行符的位置
                            abs++;
                            continue;
                        }
                        if (abs == 2)
                        {
                            two = i;//第二个换行符
                            break;//直接结束循环
                        }
                    }
                }
                //获取邮件头部信息
                string Content1 = info.Substring(0, one);
                string Content2 = info.Substring(one + 1, two - one);
                //获取正文，并删除前导，后导空格
                string mainfo = info.Substring(two + 1);
                mainfo = mainfo.Trim();
                var types1 = Content1.Split(':', ';', '=');//以冒号,分号,等号分割为数组
                var types2 = Content2.Split(':', ';', '=');
                //提取有效信息
                if (types1[0] == "Content-Type")
                {
                    type = types1[1];
                    charset = types1[3];
                    encode = types2[1];
                }
                if (types2[0] == "Content-Type")
                {
                    type = types2[1];
                    charset = types2[3];
                    encode = types1[1];
                }
                type = type.Trim();
                charset = charset.Trim();
                encode = encode.Trim();
                if (Equals(type, "text/html") && Equals(encode, "quoted-printable") && Equals(charset, "GBK"))//中文GBK解码
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册
                    Encoding encoding = Encoding.GetEncoding(charset);//GBK编码
                    mainfo = CutHtml(mainfo);
                    var list = mainfo.Split('=');
                    var buffer = GetBuffer(list);
                    return encoding.GetString(buffer);
                }
                else if (Equals(type, "text/plain") && Equals(encode, "base64") && Equals(charset, "utf-8"))//中文，base64解码
                {
                    var buffer = Convert.FromBase64String(mainfo);
                    return Encoding.UTF8.GetString(buffer);
                }
                else if (Equals(type, "text/plain") && Equals(encode, "quoted-printable") && Equals(charset, "us-ascii"))//英文，直接打印
                {
                    return mainfo;
                }
                else
                {
                    string b = "error con not analysis";
                    return b;
                }
            }
            catch
            {
                return message.TextBody;
            }
        }
    }
    public class IMAP//与IMAP服务器通信
    {
        private LOG log = Program.log;
        private string name;
        public bool flag = false;
        private string pass;
        private string imapserver;
        private int port = 993;
        private ImapClient client;
        public List<MimeMessage> Inbox = new();
        public List<MimeMessage> Dratfs = new();
        public IMAP(string name,string pass)
        {
            this.name = name;
            this.pass = pass;
            client = new ImapClient();
            string suffix = GetMAilSuffix(name);
            imapserver = "imap." + suffix;
        }
        public async Task<bool> InitAsync()//尝试与IMAP服务器建立连接
        {
            try
            {
                await client.ConnectAsync(imapserver,port,true);
                await client.AuthenticateAsync(name, pass);
                //来自CSDN大佬的解决方法，没有以下语句服务器会认为EXAMINE Unsafe Login 从而拒绝连接
                var clientImplementation = new ImapImplementation
                {
                    Name = "MailKitDemo",
                    Version = "1.0.0"
                };
                var serverImplementation = client.Identify(clientImplementation);
            }
            catch(Exception ex)
            {
                Console.WriteLine("身份验证失败，用户名或者密码有误");
                log.ERROR("IMAP身份验证失败 - 原因:" +ex.Message + "line: 629");
                return false;
            }
            return true;
        }
        public string GetMAilSuffix(string email)//获取邮箱后缀名
        {
            int index = email.IndexOf('@');//获取@符号的位置
            string suffix = email.Substring(index + 1);//获取@符后面的字符串用作动态更新服务器
            return suffix;
        }
        public async Task GetInboxAsync()//获取收件箱
        {
            try
            {
                Console.WriteLine("正在加载收件箱,请稍后");
                log.INFO("正在加载收件箱");
                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);
                int index = inbox.Count;
                for (int i = 0; i < index; i++)
                {
                    var message = await inbox.GetMessageAsync(i);
                    Inbox.Add(message);
                }
                Console.WriteLine("收件箱加载完毕,共" + index + "条数据");
                log.INFO("收件箱加载完毕,共" + index + "条数据");
            }
            catch(Exception ex)
            {
                Console.WriteLine("访问失败");
                log.ERROR("访问收件箱失败 - 原因:" + ex.Message + "line: 639");
            }
        }
        public async Task GetDraftsAsync()//获取草稿箱
        {
            try
            {
                Console.WriteLine("正在加载草稿箱,请稍后");
                log.INFO("正在加载草稿箱");
                var drafts = client.GetFolder(SpecialFolder.Drafts);
                drafts.Open(FolderAccess.ReadWrite);
                int index = drafts.Count;
                for (int i = 0; i < index; i++)
                {
                    var message = await drafts.GetMessageAsync(i);
                    Dratfs.Add(message);
                }
                Console.WriteLine("收件箱加载完毕,共" + index + "条数据");
                log.INFO("收件箱加载完毕,共" + index + "条数据");
            }
            catch(Exception ex)
            {
                Console.WriteLine("访问失败");
                log.ERROR("访问草稿箱失败 - 原因:" + ex.Message + "line: 622");
            }
        }
        public void AddDraftsAsync(MimeMessage message)//添加
        {
            Dratfs.Add(message);
        }
    }
    public class SMTP//与SMTP服务器通信
    {
        private LOG log = Program.log;
        private string name;
        private string pass;
        private string smtpserver;
        private int port = 25;
        private SmtpClient client;
        public SMTP(string name,string pass)
        {
            this.name = name;
            this.pass = pass;
            string suffix = GetMAilSuffix(name);
            this.smtpserver = "smtp." + suffix;
            Init();
        }
        public void Init()
        {
            client = new SmtpClient(smtpserver, port)
            {
                Credentials = new NetworkCredential(name, pass),
                EnableSsl = true
            };
        }
        public string GetMAilSuffix(string email)//获取邮箱后缀名
        {
            int index = email.IndexOf('@');//获取@符号的位置
            string suffix = email.Substring(index + 1);//获取@符后面的字符串用作动态更新服务器
            return suffix;
        }
        public async Task<bool> SendMeialAsync(MailMessage message)
        {
            try
            {
                await client.SendMailAsync(message);//调用异步方法发送信件
                Console.WriteLine("邮件发送成功");
                log.INFO("邮件成功送达");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("邮件发送失败");
                log.ERROR("邮件发送失败 - 原因:" + ex.Message);
                return false;
            }
        }
    }
    public class LOG//处理日志
    {
        private string? time;
        private StreamWriter sw;
        public LOG(string filename)
        {
            String Path = Environment.CurrentDirectory;//获取当前项目可执行文件的地址
            Path = Path.Substring(0, Path.Length - 16);//通过裁剪获得当前项目文件夹的地址
            Path += filename;
            sw = new StreamWriter(Path, true);//true表示追加模式
            INFO("程序启动日志开始记录");
        }
        public void INFO(string info)//向文件写入信息，记录日志
        {
            time = DateTime.Now.ToString("G");
            string msg = time + " [INFO] " + info;
            sw.WriteLine(msg);
            sw.Flush();
        }
        public void ERROR(string error)
        {
            time = DateTime.Now.ToString("G");
            string msg = time + " [ERROR] " + error;
            sw.WriteLine(msg);
            sw.Flush();
        }
        public void Write(string type,string info)
        {
            time = DateTime.Now.ToString("G");
            string msg = time + " [" + type + "] " + info;
            sw.WriteLine(msg);
            sw.Flush();
        }
    }
}
