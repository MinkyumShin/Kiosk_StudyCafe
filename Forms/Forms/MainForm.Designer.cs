namespace Forms
{
    partial class MainForm
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
            lblWelcome = new Label();
            lblInfo = new Label();
            btnSeat = new Button();
            btnReservation = new Button();
            btnLogout = new Button();
            SuspendLayout();
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblWelcome.Location = new Point(140, 40);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(176, 25);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "회원님 환영합니다.";
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.Font = new Font("맑은 고딕", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblInfo.Location = new Point(130, 90);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(210, 17);
            lblInfo.TabIndex = 1;
            lblInfo.Text = "스터디카페 예약 시스템 메인 화면";
            // 
            // btnSeat
            // 
            btnSeat.Location = new Point(170, 140);
            btnSeat.Name = "btnSeat";
            btnSeat.Size = new Size(75, 23);
            btnSeat.TabIndex = 2;
            btnSeat.Text = "좌석 선택";
            btnSeat.UseVisualStyleBackColor = true;
            // 
            // btnReservation
            // 
            btnReservation.Location = new Point(170, 190);
            btnReservation.Name = "btnReservation";
            btnReservation.Size = new Size(75, 23);
            btnReservation.TabIndex = 3;
            btnReservation.Text = "예약 조회";
            btnReservation.UseVisualStyleBackColor = true;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(170, 240);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(75, 23);
            btnLogout.TabIndex = 4;
            btnLogout.Text = "로그아웃";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // StudyCafeMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 311);
            Controls.Add(btnLogout);
            Controls.Add(btnReservation);
            Controls.Add(btnSeat);
            Controls.Add(lblInfo);
            Controls.Add(lblWelcome);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblWelcome;
        private Label lblInfo;
        private Button btnSeat;
        private Button btnReservation;
        private Button btnLogout;
    }
}