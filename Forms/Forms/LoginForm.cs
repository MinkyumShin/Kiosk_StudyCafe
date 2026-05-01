namespace Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();

            txtPassword.PasswordChar = '*';
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string id = txtId.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (id == "" || password == "")
            {
                MessageBox.Show("아이디와 비밀번호를 입력하세요.");
                return;
            }

            if (id == "admin" && password == "1234")
            {
                MessageBox.Show("로그인 성공!");

                MainForm mainForm = new MainForm(id);
                mainForm.Show();

                this.Hide();
            }
            else
            {
                MessageBox.Show("아이디 또는 비밀번호가 올바르지 않습니다.");
            }
        }
    }
}