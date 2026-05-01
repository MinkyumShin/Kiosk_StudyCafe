using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public partial class TimeSelectionForm : Form
    {
        public List<int> SelectedHours { get; private set; } = new List<int>();
        private CheckedListBox timeList;
        private Button btnConfirm;

        public TimeSelectionForm(int seatNumber, DateTime date)
        {
            this.Text = $"{date.ToShortDateString()} - {seatNumber}번 좌석 시간 선택";
            this.Size = new Size(300, 450);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblInfo = new Label
            {
                Text = "예약할 시간을 선택해주세요",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("맑은 고딕", 10, FontStyle.Bold)
            };

            // 00시부터 23시까지 리스트 생성
            timeList = new CheckedListBox
            {
                Location = new Point(20, 50),
                Size = new Size(240, 300),
                CheckOnClick = true
            };

            for (int i = 0; i < 24; i++)
            {
                timeList.Items.Add($"{i:D2}:00 ~ {i + 1:D2}:00");

                // 실제 프로젝트라면 여기서 DB를 조회해 이미 예약된 시간은 비활성화 처리해야 해
                // 예: if (isReserved) timeList.SetItemCheckState(i, CheckState.Indeterminate);
            }

            btnConfirm = new Button
            {
                Text = "선택 완료",
                Location = new Point(20, 360),
                Size = new Size(240, 40),
                BackColor = Color.CornflowerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnConfirm.Click += (s, e) => {
                foreach (var item in timeList.CheckedIndices)
                {
                    SelectedHours.Add((int)item);
                }

                if (SelectedHours.Count == 0)
                {
                    MessageBox.Show("최소 1시간 이상 선택해주세요");
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.Add(lblInfo);
            this.Controls.Add(timeList);
            this.Controls.Add(btnConfirm);
        }
    }
}