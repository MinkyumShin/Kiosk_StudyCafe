using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public class PaymentForm : Form
    {
        public bool IsPaid { get; private set; } = false;
        public bool UsePoints { get; private set; } = false; // 추가: 결제 수단 결과값

        private readonly Color navyTheme = Color.FromArgb(20, 30, 70);
        private readonly Color primaryColor = Color.FromArgb(78, 128, 238);
        private readonly Color dangerColor = Color.FromArgb(255, 112, 67);
        private readonly Color formBackColor = Color.FromArgb(245, 247, 250);

        public PaymentForm(string userId, int hours, int totalAmount)
        {
            UserManager userManager = new UserManager();
            int currentPoints = userManager.GetUserPoints(userId);

            Text = "결제 수단 선택";
            ClientSize = new Size(380, 420); // 결제 수단 라디오 버튼을 위해 높이 확장
            MinimumSize = new Size(380, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = formBackColor;
            Font = new Font("맑은 고딕", 10);

            // 상단 헤더
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = navyTheme
            };

            Label lblTitle = new Label
            {
                Text = "결제 진행",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 16, FontStyle.Bold),
                Size = new Size(380, 38),
                Location = new Point(0, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubTitle = new Label
            {
                Text = "결제 수단을 선택해주세요.",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 8, FontStyle.Bold),
                Size = new Size(380, 22),
                Location = new Point(0, 43),
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubTitle);
            Controls.Add(headerPanel);

            Panel infoPanel = new Panel
            {
                Location = new Point(35, 95),
                Size = new Size(310, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblUseTimeTitle = new Label
            {
                Text = "총 이용 시간",
                Location = new Point(20, 15),
                Size = new Size(120, 25),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 90, 90)
            };

            Label lblUseTime = new Label
            {
                Text = $"{hours}시간",
                Location = new Point(170, 15),
                Size = new Size(110, 25),
                Font = new Font("맑은 고딕", 11, FontStyle.Bold),
                ForeColor = navyTheme,
                TextAlign = ContentAlignment.MiddleRight
            };

            Label lblAmountTitle = new Label
            {
                Text = "결제 금액",
                Location = new Point(20, 50),
                Size = new Size(120, 30),
                Font = new Font("맑은 고딕", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 90, 90)
            };

            Label lblAmount = new Label
            {
                Text = $"{totalAmount:N0}원",
                Location = new Point(130, 45),
                Size = new Size(150, 40),
                Font = new Font("맑은 고딕", 16, FontStyle.Bold),
                ForeColor = dangerColor,
                TextAlign = ContentAlignment.MiddleRight
            };

            infoPanel.Controls.Add(lblUseTimeTitle);
            infoPanel.Controls.Add(lblUseTime);
            infoPanel.Controls.Add(lblAmountTitle);
            infoPanel.Controls.Add(lblAmount);
            Controls.Add(infoPanel);

            // ----- 결제 수단 선택 영역 -----
            Label lblMethodTitle = new Label
            {
                Text = "결제 수단",
                Location = new Point(35, 215),
                Size = new Size(100, 25),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold)
            };
            Controls.Add(lblMethodTitle);

            RadioButton rbCard = new RadioButton
            {
                Text = "신용/체크카드 (현금/모의결제)",
                Location = new Point(45, 245),
                Size = new Size(250, 25),
                Checked = true,
                Font = new Font("맑은 고딕", 10)
            };

            RadioButton rbPoint = new RadioButton
            {
                Text = $"포인트 결제 (보유: {currentPoints:N0} P)",
                Location = new Point(45, 275),
                Size = new Size(250, 25),
                Font = new Font("맑은 고딕", 10)
            };

            Controls.Add(rbCard);
            Controls.Add(rbPoint);

            Button btnPay = new Button
            {
                Text = "결제하기",
                Location = new Point(35, 335),
                Size = new Size(200, 45),
                BackColor = primaryColor,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPay.FlatAppearance.BorderSize = 0;

            Button btnCancel = new Button
            {
                Text = "취소",
                Location = new Point(245, 335),
                Size = new Size(100, 45),
                BackColor = dangerColor,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnPay.Click += (s, e) =>
            {
                if (rbPoint.Checked)
                {
                    if (currentPoints < totalAmount)
                    {
                        MessageBox.Show(this,
                            $"포인트가 부족합니다.\n(필요: {totalAmount:N0} P, 보유: {currentPoints:N0} P)",
                            "잔액 부족",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    UsePoints = true;
                }
                else
                {
                    UsePoints = false;
                }

                string methodText = UsePoints ? "포인트" : "신용/체크카드";

                DialogResult result = MessageBox.Show(this,
                    $"{totalAmount:N0}원을 '{methodText}'로 결제하시겠습니까?",
                    "결제 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                MessageBox.Show(this,
                    "결제가 완료되었습니다.",
                    "결제 성공",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                IsPaid = true;
                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel.Click += (s, e) =>
            {
                IsPaid = false;
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(btnPay);
            Controls.Add(btnCancel);
        }
    }
}