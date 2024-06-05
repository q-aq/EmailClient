namespace EmailClient
{
    partial class LoginForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param email="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            button2 = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(186, 415);
            button1.Name = "button1";
            button1.Size = new Size(150, 46);
            button1.TabIndex = 0;
            button1.Text = "登录";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(332, 199);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(324, 38);
            textBox1.TabIndex = 1;
            textBox1.Text = "13569718997@163.com";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(332, 302);
            textBox2.Name = "textBox2";
            textBox2.PasswordChar = '*';
            textBox2.Size = new Size(324, 38);
            textBox2.TabIndex = 2;
            textBox2.Text = "NGIVHQLOCSGASMMO";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(186, 206);
            label1.Name = "label1";
            label1.Size = new Size(110, 31);
            label1.TabIndex = 3;
            label1.Text = "电子邮箱";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(186, 305);
            label2.Name = "label2";
            label2.Size = new Size(86, 31);
            label2.TabIndex = 4;
            label2.Text = "授权码";
            // 
            // button2
            // 
            button2.Location = new Point(506, 415);
            button2.Name = "button2";
            button2.Size = new Size(150, 46);
            button2.TabIndex = 5;
            button2.Text = "退出";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click_1;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(894, 579);
            Controls.Add(button2);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Name = "LoginForm";
            Text = "Login";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox textBox1;
        private TextBox textBox2;
        private Label label1;
        private Label label2;
        private Button button2;
    }
}
