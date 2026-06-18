using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public partial class RegisterForm : Form
    {
        private bool isIdChecked = false; // 중복 확인 버튼을 눌렀는지 체크
        private UserManager userManager;

        public RegisterForm()
        {
            userManager = new UserManager();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "스터디카페 회원가입";
            this.Size = new Size(380, 600); // 폼의 높이를 넉넉하게 키움
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label { Text = "회원가입", Font = new Font("맑은 고딕", 18, FontStyle.Bold), Location = new Point(130, 20), AutoSize = true, ForeColor = Color.Navy };

            // 1. 아이디 & 중복확인 버튼
            Label lblId = new Label { Text = "아이디", Location = new Point(40, 75), AutoSize = true, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };
            TextBox txtId = new TextBox { Location = new Point(40, 100), Width = 180, Font = new Font("맑은 고딕", 11) };
            Button btnCheckId = new Button { Text = "중복확인", Location = new Point(230, 99), Width = 80, Height = 28, BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };

            // 2. 비밀번호
            Label lblPw = new Label { Text = "비밀번호", Location = new Point(40, 140), AutoSize = true, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };
            TextBox txtPw = new TextBox { Location = new Point(40, 165), Width = 270, PasswordChar = '*', Font = new Font("맑은 고딕", 11) };

            // 3. 비밀번호 확인
            Label lblPwConfirm = new Label { Text = "비밀번호 확인", Location = new Point(40, 205), AutoSize = true, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };
            TextBox txtPwConfirm = new TextBox { Location = new Point(40, 230), Width = 270, PasswordChar = '*', Font = new Font("맑은 고딕", 11) };

            // 4. 이름
            Label lblName = new Label { Text = "이름", Location = new Point(40, 270), AutoSize = true, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };
            TextBox txtName = new TextBox { Location = new Point(40, 295), Width = 270, Font = new Font("맑은 고딕", 11) };

            // 5. 전화번호
            Label lblPhone = new Label { Text = "전화번호", Location = new Point(40, 335), AutoSize = true, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };
            TextBox txtPhone = new TextBox { Location = new Point(40, 360), Width = 270, Font = new Font("맑은 고딕", 11), PlaceholderText = "010-0000-0000" };

            // 6. 주민등록번호 (앞 6자리 - 뒤 1자리 안내)
            Label lblRrn = new Label { Text = "주민등록번호", Location = new Point(40, 400), AutoSize = true, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };
            TextBox txtRrn = new TextBox { Location = new Point(40, 425), Width = 270, Font = new Font("맑은 고딕", 11), PlaceholderText = "예) 990101-1 (뒷자리는 1자리만)" };

            // 가입 버튼
            Button btnSubmit = new Button { Text = "가입 완료하기", Location = new Point(40, 490), Width = 270, Height = 45, BackColor = Color.Navy, ForeColor = Color.White, Font = new Font("맑은 고딕", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat };

            // --- 이벤트 핸들러 ---

            // 사용자가 아이디를 수정하면 다시 중복확인을 하도록 상태 초기화
            txtId.TextChanged += (s, e) => {
                isIdChecked = false;
                btnCheckId.BackColor = Color.LightGray;
                btnCheckId.Text = "중복확인";
            };

            // 중복확인 버튼 클릭 시
            btnCheckId.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtId.Text))
                {
                    MessageBox.Show("아이디를 입력해주세요.");
                    return;
                }

                if (userManager.IsIdDuplicate(txtId.Text))
                {
                    MessageBox.Show("이미 사용중인 아이디입니다.");
                    isIdChecked = false;
                }
                else
                {
                    MessageBox.Show("사용 가능한 아이디입니다.");
                    isIdChecked = true;
                    btnCheckId.BackColor = Color.LightGreen; // 시각적 확인
                    btnCheckId.Text = "확인완료";
                }
            };

            // 가입 완료 버튼 클릭 시 유효성 꼼꼼히 검증
            btnSubmit.Click += (s, e) => {
                // 검증 1: 아이디 중복확인 진행 여부
                if (!isIdChecked)
                {
                    MessageBox.Show("아이디 중복확인을 진행해주세요.", "확인 필요");
                    return;
                }

                // 검증 2: 비밀번호와 재확인 텍스트가 정확히 일치하는지
                if (string.IsNullOrWhiteSpace(txtPw.Text) || txtPw.Text != txtPwConfirm.Text)
                {
                    MessageBox.Show("비밀번호가 일치하지 않거나 비어있습니다.", "입력 오류");
                    return;
                }

                // 검증 3: 빈 칸 여부
                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
                {
                    MessageBox.Show("이름과 전화번호를 모두 입력해주세요.", "입력 오류");
                    return;
                }

                // 검증 4: 주민등록번호 형식 강제 제한 (숫자 6개 - 숫자 1~4 중 1개)
                if (!Regex.IsMatch(txtRrn.Text, @"^\d{6}-[1-4]$"))
                {
                    MessageBox.Show("주민등록번호 형식이 올바르지 않습니다.\n하이픈(-)을 포함하여 앞 6자리와 뒷 1자리를 입력해주세요.", "입력 오류");
                    return;
                }

                // 모든 검증을 통과하면 DB에 Insert
                bool isSuccess = userManager.Register(txtId.Text, txtPw.Text, txtName.Text, txtPhone.Text, txtRrn.Text);

                if (isSuccess)
                {
                    MessageBox.Show($"{txtName.Text}님, 회원가입이 완료되었습니다!\n가입 기념 10,000 포인트가 지급되었습니다.", "가입 성공");
                    this.DialogResult = DialogResult.OK;
                    this.Close(); // 성공 시 폼 닫기
                }
                else
                {
                    MessageBox.Show("가입 처리 중 데이터베이스 오류가 발생했습니다.", "시스템 오류");
                }
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblId);
            this.Controls.Add(txtId);
            this.Controls.Add(btnCheckId);
            this.Controls.Add(lblPw);
            this.Controls.Add(txtPw);
            this.Controls.Add(lblPwConfirm);
            this.Controls.Add(txtPwConfirm);
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);
            this.Controls.Add(lblRrn);
            this.Controls.Add(txtRrn);
            this.Controls.Add(btnSubmit);
        }
    }
}