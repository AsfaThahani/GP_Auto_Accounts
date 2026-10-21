using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GP_Auto_Accounts
{
    public partial class Settings: Form
    {
        public Settings()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void Settings_Load(object sender, EventArgs e)
        
            
        {

        }

        private void LoadUserData()
        {
            string connectionString = "Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT UserID, FullName, Username,  ContactNo, UserType FROM UserAccount";
                 
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                  
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No users found in the database.");
                    }

                    dgvUsers.AutoGenerateColumns = false;
                    dgvUsers.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading user data: " + ex.Message);
                }
            }
        }

        

        private void kryptonButton13_Click(object sender, EventArgs e)
        {

        }

        private void kryptonGroupBox4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void kryptonButton14_Click(object sender, EventArgs e)
        {
            signup dialog = new signup();

            dialog.ShowDialog();



            
        }

        private void dgvUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void kryptonButton15_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            Profile profileForm = new Profile();

            panel1.Controls.Add(profileForm.panel1);

            panel2.Controls.Add(profileForm.panel2);

            profileForm.panel1.Dock = DockStyle.Top;
            profileForm.panel2.Dock = DockStyle.Fill;
        }
    }
}
