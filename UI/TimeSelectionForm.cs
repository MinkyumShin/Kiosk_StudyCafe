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

        public TimeSelectionForm(int seatNumber, DateTime date, List<int> reservedHours)
        {
            this.Text = $"{date.ToShortDateString()} - {seatNumber}번 시간 선택";
            this.Size = new Size(300, 480);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblInfo = new Label { Text = "예약할 시간을 선택해주세요", Location = new Point(20, 20), AutoSize = true, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };
            timeList = new CheckedListBox { Location = new Point(20, 50), Size = new Size(240, 320), CheckOnClick = true, Font = new Font("맑은 고딕", 9) };

            // ★ 실시간 연동 로직
            DateTime now = DateTime.Now;
            bool isToday = date.Date == now.Date;
            bool isPastDate = date.Date < now.Date;

            for (int i = 0; i < 24; i++)
            {
                string timeText = $"{i:D2}:00 ~ {i + 1:D2}:00";

                // 오늘 날짜이면서 현재 시간(Hour)과 같거나 이전이면 불가 (ex: 현재 21시 30분이면 21:00~22:00 차단, 22:00~는 오픈)
                bool isPastHour = isPastDate || (isToday && i <= now.Hour);

                if (isPastHour)
                {
                    timeList.Items.Add(timeText + "  [시간 지남]");
                    if (!reservedHours.Contains(i)) reservedHours.Add(i); // 클릭 못 하도록 강제 편입
                }
                else if (reservedHours.Contains(i))
                {
                    timeList.Items.Add(timeText + "  [예약 마감]");
                }
                else
                {
                    timeList.Items.Add(timeText);
                }
            }

            timeList.ItemCheck += (s, e) => {
                if (reservedHours.Contains(e.Index)) e.NewValue = CheckState.Unchecked;
            };

            btnConfirm = new Button { Text = "선택 완료", Location = new Point(20, 390), Size = new Size(240, 40), BackColor = Color.FromArgb(20, 30, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnConfirm.Click += (s, e) => {
                foreach (var item in timeList.CheckedIndices) SelectedHours.Add((int)item);
                if (SelectedHours.Count == 0) { MessageBox.Show("최소 1시간 이상 선택해주세요"); return; }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.Add(lblInfo);
            this.Controls.Add(timeList);
            this.Controls.Add(btnConfirm);
        }
    }
}