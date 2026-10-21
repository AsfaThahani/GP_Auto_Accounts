using Krypton.Toolkit;
using RJCodeAdvance.RJControls;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace GP_Auto_Accounts
{
    public partial class Products : Form
    {
        private readonly string _connectionString =
            @"Server=.\SQLEXPRESS;Database=AMS;Integrated Security=True;";
        private object previousCellValue = null;
        private bool isUpdating = false;
        private bool isDeleting = false;

        public Products()
        {
            InitializeComponent();

            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns["ThresholdQty"].HeaderText = "Threshold\nQty";
            dataGridView1.Columns["Quantity"].HeaderText = "Stock\nQty";
            dataGridView1.Columns["PurchaseUnitPrice"].HeaderText = "Purchase\nUnit Price";
            dataGridView1.Columns["SaleUnitPrice"].HeaderText = "Sale\nUnit Price";
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridView1.ColumnHeadersHeight = 50;

            dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
            dataGridView1.CellBeginEdit += dataGridView1_CellBeginEdit;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;

            comboBoxCategory1.OnSelectedIndexChanged += comboBoxCategory1_SelectedIndexChanged;

            LoadSupplierComboBox();
            LoadCategories();
            FillProductTable();
        }

        private void LoadCategories()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName FROM Category ORDER BY CategoryName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        comboBoxCategory1.DataSource = dt.Copy();
                        comboBoxCategory1.DisplayMember = "CategoryName";
                        comboBoxCategory1.ValueMember = "CategoryID";
                        comboBoxCategory1.SelectedIndex = -1;

                        comboBoxCategory.DataSource = dt;
                        comboBoxCategory.DisplayMember = "CategoryName";
                        comboBoxCategory.ValueMember = "CategoryID";
                        comboBoxCategory.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }

        private void LoadSupplierComboBox()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT SupplierID, SupplierName FROM Supplier ORDER BY SupplierName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading suppliers: " + ex.Message);
            }
        }

        private void comboBoxCategory1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isUpdating)
                FillProductTable();
        }

        private void FillProductTable()
        {
            if (isUpdating) return;
            isUpdating = true;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
SELECT 
    P.ProductID, P.ProductName, C.CategoryName, P.Quantity,
    P.ThresholdQty, P.PurchaseUnitPrice, P.SaleUnitPrice,
    P.Description, P.Updated_By
FROM Product P
LEFT JOIN Category C ON P.Category_ID = C.CategoryID
";

                    if (comboBoxCategory1.SelectedIndex >= 0)
                    {
                        query += " WHERE P.Category_ID = @catID";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (comboBoxCategory1.SelectedIndex >= 0)
                        {
                            var selectedRow = comboBoxCategory1.SelectedItem as DataRowView;
                            if (selectedRow != null)
                            {
                                int categoryId = Convert.ToInt32(selectedRow["CategoryID"]);
                                cmd.Parameters.AddWithValue("@catID", categoryId);
                            }
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dataGridView1.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dataGridView1.Columns.Contains("ProductID"))
                dataGridView1.Columns["ProductID"].Visible = false;

            if (dataGridView1.Columns.Contains("DeleteColumn"))
                dataGridView1.Columns["DeleteColumn"].DisplayIndex = dataGridView1.Columns.Count - 1;

            dataGridView1.Columns["ProductID"].ReadOnly = true;
            dataGridView1.Columns["ProductName"].ReadOnly = false;
            dataGridView1.Columns["Quantity"].ReadOnly = false;
            dataGridView1.Columns["ThresholdQty"].ReadOnly = false;
            dataGridView1.Columns["PurchaseUnitPrice"].ReadOnly = false;
            dataGridView1.Columns["SaleUnitPrice"].ReadOnly = false;
            dataGridView1.Columns["Description"].ReadOnly = false;

            if (dataGridView1.Columns.Contains("SupplierName"))
                dataGridView1.Columns["SupplierName"].ReadOnly = true;
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            previousCellValue = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (isUpdating || e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            object newValue = row.Cells[e.ColumnIndex].Value;
            object oldValue = previousCellValue;
            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            string productId = row.Cells["ProductID"].Value?.ToString();
            previousCellValue = null;

            if ((oldValue == null && newValue == null) ||
                (oldValue != null && oldValue.Equals(newValue)))
                return;

            if ((columnName == "Quantity" || columnName == "ThresholdQty") &&
                !int.TryParse(newValue?.ToString(), out _))
            {
                MessageBox.Show(columnName + " must be an integer.");
                SafeReloadItems();
                return;
            }
            if ((columnName == "PurchaseUnitPrice" || columnName == "SaleUnitPrice") &&
                !decimal.TryParse(newValue?.ToString(), out _))
            {
                MessageBox.Show(columnName + " must be a decimal.");
                SafeReloadItems();
                return;
            }

            if (MessageBox.Show("Save changes?", "Confirm Update",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                SafeReloadItems();
                return;
            }

            if (UpdateProductInline(productId, columnName, newValue))
            {
                MessageBox.Show("Product updated successfully.");
                FillProductTable();
            }
            else
            {
                MessageBox.Show("Update failed.");
                SafeReloadItems();
            }
        }

        private bool UpdateProductInline(string productId, string columnName, object value)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Product SET " + columnName + " = @val, Updated_By = @user WHERE ProductID = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@val", value ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@user", AppContext.CurrentUser?.FullName ?? "Unknown");
                    cmd.Parameters.AddWithValue("@id", productId);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dataGridView1.Columns[e.ColumnIndex].Name == "DeleteColumn") return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            txtPartNumber.Text = row.Cells["ProductID"].Value?.ToString();
            txtProductName.Text = row.Cells["ProductName"].Value?.ToString();
            txtThresholdQty.Text = row.Cells["ThresholdQty"].Value?.ToString();
            txtPurchaseUnitPrice.Text = row.Cells["PurchaseUnitPrice"].Value?.ToString();
            txtSaleUnitPrice.Text = row.Cells["SaleUnitPrice"].Value?.ToString();
            txtDescription.Text = row.Cells["Description"].Value?.ToString();

            if (row.Cells["CategoryName"].Value != null)
            {

                var categoryIdObj = row.Cells["Category_ID"]?.Value;
                if (categoryIdObj != null && int.TryParse(categoryIdObj.ToString(), out int categoryId))
                {
                    comboBoxCategory.SelectedItem = categoryId;
                }
                else
                {
                    comboBoxCategory.SelectedIndex = -1;
                }
            }
            else
            {
                comboBoxCategory.SelectedIndex = -1;
            }
        }




        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (isUpdating || isDeleting || e.RowIndex < 0) return;

            if (dataGridView1.Columns[e.ColumnIndex].Name != "DeleteColumn") return;

            string id = dataGridView1.Rows[e.RowIndex].Cells["ProductID"].Value?.ToString();
            if (string.IsNullOrEmpty(id)) return;

            var confirmResult = MessageBox.Show("Delete this product?", "Confirm Deletion",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes) return;

            isDeleting = true;
            DeleteProduct(id);
            isDeleting = false;
        }

        private void DeleteProduct(string productId)
        {
            if (isUpdating) return;

            isUpdating = true;
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM InvoiceDetail WHERE Product_ID = @pid", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@pid", productId);
                        int usageCount = (int)checkCmd.ExecuteScalar();

                        if (usageCount > 0)
                        {
                            MessageBox.Show("Cannot delete this product because it is used in invoices.",
                                "Deletion Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (SqlCommand deleteCmd = new SqlCommand(
                        "DELETE FROM Product WHERE ProductID = @pid", conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@pid", productId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Product deleted successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting product: " + ex.Message);
            }
            finally
            {
                isUpdating = false;
                FillProductTable();
            }
        }

        private void SafeReloadItems()
        {
            if (!IsHandleCreated || IsDisposed) return;
            if (InvokeRequired) BeginInvoke(new MethodInvoker(FillProductTable));
            else FillProductTable();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string pid = txtPartNumber.Text.Trim();
            string name = txtProductName.Text.Trim();

            if (string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Part Number and Product Name are required.");
                return;
            }

            if (!int.TryParse(txtThresholdQty.Text.Trim(), out int th))
            { MessageBox.Show("Invalid Threshold."); return; }
            if (!decimal.TryParse(txtPurchaseUnitPrice.Text.Trim(), out decimal pu))
            { MessageBox.Show("Invalid Purchase Price."); return; }
            if (!decimal.TryParse(txtSaleUnitPrice.Text.Trim(), out decimal su))
            { MessageBox.Show("Invalid Sale Price."); return; }

            int catId = -1;
            if (comboBoxCategory.SelectedIndex >= 0)
            {
                var selectedRow = comboBoxCategory.SelectedItem as DataRowView;
                if (selectedRow != null)
                    catId = Convert.ToInt32(selectedRow["CategoryID"]);
            }

            string desc = txtDescription.Text.Trim();
            string user = AppContext.CurrentUser?.FullName ?? "Unknown";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlCommand checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Product WHERE ProductID = @pid", conn))
                {
                    checkCmd.Parameters.AddWithValue("@pid", pid);
                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        MessageBox.Show("Product with this Part Number already exists.");
                        return;
                    }
                }

                using (SqlCommand cmd = new SqlCommand(
    @"INSERT INTO Product
    (ProductID, ProductName, Category_ID, ThresholdQty,
     PurchaseUnitPrice, SaleUnitPrice, Description, Updated_By)
    VALUES (@pid,@name,@cid,@th,@pu,@su,@desc,@user)", conn))
                {
                    cmd.Parameters.AddWithValue("@pid", pid);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@cid", catId);
                    cmd.Parameters.AddWithValue("@th", th);
                    cmd.Parameters.AddWithValue("@pu", pu);
                    cmd.Parameters.AddWithValue("@su", su);
                    cmd.Parameters.AddWithValue("@desc", desc);
                    cmd.Parameters.AddWithValue("@user", user);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Product added successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Add failed: " + ex.Message);
                    }
                }
            }

            FillProductTable();
            btnClear_Click(null, null);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtPartNumber.Clear();
            txtProductName.Clear();
            txtThresholdQty.Clear();
            txtPurchaseUnitPrice.Clear();
            txtSaleUnitPrice.Clear();
            txtDescription.Clear();
            comboBoxCategory.SelectedIndex = -1;
            comboBoxCategory1.SelectedIndex = -1;
        }

        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void comboBoxCategory_OnSelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
