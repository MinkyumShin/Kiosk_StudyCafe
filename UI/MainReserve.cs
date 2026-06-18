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
        private Button btnAdmin = null!;

        private Panel reservationPanel = null!;
        private DateTimePicker datePicker = null!;
        private Button btnSelectSeat = null!;
        private Button btnLogout = null!;
        private Label lblStatus = null!;
        private Label lblWelcome = null!;

        // 예약 백엔드 로직을 처리하는 매니저 객체
        private ReservationManager dbManager = null!;

        // 1분(60초)마다 노쇼 및 시간 만료를 체크할 백그라운드 타이머
        private System.Windows.Forms.Timer backgroundTimer = null!;

        // 로그인한 사용자의 ID를 저장해두었다가 예약 시 사용하기 위한 변수
        private string loggedInUserId = "";

        // 공통 색상
        private readonly Color primaryColor = Color.FromArgb(78, 128, 238);
        private readonly Color dangerColor = Color.FromArgb(255, 112, 67);
        private readonly Color darkColor = Color.FromArgb(55, 55, 55);
        private readonly Color formBackColor = Color.FromArgb(245, 246, 250);

        public MainReserve()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // ----- Form 기본 설정 -----
            this.Text = "Key-Study 예약 시스템";
            this.ClientSize = new Size(800, 600);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = formBackColor;
            this.Font = new Font("맑은 고딕", 10F);
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            // 혹시 Designer에서 만들어진 컨트롤이 있으면 정리
            this.Controls.Clear();

            // ----- 상단 타이틀 -----
            Label keystudy = new Label
            {
                Text = "Key-Study",
                Font = new Font("맑은 고딕", 26, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = false,
                Size = new Size(360, 55),
                Location = new Point((this.ClientSize.Width - 360) / 2, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(keystudy);

            // ----- 관리자 모드 버튼 -----
            btnAdmin = new Button
            {
                Text = "관리자 모드",
                Location = new Point(this.ClientSize.Width - 150, 28),
                Size = new Size(120, 38),
                BackColor = darkColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnAdmin.FlatAppearance.BorderSize = 0;
            btnAdmin.Click += BtnAdmin_Click;
            this.Controls.Add(btnAdmin);

            // ----- 로그인/회원가입 영역 -----
            loginPanel = new Panel
            {
                Size = new Size(360, 190),
                Location = new Point((this.ClientSize.Width - 360) / 2, 115),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "로그인",
                Font = new Font("맑은 고딕", 18, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = false,
                Size = new Size(360, 38),
                Location = new Point(0, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };

            txtId = new TextBox
            {
                Location = new Point(70, 65),
                Size = new Size(220, 28),
                PlaceholderText = "아이디 입력",
                Font = new Font("맑은 고딕", 10)
            };

            txtPw = new TextBox
            {
                Location = new Point(70, 98),
                Size = new Size(220, 28),
                PlaceholderText = "비밀번호 입력",
                PasswordChar = '*',
                Font = new Font("맑은 고딕", 10)
            };

            btnLogin = new Button
            {
                Text = "로그인",
                Location = new Point(70, 138),
                Size = new Size(105, 34),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            ApplyPrimaryButtonStyle(btnLogin);

            btnSignUp = new Button
            {
                Text = "회원가입",
                Location = new Point(185, 138),
                Size = new Size(105, 34),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            ApplyGrayButtonStyle(btnSignUp);

            btnLogin.Click += BtnLogin_Click;
            btnSignUp.Click += BtnSignUp_Click;

            // 비밀번호 입력 후 Enter 키로 로그인
            txtPw.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnLogin_Click(btnLogin, EventArgs.Empty);
                    e.SuppressKeyPress = true;
                }
            };

            loginPanel.Controls.Add(lblTitle);
            loginPanel.Controls.Add(txtId);
            loginPanel.Controls.Add(txtPw);
            loginPanel.Controls.Add(btnLogin);
            loginPanel.Controls.Add(btnSignUp);
            this.Controls.Add(loginPanel);

            // ----- 날짜 선택 영역 -----
            reservationPanel = new Panel
            {
                Size = new Size(500, 270),
                Location = new Point((this.ClientSize.Width - 500) / 2, 315),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 로그아웃 버튼: 제목과 겹치지 않도록 우측 상단 고정
            btnLogout = new Button
            {
                Text = "로그아웃",
                Location = new Point(375, 18),
                Size = new Size(105, 34),
                BackColor = dangerColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;

            Label lblResTitle = new Label
            {
                Text = "예약 날짜 선택",
                Font = new Font("맑은 고딕", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 70, 70),
                AutoSize = false,
                Size = new Size(500, 42),
                Location = new Point(0, 58),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblWelcome = new Label
            {
                Text = "로그인 후 예약할 날짜를 선택할 수 있습니다.",
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = false,
                Size = new Size(500, 28),
                Location = new Point(0, 105),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblDate = new Label
            {
                Text = "날짜 선택:",
                Location = new Point(105, 150),
                AutoSize = true,
                Font = new Font("맑은 고딕", 10)
            };

            datePicker = new DateTimePicker
            {
                Location = new Point(195, 145),
                Size = new Size(210, 28),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Enabled = false,
                Font = new Font("맑은 고딕", 10)
            };

            btnSelectSeat = new Button
            {
                Text = "좌석 선택하기",
                Location = new Point(150, 198),
                Size = new Size(200, 42),
                Font = new Font("맑은 고딕", 11, FontStyle.Bold),
                Enabled = false,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            ApplyPrimaryButtonStyle(btnSelectSeat);
            btnSelectSeat.Click += BtnSelectSeat_Click;

            reservationPanel.Controls.Add(btnLogout);
            reservationPanel.Controls.Add(lblResTitle);
            reservationPanel.Controls.Add(lblWelcome);
            reservationPanel.Controls.Add(lblDate);
            reservationPanel.Controls.Add(datePicker);
            reservationPanel.Controls.Add(btnSelectSeat);
            this.Controls.Add(reservationPanel);

            // ----- 안내 메시지 -----
            lblStatus = new Label
            {
                Text = "로그인 해주세요.",
                AutoSize = false,
                Size = new Size(800, 30),
                Location = new Point(0, 560),
                ForeColor = Color.FromArgb(220, 60, 60),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblStatus);

            // 데이터베이스 매니저 객체 생성 및 초기화
            dbManager = new ReservationManager();

            // 백그라운드 타이머 설정
            backgroundTimer = new System.Windows.Forms.Timer
            {
                Interval = 60000
            };
            backgroundTimer.Tick += BackgroundTimer_Tick;
            backgroundTimer.Start();

            // 초기 화면 상태 적용
            SetLoginState(false);
        }

        private void ApplyPrimaryButtonStyle(Button button)
        {
            button.BackColor = primaryColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void ApplyGrayButtonStyle(Button button)
        {
            button.BackColor = Color.FromArgb(235, 238, 242);
            button.ForeColor = Color.FromArgb(40, 40, 40);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);
            button.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void SetLoginState(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                loginPanel.Visible = false;

                // 로그인 후에는 예약 패널을 화면 중앙 쪽으로 올림
                reservationPanel.Location = new Point((this.ClientSize.Width - reservationPanel.Width) / 2, 170);

                datePicker.Enabled = true;
                btnSelectSeat.Enabled = true;
                btnLogout.Visible = true;

                // 포인트 조회 및 환영 메시지에 반영
                UserManager userManager = new UserManager();
                int currentPoints = userManager.GetUserPoints(loggedInUserId);

                lblWelcome.Text = $"{loggedInUserId}님 환영합니다! (보유 포인트: {currentPoints:N0} P)";
                lblWelcome.ForeColor = Color.FromArgb(70, 70, 70);
                lblWelcome.Location = new Point(0, 105);
                lblWelcome.Size = new Size(500, 28);
                lblWelcome.TextAlign = ContentAlignment.MiddleCenter;

                lblStatus.Text = "날짜를 선택한 뒤 좌석 선택하기를 눌러주세요.";
                lblStatus.ForeColor = primaryColor;
            }
            else
            {
                loginPanel.Visible = true;
                loginPanel.Enabled = true;

                // 로그인 전에는 로그인 패널 아래쪽에 예약 패널 배치
                reservationPanel.Location = new Point((this.ClientSize.Width - reservationPanel.Width) / 2, 315);

                datePicker.Enabled = false;
                btnSelectSeat.Enabled = false;
                btnLogout.Visible = false;

                lblWelcome.Text = "로그인 후 예약할 날짜를 선택할 수 있습니다.";
                lblWelcome.ForeColor = Color.Gray;
                lblWelcome.Location = new Point(0, 105);
                lblWelcome.Size = new Size(500, 28);
                lblWelcome.TextAlign = ContentAlignment.MiddleCenter;

                lblStatus.Text = "로그인 해주세요.";
                lblStatus.ForeColor = Color.FromArgb(220, 60, 60);
            }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string userId = txtId.Text.Trim();
            string password = txtPw.Text.Trim();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(this,
                    "아이디와 비밀번호를 모두 입력해주세요.",
                    "로그인 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            UserManager userManager = new UserManager();
            bool result = userManager.Login(userId, password);

            if (result)
            {
                loggedInUserId = userId;

                // 로그인 성공 안내에도 포인트 표시 추가
                int currentPoints = userManager.GetUserPoints(userId);
                MessageBox.Show(this,
                    $"{userId}님, 로그인 성공!\n현재 잔여 포인트는 {currentPoints:N0} P 입니다.",
                    "로그인 성공",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                SetLoginState(true);
            }
            else
            {
                MessageBox.Show(this,
                    "아이디 또는 비밀번호가 틀렸습니다.",
                    "로그인 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtPw.Clear();
                txtPw.Focus();
            }
        }

        private void BtnSignUp_Click(object? sender, EventArgs e)
        {
            using (var registerForm = new RegisterForm())
            {
                registerForm.ShowDialog(this);
            }
        }

        private void BtnSelectSeat_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(loggedInUserId))
            {
                MessageBox.Show(this,
                    "로그인 후 이용해주세요.",
                    "알림",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string selectedDate = datePicker.Value.ToString("yyyy-MM-dd");

            // 날짜, 로그인 ID를 좌석 선택 폼에 전달
            using (Seat seatForm = new Seat(selectedDate, loggedInUserId))
            {
                seatForm.ShowDialog(this);
                // 좌석 선택 창에서 결제로 인해 포인트가 변동되었을 수 있으므로 화면 갱신
                SetLoginState(true);
            }
        }

        // 타이머가 1분마다 실행하는 이벤트 핸들러. DB의 시간 만료 데이터와 노쇼 데이터를 자동 정리함.
        private void BackgroundTimer_Tick(object? sender, EventArgs e)
        {
            dbManager.ProcessTimeoutsAndNoShows();
        }

        private void MainReserve_Load(object sender, EventArgs e)
        {
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
                "로그아웃 하시겠습니까?",
                "로그아웃",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            loggedInUserId = "";
            txtId.Clear();
            txtPw.Clear();

            SetLoginState(false);

            MessageBox.Show(this,
                "로그아웃 되었습니다.",
                "알림",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BtnAdmin_Click(object? sender, EventArgs e)
        {
            using (Form pwForm = new Form()
            {
                Width = 320,
                Height = 180,
                Text = "관리자 인증",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            })
            {
                Label lblPw = new Label()
                {
                    Text = "관리자 비밀번호",
                    Left = 25,
                    Top = 25,
                    AutoSize = true,
                    Font = new Font("맑은 고딕", 10, FontStyle.Bold)
                };

                TextBox txtInput = new TextBox()
                {
                    Left = 25,
                    Top = 55,
                    Width = 250,
                    PasswordChar = '*',
                    Font = new Font("맑은 고딕", 10)
                };

                Button btnOk = new Button()
                {
                    Text = "확인",
                    Left = 95,
                    Top = 95,
                    Width = 90,
                    Height = 32,
                    DialogResult = DialogResult.OK,
                    Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                ApplyPrimaryButtonStyle(btnOk);

                Button btnCancel = new Button()
                {
                    Text = "취소",
                    Left = 190,
                    Top = 95,
                    Width = 90,
                    Height = 32,
                    DialogResult = DialogResult.Cancel,
                    Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                ApplyGrayButtonStyle(btnCancel);

                pwForm.Controls.Add(lblPw);
                pwForm.Controls.Add(txtInput);
                pwForm.Controls.Add(btnOk);
                pwForm.Controls.Add(btnCancel);
                pwForm.AcceptButton = btnOk;
                pwForm.CancelButton = btnCancel;

                if (pwForm.ShowDialog(this) == DialogResult.OK)
                {
                    if (txtInput.Text == "1234")
                    {
                        AdminForm adminForm = new AdminForm();
                        adminForm.Show(this);
                    }
                    else
                    {
                        MessageBox.Show(this,
                            "비밀번호가 틀렸습니다.",
                            "인증 실패",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }
}