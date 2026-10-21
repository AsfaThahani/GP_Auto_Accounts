using Krypton.Toolkit;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace GP_Auto_Accounts
{
    public partial class Expense : Form
    {
        public Expense()
        {
            InitializeComponent();
            LoadExpenseNames();
            LoadAllExpenses();
        }

        private void Expense_Load(object sender, EventArgs e)
        {
            LoadExpenseNames();
            LoadAllExpenses();
        }

        private class CategoryItem
        {
            public int CategoryID { get; set; }
            public string CategoryName { get; set; }
            public override string ToString()
            {
                return CategoryName;
            }
        }

        private void LoadExpenseNames()
        {
            string connectionString = @"Data Source=DESKTOP-KKN421P\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = "SELECT CategoryID, CategoryName FROM ExpenseCategory";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    rjComboBox1.Items.Clear();
                    cmbExp.Items.Clear();

                    cmbExp.Items.Add(new CategoryItem { CategoryID = 0, CategoryName = "-- All Categories --" });
                    cmbExp.SelectedIndex = 0;

                    while (reader.Read())
                    {
                        var item = new CategoryItem
                        {
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"].ToString()
                        };

                        rjComboBox1.Items.Add(item);
                        cmbExp.Items.Add(item);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading categories: " + ex.Message);
                }
            }
        }

        private void LoadAllExpenses()
        {
            string connectionString = @"Data Source=DESKTOP-KKN421P\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = @"
                SELECT e.ExpenseID, c.CategoryName, e.CreatedAt, e.TotalAmount, e.Description
                FROM Expense e
                INNER JOIN ExpenseCategory c ON e.Category_ID = c.CategoryID
                ORDER BY e.CreatedAt DESC";

            dgvExpenses.Rows.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        dgvExpenses.Rows.Add(
                            reader["CategoryName"].ToString(),
                            Convert.ToDateTime(reader["CreatedAt"]).ToString("yyyy-MM-dd"),
                            Convert.ToDecimal(reader["TotalAmount"]).ToString("F2"),
                            reader["Description"].ToString()
                        );
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading expenses: " + ex.Message);
                }
            }
        }

        private void LoadExpensesByCategory(int categoryId)
        {
            string connectionString = @"Data Source=DESKTOP-KKN421P\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = @"
                SELECT e.ExpenseID, c.CategoryName, e.CreatedAt, e.TotalAmount, e.Description
                FROM Expense e
                INNER JOIN ExpenseCategory c ON e.Category_ID = c.CategoryID
                WHERE e.Category_ID = @CategoryID
                ORDER BY e.CreatedAt DESC";

            dgvExpenses.Rows.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        dgvExpenses.Rows.Add(
                            reader["CategoryName"].ToString(),
                            Convert.ToDateTime(reader["CreatedAt"]).ToString("yyyy-MM-dd"),
                            Convert.ToDecimal(reader["TotalAmount"]).ToString("F2"),
                            reader["Description"].ToString()
                        );
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading filtered expenses: " + ex.Message);
                }
            }
        }

        private void cmbExp_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = cmbExp.SelectedItem as CategoryItem;
            if (selectedItem != null)
            {
                if (selectedItem.CategoryID == 0)
                {
                    LoadAllExpenses();
                }
                else
                {
                    LoadExpensesByCategory(selectedItem.CategoryID);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var selectedItem = rjComboBox1.SelectedItem as CategoryItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Please select a category.");
                return;
            }

            int categoryId = selectedItem.CategoryID;
            string description = txtDescription.Text.Trim();
            string amountText = txtAmount.Text.Trim();
            DateTime createdAt = dtpExpenseDate.Value;
            int createdBy = 1;

            if (string.IsNullOrEmpty(amountText))
            {
                MessageBox.Show("Please enter an amount.");
                return;
            }

            if (!decimal.TryParse(amountText, out decimal amount) || amount < 0)
            {
                MessageBox.Show("Invalid amount format.");
                return;
            }

            try
            {
                string connectionString = @"Data Source=DESKTOP-KKN421P\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Expense (Category_ID, CreatedBy, CreatedAt, TotalAmount, Description) " +
                                   "VALUES (@CategoryID, @CreatedBy, @CreatedAt, @Amount, @Description)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                    cmd.Parameters.AddWithValue("@CreatedAt", createdAt);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.Parameters.AddWithValue("@Description", description);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Expense saved successfully.");
                LoadAllExpenses();

                rjComboBox1.SelectedIndex = -1;
                txtAmount.Clear();
                txtDescription.Clear();
                dtpExpenseDate.Value = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
        }

        private void kryptonButton12_Click(object sender, EventArgs e)
        {
            addExpenseDialog dialog = new addExpenseDialog();
            dialog.ShowDialog();
            LoadExpenseNames();
        }
    }
}
