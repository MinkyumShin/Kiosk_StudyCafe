using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace Kiosk_StudyCafe
{
    public class AdminForm : Form
    {
        private DataGridView dgv = null!;
        private TextBox txtKeyword = null!;
        private ComboBox cboStatus = null!;
        private Label lblSummary = null!;

        private readonly string connString = "Data Source=StudyCafe.sqlite;Version=3;";
        private readonly ReservationManager reservationManager = new ReservationManager();

        private readonly Color navyTheme = Color.FromArgb(20, 30, 70);
        private readonly Color primaryColor = Color.FromArgb(78, 128, 238);
        private readonly Color dangerColor = Color.FromArgb(255, 112, 67);
        private readonly Color warningColor = Color.FromArgb(255, 193, 7);
        private readonly Color formBackColor = Color.FromArgb(245, 247, 250);

        public AdminForm()
        {
            Text = "Key-Study 관리자 대시보드";
            ClientSize = new Size(1050, 650);
            MinimumSize = new Size(1050, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = formBackColor;
            Font = new Font("맑은 고딕", 10);

            InitializeLayout();
            LoadData();
        }

        private void InitializeLayout()
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
                Text = "관리자 대시보드",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 18, FontStyle.Bold),
                Location = new Point(25, 13),
                Size = new Size(300, 35),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblSubTitle = new Label
            {
                Text = "전체 예약 현황 조회 및 예약 상태 제어",
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Location = new Point(28, 47),
                Size = new Size(400, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Button btnClose = new Button
            {
                Text = "닫기",
                Location = new Point(955, 22),
                Size = new Size(70, 34),
                BackColor = dangerColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubTitle);
            headerPanel.Controls.Add(btnClose);
            Controls.Add(headerPanel);

            // ----- 필터 영역 -----
            Panel filterPanel = new Panel
            {
                Location = new Point(20, 95),
                Size = new Size(1010, 60),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblKeyword = new Label
            {
                Text = "검색:",
                Location = new Point(20, 18),
                Size = new Size(50, 25),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtKeyword = new TextBox
            {
                Location = new Point(75, 16),
                Size = new Size(220, 28),
                PlaceholderText = "아이디 / 좌석 / 날짜 검색"
            };

            Label lblStatus = new Label
            {
                Text = "상태:",
                Location = new Point(325, 18),
                Size = new Size(50, 25),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            cboStatus = new ComboBox
            {
                Location = new Point(380, 16),
                Size = new Size(130, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatus.Items.AddRange(new object[]
            {
                "전체", "예약됨", "입실", "외출", "퇴실", "노쇼"
            });
            cboStatus.SelectedIndex = 0;

            Button btnSearch = new Button
            {
                Text = "조회",
                Location = new Point(535, 14),
                Size = new Size(80, 32),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => LoadData();

            Button btnRefresh = new Button
            {
                Text = "새로고침",
                Location = new Point(625, 14),
                Size = new Size(90, 32),
                BackColor = navyTheme,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadData();

            lblSummary = new Label
            {
                Text = "조회된 예약: 0건",
                Location = new Point(760, 17),
                Size = new Size(220, 28),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                ForeColor = navyTheme,
                TextAlign = ContentAlignment.MiddleRight
            };

            filterPanel.Controls.Add(lblKeyword);
            filterPanel.Controls.Add(txtKeyword);
            filterPanel.Controls.Add(lblStatus);
            filterPanel.Controls.Add(cboStatus);
            filterPanel.Controls.Add(btnSearch);
            filterPanel.Controls.Add(btnRefresh);
            filterPanel.Controls.Add(lblSummary);
            Controls.Add(filterPanel);

            // ----- 예약 현황 Grid -----
            dgv = new DataGridView
            {
                Location = new Point(20, 175),
                Size = new Size(1010, 380),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            ApplyGridStyle();
            Controls.Add(dgv);

            // ----- 관리자 제어 버튼 -----
            Panel actionPanel = new Panel
            {
                Location = new Point(20, 575),
                Size = new Size(1010, 55),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblActionInfo = new Label
            {
                Text = "선택한 예약 상태 제어:",
                Location = new Point(20, 15),
                Size = new Size(170, 25),
                Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Button btnCheckIn = new Button
            {
                Text = "입실 처리",
                Location = new Point(210, 10),
                Size = new Size(110, 34),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCheckIn.FlatAppearance.BorderSize = 0;
            btnCheckIn.Click += (s, e) => UpdateSelectedReservationStatus("입실");

            Button btnOuting = new Button
            {
                Text = "외출 처리",
                Location = new Point(330, 10),
                Size = new Size(110, 34),
                BackColor = warningColor,
                ForeColor = Color.FromArgb(40, 40, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOuting.FlatAppearance.BorderSize = 0;
            btnOuting.Click += (s, e) => UpdateSelectedReservationStatus("외출");

            Button btnForceCheckout = new Button
            {
                Text = "강제 퇴실/취소",
                Location = new Point(450, 10),
                Size = new Size(130, 34),
                BackColor = dangerColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnForceCheckout.FlatAppearance.BorderSize = 0;
            btnForceCheckout.Click += BtnForceCheckout_Click;

            Label lblHelp = new Label
            {
                Text = "※ 예약 행을 먼저 선택한 뒤 버튼을 눌러주세요.",
                Location = new Point(610, 15),
                Size = new Size(360, 25),
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleRight
            };

            actionPanel.Controls.Add(lblActionInfo);
            actionPanel.Controls.Add(btnCheckIn);
            actionPanel.Controls.Add(btnOuting);
            actionPanel.Controls.Add(btnForceCheckout);
            actionPanel.Controls.Add(lblHelp);
            Controls.Add(actionPanel);

            AcceptButton = btnSearch;
        }

        private void ApplyGridStyle()
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = navyTheme;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("맑은 고딕", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 36;

            dgv.DefaultCellStyle.Font = new Font("맑은 고딕", 9);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252);
            dgv.RowTemplate.Height = 32;
        }

        private void LoadData()
        {
            try
            {
                string keyword = txtKeyword?.Text.Trim() ?? "";
                string status = cboStatus?.SelectedItem?.ToString() ?? "전체";

                using (var conn = new SQLiteConnection(connString))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            Id,
                            UserId AS '아이디',
                            SeatNumber AS '좌석',
                            ReservationDate AS '날짜',
                            DurationHours AS '이용시간',
                            ReservedHours AS '시간대',
                            Status AS '상태'
                        FROM Reservations
                        WHERE
                            (@Status = '전체' OR Status = @Status)
                            AND
                            (
                                @Keyword = ''
                                OR UserId LIKE @LikeKeyword
                                OR CAST(SeatNumber AS TEXT) LIKE @LikeKeyword
                                OR ReservationDate LIKE @LikeKeyword
                                OR ReservedHours LIKE @LikeKeyword
                            )
                        ORDER BY ReservationDate DESC, SeatNumber ASC, Id DESC;
                    ";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@Keyword", keyword);
                        cmd.Parameters.AddWithValue("@LikeKeyword", $"%{keyword}%");

                        using (var adapter = new SQLiteDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            dgv.DataSource = dt;
                            lblSummary.Text = $"조회된 예약: {dt.Rows.Count}건";

                            if (dgv.Columns.Contains("Id"))
                                dgv.Columns["Id"].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "예약 현황을 불러오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "DB 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private int GetSelectedReservationId()
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.IsNewRow)
            {
                MessageBox.Show(this,
                    "예약 행을 먼저 선택해주세요.",
                    "선택 필요",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return -1;
            }

            if (!dgv.Columns.Contains("Id"))
            {
                MessageBox.Show(this,
                    "예약 ID 컬럼을 찾을 수 없습니다.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return -1;
            }

            object? value = dgv.CurrentRow.Cells["Id"].Value;

            if (value == null || !int.TryParse(value.ToString(), out int id))
            {
                MessageBox.Show(this,
                    "선택한 예약 ID가 올바르지 않습니다.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return -1;
            }

            return id;
        }

        private void UpdateSelectedReservationStatus(string status)
        {
            int id = GetSelectedReservationId();
            if (id == -1)
                return;

            DialogResult result = MessageBox.Show(this,
                $"선택한 예약을 '{status}' 상태로 변경하시겠습니까?",
                "상태 변경 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            reservationManager.UpdateReservationStatusById(id, status);

            MessageBox.Show(this,
                $"예약 상태가 '{status}'로 변경되었습니다.",
                "처리 완료",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadData();
        }

        private void BtnForceCheckout_Click(object? sender, EventArgs e)
        {
            int id = GetSelectedReservationId();
            if (id == -1)
                return;

            DialogResult result = MessageBox.Show(this,
                "선택한 예약을 강제 퇴실 또는 취소 처리하시겠습니까?",
                "강제 처리 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            reservationManager.CancelReservationById(id);

            MessageBox.Show(this,
                "선택한 예약이 강제 퇴실/취소 처리되었습니다.",
                "처리 완료",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadData();
        }
    }
}