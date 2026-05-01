using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public partial class MainReserve : Form
    {
        // UI 컨트롤 선언
        private Panel loginPanel;
        private TextBox txtId;
        private TextBox txtPw;
        private Button btnLogin;
        private Button btnSignUp;

        private Panel reservationPanel;
        private DateTimePicker datePicker;
        private Button btnSelectSeat;
        private Label lblStatus;

        public MainReserve()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "예약 메인 화면";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            // --- 1. 로그인/회원가입 영역 ---
            loginPanel = new Panel { Size = new Size(300, 160), Location = new Point(250, 80), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            Label lblTitle = new Label { Text = "로그인", Font = new Font("맑은 고딕", 14, FontStyle.Bold), Location = new Point(110, 10), AutoSize = true };
            txtId = new TextBox { Location = new Point(50, 50), Width = 200, PlaceholderText = "아이디 입력" };
            txtPw = new TextBox { Location = new Point(50, 80), Width = 200, PlaceholderText = "비밀번호 입력", PasswordChar = '*' };
            btnLogin = new Button { Text = "로그인", Location = new Point(50, 115), Width = 95, BackColor = Color.LightGray };
            btnSignUp = new Button { Text = "회원가입", Location = new Point(155, 115), Width = 95, BackColor = Color.LightGray };

            btnLogin.Click += BtnLogin_Click;
            btnSignUp.Click += (s, e) => MessageBox.Show("회원가입 폼과 연결");

            loginPanel.Controls.Add(lblTitle);
            loginPanel.Controls.Add(txtId);
            loginPanel.Controls.Add(txtPw);
            loginPanel.Controls.Add(btnLogin);
            loginPanel.Controls.Add(btnSignUp);

            // --- 2. 날짜 선택 영역 ---
            // 처음엔 Enabled = false로 둬서 로그인 전에는 못 누르게 막아둠
            reservationPanel = new Panel { Size = new Size(400, 220), Location = new Point(200, 270), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Enabled = false };

            Label lblResTitle = new Label { Text = "예약 날짜 선택", Font = new Font("맑은 고딕", 14, FontStyle.Bold), Location = new Point(130, 15), AutoSize = true };

            Label lblDate = new Label { Text = "날짜 선택:", Location = new Point(50, 75), AutoSize = true, Font = new Font("맑은 고딕", 10) };
            datePicker = new DateTimePicker { Location = new Point(150, 70), Width = 200, Format = DateTimePickerFormat.Short };

            btnSelectSeat = new Button { Text = "좌석 선택하기", Location = new Point(100, 110), Width = 200, Height = 40, BackColor = Color.CornflowerBlue, ForeColor = Color.White, Font = new Font("맑은 고딕", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnSelectSeat.Click += BtnSelectSeat_Click;

            reservationPanel.Controls.Add(lblResTitle);
            reservationPanel.Controls.Add(lblDate);
            reservationPanel.Controls.Add(datePicker);
            reservationPanel.Controls.Add(btnSelectSeat);

            // 안내 메시지
            lblStatus = new Label { Text = "로그인 해주세요.", Location = new Point(320, 460), AutoSize = true, ForeColor = Color.Red, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };

            this.Controls.Add(loginPanel);
            this.Controls.Add(reservationPanel);
            this.Controls.Add(lblStatus);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // 간단한 유효성 검사 (아무거나 입력하면 로그인 성공)
            if (!string.IsNullOrEmpty(txtId.Text) && !string.IsNullOrEmpty(txtPw.Text))
            {
                MessageBox.Show($"{txtId.Text}님, 로그인 성공!");
                reservationPanel.Enabled = true; // 로그인 성공 시 예약 패널 활성화
                loginPanel.Enabled = false;      // 중복 로그인 방지

                lblStatus.Text = "날짜를 먼저 선택해주세요.";
                lblStatus.ForeColor = Color.Blue;
                lblStatus.Location = new Point(260, 460); // 글씨 길어지면 위치 살짝 조정
            }
        }

        private void BtnSelectSeat_Click(object sender, EventArgs e)
        {
            string selectedDate = datePicker.Value.ToShortDateString();

            Seat seatForm = new Seat();
            seatForm.ShowDialog();
        }
    }
}