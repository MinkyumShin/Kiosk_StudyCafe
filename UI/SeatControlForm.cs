using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public class SeatControlForm : Form
    {
        public string ActionStatus { get; private set; } = "";
        public int SelectedReservationId { get; private set; } = -1;

        private ComboBox cboReservations = null!;
        private Label lblSelectedInfo = null!;

        private readonly DateTime reservationDate;

        private readonly Color navyTheme = Color.FromArgb(20, 30, 70);
        private readonly Color primaryColor = Color.FromArgb(78, 128, 238);
        private readonly Color dangerColor = Color.FromArgb(255, 112, 67);
        private readonly Color warningColor = Color.FromArgb(255, 193, 7);
        private readonly Color formBackColor = Color.FromArgb(245, 247, 250);

        public SeatControlForm(int seatNum, List<ReservationInfo> myReservations, DateTime date)
        {
            reservationDate = date;

            Text = $"{seatNum}번 좌석 상태 제어";
            ClientSize = new Size(420, 430);
            MinimumSize = new Size(420, 430);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = formBackColor;
            Font = new Font("맑은 고딕", 10);

            InitializeLayout(seatNum, myReservations);
        }

        private void InitializeLayout(int seatNum, List<ReservationInfo> myReservations)
        {
            // ----- 상단 헤더 -----
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = navyTheme
            };

            Label lblTitle = new Label
            {
                Text = "내 예약 상태 제어",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 16, FontStyle.Bold),
                Location = new Point(0, 12),
                Size = new Size(420, 32),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubTitle = new Label
            {
                Text = $"{seatNum}번 좌석  |  {reservationDate:yyyy-MM-dd}",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Location = new Point(0, 45),
                Size = new Size(420, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubTitle);
            Controls.Add(headerPanel);

            Label lblInfo = new Label
            {
                Text = "제어할 예약 시간을 선택하세요.",
                Location = new Point(30, 95),
                Size = new Size(360, 26),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleLeft
            };

            cboReservations = new ComboBox
            {
                Location = new Point(30, 128),
                Size = new Size(360, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("맑은 고딕", 10)
            };

            cboReservations.DataSource = myReservations;
            cboReservations.DisplayMember = "DisplayText";
            cboReservations.ValueMember = "Id";
            cboReservations.SelectedIndexChanged += (s, e) => UpdateSelectedInfo();

            lblSelectedInfo = new Label
            {
                Text = "",
                Location = new Point(30, 170),
                Size = new Size(360, 58),
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                ForeColor = navyTheme,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblGuide = new Label
            {
                Text = "※ 입실은 예약 날짜와 현재 시간이 예약 시간대에 포함될 때 가능합니다.\n※ 외출은 입실 상태에서만 가능합니다.",
                Location = new Point(30, 240),
                Size = new Size(360, 45),
                Font = new Font("맑은 고딕", 8, FontStyle.Bold),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Button btnEnter = new Button
            {
                Text = "입실 / 복귀",
                Location = new Point(30, 305),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEnter.FlatAppearance.BorderSize = 0;
            btnEnter.Click += BtnEnter_Click;

            Button btnOuting = new Button
            {
                Text = "잠시 외출",
                Location = new Point(155, 305),
                Size = new Size(110, 42),
                BackColor = warningColor,
                ForeColor = Color.FromArgb(40, 40, 40),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOuting.FlatAppearance.BorderSize = 0;
            btnOuting.Click += BtnOuting_Click;

            Button btnLeave = new Button
            {
                Text = "퇴실 / 취소",
                Location = new Point(280, 305),
                Size = new Size(110, 42),
                BackColor = dangerColor,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLeave.FlatAppearance.BorderSize = 0;
            btnLeave.Click += BtnLeave_Click;

            Button btnCancel = new Button
            {
                Text = "닫기",
                Location = new Point(145, 365),
                Size = new Size(130, 36),
                BackColor = Color.FromArgb(235, 238, 242),
                ForeColor = Color.FromArgb(40, 40, 40),
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(lblInfo);
            Controls.Add(cboReservations);
            Controls.Add(lblSelectedInfo);
            Controls.Add(lblGuide);
            Controls.Add(btnEnter);
            Controls.Add(btnOuting);
            Controls.Add(btnLeave);
            Controls.Add(btnCancel);

            UpdateSelectedInfo();
        }

        private ReservationInfo? GetSelectedReservation()
        {
            return cboReservations.SelectedItem as ReservationInfo;
        }

        private void UpdateSelectedInfo()
        {
            var selected = GetSelectedReservation();

            if (selected == null)
            {
                lblSelectedInfo.Text = "선택된 예약이 없습니다.";
                return;
            }

            List<int> hours = selected.GetHoursList();
            string timeText = GetHoursText(hours);

            lblSelectedInfo.Text =
                $"선택 예약 상태: {selected.Status}\n" +
                $"예약 시간대: {timeText}";
        }

        private string GetHoursText(List<int> hours)
        {
            if (hours == null || hours.Count == 0)
                return "시간 정보 없음";

            hours.Sort();

            if (hours.Count == 1)
                return $"{hours[0]:D2}:00 ~ {hours[0] + 1:D2}:00";

            return $"{hours[0]:D2}:00 ~ {hours[hours.Count - 1] + 1:D2}:00";
        }

        private void BtnEnter_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedReservation();
            if (selected == null)
                return;

            if (selected.Status == "입실")
            {
                MessageBox.Show(this,
                    "이미 입실 상태입니다.",
                    "입실 불가",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DateTime now = DateTime.Now;
            List<int> hours = selected.GetHoursList();

            if (reservationDate.Date != now.Date || !hours.Contains(now.Hour))
            {
                MessageBox.Show(this,
                    $"현재 입실 가능한 시간이 아닙니다.\n예약 시간: {GetHoursText(hours)}",
                    "입실 불가",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ConfirmAndClose(selected.Id, "입실", "선택한 예약을 입실 상태로 변경하시겠습니까?");
        }

        private void BtnOuting_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedReservation();
            if (selected == null)
                return;

            if (selected.Status != "입실")
            {
                MessageBox.Show(this,
                    "외출은 '입실' 상태에서만 가능합니다.",
                    "외출 불가",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ConfirmAndClose(selected.Id, "외출", "선택한 예약을 외출 상태로 변경하시겠습니까?");
        }

        private void BtnLeave_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedReservation();
            if (selected == null)
                return;

            ConfirmAndClose(selected.Id, "퇴실", "선택한 예약을 퇴실/예약 취소 처리하시겠습니까?");
        }

        private void ConfirmAndClose(int reservationId, string actionStatus, string message)
        {
            DialogResult result = MessageBox.Show(this,
                message,
                "상태 변경 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            SelectedReservationId = reservationId;
            ActionStatus = actionStatus;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}