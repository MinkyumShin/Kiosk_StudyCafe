namespace Forms
{
    public partial class MainForm : Form
    {
        private string loginId;

        public MainForm(string id)
        {
            InitializeComponent();
            loginId = id;

            lblWelcome.Text = loginId + "님 환영합니다.";
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // 로그아웃 메시지
            MessageBox.Show("로그아웃 되었습니다.");

            // 로그인 폼 다시 띄우기
            LoginForm loginForm = new LoginForm();
            loginForm.Show();

            // 현재 메인폼 닫기
            this.Close();
        }
    }
}