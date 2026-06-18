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

        private ComboBox cboReservations;
        private DateTime reservationDate; // 예약 날짜 저장용

        // ★ [수정] 생성자에 DateTime date 파라미터 추가
        public SeatControlForm(int seatNum, List<ReservationInfo> myReservations, DateTime date)
        {
            this.reservationDate = date;
            this.Text = $"{seatNum}번 좌석 상태 제어";
            this.Size = new Size(320, 320);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblInfo = new Label { Text = "제어할 예약 건을 선택하세요:", Location = new Point(30, 20), AutoSize = true, Font = new Font("맑은 고딕", 10, FontStyle.Bold) };

            cboReservations = new ComboBox
            {
                Location = new Point(30, 50),
                Size = new Size(240, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("맑은 고딕", 10)
            };
            cboReservations.DataSource = myReservations;
            cboReservations.DisplayMember = "DisplayText";
            cboReservations.ValueMember = "Id";

            Button btnEnter = new Button { Text = "입실 처리", Location = new Point(60, 100), Size = new Size(180, 40), BackColor = Color.LightGreen, Font = new Font("맑은 고딕", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            Button btnOuting = new Button { Text = "잠시 외출", Location = new Point(60, 150), Size = new Size(180, 40), BackColor = Color.Gold, Font = new Font("맑은 고딕", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            Button btnLeave = new Button { Text = "퇴실 / 예약 취소", Location = new Point(60, 200), Size = new Size(180, 40), BackColor = Color.Tomato, ForeColor = Color.White, Font = new Font("맑은 고딕", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };

            // ★ [수정] 버튼별 독립적인 유효성 검사 로직 적용
            btnEnter.Click += (s, e) => {
                var selected = cboReservations.SelectedItem as ReservationInfo;
                if (selected == null) return;

                DateTime now = DateTime.Now;
                List<int> hours = selected.GetHoursList();

                // 1. 날짜가 다르거나, 현재 시간이 예약된 시간대 리스트에 없으면 입실 불가
                if (reservationDate.Date != now.Date || !hours.Contains(now.Hour))
                {
                    MessageBox.Show($"현재 시간이 아닙니다.\n예약하신 시간({hours[0]}시)부터 입실 가능합니다.", "입실 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SelectedReservationId = selected.Id;
                ActionStatus = "입실";
                this.DialogResult = DialogResult.OK;
            };

            btnOuting.Click += (s, e) => {
                var selected = cboReservations.SelectedItem as ReservationInfo;
                if (selected == null) return;

                // 2. 현재 상태가 '입실'이 아니면 외출 불가
                if (selected.Status != "입실")
                {
                    MessageBox.Show("외출은 '입실' 상태에서만 가능합니다.", "외출 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SelectedReservationId = selected.Id;
                ActionStatus = "외출";
                this.DialogResult = DialogResult.OK;
            };

            btnLeave.Click += (s, e) => {
                var selected = cboReservations.SelectedItem as ReservationInfo;
                if (selected == null) return;

                // 3. 퇴실(취소)는 언제든 가능
                SelectedReservationId = selected.Id;
                ActionStatus = "퇴실";
                this.DialogResult = DialogResult.OK;
            };

            this.Controls.Add(lblInfo);
            this.Controls.Add(cboReservations);
            this.Controls.Add(btnEnter);
            this.Controls.Add(btnOuting);
            this.Controls.Add(btnLeave);
        }
    }
}