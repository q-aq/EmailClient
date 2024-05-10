namespace EmailClient
{
    partial class Forms
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelwrite = new Panel();
            textBox4 = new TextBox();
            textBox3 = new TextBox();
            textBox2 = new TextBox();
            textBox1 = new TextBox();
            label5 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            buttonexit = new Button();
            buttonsave = new Button();
            buttonsend = new Button();
            panelrecv = new Panel();
            label6 = new Label();
            label4 = new Label();
            panelmain = new Panel();
            panelwrite.SuspendLayout();
            panelrecv.SuspendLayout();
            panelmain.SuspendLayout();
            SuspendLayout();
            // 
            // panelwrite
            // 
            panelwrite.BackColor = Color.WhiteSmoke;
            panelwrite.Controls.Add(textBox4);
            panelwrite.Controls.Add(textBox3);
            panelwrite.Controls.Add(textBox2);
            panelwrite.Controls.Add(textBox1);
            panelwrite.Controls.Add(label5);
            panelwrite.Controls.Add(label3);
            panelwrite.Controls.Add(label2);
            panelwrite.Controls.Add(label1);
            panelwrite.Controls.Add(buttonexit);
            panelwrite.Controls.Add(buttonsave);
            panelwrite.Controls.Add(buttonsend);
            panelwrite.Location = new Point(1415, 156);
            panelwrite.Name = "panelwrite";
            panelwrite.Size = new Size(1269, 670);
            panelwrite.TabIndex = 6;
            // 
            // textBox4
            // 
            textBox4.Location = new Point(138, 267);
            textBox4.Multiline = true;
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(1110, 382);
            textBox4.TabIndex = 10;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(138, 207);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(1110, 38);
            textBox3.TabIndex = 9;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(138, 146);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(1110, 38);
            textBox2.TabIndex = 8;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(138, 84);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(1110, 38);
            textBox1.TabIndex = 7;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(61, 267);
            label5.Name = "label5";
            label5.Size = new Size(62, 31);
            label5.TabIndex = 6;
            label5.Text = "正文";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(61, 207);
            label3.Name = "label3";
            label3.Size = new Size(62, 31);
            label3.TabIndex = 5;
            label3.Text = "主题";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(61, 149);
            label2.Name = "label2";
            label2.Size = new Size(62, 31);
            label2.TabIndex = 4;
            label2.Text = "抄送";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(37, 91);
            label1.Name = "label1";
            label1.Size = new Size(86, 31);
            label1.TabIndex = 3;
            label1.Text = "收件人";
            // 
            // buttonexit
            // 
            buttonexit.Location = new Point(412, 16);
            buttonexit.Name = "buttonexit";
            buttonexit.Size = new Size(150, 46);
            buttonexit.TabIndex = 2;
            buttonexit.Text = "退出";
            buttonexit.UseVisualStyleBackColor = true;
            // 
            // buttonsave
            // 
            buttonsave.Location = new Point(229, 16);
            buttonsave.Name = "buttonsave";
            buttonsave.Size = new Size(150, 46);
            buttonsave.TabIndex = 1;
            buttonsave.Text = "存草稿";
            buttonsave.UseVisualStyleBackColor = true;
            // 
            // buttonsend
            // 
            buttonsend.Location = new Point(37, 16);
            buttonsend.Name = "buttonsend";
            buttonsend.Size = new Size(150, 46);
            buttonsend.TabIndex = 0;
            buttonsend.Text = "发送";
            buttonsend.UseVisualStyleBackColor = true;
            // 
            // panelrecv
            // 
            panelrecv.BackColor = Color.WhiteSmoke;
            panelrecv.Controls.Add(label6);
            panelrecv.Location = new Point(12, 704);
            panelrecv.Name = "panelrecv";
            panelrecv.Size = new Size(1272, 676);
            panelrecv.TabIndex = 5;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft YaHei UI", 16.125F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label6.Location = new Point(28, 27);
            label6.Name = "label6";
            label6.Size = new Size(154, 57);
            label6.TabIndex = 0;
            label6.Text = "收信箱";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft YaHei UI", 42F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label4.Location = new Point(415, 210);
            label4.Name = "label4";
            label4.Size = new Size(286, 146);
            label4.TabIndex = 0;
            label4.Text = "你好";
            // 
            // panelmain
            // 
            panelmain.Controls.Add(label4);
            panelmain.Location = new Point(12, 12);
            panelmain.Name = "panelmain";
            panelmain.Size = new Size(1272, 676);
            panelmain.TabIndex = 4;
            // 
            // Forms
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1977, 1028);
            Controls.Add(panelrecv);
            Controls.Add(panelmain);
            Controls.Add(panelwrite);
            Name = "Forms";
            Text = "Forms";
            panelwrite.ResumeLayout(false);
            panelwrite.PerformLayout();
            panelrecv.ResumeLayout(false);
            panelrecv.PerformLayout();
            panelmain.ResumeLayout(false);
            panelmain.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panelwrite;
        private TextBox textBox4;
        private TextBox textBox3;
        private TextBox textBox2;
        private TextBox textBox1;
        private Label label5;
        private Label label3;
        private Label label2;
        private Label label1;
        private Button buttonexit;
        private Button buttonsave;
        private Button buttonsend;
        private Panel panelrecv;
        private Label label6;
        private Label label4;
        private Panel panelmain;
    }
}