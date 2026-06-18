using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private bool _isMine = false;

        private readonly Color navyTheme = Color.FromArgb(20, 30, 70);
        private readonly Color reservedColor = Color.FromArgb(255, 213, 79);
        private readonly Color myReservedColor = Color.FromArgb(76, 175, 80);
        private readonly Color inUseColor = Color.FromArgb(239, 83, 80);
        private readonly Color maintenanceColor = Color.FromArgb(210, 210, 210);

        public SeatStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                UpdateColor();
            }
        }

        public bool IsMine
        {
            get { return _isMine; }
            set
            {
                _isMine = value;
                UpdateColor();
            }
        }

        public CafeSeat(int dbId, int displayNum, SeatType type)
        {
            SeatNumber = dbId;
            DisplayNumber = displayNum;
            Type = type;

            FlatStyle = FlatStyle.Flat;
            Font = new Font("맑은 고딕", 10, FontStyle.Bold);
            Cursor = Cursors.Hand;
            TextAlign = ContentAlignment.MiddleCenter;
            UseVisualStyleBackColor = false;

            UpdateColor();
        }

        private string GetBaseText()
        {
            return Type == SeatType.Single ? $"{DisplayNumber}" : $"G{DisplayNumber}";
        }

        private void UpdateColor()
        {
            Text = IsMine ? $"★\n{GetBaseText()}" : GetBaseText();

            switch (Status)
            {
                case SeatStatus.Empty:
                    BackColor = Color.White;
                    ForeColor = navyTheme;
                    Enabled = true;
                    break;

                case SeatStatus.InUse:
                    BackColor = inUseColor;
                    ForeColor = Color.White;
                    Enabled = true;
                    break;

                case SeatStatus.Reserved:
                    if (IsMine)
                    {
                        BackColor = myReservedColor;
                        ForeColor = Color.White;
                    }
                    else
                    {
                        BackColor = reservedColor;
                        ForeColor = Color.FromArgb(40, 40, 40);
                    }
                    Enabled = true;
                    break;

                case SeatStatus.Maintenance:
                    // 본인 예약이 포함된 좌석은 마감 상태여도 상태 제어를 위해 초록색 유지
                    if (IsMine)
                    {
                        BackColor = myReservedColor;
                        ForeColor = Color.White;
                        Enabled = true;
                    }
                    else
                    {
                        BackColor = maintenanceColor;
                        ForeColor = Color.DimGray;
                        Enabled = true;
                    }
                    break;
            }

            FlatAppearance.BorderColor = IsMine ? myReservedColor : navyTheme;
            FlatAppearance.BorderSize = IsMine ? 3 : (Type == SeatType.Single ? 1 : 2);
        }
    }

    public partial class Seat : Form
    {
        private readonly ReservationManager dbManager;
        private readonly string currentDate;
        private readonly string currentUserId;

        private System.Windows.Forms.Timer clockTimer = null!;
        private Label lblClock = null!;
        private Label lblGuide = null!;

        private readonly Color navyTheme = Color.FromArgb(20, 30, 70);
        private readonly Color primaryColor = Color.FromArgb(78, 128, 238);
        private readonly Color formBackColor = Color.FromArgb(245, 247, 250);

        public Seat(string date, string userId)
        {
            currentDate = date;
            currentUserId = userId;
            dbManager = new ReservationManager();

            Text = $"{currentDate} Key-Study 좌석 선택";
            ClientSize = new Size(1100, 750);
            MinimumSize = new Size(1100, 750);
            BackColor = formBackColor;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("맑은 고딕", 10);

            InitTopBar();
            InitGuideArea();
            GenerateSectionTitles();
            GenerateSeats();
            GenerateFacilities();
            GenerateLegend();

            SyncSeatsWithDatabase();
        }

        private void InitTopBar()
        {
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = navyTheme
            };

            Label lblTitle = new Label
            {
                Text = "Key-Study 좌석 선택",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 17, FontStyle.Bold),
                Location = new Point(25, 17),
                AutoSize = true
            };

            Label lblUserInfo = new Label
            {
                Text = $"사용자: {currentUserId}   |   예약일: {currentDate}",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                Location = new Point(350, 23),
                AutoSize = true
            };

            lblClock = new Label
            {
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 11, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(190, 30),
                Location = new Point(705, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            Button btnRefresh = new Button
            {
                Text = "새로고침",
                Location = new Point(915, 17),
                Size = new Size(80, 32),
                BackColor = Color.White,
                ForeColor = navyTheme,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) =>
            {
                SyncSeatsWithDatabase();
                MessageBox.Show(this, "좌석 상태를 새로고침했습니다.", "새로고침",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Button btnClose = new Button
            {
                Text = "닫기",
                Location = new Point(1005, 17),
                Size = new Size(65, 32),
                BackColor = Color.FromArgb(255, 112, 67),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(lblUserInfo);
            topPanel.Controls.Add(lblClock);
            topPanel.Controls.Add(btnRefresh);
            topPanel.Controls.Add(btnClose);
            Controls.Add(topPanel);

            clockTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            clockTimer.Tick += (s, e) =>
            {
                lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            clockTimer.Start();

            lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void InitGuideArea()
        {
            lblGuide = new Label
            {
                Text = "좌석을 클릭하면 예약 가능한 시간대를 선택할 수 있습니다. ★ 표시 좌석은 본인의 예약이 포함된 좌석입니다.",
                Location = new Point(50, 78),
                Size = new Size(1000, 28),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 70, 70),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblGuide);
        }

        private void GenerateSectionTitles()
        {
            Label lblSingle = new Label
            {
                Text = "1인석",
                Location = new Point(50, 112),
                Size = new Size(350, 30),
                Font = new Font("맑은 고딕", 13, FontStyle.Bold),
                ForeColor = navyTheme,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblGroup = new Label
            {
                Text = "그룹석",
                Location = new Point(500, 112),
                Size = new Size(450, 30),
                Font = new Font("맑은 고딕", 13, FontStyle.Bold),
                ForeColor = navyTheme,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Controls.Add(lblSingle);
            Controls.Add(lblGroup);
        }

        private void GenerateSeats()
        {
            int dbSeatId = 1;

            // --- 1. 1인석 48석 ---
            int singleDisplayNum = 1;
            int singleWidth = 52;
            int singleHeight = 38;
            int startX = 50;
            int startY = 150;

            for (int col = 0; col < 4; col++)
            {
                for (int row = 0; row < 12; row++)
                {
                    CafeSeat seat = new CafeSeat(dbSeatId++, singleDisplayNum++, SeatType.Single)
                    {
                        Size = new Size(singleWidth, singleHeight),
                        Location = new Point(startX + (col * 95), startY + (row * singleHeight))
                    };

                    seat.Click += Seat_Click;
                    Controls.Add(seat);
                }
            }

            // --- 2. 그룹석 20석 ---
            int groupDisplayNum = 1;
            int groupWidth = 82;
            int groupHeight = 74;
            startX = 500;
            startY = 150;

            for (int col = 0; col < 4; col++)
            {
                for (int row = 0; row < 5; row++)
                {
                    CafeSeat seat = new CafeSeat(dbSeatId++, groupDisplayNum++, SeatType.Group)
                    {
                        Size = new Size(groupWidth, groupHeight),
                        Location = new Point(startX + (col * 115), startY + (row * groupHeight))
                    };

                    seat.Click += Seat_Click;
                    Controls.Add(seat);
                }
            }
        }

        private void GenerateFacilities()
        {
            Panel locker = CreateFacilityPanel("사물함", new Point(50, 630), new Size(350, 60));
            Panel counter = CreateFacilityPanel("입구 / 카운터", new Point(810, 630), new Size(150, 60));

            Controls.Add(locker);
            Controls.Add(counter);
        }

        private Panel CreateFacilityPanel(string text, Point location, Size size)
        {
            Panel panel = new Panel
            {
                BackColor = navyTheme,
                Size = size,
                Location = location
            };

            panel.Controls.Add(new Label
            {
                Text = text,
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("맑은 고딕", 12, FontStyle.Bold)
            });

            return panel;
        }

        private void GenerateLegend()
        {
            Panel legendPanel = new Panel
            {
                Location = new Point(500, 555),
                Size = new Size(460, 60),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            AddLegendItem(legendPanel, "빈 좌석", Color.White, Color.FromArgb(20, 30, 70), 15);
            AddLegendItem(legendPanel, "예약됨", Color.FromArgb(255, 213, 79), Color.FromArgb(40, 40, 40), 115);
            AddLegendItem(legendPanel, "내 예약", Color.FromArgb(76, 175, 80), Color.White, 215);
            AddLegendItem(legendPanel, "마감/점검", Color.FromArgb(210, 210, 210), Color.DimGray, 315);

            Controls.Add(legendPanel);
        }

        private void AddLegendItem(Panel parent, string text, Color backColor, Color foreColor, int x)
        {
            Panel colorBox = new Panel
            {
                Location = new Point(x, 18),
                Size = new Size(22, 22),
                BackColor = backColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label label = new Label
            {
                Text = text,
                Location = new Point(x + 28, 16),
                Size = new Size(75, 26),
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                ForeColor = foreColor == Color.White ? Color.FromArgb(50, 50, 50) : foreColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            parent.Controls.Add(colorBox);
            parent.Controls.Add(label);
        }

        private void SyncSeatsWithDatabase()
        {
            foreach (Control control in Controls)
            {
                if (control is CafeSeat seat)
                {
                    // DB에 실제 예약된 시간
                    List<int> reservedHours = dbManager.GetReservedHours(seat.SeatNumber, currentDate);

                    // 오늘 날짜 기준 이미 지나서 선택할 수 없는 시간
                    List<int> closedHours = GetClosedHoursByDate(currentDate);

                    // 예약된 시간 + 닫힌 시간 합산
                    // 이 값은 "하루 전체가 막혔는지" 판단할 때만 사용
                    List<int> blockedHours = reservedHours
                        .Concat(closedHours)
                        .Distinct()
                        .OrderBy(h => h)
                        .ToList();

                    bool hasMyRes = dbManager.IsMyReservation(currentUserId, seat.SeatNumber, currentDate);

                    seat.IsMine = hasMyRes;

                    // 1. 예약 + 지난 시간까지 합쳐서 24시간 전부 막혔으면 마감 처리
                    if (blockedHours.Count >= 24)
                    {
                        seat.Status = SeatStatus.Maintenance;
                    }
                    // 2. 실제 예약된 시간이 있을 때만 예약됨 노란색 처리
                    else if (reservedHours.Count > 0)
                    {
                        seat.Status = SeatStatus.Reserved;
                    }
                    // 3. 지난 시간만 있고 실제 예약이 없으면 빈 좌석 유지
                    else
                    {
                        seat.Status = SeatStatus.Empty;
                    }
                }
            }
        }

        private List<int> GetClosedHoursByDate(string dateText)
        {
            List<int> closedHours = new List<int>();

            if (!DateTime.TryParse(dateText, out DateTime selectedDate))
                return closedHours;

            DateTime now = DateTime.Now;

            // 과거 날짜는 하루 전체 예약 불가
            if (selectedDate.Date < now.Date)
            {
                for (int i = 0; i < 24; i++)
                {
                    closedHours.Add(i);
                }

                return closedHours;
            }

            // 오늘 날짜는 현재 시간 이하를 예약 불가로 처리
            // TimeSelectionForm과 동일하게 현재 시간대도 막음
            if (selectedDate.Date == now.Date)
            {
                for (int i = 0; i <= now.Hour && i < 24; i++)
                {
                    closedHours.Add(i);
                }
            }

            // 미래 날짜는 닫힌 시간 없음
            return closedHours;
        }

        private void Seat_Click(object? sender, EventArgs e)
        {
            CafeSeat? clickedSeat = sender as CafeSeat;
            if (clickedSeat == null)
                return;

            bool hasMyRes = dbManager.IsMyReservation(currentUserId, clickedSeat.SeatNumber, currentDate);

            if (clickedSeat.Status == SeatStatus.Maintenance && !hasMyRes)
            {
                MessageBox.Show(this,
                    "해당 좌석은 모든 시간대의 예약이 마감되었습니다.",
                    "선택 불가",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (hasMyRes)
            {
                DialogResult result = MessageBox.Show(this,
                    "이 좌석에는 본인의 예약이 있습니다.\n\n" +
                    "[예] 예약 상태 변경 / 입실 / 외출 / 퇴실\n" +
                    "[아니오] 다른 시간대 추가 예약\n" +
                    "[취소] 아무 작업 안 함",
                    "내 예약 좌석",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var myReservations = dbManager.GetMyReservations(currentUserId, clickedSeat.SeatNumber, currentDate);

                    using (var controlForm = new SeatControlForm(clickedSeat.DisplayNumber, myReservations, DateTime.Parse(currentDate)))
                    {
                        if (controlForm.ShowDialog(this) == DialogResult.OK)
                        {
                            int targetId = controlForm.SelectedReservationId;

                            if (controlForm.ActionStatus == "퇴실")
                            {
                                dbManager.CancelReservationById(targetId);
                                MessageBox.Show(this, "선택한 예약의 퇴실 처리가 완료되었습니다.", "완료",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (controlForm.ActionStatus == "입실")
                            {
                                dbManager.UpdateReservationStatusById(targetId, "입실");
                                MessageBox.Show(this, "입실 처리되었습니다.", "완료",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (controlForm.ActionStatus == "외출")
                            {
                                dbManager.UpdateReservationStatusById(targetId, "외출");
                                MessageBox.Show(this, "외출 상태로 변경되었습니다.", "완료",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

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

            // 추가 예약 플로우
            List<int> reservedHours = dbManager.GetReservedHours(clickedSeat.SeatNumber, currentDate);

            using (var timeForm = new TimeSelectionForm(clickedSeat.SeatNumber, DateTime.Parse(currentDate), reservedHours))
            {
                if (timeForm.ShowDialog(this) == DialogResult.OK)
                {
                    int totalHours = timeForm.SelectedHours.Count;
                    int totalPrice = totalHours * 2000;

                    using (var paymentForm = new PaymentForm(totalHours, totalPrice))
                    {
                        if (paymentForm.ShowDialog(this) == DialogResult.OK && paymentForm.IsPaid)
                        {
                            bool isSuccess = dbManager.ReserveSeat(
                                currentUserId,
                                clickedSeat.SeatNumber,
                                currentDate,
                                timeForm.SelectedHours
                            );

                            if (isSuccess)
                            {
                                MessageBox.Show(this,
                                    "결제 및 예약이 확정되었습니다.",
                                    "예약 완료",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show(this,
                                    "결제 도중 다른 사용자가 해당 시간대를 선점했거나 포인트가 부족합니다.\n결제는 취소됩니다.",
                                    "예약 실패",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }

                            SyncSeatsWithDatabase();
                        }
                    }
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (clockTimer != null)
            {
                clockTimer.Stop();
                clockTimer.Dispose();
            }

            base.OnFormClosed(e);
        }
    }
}