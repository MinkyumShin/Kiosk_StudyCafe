using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public enum SeatStatus { Empty, InUse, Reserved, Maintenance }
    public enum SeatType { Single, Group }

    public class CafeSeat : Button
    {
        public int SeatNumber { get; set; }     // DB 연동용 고유 ID
        public SeatType Type { get; set; }
        public int DisplayNumber { get; set; }  // 화면 표시용 번호

        private SeatStatus _status = SeatStatus.Empty;
        public SeatStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                UpdateColor();
            }
        }

        public CafeSeat(int dbId, int displayNum, SeatType type)
        {
            SeatNumber = dbId;
            DisplayNumber = displayNum;
            Type = type;

            // 타입에 따른 번호 텍스트 분리 (개인석: 1~, 그룹석: G1~)
            Text = (Type == SeatType.Single) ? $"{DisplayNumber}" : $"G{DisplayNumber}";

            FlatStyle = FlatStyle.Flat;
            Font = new Font("맑은 고딕", 10, FontStyle.Bold);
            Cursor = Cursors.Hand;
            UpdateColor();
        }

        private void UpdateColor()
        {
            // 메인 테마 컬러 (네이비)
            Color navyTheme = Color.FromArgb(20, 30, 70);

            switch (Status)
            {
                case SeatStatus.Empty:
                    BackColor = Color.White;
                    ForeColor = navyTheme;
                    break;
                case SeatStatus.InUse:
                    BackColor = Color.Tomato;
                    ForeColor = Color.White;
                    break;
                case SeatStatus.Reserved:
                    BackColor = navyTheme;
                    ForeColor = Color.White;
                    break;
                case SeatStatus.Maintenance:
                    BackColor = Color.LightGray;
                    ForeColor = Color.DimGray;
                    break;
            }

            FlatAppearance.BorderColor = navyTheme;
            FlatAppearance.BorderSize = (Type == SeatType.Single) ? 1 : 2; // 그룹석은 테두리 강조
        }
    }

    public partial class Seat : Form
    {
        private ReservationManager dbManager;
        private string currentDate;
        private string currentUserId;

        // 상단 시계용 타이머
        private System.Windows.Forms.Timer clockTimer;
        private Label lblClock;

        public Seat(string date, string userId)
        {
            this.currentDate = date;
            this.currentUserId = userId;
            this.dbManager = new ReservationManager();

            this.Text = $"{currentDate} 스터디카페 좌석 배치도";
            this.Size = new Size(1100, 750);
            this.BackColor = Color.FromArgb(245, 247, 250); // 살짝 밝은 그레이화이트 톤
            this.StartPosition = FormStartPosition.CenterScreen;

            InitTopBar();
            GenerateSeats();
            GenerateFacilities();

            SyncSeatsWithDatabase();
        }

        private void InitTopBar()
        {
            Color navyTheme = Color.FromArgb(20, 30, 70);
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = navyTheme };

            Label lblTitle = new Label
            {
                Text = $"Key-Study 좌석 선택",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            lblClock = new Label
            {
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 14, FontStyle.Bold),
                Location = new Point(850, 15),
                AutoSize = true
            };

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(lblClock);
            this.Controls.Add(topPanel);

            // 실시간 시계 타이머 설정
            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) => { lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); };
            clockTimer.Start();
            lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 폼 켜질 때 초기화
        }

        private void GenerateSeats()
        {
            int dbSeatId = 1;

            // --- 1. 1인실 48석 (왼쪽 영역) ---
            int singleDisplayNum = 1;
            int singleWidth = 50, singleHeight = 40, startX = 50, startY = 100;

            for (int col = 0; col < 4; col++)
            {
                for (int row = 0; row < 12; row++)
                {
                    CafeSeat seat = new CafeSeat(dbSeatId++, singleDisplayNum++, SeatType.Single);
                    seat.Size = new Size(singleWidth, singleHeight);
                    seat.Location = new Point(startX + (col * 100), startY + (row * singleHeight));
                    seat.Click += Seat_Click;
                    this.Controls.Add(seat);
                }
            }

            // --- 2. 그룹석 확장 20석 (우측 영역) ---
            int groupDisplayNum = 1;
            int groupWidth = 80, groupHeight = 80;
            startX = 500; startY = 100;

            // 휴게실을 치우고 그룹석 열을 늘림 (2열 -> 4열)
            for (int col = 0; col < 4; col++)
            {
                for (int row = 0; row < 5; row++)
                {
                    CafeSeat seat = new CafeSeat(dbSeatId++, groupDisplayNum++, SeatType.Group);
                    seat.Size = new Size(groupWidth, groupHeight);
                    seat.Location = new Point(startX + (col * 120), startY + (row * groupHeight));
                    seat.Click += Seat_Click;
                    this.Controls.Add(seat);
                }
            }
        }

        private void GenerateFacilities()
        {
            Color navyTheme = Color.FromArgb(20, 30, 70);

            // --- 3. 고정 시설물 배치 (색상 테마 통일) ---
            Panel locker = new Panel { BackColor = navyTheme, Size = new Size(350, 60), Location = new Point(50, 620) };
            locker.Controls.Add(new Label { Text = "사물함", ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("맑은 고딕", 12, FontStyle.Bold) });
            this.Controls.Add(locker);

            Panel counter = new Panel { BackColor = navyTheme, Size = new Size(150, 60), Location = new Point(810, 620) };
            counter.Controls.Add(new Label { Text = "입구 / 카운터", ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("맑은 고딕", 12, FontStyle.Bold) });
            this.Controls.Add(counter);
        }

        private void SyncSeatsWithDatabase()
        {
            foreach (Control control in this.Controls)
            {
                if (control is CafeSeat seat)
                {
                    List<int> reservedHours = dbManager.GetReservedHours(seat.SeatNumber, currentDate);

                    if (reservedHours.Count >= 24)
                    {
                        seat.Status = SeatStatus.Maintenance; // 24시간 매진 시 회색 블록
                    }
                    else if (reservedHours.Count > 0)
                    {
                        seat.Status = SeatStatus.Reserved; // 예약 가능 시간이 남아있으면 네이비 유지
                    }
                    else
                    {
                        seat.Status = SeatStatus.Empty; // 완전 빈 좌석은 화이트
                    }
                }
            }
        }

        private void Seat_Click(object? sender, EventArgs e)
        {
            CafeSeat? clickedSeat = sender as CafeSeat;
            if (clickedSeat == null) return;

            bool hasMyRes = dbManager.IsMyReservation(currentUserId, clickedSeat.SeatNumber, currentDate);

            // ★ 1번 요구사항: 24시간 풀 예약으로 회색 블록이 되었더라도, 본인 예약이 껴있으면 막지 않음
            if (clickedSeat.Status == SeatStatus.Maintenance && !hasMyRes)
            {
                MessageBox.Show("해당 좌석은 모든 시간대의 예약이 마감되었습니다.", "선택 불가");
                return;
            }

            if (hasMyRes)
            {
                DialogResult result = MessageBox.Show(
                    "이 좌석에 대한 본인의 예약 상태를 변경하시겠습니까?\n\n['아니오' 선택 시 다른 시간대 추가 예약 가능]",
                    "상태 제어 및 예약", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    // DB에서 내 예약 '목록' 가져오기
                    var myReservations = dbManager.GetMyReservations(currentUserId, clickedSeat.SeatNumber, currentDate);

                    // 폼에 예약 목록 넘겨주기
                    using (var controlForm = new SeatControlForm(clickedSeat.DisplayNumber, myReservations, DateTime.Parse(currentDate)))
                    {
                        if (controlForm.ShowDialog() == DialogResult.OK)
                        {
                            int targetId = controlForm.SelectedReservationId; // 콤보박스에서 고른 타임의 ID

                            if (controlForm.ActionStatus == "퇴실")
                            {
                                dbManager.CancelReservationById(targetId);
                                MessageBox.Show("선택한 타임의 퇴실 처리가 완료되었습니다.", "완료");
                            }
                            else if (controlForm.ActionStatus == "입실")
                            {
                                dbManager.UpdateReservationStatusById(targetId, "입실");
                                MessageBox.Show("입실 처리되었습니다.", "완료");
                            }
                            else if (controlForm.ActionStatus == "외출")
                            {
                                dbManager.UpdateReservationStatusById(targetId, "외출");
                                MessageBox.Show("외출 상태로 변경되었습니다.", "완료");
                            }

                            // ★ 이 새로고침 메서드가 다시 DB를 읽어서 시간이 비었으면 자동으로 네이비(Reserved)나 화이트(Empty)로 복구해 줌!
                            SyncSeatsWithDatabase();
                        }
                    }
                    return;
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            // [추가 예약 플로우]
            List<int> reservedHours = dbManager.GetReservedHours(clickedSeat.SeatNumber, currentDate);
            using (var timeForm = new TimeSelectionForm(clickedSeat.SeatNumber, DateTime.Parse(currentDate), reservedHours))
            {
                if (timeForm.ShowDialog() == DialogResult.OK)
                {
                    int totalHours = timeForm.SelectedHours.Count;
                    int totalPrice = totalHours * 2000;

                    using (var paymentForm = new PaymentForm(totalHours, totalPrice))
                    {
                        if (paymentForm.ShowDialog() == DialogResult.OK && paymentForm.IsPaid)
                        {
                            bool isSuccess = dbManager.ReserveSeat(currentUserId, clickedSeat.SeatNumber, currentDate, timeForm.SelectedHours);
                            if (isSuccess) MessageBox.Show("결제 및 예약이 확정되었습니다.", "예약 완료");
                            else MessageBox.Show("결제 도중 다른 사용자가 선점했습니다. 환불 처리됩니다.", "예약 실패");

                            SyncSeatsWithDatabase();
                        }
                    }
                }
            }
        }
    }
}