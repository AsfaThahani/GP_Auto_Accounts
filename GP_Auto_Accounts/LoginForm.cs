using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GP_Auto_Accounts
{
    public partial class LoginForm : SmoothRoundedForm

    {
        public LoginForm()
        {
            InitializeComponent();
            txtHashPassword.UseSystemPasswordChar = true;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
          
        }

        string connectionString = "Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;";

        private byte[] HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }


        private void kryptonButton22_Click(object sender, EventArgs e)
        {
            string userInput = txtUsernameOrEmail.Text.Trim();
            byte[] hashedPassword = HashPassword(txtHashPassword.Text.Trim());

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM UserAccount 
                     WHERE (Username = @Input OR Email = @Input) 
                     AND Password = @Password";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Input", userInput);
                cmd.Parameters.Add("@Password", SqlDbType.VarBinary, 64).Value = hashedPassword;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    AppContext.CurrentUser = new UserInfo
                    {
                        UserID = Convert.ToInt32(reader["UserID"]),
                        FullName = reader["FullName"].ToString(),
                        UserType = reader["UserType"].ToString()
                    };

                    this.Hide();
                    Form1 mainForm = new Form1();
                    mainForm.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid email/username or password.", "Login Failed");
                }

                conn.Close();
            }
        }

        private bool passwordVisible = false;

        private void btnPasswordToggle_Click(object sender, EventArgs e)
        {
           
            if (passwordVisible)
            {
                txtHashPassword.UseSystemPasswordChar = true;  
                passwordVisible = false;

              
            }
            else
            {
                txtHashPassword.UseSystemPasswordChar = false; 
                passwordVisible = true;

           
            }
        }

      
        private void btnPasswordToggle_MouseDown(object sender, MouseEventArgs e)
        {
           
            txtHashPassword.UseSystemPasswordChar = false; 
            passwordVisible = true;

        }

      
        private void btnPasswordToggle_MouseUp(object sender, MouseEventArgs e)
        {
            
            if (!passwordVisible)
            {
                txtHashPassword.UseSystemPasswordChar = true;  
            }

          
        }

        private void kryptonButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

