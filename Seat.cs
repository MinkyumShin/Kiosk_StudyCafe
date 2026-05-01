using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    // 좌석 상태와 타입을 정의
    public enum SeatStatus { Empty, InUse, Reserved, Maintenance }
    public enum SeatType { Single, Group }

    // 기본 Button을 상속받아 커스텀 좌석 컨트롤 생성
    public class CafeSeat : Button
    {
        public int SeatNumber { get; set; }
        public SeatType Type { get; set; }

        private SeatStatus _status = SeatStatus.Empty;
        public SeatStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                UpdateColor(); // 상태가 바뀔 때마다 색상 자동 업데이트
            }
        }

        public CafeSeat(int number, SeatType type)
        {
            SeatNumber = number;
            Type = type;
            Text = number.ToString();
            FlatStyle = FlatStyle.Flat;
            Font = new Font("맑은 고딕", 10, FontStyle.Bold);
            Cursor = Cursors.Hand;
            UpdateColor();
        }

        private void UpdateColor()
        {
            // 상태에 따른 배경색
            switch (Status)
            {
                case SeatStatus.Empty: BackColor = Color.LightGreen; break;
                case SeatStatus.InUse: BackColor = Color.Tomato; break;
                case SeatStatus.Reserved: BackColor = Color.Gold; break;
                case SeatStatus.Maintenance: BackColor = Color.LightGray; break;
            }

            // 좌석 타입에 따른 테두리 색상
            FlatAppearance.BorderColor = (Type == SeatType.Single) ? Color.Red : Color.Blue;
            FlatAppearance.BorderSize = 2;
        }
    }

    public partial class Seat : Form
    {
        //백엔드 연동을 위한 변수 선언
        private ReservationManager dbManager;
        private string currentDate;
        private string currentUserId;

        //생성자에서 날짜와 유저 ID를 받아오도록 수정
        public Seat(string date, string userId)
        {
            this.currentDate = date;
            this.currentUserId = userId;
            this.dbManager = new ReservationManager();

            // 폼 기본 설정
            this.Text = $"{currentDate} 스터디카페 좌석 배치도";
            this.Size = new Size(1100, 750);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            GenerateSeats();
            GenerateFacilities();

            //화면이 열릴 때 DB를 확인해서 이미 예약된 좌석 칠하기
            SyncSeatsWithDatabase();
        }

        private void GenerateSeats()
        {
            int seatCount = 1;

            // --- 1. 1인실 50개 배치 (왼쪽 영역) ---
            int singleWidth = 50, singleHeight = 40, startX = 50, startY = 50;

            for (int col = 0; col < 4; col++)
            {
                for (int row = 0; row < 12; row++)
                {
                    CafeSeat seat = new CafeSeat(seatCount++, SeatType.Single);
                    seat.Size = new Size(singleWidth, singleHeight);
                    seat.Location = new Point(startX + (col * 100), startY + (row * singleHeight));
                    seat.Click += Seat_Click;
                    this.Controls.Add(seat);
                }
            }

            // --- 2. 그룹룸 10개 배치 (중앙-우측 영역) ---
            int groupWidth = 80, groupHeight = 80;
            startX = 500; startY = 50;

            for (int col = 0; col < 2; col++)
            {
                for (int row = 0; row < 5; row++)
                {
                    CafeSeat seat = new CafeSeat(seatCount++, SeatType.Group);
                    seat.Size = new Size(groupWidth, groupHeight);
                    seat.Location = new Point(startX + (col * 150), startY + (row * groupHeight));
                    seat.Click += Seat_Click;
                    this.Controls.Add(seat);
                }
            }
        }

        private void GenerateFacilities()
        {
            // --- 3. 고정 시설물 배치 ---
            Panel locker = new Panel { BackColor = Color.Yellow, Size = new Size(350, 80), Location = new Point(50, 600) };
            locker.Controls.Add(new Label { Text = "사물함", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("맑은 고딕", 12, FontStyle.Bold) });
            this.Controls.Add(locker);

            Panel lounge = new Panel { BackColor = Color.Blue, ForeColor = Color.White, Size = new Size(200, 100), Location = new Point(850, 50) };
            lounge.Controls.Add(new Label { Text = "휴게실(남)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("맑은 고딕", 12, FontStyle.Bold) });
            this.Controls.Add(lounge);

            Panel extraRoom = new Panel { BackColor = Color.Red, ForeColor = Color.White, Size = new Size(200, 100), Location = new Point(850, 200) };
            extraRoom.Controls.Add(new Label { Text = "휴게실(여)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("맑은 고딕", 12, FontStyle.Bold) });
            this.Controls.Add(extraRoom);

            Panel counter = new Panel { BackColor = Color.Black, ForeColor = Color.White, Size = new Size(150, 100), Location = new Point(900, 580) };
            counter.Controls.Add(new Label { Text = "입구 / 카운터", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("맑은 고딕", 12, FontStyle.Bold) });
            this.Controls.Add(counter);
        }

        //DB에서 예약된 좌석 목록을 불러와서 색상을 동기화
        private void SyncSeatsWithDatabase()
        {
            List<int> reservedSeats = dbManager.GetReservedSeats(currentDate);

            foreach (Control control in this.Controls)
            {
                if (control is CafeSeat seat)
                {
                    if (reservedSeats.Contains(seat.SeatNumber))
                        seat.Status = SeatStatus.Reserved;
                    else
                        seat.Status = SeatStatus.Empty;
                }
            }
        }

        // 좌석 클릭 시 실행되는 이벤트
        private void Seat_Click(object? sender, EventArgs e)
        {
            CafeSeat? clickedSeat = sender as CafeSeat;
            if (clickedSeat == null) return;

            // 점검 중이거나 예약된 좌석은 클릭 불가
            if (clickedSeat.Status == SeatStatus.Maintenance)
            {
                MessageBox.Show("현재 점검 중인 좌석입니다.", "선택 불가");
                return;
            }
            if (clickedSeat.Status != SeatStatus.Empty)
            {
                MessageBox.Show("이미 예약되거나 이용 중인 좌석입니다.", "선택 불가");
                return;
            }

            // 시간 선택 창 띄우기
            using (var timeForm = new TimeSelectionForm(clickedSeat.SeatNumber, DateTime.Parse(currentDate)))
            {
                if (timeForm.ShowDialog() == DialogResult.OK)
                {
                    //선택한 시간 목록(리스트)의 개수를 이용 시간(hours)으로 계산
                    int hours = timeForm.SelectedHours.Count > 0 ? timeForm.SelectedHours.Count : 1;

                    //DB에 실제 예약 정보 저장 (중복 예약 방지 트랜잭션 포함)
                    bool isSuccess = dbManager.ReserveSeat(currentUserId, clickedSeat.SeatNumber, currentDate, hours);

                    if (isSuccess)
                    {
                        string timeList = string.Join(", ", timeForm.SelectedHours);
                        MessageBox.Show($"{clickedSeat.SeatNumber}번 좌석\n선택한 시간: {timeList}시\n예약이 완료되었습니다.", "예약 확정");

                        // 화면 좌석 색상 변경
                        clickedSeat.Status = SeatStatus.Reserved;
                    }
                    else
                    {
                        MessageBox.Show("방금 전 다른 사용자가 예약한 좌석입니다. 다른 좌석을 선택해 주세요.", "예약 실패");
                        SyncSeatsWithDatabase(); // 뺏긴 좌석 화면 새로고침
                    }
                }
            }
        }
    }
}