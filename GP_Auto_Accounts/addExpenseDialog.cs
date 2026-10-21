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
    public partial class addExpenseDialog: Form
    {
        public addExpenseDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string categoryName = txtCategoryName.Text.Trim();

            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Please enter a category name.");
                return;
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO ExpenseCategory (CategoryName) VALUES (@CategoryName)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CategoryName", categoryName);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Category '" + categoryName + "' added successfully.");
                txtCategoryName.Clear();

                // Optionally reload the category list
                // LoadExpenseNames(); 
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Duplicate key error
                {
                    MessageBox.Show("This category already exists.");
                }
                else
                {
                    MessageBox.Show("Database error: " + ex.Message);
                }
            }
        }
    }
}
