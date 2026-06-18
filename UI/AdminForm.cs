using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public class AdminForm : Form
    {
        private DataGridView dgv;
        private string connString = "Data Source=StudyCafe.sqlite;Version=3;";

        public AdminForm()
        {
            this.Text = "관리자 대시보드 - 전체 예약 현황";

            // 폼 크기 대폭 확장
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            dgv = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(940, 480), // 그리드뷰 크기도 확장
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            // 버튼 위치 조정
            Button btnRefresh = new Button { Text = "새로고침", Location = new Point(860, 510), Size = new Size(100, 40) };
            btnRefresh.Click += (s, e) => LoadData();

            this.Controls.Add(dgv);
            this.Controls.Add(btnRefresh);

            LoadData();
        }

        private void LoadData()
        {
            using (var conn = new SQLiteConnection(connString))
            {
                conn.Open();
                // 'Status as 상태' 컬럼 추가
                string query = "SELECT Id, UserId as '아이디', SeatNumber as '좌석', ReservationDate as '날짜', DurationHours as '이용시간', ReservedHours as '시간대', Status as '상태' FROM Reservations";
                using (var adapter = new SQLiteDataAdapter(query, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgv.DataSource = dt;
                }
            }
        }
    }
}