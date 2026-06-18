using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public class PaymentForm : Form
    {
        public bool IsPaid { get; private set; } = false;

        private readonly Color navyTheme = Color.FromArgb(20, 30, 70);
        private readonly Color primaryColor = Color.FromArgb(78, 128, 238);
        private readonly Color dangerColor = Color.FromArgb(255, 112, 67);
        private readonly Color formBackColor = Color.FromArgb(245, 247, 250);

        public PaymentForm(int hours, int totalAmount)
        {
            Text = "모의 결제";
            ClientSize = new Size(380, 320);
            MinimumSize = new Size(380, 320);
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
                Text = "Key-Study 결제",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 16, FontStyle.Bold),
                Size = new Size(380, 38),
                Location = new Point(0, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubTitle = new Label
            {
                Text = "실제 결제가 아닌 프로젝트용 모의 결제입니다.",
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
                Size = new Size(310, 125),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblUseTimeTitle = new Label
            {
                Text = "총 이용 시간",
                Location = new Point(25, 22),
                Size = new Size(120, 25),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 90, 90)
            };

            Label lblUseTime = new Label
            {
                Text = $"{hours}시간",
                Location = new Point(170, 22),
                Size = new Size(110, 25),
                Font = new Font("맑은 고딕", 11, FontStyle.Bold),
                ForeColor = navyTheme,
                TextAlign = ContentAlignment.MiddleRight
            };

            Label lblAmountTitle = new Label
            {
                Text = "결제 금액",
                Location = new Point(25, 68),
                Size = new Size(120, 30),
                Font = new Font("맑은 고딕", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 90, 90)
            };

            Label lblAmount = new Label
            {
                Text = $"{totalAmount:N0}원",
                Location = new Point(130, 62),
                Size = new Size(150, 40),
                Font = new Font("맑은 고딕", 17, FontStyle.Bold),
                ForeColor = dangerColor,
                TextAlign = ContentAlignment.MiddleRight
            };

            infoPanel.Controls.Add(lblUseTimeTitle);
            infoPanel.Controls.Add(lblUseTime);
            infoPanel.Controls.Add(lblAmountTitle);
            infoPanel.Controls.Add(lblAmount);
            Controls.Add(infoPanel);

            Button btnPay = new Button
            {
                Text = "카드 결제하기",
                Location = new Point(35, 240),
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
                Location = new Point(245, 240),
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
                DialogResult result = MessageBox.Show(this,
                    $"{totalAmount:N0}원을 결제하시겠습니까?",
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