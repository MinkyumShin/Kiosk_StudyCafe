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
                UpdateColor();
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
            switch (Status)
            {
                case SeatStatus.Empty: BackColor = Color.LightGreen; break;
                case SeatStatus.InUse: BackColor = Color.Tomato; break;
                case SeatStatus.Reserved: BackColor = Color.Gold; break;
                case SeatStatus.Maintenance: BackColor = Color.LightGray; break;
            }

            FlatAppearance.BorderColor = (Type == SeatType.Single) ? Color.Red : Color.Blue;
            FlatAppearance.BorderSize = 2;
        }
    }

    public partial class Seat : Form
    {
        public Seat()
        {
            this.Text = "카페 좌석 배치";
            this.Size = new Size(1100, 750);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            GenerateSeats();
            AddLegend(); // 범례 추가
        }

        private void GenerateSeats()
        {
            int seatCount = 1;

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
                    seat.Tag = seat; // Tag에 CafeSeat 객체 저장
                    this.Controls.Add(seat);
                }
            }

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
                    seat.Tag = seat; // Tag에 CafeSeat 객체 저장
                    this.Controls.Add(seat);
                }
            }
        }

        private void AddLegend()
        {
            Panel legendPanel = new Panel
            {
                Size = new Size(400, 50),
                Location = new Point(50, 650),
                BackColor = Color.White
            };

            legendPanel.Controls.Add(CreateLegendItem("사용 가능", Color.LightGreen, 0));
            legendPanel.Controls.Add(CreateLegendItem("예약됨", Color.Gold, 100));
            legendPanel.Controls.Add(CreateLegendItem("사용중", Color.Tomato, 200));
            legendPanel.Controls.Add(CreateLegendItem("점검중", Color.LightGray, 300));

            this.Controls.Add(legendPanel);
        }

        private Label CreateLegendItem(string text, Color color, int offsetX)
        {
            Label label = new Label
            {
                Text = text,
                AutoSize = true,
                Location = new Point(offsetX, 15),
                BackColor = color,
                ForeColor = Color.Black,
                Padding = new Padding(5),
                Font = new Font("맑은 고딕", 9, FontStyle.Bold)
            };
            return label;
        }

        private void Seat_Click(object sender, EventArgs e)
        {
            CafeSeat clickedSeat = sender as CafeSeat;
            if (clickedSeat == null) return;

            if (clickedSeat.Status == SeatStatus.Maintenance)
            {
                MessageBox.Show("점검 중인 좌석입니다.");
                return;
            }

            if (clickedSeat.Status == SeatStatus.Reserved)
            {
                MessageBox.Show("이미 예약된 좌석입니다.");
                return;
            }

            using (var timeForm = new TimeSelectionForm(clickedSeat.SeatNumber, DateTime.Now))
            {
                if (timeForm.ShowDialog() == DialogResult.OK)
                {
                    clickedSeat.Status = SeatStatus.Reserved;
                }
            }
        }
    }
}
