using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public class PaymentForm : Form
    {
        public bool IsPaid { get; private set; } = false;

        public PaymentForm(int hours, int totalAmount)
        {
            this.Text = "결제 진행";
            this.Size = new Size(300, 220);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblInfo = new Label
            {
                Text = $"총 이용 시간: {hours}시간\n결제 금액: {totalAmount:N0}원",
                Location = new Point(40, 30),
                AutoSize = true,
                Font = new Font("맑은 고딕", 12, FontStyle.Bold)
            };

            Button btnPay = new Button { Text = "카드 결제하기", Location = new Point(40, 100), Size = new Size(200, 50), BackColor = Color.FromArgb(20, 30, 70), ForeColor = Color.White, Font = new Font("맑은 고딕", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat };

            btnPay.Click += (s, e) => {
                MessageBox.Show("결제가 완료되었습니다.", "결제 성공");
                IsPaid = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.Add(lblInfo);
            this.Controls.Add(btnPay);
        }
    }
}