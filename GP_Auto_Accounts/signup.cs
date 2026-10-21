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
    public partial class signup: SmoothRoundedForm
    {
        public signup()
        {
            InitializeComponent();
            LoadUserTypes();
        }
        private byte[] HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private void LoadUserTypes()
        {
            cmbUserType.Items.Clear();
            cmbUserType.Items.AddRange(new string[] { "Admin", "Staff", "Manager", "Viewer" });
            cmbUserType.SelectedIndex = -1; // optional: select the first type by default
        }
        private void signup_Load(object sender, EventArgs e)
        {
            LoadUserTypes();
        }

        private void kryptonButton13_Click(object sender, EventArgs e)
        {
            
        }

        private void kryptonButton12_Click(object sender, EventArgs e)
        {

        }

        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void kryptonButton11_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void kryptonButton22_Click(object sender, EventArgs e)
        {

            // Step 1: Check for empty required fields
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtContact.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                string.IsNullOrWhiteSpace(txtConfirmPassword.Text) ||
                cmbUserType.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Step 2: Check if passwords match
            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Step 3: Optionally check for duplicate usernames
            string connectionString = "Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Check if username already exists
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM UserAccount WHERE Username = @Username", conn);
                checkCmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());

                int existingCount = (int)checkCmd.ExecuteScalar();
                if (existingCount > 0)
                {
                    MessageBox.Show("Username already exists. Please choose another.", "Duplicate Username", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Insert new user
                string insertQuery = @"INSERT INTO UserAccount (FullName, Username, Email, ContactNo, Password, UserType)
                               VALUES (@FullName, @Username, @Email, @Contact, @Password, @UserType)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Contact", txtContact.Text.Trim());
                    cmd.Parameters.Add("@Password", SqlDbType.VarBinary, 64).Value = HashPassword(txtPassword.Text.Trim());
                    cmd.Parameters.AddWithValue("@UserType", cmbUserType.SelectedItem.ToString());

                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("User registered successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close(); // Optional: Close the signup form
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving user: " + ex.Message);
                    }

                    txtFullName.Clear();
                    txtUsername.Clear();
                    txtEmail.Clear();
                    txtContact.Clear();
                    txtPassword.Clear();
                    txtConfirmPassword.Clear();
                    cmbUserType.SelectedIndex = 0;
                }
            }
        }
}
}
