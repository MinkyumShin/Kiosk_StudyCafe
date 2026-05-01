using System;
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
        public Seat() 
        {
            // 폼 기본 설정
            this.Text = "스터디카페 좌석 배치도";
            this.Size = new Size(1100, 750);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            GenerateSeats();
            GenerateFacilities();
        }

        private void GenerateSeats()
        {
            int seatCount = 1;

            // --- 1. 1인실 50개 배치 (왼쪽 영역) ---
            int singleWidth = 50;
            int singleHeight = 40;
            int startX = 50;
            int startY = 50;

            for (int col = 0; col < 4; col++)
            {
                int rows = 12;
                for (int row = 0; row < rows; row++)
                {
                    CafeSeat seat = new CafeSeat(seatCount++, SeatType.Single);
                    seat.Size = new Size(singleWidth, singleHeight);
                    seat.Location = new Point(startX + (col * 100), startY + (row * singleHeight));
                    seat.Click += Seat_Click;
                    this.Controls.Add(seat);
                }
            }

            // --- 2. 그룹룸 10개 배치 (중앙-우측 영역) ---
            int groupWidth = 80;
            int groupHeight = 80;
            startX = 500;
            startY = 50;

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

        // 좌석 클릭 시 실행되는 테스트 이벤트
        private void Seat_Click(object sender, EventArgs e)
        {
            CafeSeat clickedSeat = sender as CafeSeat;
            if (clickedSeat == null) return;

            // 점검 중인 좌석은 클릭 불가
            if (clickedSeat.Status == SeatStatus.Maintenance)
            {
                MessageBox.Show("현재 점검 중인 좌석입니다.");
                return;
            }

            // 시간 선택 창 띄우기 (현재 날짜 기준)
            using (var timeForm = new TimeSelectionForm(clickedSeat.SeatNumber, DateTime.Now))
            {
                if (timeForm.ShowDialog() == DialogResult.OK)
                {
                    string hours = string.Join(", ", timeForm.SelectedHours);
                    MessageBox.Show($"{clickedSeat.SeatNumber}번 좌석\n선택한 시간: {hours}시\n예약이 완료되었습니다.");

                    // 예약이 완료되면 좌석 상태를 '이용 중' 혹은 '예약됨'으로 변경
                    clickedSeat.Status = SeatStatus.InUse;
                }
            }
        }
    }
}