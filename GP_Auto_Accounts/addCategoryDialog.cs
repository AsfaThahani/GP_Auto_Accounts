using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net;
using System.Xml.Linq;

namespace GP_Auto_Accounts
{
    public partial class addCategoryDialog: SmoothRoundedForm
    {
        public addCategoryDialog()
        {
            InitializeComponent();
        }

        private void addCategoryDialog_Load(object sender, EventArgs e)
        {

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string category = txtCategory.Text.Trim();

            if (string.IsNullOrEmpty(category))
            {
                MessageBox.Show("Category Name is required");
                return;
            }

                string connectionString = "Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    string query = "INSERT INTO Category (CategoryName, Updated_By ) VALUES (@CategoryName, @UpdatedBy )";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", category);
                       
                        cmd.Parameters.AddWithValue("@UpdatedBy", "Admin");

                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();
                        conn.Close();

                        if (rows > 0)
                        {
                            MessageBox.Show("Category added successfully.");
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add category.");
                        }
                    }
                }
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rjToggleButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (rjToggleButton1.Checked)
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                foreach (Control ctrl in this.Controls)
                {
                    ctrl.BackColor = Color.FromArgb(45, 45, 45);
                    ctrl.ForeColor = Color.White;
                }
            }
            else
            {
                this.BackColor = SystemColors.Control;
                foreach (Control ctrl in this.Controls)
                {
                    ctrl.BackColor = SystemColors.Window;
                    ctrl.ForeColor = Color.Black;
                }
            }
        }
    }
}
