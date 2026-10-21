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
    public partial class addServiceDialog: SmoothRoundedForm
    {
        public addServiceDialog()
        {
            InitializeComponent();
        }

        private void addServiceDialog_Load(object sender, EventArgs e)
        {
            
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string serviceName = txtServiceName.Text.Trim();

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                MessageBox.Show("Please enter a service name.");
                return;
            }

            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Optional: Check for duplicates
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Service WHERE ServiceName = @name", con);
                checkCmd.Parameters.AddWithValue("@name", serviceName);

                int exists = (int)checkCmd.ExecuteScalar();
                if (exists > 0)
                {
                    MessageBox.Show("This service already exists.");
                    return;
                }

                // Insert new service
                SqlCommand cmd = new SqlCommand("INSERT INTO Service (ServiceName) VALUES (@name)", con);
                cmd.Parameters.AddWithValue("@name", serviceName);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Service added successfully.");
                    this.DialogResult = DialogResult.OK; // To notify the calling form
                    this.Close(); // Close the dialog
                }
                else
                {
                    MessageBox.Show("Failed to add service.");
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
