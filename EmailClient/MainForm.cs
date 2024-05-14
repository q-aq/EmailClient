using System.Diagnostics;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace EmailClient
{
    public partial class MainForm : Form
    {
        public string email;
        public string pass;
        public int flag;
        public Message message;
        public BinaryReader Reader;
        public BinaryWriter Writer;
        public MailRecver Recver;
        public MailSender Sender;
        public TcpClient client;
        public int port = 25535;
        public string ip = "127.0.0.1";
        public static Log log = new Log("journal.log");
        public MainForm(string email, string pass)
        {
            InitializeComponent();
            panelmain.Visible = true;
            panel9.Visible = false;
            panelrecv.Visible = false;
            panelwrite.Visible = false;
            paneldraft.Visible = false;
            panelmessage.Visible = false;
            panelAddLinkMan.Visible = false;
            panelAddressBook.Visible = false;

            this.email = email;
            this.flag = 0;//flag默认为0
            this.pass = pass;
            label8.Text = email;
            client = new TcpClient();
            var task = Task.Run(() => ConnectAsync());
            task.Wait();//等待连接成功
            Task.Run(() => RecvInfo());//开始接收信息
            InitAsync();
        }
        public void InitAsync()//初始化
        {
            Recver = new MailRecver(email, pass);
            Sender = new MailSender(email, pass);
            //请求连接
            Send("IMAP", "LOGIN", email);
            Send("IMAP", "PASSW", pass);
            log.write("GET", "密钥验证通过，IMAP与SMTP服务器准许登入");
            //请求收件箱
            Send("IMAP", "INBOX", "");
            log.write("GET", "请求访问收件箱");
            //请求草稿箱
            Send("IMAP", "DRAFT", "");
            log.write("GET", "请求访问草稿箱");
        }
        public async Task ConnectAsync()//和服务器建立TCP连接
        {
            try
            {
                await client.ConnectAsync(ip, port);
                Reader = new BinaryReader(client.GetStream());
                Writer = new BinaryWriter(client.GetStream());
                log.write("LOGIN", "成功与服务器建立连接");
            }
            catch (Exception ex)
            {
                log.write("REEOR", "无法连接到服务器 - 原因:" + ex.Message);
            }
        }
        public void RecvInfo()//接收信息
        {
            log.write("INFO", "消息接收循环开始");
            string? information;
            while (true)
            {
                try
                {
                    information = Reader.ReadString();
                    if (information != null)
                        DealInfo(information);
                }
                catch (Exception ex)
                {
                    log.write("ERROR", "接收信息错误 - 原因:" + ex.Message);
                    break;
                }
            }
        }
        public void DealInfo(string information)//处理信息
        {
            string operation = information[..5];
            string info = information[5..];
            if (operation == "INBOX")//准备接收收件箱
            {
                log.write("INFO", "准备接收收件箱");
                Recver.Inbox.Clear();
                flag = 1;
            }
            else if (operation == "NEWEM")//表示是新邮件加入
            {
                flag = 0;
            }
            else if (operation == "DRAFT")//准备接收是草稿箱
            {
                log.write("INFO", "准备接收草稿箱");
                flag = 2;
            }
            else if (operation == "OVERR")//接收完毕，将标识置为默认
            {
                if (flag == 1)
                {
                    flag = -1;
                    log.write("INFO", "收件箱接收完毕正在向数据表中添加");
                    UpDateInbox();
                    log.write("INFO", "添加完毕");
                }
                if (flag == 2)
                {
                    flag = -1;
                    log.write("INFO", "草稿箱接收完毕，正在向数据表中添加");
                    UpDateDrafts();
                    log.write("INFO", "添加完毕");
                }
            }
            else if (operation == "BEGIN")//表示一封邮件开始
            {
                log.write("INFO", "准备接收邮件");
                message = new Message();
            }
            else if (operation == "ENDRR")//表示一封邮件结束
            {
                log.write("INFO", "邮件接收完毕");
                if (flag == -1)
                {

                }
                else if (flag == 0)//新邮件
                {
                    log.write("INFO", "接收到新邮件");
                    Recver.Inbox.Add(message);//向收件箱更新文件
                    console.Text = "有新文件";
                }
                else if (flag == 1)
                {
                    Recver.Inbox.Add(message);//表示向收件箱添加文件
                }
                else if (flag == 2)
                {
                    Recver.Drafts.Add(message);//表示向草稿箱添加文件
                }
            }
            else if (operation == "TIMER")//时间
            {
                message.time = info;
            }
            else if (operation == "SUBJE")//主题
            {
                message.subject = info;
            }
            else if (operation == "FROMR")//发件人
            {
                message.from = info;
            }
            else if (operation == "TORRR")//收件人
            {
                log.write("OBTAIN", "正在接收信息");
                message.to = info;
            }
            else if (operation == "BODYR")//正文
            {
                message.body = info;
            }
            else if (operation == "CCRRR")//抄送
            {
                var cc = info.Split('<', '>');//奇数部分为抄送内容
                int index = cc.Length;
                for (int i = 0; i < cc.Length; i++)
                {
                    if (i % 2 != 0)
                    {
                        message.cc += cc[i] + ";";
                    }
                }
            }
            else if (operation == "TRUER")//发送成功
            {
                MessageBox.Show("邮件发送成功");
                log.write("INFO", "邮件发送完毕");
                textBox1.Text = string.Empty;
                textBox2.Text = string.Empty;
                textBox3.Text = string.Empty;
                textBox4.Text = string.Empty;
            }
            else if (operation == "FALSE")
            {
                MessageBox.Show("邮件发送失败");
                log.write("INFO", "邮件发送失败 - 原因:" + info);
            }
            else if (operation == "CHANG")
            {
                MessageBox.Show(info);
            }
        }
        public void Send(string server, string oper, string info)//发送信息
        {
            var mainfo = server + oper + info;
            Writer.Write(mainfo);
            Writer.Flush();
        }
        public void UpDateInbox()//更新收件箱
        {
            int i = 1;
            dataGridView1.Invoke(() =>
            {
                dataGridView1.Rows.Clear();
                foreach (var s in Recver.Inbox)
                {
                    var row = new DataGridViewRow();
                    row.CreateCells(dataGridView1);
                    row.Cells[0].Value = i;
                    row.Cells[1].Value = s.from;
                    row.Cells[2].Value = s.subject;
                    row.Cells[3].Value = s.time;
                    dataGridView1.Rows.Add(row);
                    i++;
                }
            });
        }
        public void UpDateDrafts()//更新草稿箱
        {
            int i = 1;
            dataGridView2.Invoke(() =>
            {
                foreach (var s in Recver.Drafts)
                {
                    var row = new DataGridViewRow();
                    row.CreateCells(dataGridView2);
                    row.Cells[0].Value = i;
                    row.Cells[1].Value = s.from;
                    row.Cells[2].Value = s.subject;
                    row.Cells[3].Value = s.time;
                    dataGridView2.Rows.Add(row);
                    i++;
                }
            });
        }
        public void SendMail(Message message, string server)
        {
            Send(server, "EMAIL", "");
            Send(server, "TORRR", message.to);
            Send(server, "SUBJE", message.subject);
            Send(server, "BODYR", message.body);
            Send(server, "FROMR", email);
            Send(server, "CCRRR", message.cc);
            Send(server, "ENDRR", "");
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)//窗口关闭调用此函数
        {
            log.write("EXIT", "退出客户端");
            Application.Exit();
        }
        //鼠标进入控件变色
        private void panel_MouseEnter(object sender, EventArgs e)//鼠标进入panel容器
        {
            if (sender is Panel panel)
            {
                panel.BackColor = Color.FromArgb(224, 236, 250);
            }
        }
        private void linkLabel_MouseEnter(object sender, EventArgs e)//鼠标进入linklabel
        {
            if (sender is LinkLabel linkLabel)
            {
                linkLabel.Parent.BackColor = Color.FromArgb(224, 236, 250);
            }
        }
        //鼠标退出控件重新变为白色
        private void panel_MouseLeave(object sender, EventArgs e)//鼠标退出panel容器
        {
            if (sender is Panel panel)
            {
                panel.BackColor = SystemColors.Control;
            }
        }
        private void linkLabel_MouseLeave(object sender, EventArgs e)//鼠标退出linklabel
        {
            if (sender is LinkLabel linkLabel)
            {
                linkLabel.Parent.BackColor = SystemColors.Control;
            }
        }
        //点击事件
        private void buttonsend_Click(object sender, EventArgs e)//发信按钮
        {
            bool AllEmailTrue = true;
            //清空所有控件
            log.write("INFO", "正在初始化邮件信息");
            Message message = new Message();
            message.to = textBox1.Text;//收件人，这里需要判断邮件格式是否合法
            message.subject = textBox3.Text;
            message.body = textBox4.Text;
            var list = message.to.Split(";");
            foreach (var item in list)
            {
                if (item.Length == 0) continue;
                if (!IsValidEmail(item))//有一个是不合法的邮箱地址
                {
                    AllEmailTrue = false;
                }
            }
            log.write("INFO", "邮件初始化完毕");
            if (AllEmailTrue)
            {
                SendMail(message, "SMTP");
                log.write("INFO", "邮件已发送");
            }
            else
            {
                MessageBox.Show("邮件格式不合法");
                log.write("WARNING", "邮件格式不合法 - 不合法目标:" + message.to);
            }
        }
        private void buttonsave_Click(object sender, EventArgs e)//保存按钮，使用文件保存
        {

        }
        private void buttonexit_Click(object sender, EventArgs e)//退出按钮
        {
            panelmain.Visible = true;
            panelrecv.Visible = false;
            panelwrite.Visible = false;
            paneldraft.Visible = false;
            panelmessage.Visible = false;
        }
        private void panel2_MouseClick(object sender, MouseEventArgs e)//写信按钮的父控件
        {
            panelAddressBook.Visible = false;//xc
            panelrecv.Visible = false;
            paneldraft.Visible = false;
            panelmain.Visible = false;
            panelmessage.Visible = false;
            panelwrite.Visible = true;
        }
        private void panel3_MouseClick(object sender, MouseEventArgs e)//收信按钮的父控件
        {
            panelAddressBook.Visible = false;//xc
            panelwrite.Visible = false;
            paneldraft.Visible = false;
            panelmain.Visible = false;
            panelmessage.Visible = false;
            panelrecv.Visible = true;
        }
        private void panel4_MouseClick(object sender, MouseEventArgs e)//通讯录按钮的父控件
        {
            panelwrite.Visible = false;
            paneldraft.Visible = false;
            panelmain.Visible = false;
            panelmessage.Visible = false;
            panelrecv.Visible = false;
            panelAddressBook.Visible = true;//通讯录
        }
        private void panel6_MouseClick(object sender, MouseEventArgs e)//草稿箱按钮的父控件
        {
            panelwrite.Visible = false;
            panelrecv.Visible = false;
            panelmain.Visible = false;
            panelmessage.Visible = false;
            panelAddressBook.Visible = false;//通讯录
            paneldraft.Visible = true;
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)//写信
        {
            panelrecv.Visible = false;
            paneldraft.Visible = false;
            panelmain.Visible = false;
            panelmessage.Visible = false;
            panelAddressBook.Visible = false;//xc
            panelwrite.Visible = true;
        }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)//收信
        {
            panelwrite.Visible = false;
            paneldraft.Visible = false;
            panelmain.Visible = false;
            panelmessage.Visible = false;
            panelAddressBook.Visible = false;//xc
            panelrecv.Visible = true;
        }
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)//通讯录
        {
            panelAddressBook.Visible = true;
            panel9.Visible = false;
            panelwrite.Visible = false;
            paneldraft.Visible = false;
            panelmain.Visible = false;
            panelmessage.Visible = false;
            panelrecv.Visible = false;
            string filePath = "contacts.txt";// 获取文件路径
            DataGridView dataGridView3 = this.dataGridView3;
            if (!File.Exists(filePath))// 检查文件是否存在
            {
                try// 文件不存在，创建一个空的contacts.txt文件
                {
                    File.Create(filePath).Close();
                    MessageBox.Show("联系人文件不存在，已为您创建一个新的空文件。", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("创建联系人文件时发生错误：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            LoadContactsIntoDataGridView(filePath, dataGridView3);// 加载并显示联系人信息
        }
        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)//草稿箱
        {
            panelwrite.Visible = false;
            panelrecv.Visible = false;
            panelmain.Visible = false;
            panelmessage.Visible = false;
            paneldraft.Visible = true;
        }
        private void button1_Click(object sender, EventArgs e)//删除草稿箱
        {
            //dataGridView2.SelectedRows表示被选中的行的集合，如果有行被选中则count大于0
            if (dataGridView2.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView2.SelectedRows[0];//获取被选中的行的第一个，但是默认只能单选
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                var yn = MessageBox.Show("您确定要删除此项草稿吗", "警告", buttons);
                if (yn == DialogResult.Yes)
                {
                    dataGridView2.Rows.Remove(row);//删除被选中行
                    log.write("INFO", "用户尝试删除草稿箱");
                }
            }
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)//当某个单元格被双击，获取行号，通过链表获取对应邮件信息
        {
            int index = e.RowIndex;
            if (index < 0) return;
            panelmain.Visible = false;
            panelrecv.Visible = false;
            panelwrite.Visible = false;
            paneldraft.Visible = false;
            panelmessage.Visible = true;
            labelfrom.Text = "收件人:" + Recver.Inbox[index].to;
            labeltime.Text = "时间:" + Recver.Inbox[index].time;
            labelto.Text = "发件人:" + Recver.Inbox[index].from;
            labelsubject.Text = "主题:" + Recver.Inbox[index].subject;
            //body需要解码操作，可以在message类中添加解码函数
            textBoxmessage.Text = Recver.Inbox[index].body;
            //textBoxmessage.Text = EnCode(Recver.Inbox[index].body);
        }
        private void button2_Click(object sender, EventArgs e)//邮件详细信息返回界面
        {
            panelwrite.Visible = false;
            panelmain.Visible = false;
            paneldraft.Visible = false;
            panelmessage.Visible = false;
            panelrecv.Visible = true;
        }
        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = e.RowIndex;
            if (index < 0) return;
            panelmain.Visible = false;
            panelrecv.Visible = false;
            paneldraft.Visible = false;
            panelmessage.Visible = false;
            panelwrite.Visible = true;
            textBox1.Text = Recver.Drafts[index].to;
            textBox3.Text = Recver.Drafts[index].subject;
            textBox4.Text = Recver.Drafts[index].body;
            //textBox4.Text = EnCode(Recver.Drafts[index].body);
        }
        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 确保点击发生在第一列（复选框列）
            if (e.ColumnIndex == 0) // 第一列的索引是 0
            {
                // 手动结束编辑并提交更改
                dataGridView3.EndEdit();
                dataGridView3.CommitEdit(DataGridViewDataErrorContexts.Commit);
                // 更新panel9的可见性
                UpdatePanelVisibilityBasedOnSelection();
            }
        }
        private void buttonAddLinkman_Click(object sender, EventArgs e)//添加通讯录按钮
        {
            panelAddressBook.Visible = false;
            panelAddLinkMan.Visible = true;
        }
        private void buttonCancelLinkMan_Click(object sender, EventArgs e)//取消按钮
        {
            panelAddressBook.Visible = true;
            panelAddLinkMan.Visible = false;
        }
        private void buttonDelete_Click(object sender, EventArgs e)//删除联系人按钮
        {
            // 存储未选中的联系人信息
            var contactsToKeep = new List<string>();
            // 遍历所有行，收集未选中的行信息
            for (int i = 0; i < dataGridView3.Rows.Count; i++)
            {
                // 如果行未被选中，则添加到未选中的联系人信息列表
                if (!(dataGridView3.Rows[i].Cells[0].Value is bool && (bool)dataGridView3.Rows[i].Cells[0].Value))
                {
                    // 假设每个联系人信息占用6行（5个详细信息 + 1个空行）
                    contactsToKeep.AddRange(GetContactLines(i * 6, 6));
                }
            }
            // 将未选中的联系人信息写入临时文件
            string tempFilePath = "temp_contacts.txt";
            File.WriteAllLines(tempFilePath, contactsToKeep);
            // 删除原始文件，并将临时文件重命名为原始文件的名字
            File.Delete("contacts.txt");
            File.Move(tempFilePath, "contacts.txt");
            // 从 DataGridView 中删除选中的行
            dataGridView3.Rows.Cast<DataGridViewRow>()
                             .Where(row => row.Cells[0].Value is bool && (bool)row.Cells[0].Value)
                             .ToList()
                             .ForEach(row => dataGridView3.Rows.Remove(row));
            // 重新加载联系人信息到 DataGridView
            LoadContactsIntoDataGridView("contacts.txt", dataGridView3);
        }
        private void buttonSaveLinkMan_Click(object sender, EventArgs e)//保存联系人按钮
        {
            // 检查文本框是否为空
            bool isAnyTextBoxFilled = !string.IsNullOrEmpty(textBoxName.Text) ||
                                      !string.IsNullOrEmpty(textBoxAddress.Text) ||
                                      !string.IsNullOrEmpty(textBoxPhone.Text) ||
                                      !string.IsNullOrEmpty(textBoxQQ.Text) ||
                                      !string.IsNullOrEmpty(textBoxRemarks.Text);
            if (!isAnyTextBoxFilled)
            {
                // 如果所有文本框都为空，则弹出警告
                MessageBox.Show("请至少填写一个信息项！", "验证结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 检查邮箱格式
            if (!IsValidEmail(textBoxAddress.Text))
            {
                MessageBox.Show("邮箱格式错误！", "验证结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 检查电话号码格式
            if (!IsValidPhoneNumber(textBoxPhone.Text))
            {
                MessageBox.Show("电话号码格式错误！", "验证结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 获取输入的邮箱地址
            string email = textBoxAddress.Text;
            string name1 = textBoxName.Text;
            // 检查邮箱或姓名是否已经存在
            if (IsContactInfoAlreadyExists(name1, email))
            {
                MessageBox.Show("该姓名或邮箱地址的联系人已存在！", "验证结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 如果通过所有检查，则保存联系人信息
            string name = textBoxName.Text;
            string address = textBoxAddress.Text;
            string phone = textBoxPhone.Text;
            string qq = textBoxQQ.Text;
            string remarks = textBoxRemarks.Text;
            SaveContactInfo(name, address, phone, qq, remarks);
            // 刷新并显示联系人信息
            // 定义文件路径，这里以程序当前目录为例
            string filePath = "contacts.txt";
            LoadContactsIntoDataGridView(filePath, dataGridView3);
        }
        private void buttonCancel_Click(object sender, EventArgs e)//取消选择按钮
        {
            UncheckAllCheckboxes();// 将所有复选框设置为未选中状态
            panel9.Visible = false;// 隐藏 panel9
        }
        //通讯录操作函数
        private void UpdatePanelVisibilityBasedOnSelection()
        {
            // 初始化选中的行数
            int selectedCount = 0;
            // 遍历所有行，统计被选中的行数
            foreach (DataGridViewRow row in dataGridView3.Rows)
            {
                // 确保行不是新行（即用户未开始编辑的行）
                if (!row.IsNewRow)
                {
                    // 检查复选框列的值
                    if (row.Cells[0].Value is bool && (bool)row.Cells[0].Value) // 第一列的索引是 0
                    {
                        selectedCount++;
                    }
                }
            }
            // 根据选中的行数显示或隐藏panel9
            panel9.Visible = (selectedCount > 0);
        }
        // 辅助方法，用于从文件中提取特定联系人的信息
        private IEnumerable<string> GetContactLines(int startLine, int count)
        {
            return File.ReadAllLines("contacts.txt")
                      .Skip(startLine)
                      .Take(count);
        }
        public bool IsContactInfoAlreadyExists(string nameToCheck, string emailToCheck)
        {
            if (string.IsNullOrEmpty(nameToCheck) && string.IsNullOrEmpty(emailToCheck))
                return false;
            string filePath = "contacts.txt";
            try
            {
                string[] lines = File.ReadAllLines(filePath);// 读取文件中的所有行
                foreach (string line in lines)// 遍历文件的每一行，查找邮箱地址和姓名
                {
                    if (line.StartsWith("Name:"))
                    {
                        string nameInFile = line.Substring(5).Trim();// 提取姓名，"Name:" 的长度是5，所以从第6个字符开始截取
                        // 如果提供的姓名与文件中的姓名匹配
                        if (nameInFile == nameToCheck && !string.IsNullOrEmpty(nameToCheck) && !string.IsNullOrEmpty(nameInFile))
                        {
                            return true; // 直接返回结果，姓名已存在
                        }
                    }
                    else if (line.StartsWith("Address:"))
                    {
                        // 提取邮箱地址，"Address:" 的长度是8，所以从第9个字符开始截取
                        string emailInFile = line.Substring(8).Trim();
                        // 如果提供的邮箱地址与文件中的邮箱匹配
                        if (emailInFile == emailToCheck && !string.IsNullOrEmpty(emailToCheck) && string.IsNullOrEmpty(emailInFile))
                        {
                            return true; // 直接返回结果，邮箱已存在
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("在检查联系人信息是否存在时发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false; // 如果没有找到匹配的姓名或邮箱，返回false
        }
        public void SaveContactInfo(string name, string address, string phone, string qq, string remarks)
        {
            string filePath = "contacts.txt";// 定义文件路径，这里以程序当前目录为例
            if (!File.Exists(filePath))// 检查文件是否存在，如果不存在则创建
            {
                File.Create(filePath).Dispose();
            }
            // 将联系人信息追加到文件中
            // 这里使用追加模式，如果需要覆盖文件内容，可以使用FileMode.Create
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("Name: " + (!string.IsNullOrEmpty(name) ? name : ""));
                writer.WriteLine("Address: " + (!string.IsNullOrEmpty(address) ? address : ""));
                writer.WriteLine("Phone: " + (!string.IsNullOrEmpty(phone) ? phone : ""));
                writer.WriteLine("QQ: " + (!string.IsNullOrEmpty(qq) ? qq : ""));
                writer.WriteLine("Remarks: " + (!string.IsNullOrEmpty(remarks) ? remarks : ""));
                writer.WriteLine(); // 每个联系人信息后跟一个空行
            }
            MessageBox.Show("联系人信息已保存！", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void LoadContactsIntoDataGridView(string filePath, DataGridView dataGridView)
        {
            dataGridView.Rows.Clear();// 清空dataGridView中现有的数据
            try
            {
                string[] lines = File.ReadAllLines(filePath);// 读取所有行
                string[] currentContact = new string[5]; // 假设每个联系人有5个字段
                foreach (string line in lines)// 逐行读取文件
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;// 跳过空行
                    // 解析每一行，假设每个字段前都有关键字（如 "Name:"）
                    var parts = line.Split(new[] { ": " }, StringSplitOptions.None);
                    if (parts.Length != 2) continue;  // 如果格式不正确，跳过这一行
                    string key = parts[0];
                    string value = parts[1];
                    switch (key)// 根据字段类型存储到数组中
                    {
                        case "Name":
                            currentContact[0] = value; // Name字段存储在数组的第一个元素
                            break;
                        case "Address":
                            currentContact[1] = value; // Address字段存储在数组的第二个元素
                            break;
                        case "Phone":
                            currentContact[2] = value; // Phone字段存储在数组的第三个元素
                            break;
                        case "QQ":
                            currentContact[3] = value; // QQ字段存储在数组的第四个元素
                            break;
                        case "Remarks":
                            currentContact[4] = value; // Remarks字段存储在数组的第五个元素
                            // 将数组中的联系人信息作为对象数组添加到dataGridView中
                            object[] rowObjects = new object[currentContact.Length + 1]; // +1 为了复选框列
                            rowObjects[0] = false; // 第一列是复选框，先默认设置为false（未选中）
                            Array.Copy(currentContact, 0, rowObjects, 1, currentContact.Length);
                            dataGridView.Rows.Add(rowObjects);
                            currentContact = new string[5]; // 重置数组以便下一个联系人
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取文件时发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public bool IsValidEmail(string email)
        {
            if (email == null || email.Length == 0)
                return true;
            // 正则表达式模式，用于匹配电子邮件地址
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            // 创建正则表达式对象，并设置为忽略大小写
            Regex regex = new Regex(pattern);
            // 使用正则表达式对象的IsMatch方法判断字符串是否符合模式
            return regex.IsMatch(email);
        }
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            // 正则表达式模式，用于匹配电话号码
            // 这个模式允许数字、加号、减号、星号和井号
            // 假设电话号码以数字或加号开头，可能包含上述特殊字符
            string pattern = @"^[+-]?[0-9\-*#]*$";
            // 创建正则表达式对象，并设置为忽略大小写
            Regex regex = new Regex(pattern);
            // 使用正则表达式对象的IsMatch方法判断字符串是否符合模式
            return regex.IsMatch(phoneNumber);
        }
        private void UncheckAllCheckboxes()
        {
            foreach (DataGridViewRow row in dataGridView3.Rows)// 遍历所有行
            {
                // 确保行不是新行（即用户未开始编辑的行）
                if (!row.IsNewRow)
                {
                    // 尝试将第一列的值设置为 false（未选中状态）
                    // 需要检查 row.Cells[0].Value 是否为 bool 类型
                    if (row.Cells[0].OwningColumn is DataGridViewCheckBoxColumn && row.Cells[0].Value is bool)
                    {
                        row.Cells[0].Value = false;
                    }
                }
            }
        }
        //
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
        private string EnCode(string info)//对邮件正文进行解码操作
        {
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
                return info;
            }
        }

        private void button3_Click(object sender, EventArgs e)//刷新收件箱
        {
            Send("IMAP", "INBOX", "");//发送刷新指令
            //dataGridView1.SelectedRows.Clear();
            label16.Text = "上次刷新事件:" + DateTime.Now.ToString("F");
        }
    }
    public class Message
    {
        public string time { get; set; }//时间，发信的时候不需要设置
        public string body { get; set; }//正文
        public string from {  get; set; }//发件人
        public string to { get; set; }//收件人
        public string subject { get; set; }//主题
        public string cc { get; set; }//抄送
        public Message()
        {

        }
    }
    public class MailSender
    {
        public Log log;//记录日志信息
        public string email;//邮箱名
        public string pass;//授权码
        public string smtpServer;//smtp服务器
        public int port = 25;//端口号
        public SmtpClient smtp;//发邮件
        public MailRecver recver;//接收邮件
        public MailSender(string email,string pass)
        {
            this.email = email;
            this.pass = pass;
            this.log = MainForm.log;
        }
    }
    public class MailRecver//接收邮件类
    {
        public Log log;
        public string email;
        public string pass;
        public List<Message> Inbox = new();//消息链表，用来存放收件箱的所有消息
        public List<Message> Drafts = new();//草稿箱
        public MainForm form;//控制主界面的控件
        public MailRecver(string email, string pass)
        {
            this.email = email;
            this.pass = pass;
        }
    }
    public class Log
    {
        private string? time;
        private StreamWriter sw;
        public Log(string filename)
        {
            String Path = Environment.CurrentDirectory;//获取当前项目可执行文件的地址
            Path = Path.Substring(0, Path.Length - 24);//通过裁剪获得当前项目文件夹的地址
            Path += filename;
            sw = new StreamWriter(Path, true);//true表示追加模式
        }
        public void write(string type, string info)//向文件写入信息，记录日志
        {
            time = DateTime.Now.ToString("G");
            string msg = time + " [" + type + "] " + info;
            sw.WriteLine(msg);
            sw.Flush();
        }
    }
}
