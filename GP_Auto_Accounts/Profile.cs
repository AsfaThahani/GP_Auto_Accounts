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
    public partial class Profile: Form
    {
        public Profile()
        {
            InitializeComponent();
            LoadUsernames();
            LoadUserTypes();
        }

        private void ProfileForm_Load(object sender, EventArgs e)
        {
            LoadUsernames();
            LoadUserTypes();
        }

        private void LoadUsernames()
        {
            cmbUsername.Items.Clear();

            try
            {
                using (SqlConnection con = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;"))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Username FROM UserAccount ORDER BY Username", con);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        cmbUsername.Items.Add(reader["Username"].ToString());
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading usernames: " + ex.Message);
            }
        }

        private void LoadUserTypes()
        {
            cmbUserType.Items.Clear();
            cmbUserType.Items.AddRange(new string[] { "Admin", "Staff", "Manager", "Viewer" });
        }

        private void cmbUsername_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbUsername.SelectedItem == null)
                return;

            string selectedUsername = cmbUsername.SelectedItem.ToString();

            using (SqlConnection con = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;"))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT FullName, Email, ContactNo, UserType FROM UserAccount WHERE Username = @Username", con);
                cmd.Parameters.AddWithValue("@Username", selectedUsername);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtFullName.Text = reader["FullName"].ToString();
                    txtEmail.Text = reader["Email"].ToString();
                    txtContactNo.Text = reader["ContactNo"].ToString();
                    cmbUserType.SelectedItem = reader["UserType"].ToString();
                }
                reader.Close();
            }
        }

        private void btnUpdate_Click_1(object sender, EventArgs e)
        {
            if (cmbUsername.SelectedItem == null)
            {
                MessageBox.Show("Please select a user.");
                return;
            }

            string username = cmbUsername.SelectedItem.ToString();
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string contactNo = txtContactNo.Text.Trim();
            string userType = cmbUserType.SelectedItem?.ToString();
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            byte[] passwordBytes = null;

            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("New password and confirm password do not match.");
                    return;
                }

                using (SHA256 sha = SHA256.Create())
                {
                    passwordBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(newPassword));
                }
            }

            using (SqlConnection con = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;"))
            {
                con.Open();
                SqlCommand cmd;

                if (passwordBytes != null)
                {
                    cmd = new SqlCommand("UPDATE UserAccount SET FullName = @FullName, Email = @Email, ContactNo = @ContactNo, UserType = @UserType, Password = @Password WHERE Username = @Username", con);
                    cmd.Parameters.AddWithValue("@Password", passwordBytes);
                }
                else
                {
                    cmd = new SqlCommand("UPDATE UserAccount SET FullName = @FullName, Email = @Email, ContactNo = @ContactNo, UserType = @UserType WHERE Username = @Username", con);
                }

                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@ContactNo", contactNo);
                cmd.Parameters.AddWithValue("@UserType", userType);
                cmd.Parameters.AddWithValue("@Username", username);

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("User profile updated successfully.");
                    ClearFormFields();
                }
                else
                {
                    MessageBox.Show("Update failed.");
                }
            }
        }

        private void ClearFormFields()
        {
            cmbUsername.SelectedIndex = -1;
            cmbUserType.SelectedIndex = -1;

            txtFullName.Clear();
            txtEmail.Clear();
            txtContactNo.Clear();
            txtNewPassword.Clear();
            txtConfirmPassword.Clear();
        }
    }
}

