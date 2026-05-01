using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public partial class MainReserve : Form
    {
        // UI 컨트롤 선언
        private Panel loginPanel = null!;
        private TextBox txtId = null!;
        private TextBox txtPw = null!;
        private Button btnLogin = null!;
        private Button btnSignUp = null!;

        private Panel reservationPanel = null!;
        private DateTimePicker datePicker = null!;
        private Button btnSelectSeat = null!;
        private Label lblStatus = null!;

        //예약 백엔드 로직을 처리하는 매니저 객체
        private ReservationManager dbManager = null!;

        //1분(60초)마다 노쇼 및 시간 만료를 체크할 백그라운드 타이머
        private System.Windows.Forms.Timer backgroundTimer = null!;

        //로그인한 사용자의 ID를 저장해두었다가 예약 시 사용하기 위한 변수
        private string loggedInUserId = "";

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

            //데이터베이스 매니저 객체 생성 및 초기화
            dbManager = new ReservationManager();

            //백그라운드 타이머 설정 (60000ms = 1분 주기)
            backgroundTimer = new System.Windows.Forms.Timer();
            backgroundTimer.Interval = 60000;
            backgroundTimer.Tick += BackgroundTimer_Tick;
            backgroundTimer.Start();
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            UserManager userManager = new UserManager();

            bool result = userManager.Login(txtId.Text, txtPw.Text);

            if (result)
            {
                loggedInUserId = txtId.Text;

                MessageBox.Show($"{txtId.Text}님, 로그인 성공!");
                reservationPanel.Enabled = true;
                loginPanel.Enabled = false;

                lblStatus.Text = "날짜를 먼저 선택해주세요.";
                lblStatus.ForeColor = Color.Blue;
                lblStatus.Location = new Point(260, 460);
            }
            else
            {
                MessageBox.Show("아이디 또는 비밀번호가 틀렸습니다.");
            }
        }

        private void BtnSelectSeat_Click(object? sender, EventArgs e)
        {
            string selectedDate = datePicker.Value.ToShortDateString();

            // 에러 안 나도록 파라미터(날짜, 로그인ID) 넘겨주기 적용
            Seat seatForm = new Seat(selectedDate, loggedInUserId);
            seatForm.ShowDialog();
        }

        //타이머가 1분마다 실행하는 이벤트 핸들러. DB의 시간 만료 데이터와 노쇼 데이터를 자동 정리함.
        private void BackgroundTimer_Tick(object? sender, EventArgs e)
        {
            dbManager.ProcessTimeoutsAndNoShows();
        }

        private void MainReserve_Load(object sender, EventArgs e)
        {

        }
    }
}