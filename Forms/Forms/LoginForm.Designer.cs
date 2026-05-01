namespace Forms
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            lblId = new Label();
            lblTitle = new Label();
            txtId = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnLogin = new Button();
            SuspendLayout();
            // 
            // lblId
            // 
            lblId.AutoSize = true;
            lblId.Location = new Point(70, 95);
            lblId.Name = "lblId";
            lblId.Size = new Size(43, 15);
            lblId.TabIndex = 0;
            lblId.Text = "아이디";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblTitle.Location = new Point(85, 30);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(237, 32);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "STUDY CAFE KIOSK";
            // 
            // txtId
            // 
            txtId.Location = new Point(150, 90);
            txtId.Name = "txtId";
            txtId.Size = new Size(160, 23);
            txtId.TabIndex = 2;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(70, 135);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(55, 15);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "비밀번호";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(150, 130);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.ShortcutsEnabled = false;
            txtPassword.Size = new Size(160, 23);
            txtPassword.TabIndex = 4;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(150, 180);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(75, 23);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "로그인";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 261);
            Controls.Add(btnLogin);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtId);
            Controls.Add(lblTitle);
            Controls.Add(lblId);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Study Cafe Login";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblId;
        private Label lblTitle;
        private TextBox txtId;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnLogin;
    }
}