using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public partial class TimeSelectionForm : Form
    {
        public List<int> SelectedHours { get; private set; } = new List<int>();

        private readonly CheckedListBox timeList;
        private readonly Button btnConfirm;
        private readonly Button btnCancel;
        private readonly Label lblSummary;

        private readonly HashSet<int> blockedHours = new HashSet<int>();
        private readonly Dictionary<int, string> blockedReasons = new Dictionary<int, string>();

        private readonly Color navyTheme = Color.FromArgb(20, 30, 70);
        private readonly Color primaryColor = Color.FromArgb(78, 128, 238);
        private readonly Color dangerColor = Color.FromArgb(255, 112, 67);
        private readonly Color formBackColor = Color.FromArgb(245, 247, 250);

        public TimeSelectionForm(int seatNumber, DateTime date, List<int> reservedHours)
        {
            Text = $"{date:yyyy-MM-dd} - {seatNumber}번 좌석 시간 선택";
            ClientSize = new Size(360, 560);
            MinimumSize = new Size(360, 560);
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
                Text = "예약 시간 선택",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 16, FontStyle.Bold),
                Size = new Size(360, 32),
                Location = new Point(0, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubTitle = new Label
            {
                Text = $"{seatNumber}번 좌석  |  {date:yyyy-MM-dd}",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Size = new Size(360, 24),
                Location = new Point(0, 42),
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubTitle);
            Controls.Add(headerPanel);

            Label lblInfo = new Label
            {
                Text = "예약할 시간대를 선택해주세요.",
                Location = new Point(25, 88),
                Size = new Size(310, 24),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblGuide = new Label
            {
                Text = "※ [시간 지남], [예약 마감] 시간대는 선택할 수 없습니다.",
                Location = new Point(25, 112),
                Size = new Size(310, 24),
                Font = new Font("맑은 고딕", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 120, 120),
                TextAlign = ContentAlignment.MiddleLeft
            };

            timeList = new CheckedListBox
            {
                Location = new Point(25, 145),
                Size = new Size(310, 275),
                CheckOnClick = true,
                Font = new Font("맑은 고딕", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            DateTime now = DateTime.Now;
            bool isToday = date.Date == now.Date;
            bool isPastDate = date.Date < now.Date;

            for (int i = 0; i < 24; i++)
            {
                string timeText = $"{i:D2}:00 ~ {i + 1:D2}:00";
                bool isPastHour = isPastDate || (isToday && i <= now.Hour);
                bool isReserved = reservedHours.Contains(i);

                if (isPastHour)
                {
                    timeList.Items.Add(timeText + "   [시간 지남]");
                    blockedHours.Add(i);
                    blockedReasons[i] = "이미 지난 시간대입니다.";
                }
                else if (isReserved)
                {
                    timeList.Items.Add(timeText + "   [예약 마감]");
                    blockedHours.Add(i);
                    blockedReasons[i] = "이미 예약이 마감된 시간대입니다.";
                }
                else
                {
                    timeList.Items.Add(timeText);
                }
            }

            timeList.ItemCheck += TimeList_ItemCheck;

            timeList.SelectedIndexChanged += (s, e) =>
            {
                UpdateSummary();
            };

            timeList.ItemCheck += (s, e) =>
            {
                BeginInvoke(new Action(UpdateSummary));
            };

            lblSummary = new Label
            {
                Text = "선택 시간: 0시간  |  예상 금액: 0원",
                Location = new Point(25, 435),
                Size = new Size(310, 30),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                ForeColor = navyTheme,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnConfirm = new Button
            {
                Text = "선택 완료",
                Location = new Point(25, 480),
                Size = new Size(190, 42),
                BackColor = primaryColor,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new Button
            {
                Text = "취소",
                Location = new Point(225, 480),
                Size = new Size(110, 42),
                BackColor = dangerColor,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(lblInfo);
            Controls.Add(lblGuide);
            Controls.Add(timeList);
            Controls.Add(lblSummary);
            Controls.Add(btnConfirm);
            Controls.Add(btnCancel);
        }

        private void TimeList_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (blockedHours.Contains(e.Index))
            {
                e.NewValue = CheckState.Unchecked;

                string reason = blockedReasons.ContainsKey(e.Index)
                    ? blockedReasons[e.Index]
                    : "선택할 수 없는 시간대입니다.";

                MessageBox.Show(this, reason, "선택 불가",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateSummary()
        {
            int selectedCount = 0;

            foreach (int index in timeList.CheckedIndices)
            {
                if (!blockedHours.Contains(index))
                    selectedCount++;
            }

            int totalPrice = selectedCount * 2000;
            lblSummary.Text = $"선택 시간: {selectedCount}시간  |  예상 금액: {totalPrice:N0}원";
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            SelectedHours.Clear();

            foreach (int index in timeList.CheckedIndices)
            {
                if (!blockedHours.Contains(index))
                    SelectedHours.Add(index);
            }

            if (SelectedHours.Count == 0)
            {
                MessageBox.Show(this, "최소 1시간 이상 선택해주세요.", "시간 선택 필요",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}