using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GP_Auto_Accounts
{
    public partial class Suppliers : Form
    {
        private object previousCellValue = null;
        private bool isUpdating = false;
        string connectionString = "Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;";

        public Suppliers()
        {
            InitializeComponent();

            SupplierTable.CellBeginEdit += SupplierTable_CellBeginEdit;
            SupplierTable.CellEndEdit += SupplierTable_CellEndEdit;
            SupplierTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            FillSupplierTable();
            LoadSupplierNames();
            cmbSupplier.OnSelectedIndexChanged += cmbSupplier_OnSelectedIndexChanged;
        }

        private void Supplier_Load(object sender, EventArgs e)
        {
            SupplierTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            SupplierTable.AutoGenerateColumns = false;

            if (SupplierTable.Columns.Count > 0)
                SupplierTable.Columns[0].Visible = false;

            ConfigureEditableColumns();
            FillSupplierTable();
        }

        private class SupplierItem
        {
            public int SupplierID { get; set; }
            public string SupplierName { get; set; }
            public override string ToString() => SupplierName;
        }

        private void LoadSupplierNames()
        {
            try
            {
                int selectedSupplierID = 0;
                if (cmbSupplier.SelectedItem is SupplierItem selected)
                {
                    selectedSupplierID = selected.SupplierID;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT SupplierID, SupplierName FROM Supplier";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    cmbSupplier.Items.Clear();

                    cmbSupplier.Items.Add(new SupplierItem { SupplierID = 0, SupplierName = "All Suppliers" });

                    while (reader.Read())
                    {
                        var item = new SupplierItem
                        {
                            SupplierID = Convert.ToInt32(reader["SupplierID"]),
                            SupplierName = reader["SupplierName"].ToString()
                        };
                        cmbSupplier.Items.Add(item);
                    }

                    reader.Close();
                }

                if (selectedSupplierID != 0)
                {
                    foreach (SupplierItem item in cmbSupplier.Items)
                    {
                        if (item.SupplierID == selectedSupplierID)
                        {
                            cmbSupplier.SelectedItem = item;
                            return;
                        }
                    }
                    cmbSupplier.SelectedIndex = 0;
                }
                else
                {
                    cmbSupplier.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading suppliers: " + ex.Message);
            }
        }



        private void cmbSupplier_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSupplier.SelectedItem is SupplierItem selectedSupplier)
            {
                if (selectedSupplier.SupplierID == 0)
                {
                    FillSupplierTable();
                }
                else
                {
                    FilterSupplierByID(selectedSupplier.SupplierID);
                }
            }
        }

        private void FilterSupplierByID(int supplierID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT SupplierID, SupplierName, ContactNo, Address, Description, Updated_By 
                             FROM Supplier 
                             WHERE SupplierID = @SupplierID";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SupplierID", supplierID);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    SupplierTable.DataSource = dt;

                    if (SupplierTable.Columns.Contains("SupplierID"))
                        SupplierTable.Columns["SupplierID"].Visible = false;

                    ConfigureEditableColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering supplier data: " + ex.Message);
            }
        }


      

        private void ConfigureEditableColumns()
        {
            SupplierTable.Columns["SupplierID"].ReadOnly = true;
            SupplierTable.Columns["SupplierName"].ReadOnly = false;
            SupplierTable.Columns["ContactNo"].ReadOnly = false;
            SupplierTable.Columns["Address"].ReadOnly = false;
            SupplierTable.Columns["Description"].ReadOnly = false;
        }

        private void SupplierTable_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                previousCellValue = SupplierTable.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }

        private void SupplierTable_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (isUpdating) return;
            if (e.RowIndex < 0) return;

            var row = SupplierTable.Rows[e.RowIndex];
            var currentValue = row.Cells[e.ColumnIndex].Value;

            if ((previousCellValue == null && currentValue == null) ||
                (previousCellValue != null && previousCellValue.Equals(currentValue)))
            {
                previousCellValue = null;
                return;
            }

            try
            {
                isUpdating = true;

                string columnName = SupplierTable.Columns[e.ColumnIndex].Name;

                string supplierName = row.Cells["SupplierName"].Value?.ToString().Trim() ?? "";
                string contactNo = row.Cells["ContactNo"].Value?.ToString().Trim() ?? "";
                string address = row.Cells["Address"].Value?.ToString().Trim() ?? "";
                string description = row.Cells["Description"].Value?.ToString().Trim() ?? "";

                if (columnName == "SupplierName" && !Regex.IsMatch(supplierName, @"^[a-zA-Z0-9\s]+$"))
                {
                    MessageBox.Show("Supplier Name must contain only letters, numbers, and spaces.");
                    row.Cells[e.ColumnIndex].Value = previousCellValue;
                    return;
                }

                if (columnName == "ContactNo" && !contactNo.All(char.IsDigit))
                {
                    MessageBox.Show("Contact Number must contain digits only.");
                    row.Cells[e.ColumnIndex].Value = previousCellValue;
                    return;
                }

                var confirm = MessageBox.Show("Confirm update to this supplier?", "Update Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes)
                {
                    SafeReloadTable();
                    return;
                }

                if (!int.TryParse(row.Cells["SupplierID"].Value?.ToString(), out int supplierID))
                {
                    MessageBox.Show("Invalid SupplierID, cannot update.");
                    SafeReloadTable();
                    return;
                }

                bool updateSuccess = UpdateSupplierInline(supplierID, supplierName, contactNo, address, description);

                if (updateSuccess)
                {
                    row.Cells["Updated_By"].Value = AppContext.CurrentUser?.FullName ?? "Unknown";
                    MessageBox.Show("Supplier updated successfully.");
                    LoadSupplierNames();
                }
                else
                {
                    MessageBox.Show("Update failed. Supplier not found.");
                    SafeReloadTable();
                }

                previousCellValue = null;
            }
            finally
            {
                isUpdating = false;
            }
        }

        private bool UpdateSupplierInline(int supplierID, string supplierName, string contactNo, string address, string description)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"UPDATE Supplier
                       SET SupplierName = @SupplierName,
                           ContactNo = @ContactNo,
                           Address = @Address,
                           Description = @Description,
                           Updated_By = @Updated_By
                       WHERE SupplierID = @SupplierID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SupplierName", supplierName);
                    command.Parameters.AddWithValue("@ContactNo", contactNo);
                    command.Parameters.AddWithValue("@Address", address);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Updated_By", AppContext.CurrentUser?.FullName ?? "Unknown");
                    command.Parameters.AddWithValue("@SupplierID", supplierID);

                    connection.Open();
                    int affectedRows = command.ExecuteNonQuery();

                    return affectedRows > 0;
                }
            }
        }


        private void SafeReloadTable()
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(new MethodInvoker(FillSupplierTable));
                else
                    FillSupplierTable();
            }
        }

        private void kryptonButton14_Click(object sender, EventArgs e)
        {
            string supplierName = txtSupplierName.Text.Trim();
            string contactNo = txtContactNo.Text.Trim();
            string address = txtAddress.Text.Trim();
            string description = txtDescription.Text.Trim();
            string updatedBy = AppContext.CurrentUser?.FullName ?? "Unknown";

            if (string.IsNullOrWhiteSpace(supplierName) || string.IsNullOrWhiteSpace(contactNo))
            {
                MessageBox.Show("Fields Supplier Name and Contact No can't be empty.");
                return;
            }

            if (!Regex.IsMatch(supplierName, @"^[a-zA-Z0-9\s]+$"))
            {
                MessageBox.Show("Supplier Name must contain only letters, numbers, and spaces.");
                return;
            }

            if (!contactNo.All(char.IsDigit))
            {
                MessageBox.Show("Contact Number must contain digits only.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Supplier (SupplierName, ContactNo, Address, Description, Updated_By)
                                 VALUES (@SupplierName, @ContactNo, @Address, @Description, @Updated_By)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SupplierName", supplierName);
                    cmd.Parameters.AddWithValue("@ContactNo", contactNo);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Updated_By", updatedBy);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        MessageBox.Show($"Supplier '{supplierName}' saved successfully.");

                        txtSupplierName.Clear();
                        txtContactNo.Clear();
                        txtAddress.Clear();
                        txtDescription.Clear();

                        LoadSupplierNames();

                        FillSupplierTable();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        public void UpdateSuppliert(object sender, EventArgs e)
        {
            try
            {
                if (SupplierTable.CurrentRow == null)
                {
                    MessageBox.Show("Please select a Supplier to update.");
                    return;
                }

                string supplierName = txtSupplierName.Text.Trim();
                string contactNo = txtContactNo.Text.Trim();
                string address = txtAddress.Text.Trim();
                string description = txtDescription.Text.Trim();

                if (!Regex.IsMatch(supplierName, @"^[a-zA-Z0-9\s]+$"))
                {
                    MessageBox.Show("Supplier Name must contain only letters, numbers, and spaces.");
                    return;
                }

                if (!contactNo.All(char.IsDigit))
                {
                    MessageBox.Show("Contact Number must contain digits only.");
                    return;
                }

                if (!int.TryParse(SupplierTable.CurrentRow.Cells["SupplierID"].Value?.ToString(), out int supplierID))
                {
                    MessageBox.Show("Invalid Supplier ID.");
                    return;
                }

                var confirm = MessageBox.Show("Are you sure you want to update this supplier?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"UPDATE Supplier
               SET SupplierName = @SupplierName,
                   ContactNo = @ContactNo,
                   Address = @Address,
                   Description = @Description,
                   Updated_By = @Updated_By
               WHERE SupplierID = @SupplierID";


                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@SupplierName", supplierName);
                        command.Parameters.AddWithValue("@ContactNo", contactNo);
                        command.Parameters.AddWithValue("@Address", address);
                        command.Parameters.AddWithValue("@Description", description);
                        command.Parameters.AddWithValue("@Updated_By", AppContext.CurrentUser?.FullName ?? "Unknown");
                        command.Parameters.AddWithValue("@SupplierID", supplierID);

                        int rowsAffected = command.ExecuteNonQuery();

                      

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Supplier updated successfully.");
                            LoadSupplierNames();
                            FillSupplierTable();
                        }
                        else
                        {
                            MessageBox.Show("Update failed. Supplier not found.");
                        }
                      
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }

           
        }

        private void FillSupplierTable()
        {
            if (isUpdating) return;

            try
            {
                isUpdating = true;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT SupplierID, SupplierName, ContactNo, Address, Description, Updated_By FROM Supplier";

                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        sqlDataAdapter.Fill(dt);

                        SupplierTable.DataSource = dt;

                        if (SupplierTable.Columns.Count > 0)
                            SupplierTable.Columns["SupplierID"].Visible = false;

                        if (SupplierTable.Columns.Contains("DeleteColumn"))
                            SupplierTable.Columns["DeleteColumn"].DisplayIndex = SupplierTable.Columns.Count - 1;

                        ConfigureEditableColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading supplier data: " + ex.Message);
            }
            finally
            {
                isUpdating = false;
            }
        }



        private void btnClear_Click(object sender, EventArgs e)
        {
            txtSupplierName.Clear();
            txtContactNo.Clear();
            txtAddress.Clear();
            txtDescription.Clear();
        }

        private void kryptonButton6_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            Products productsForm = new Products();
            panel1.Controls.Add(productsForm.panel1);
            panel2.Controls.Add(productsForm.panel2);

            productsForm.panel1.Dock = DockStyle.Top;
            productsForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton12_Click(object sender, EventArgs e) { }
        private void kryptonGroupBox2_Paint(object sender, PaintEventArgs e) { }
        private void kryptonLabel4_Click(object sender, EventArgs e) { }
        private void kryptonDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void SupplierTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == SupplierTable.Columns["DeleteColumn"].Index && e.RowIndex >= 0)
            {
                var supplierIDValue = SupplierTable.Rows[e.RowIndex].Cells["SupplierID"].Value;
                if (supplierIDValue == null || !int.TryParse(supplierIDValue.ToString(), out int supplierID))
                {
                    MessageBox.Show("Invalid Supplier ID. Cannot delete.");
                    return;
                }

                var confirm = MessageBox.Show("Are you sure you want to delete this supplier?", "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes)
                    return;

                DeleteSupplier(supplierID);

                LoadSupplierNames();
                FillSupplierTable();
            }
        }


        private void DeleteSupplier(int supplierID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "DELETE FROM Supplier WHERE SupplierID = @SupplierID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplierID);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                            MessageBox.Show("Supplier deleted successfully.");
                        else
                            MessageBox.Show("Delete failed. Supplier not found.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting supplier: " + ex.Message);
                    }
                }
            }
        }

        private void txtContactNo_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
