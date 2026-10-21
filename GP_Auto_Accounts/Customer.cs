using Krypton.Toolkit;
using RJCodeAdvance.RJControls;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GP_Auto_Accounts
{
    public partial class Customer : Form
    {
        private readonly string _connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
        private object previousCellValue = null;
        private bool isUpdating = false;

        public Customer()
        {
            InitializeComponent();

            // Event subscriptions
            dgvCustomers.CellBeginEdit += dgvCustomers_CellBeginEdit;
            dgvCustomers.CellEndEdit += dgvCustomers_CellEndEdit;
            dgvCustomers.CellContentClick += dgvCustomers_CellContentClick;
            comboBoxCustomer.OnSelectedIndexChanged += comboBoxCustomer_OnSelectedIndexChanged;
            dgvCustomers.DataBindingComplete += dgvCustomers_DataBindingComplete;

            // Initial load
            LoadCustomers();
            LoadCustomersNames();
        }

        private void Customer_Load(object sender, EventArgs e)
        {
            dgvCustomers.AutoGenerateColumns = false;
            LoadCustomers();
            LoadCustomersNames();
        }

        private class CustomerItem
        {
            public int CustomerID { get; set; }
            public string CustomerName { get; set; }
            public override string ToString() => CustomerName;
        }

        private void LoadCustomersNames()
        {
            int prevId = (comboBoxCustomer.SelectedItem as CustomerItem)?.CustomerID ?? 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT CustomerID, CustomerName FROM Customer", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        comboBoxCustomer.Items.Clear();
                        comboBoxCustomer.Items.Add(new CustomerItem { CustomerID = 0, CustomerName = "All Customers" });
                        while (reader.Read())
                        {
                            comboBoxCustomer.Items.Add(new CustomerItem
                            {
                                CustomerID = reader.GetInt32(0),
                                CustomerName = reader.GetString(1)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading customer names: " + ex.Message);
                }
            }

            var toSelect = comboBoxCustomer.Items
                              .OfType<CustomerItem>()
                              .FirstOrDefault(ci => ci.CustomerID == prevId)
                          ?? comboBoxCustomer.Items.OfType<CustomerItem>().First();
            comboBoxCustomer.SelectedItem = toSelect;
        }

        private void comboBoxCustomer_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (isUpdating) return;

            if (comboBoxCustomer.SelectedItem is CustomerItem item)
            {
                if (item.CustomerID == 0) LoadCustomers();
                else FilterCustomerByID(item.CustomerID);
            }
        }

        private void LoadCustomers()
        {
            if (isUpdating) return;
            try
            {
                isUpdating = true;
                using (var conn = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter(
                    "SELECT CustomerID, CustomerName, ContactNo, Address, VehicleNo, ChassisNo, Description, Updated_By FROM Customer",
                    conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvCustomers.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message);
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void FilterCustomerByID(int customerID)
        {
            if (isUpdating) return;
            try
            {
                isUpdating = true;
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
                    @"SELECT CustomerID, CustomerName, ContactNo, Address, VehicleNo, ChassisNo, Description, Updated_By
                      FROM Customer WHERE CustomerID = @CustomerID", conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);
                    var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvCustomers.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering customers: " + ex.Message);
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void dgvCustomers_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Hide the ID column
            if (dgvCustomers.Columns.Contains("CustomerID"))
                dgvCustomers.Columns["CustomerID"].Visible = false;

            // Force the Delete button column to the very end
            if (dgvCustomers.Columns.Contains("DeleteColumn"))
                dgvCustomers.Columns["DeleteColumn"].DisplayIndex = dgvCustomers.Columns.Count - 1;
        }

        private void dgvCustomers_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                previousCellValue = dgvCustomers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }

        private void dgvCustomers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (isUpdating || e.RowIndex < 0) return;

            var row = dgvCustomers.Rows[e.RowIndex];
            var currentValue = row.Cells[e.ColumnIndex].Value;

            // Skip if no change
            if ((previousCellValue == null && currentValue == null) ||
                (previousCellValue != null && previousCellValue.Equals(currentValue)))
            {
                previousCellValue = null;
                return;
            }

            string columnName = dgvCustomers.Columns[e.ColumnIndex].Name;
            string customerName = row.Cells["CustomerName"].Value?.ToString().Trim() ?? "";
            string contactNo = row.Cells["ContactNo"].Value?.ToString().Trim() ?? "";
            string address = row.Cells["Address"].Value?.ToString().Trim() ?? "";
            string vehicleNo = row.Cells["VehicleNo"].Value?.ToString().Trim() ?? "";
            string chassisNo = row.Cells["ChassisNo"].Value?.ToString().Trim() ?? "";
            string description = row.Cells["Description"].Value?.ToString().Trim() ?? "";

            if (!int.TryParse(row.Cells["CustomerID"].Value?.ToString(), out int customerID))
            {
                MessageBox.Show("Invalid Customer ID.");
                SafeReloadCustomers();
                previousCellValue = null;
                return;
            }

            // Validation
            if (columnName == "CustomerName" && !Regex.IsMatch(customerName, @"^[a-zA-Z0-9\s]+$"))
            {
                MessageBox.Show("Customer Name must contain only letters, numbers, and spaces.");
                row.Cells[e.ColumnIndex].Value = previousCellValue;
                previousCellValue = null;
                return;
            }
            if (columnName == "ContactNo" && !contactNo.All(char.IsDigit))
            {
                MessageBox.Show("Contact Number must contain digits only.");
                row.Cells[e.ColumnIndex].Value = previousCellValue;
                previousCellValue = null;
                return;
            }

            // Confirm save
            if (MessageBox.Show("Do you want to save the changes?", "Confirm Update",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                SafeReloadCustomers();
                previousCellValue = null;
                return;
            }

            try
            {
                isUpdating = true;
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
                    @"UPDATE Customer SET
                        CustomerName=@CustomerName, ContactNo=@ContactNo, Address=@Address,
                        VehicleNo=@VehicleNo, ChassisNo=@ChassisNo, Description=@Description,
                        Updated_By=@UpdatedBy
                      WHERE CustomerID=@CustomerID", conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerName", customerName);
                    cmd.Parameters.AddWithValue("@ContactNo", contactNo);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);
                    cmd.Parameters.AddWithValue("@ChassisNo", chassisNo);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@UpdatedBy", AppContext.CurrentUser?.FullName ?? "Unknown");
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);

                    conn.Open();
                    int count = cmd.ExecuteNonQuery();
                    if (count > 0)
                    {
                        row.Cells["Updated_By"].Value = AppContext.CurrentUser?.FullName ?? "Unknown";
                        MessageBox.Show("Customer updated successfully.");
                        LoadCustomersNames();
                    }
                    else
                    {
                        MessageBox.Show("Update failed. Customer not found.");
                        SafeReloadCustomers();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating customer: " + ex.Message);
                SafeReloadCustomers();
            }
            finally
            {
                isUpdating = false;
                previousCellValue = null;
            }
        }

        private void dgvCustomers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (isUpdating) return;
            if (e.RowIndex < 0 || dgvCustomers.Columns[e.ColumnIndex].Name != "DeleteColumn") return;

            var value = dgvCustomers.Rows[e.RowIndex].Cells["CustomerID"].Value;
            if (value == null || !int.TryParse(value.ToString(), out int customerID))
            {
                MessageBox.Show("Invalid Customer ID.");
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete this customer?", "Delete Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("DELETE FROM Customer WHERE CustomerID = @CustomerID", conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);
                    conn.Open();
                    if (cmd.ExecuteNonQuery() > 0)
                        MessageBox.Show("Customer deleted successfully.");
                    else
                        MessageBox.Show("Delete failed.");
                }
                LoadCustomersNames();
                LoadCustomers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting customer: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text) || string.IsNullOrWhiteSpace(txtContactNo.Text))
            {
                MessageBox.Show("Please fill mandatory fields.");
                return;
            }
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
                    @"INSERT INTO Customer
                      (CustomerName, ContactNo, Address, VehicleNo, ChassisNo, Description, Updated_By)
                      VALUES (@CustomerName,@ContactNo,@Address,@VehicleNo,@ChassisNo,@Description,@UpdatedBy)", conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                    cmd.Parameters.AddWithValue("@ContactNo", txtContactNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@VehicleNo", txtVehicleNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@ChassisNo", txtChassisNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@UpdatedBy", AppContext.CurrentUser?.FullName ?? "Unknown");
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Customer added successfully!");
                }
                ClearFields();
                LoadCustomers();
                LoadCustomersNames();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding customer: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            txtCustomerName.Clear();
            txtContactNo.Clear();
            txtAddress.Clear();
            txtVehicleNo.Clear();
            txtChassisNo.Clear();
            txtDescription.Clear();
        }

        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SafeReloadCustomers()
        {
            if (IsHandleCreated && !IsDisposed)
            {
                if (InvokeRequired)
                    BeginInvoke(new MethodInvoker(LoadCustomers));
                else
                    LoadCustomers();
            }
        }
    }
}