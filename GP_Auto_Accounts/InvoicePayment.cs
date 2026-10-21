using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GP_Auto_Accounts
{
    public partial class InvoicePayment : SmoothRoundedForm
    {
        string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";

        public InvoicePayment()
        {

            InitializeComponent();
            LoadCustomerNames();
            LoadAllInvoicePayments();
            invoiceDetailsTable.AutoGenerateColumns = false;
            invoiceDetailsTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            invoiceDetailsTable.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            invoiceDetailsTable.DefaultCellStyle.SelectionForeColor = Color.Black;
            invoiceDetailsTable.DataError += invoiceDetailsTable_DataError;
            invoiceDetailsTable.DataBindingComplete += InvoiceDetailsTable_DataBindingComplete;

            LoadAllInvoicePayments();

            ComboBoxCustomer.OnSelectedIndexChanged += ComboBoxCustomer_OnSelectedIndexChanged;

            txtInvoice.ReadOnly = true;
        }
        private void InvoiceDetailsTable_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            invoiceDetailsTable.ClearSelection();


        }
        public class CustomerItem
        {
            public int CustomerID { get; set; }
            public string CustomerName { get; set; }

            public override string ToString()
            {
                return CustomerName;
            }
        }

        private void LoadAllInvoicePayments()
        {
            dtpFromDate.Value = DateTime.Today.AddMonths(-1);
            dtpToDate.Value = DateTime.Today;

            ComboBoxCustomer.SelectedIndex = 0;

            LoadFilteredInvoices();
        }

        private void invoiceDetailsTable_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void LoadFilteredInvoices()
        {
            try
            {
                if (ComboBoxCustomer.SelectedItem == null) return;

                var selectedCustomer = (CustomerItem)ComboBoxCustomer.SelectedItem;

                string query = @"
        SELECT 
            i.InvoiceID,
            ip.InvoicePaymentID,
            c.CustomerName,
            i.GrandTotal AS TotalAmount,
            ip.PaymentAmount,
            (i.GrandTotal - (
                SELECT ISNULL(SUM(ip2.PaymentAmount), 0) 
                FROM InvoicePayment ip2 
                WHERE ip2.Invoice_ID = i.InvoiceID 
                AND ip2.PaymentDate <= ip.PaymentDate -- Only payments up to this row's date
            )) AS OutstandingAmount,
            ip.PaymentDate,
            u.FullName AS UpdatedBy
        FROM 
            InvoicePayment ip
        JOIN 
            Invoice i ON ip.Invoice_ID = i.InvoiceID
        JOIN 
            Customer c ON i.Customer_ID = c.CustomerID
        LEFT JOIN
            UserAccount u ON ip.Updated_By = u.UserID
        WHERE 
            ip.PaymentDate BETWEEN @FromDate AND @ToDate";

                if (selectedCustomer.CustomerID != -1)
                {
                    query += " AND c.CustomerID = @CustomerID";
                }

                query += " ORDER BY ip.PaymentDate DESC";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FromDate", dtpFromDate.Value.Date);
                        cmd.Parameters.AddWithValue("@ToDate", dtpToDate.Value.Date.AddDays(1).AddTicks(-1));

                        if (selectedCustomer.CustomerID != -1)
                        {
                            cmd.Parameters.AddWithValue("@CustomerID", selectedCustomer.CustomerID);
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        invoiceDetailsTable.DataSource = dt;

                        if (dt.Rows.Count > 0)
                        {
                            FormatDataGridViewColumns();
                            UpdateSummaryTotals();
                        }
                        else
                        {
                            MessageBox.Show("No payments found for the selected criteria.",
                                            "No Records",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatDataGridViewColumns()
        {
            try
            {
                if (invoiceDetailsTable.Columns.Contains("InvoiceID"))
                    invoiceDetailsTable.Columns["InvoiceID"].HeaderText = "Invoice No.";

                if (invoiceDetailsTable.Columns.Contains("InvoicePaymentID"))
                    invoiceDetailsTable.Columns["InvoicePaymentID"].HeaderText = "Payment ID";

                if (invoiceDetailsTable.Columns.Contains("CustomerName"))
                    invoiceDetailsTable.Columns["CustomerName"].HeaderText = "Customer Name";

                if (invoiceDetailsTable.Columns.Contains("TotalAmount"))
                {
                    invoiceDetailsTable.Columns["TotalAmount"].HeaderText = "Total Amount";
                    invoiceDetailsTable.Columns["TotalAmount"].DefaultCellStyle.Format = "N2";
                }

                if (invoiceDetailsTable.Columns.Contains("PaymentAmount"))
                {
                    invoiceDetailsTable.Columns["PaymentAmount"].HeaderText = "Paid Amount";
                    invoiceDetailsTable.Columns["PaymentAmount"].DefaultCellStyle.Format = "N2";
                }

                if (invoiceDetailsTable.Columns.Contains("OutstandingAmount"))
                {
                    invoiceDetailsTable.Columns["OutstandingAmount"].HeaderText = "Outstanding Amount";
                    invoiceDetailsTable.Columns["OutstandingAmount"].DefaultCellStyle.Format = "N2";
                    invoiceDetailsTable.Columns["OutstandingAmount"].ReadOnly = true;
                }

                if (invoiceDetailsTable.Columns.Contains("PaymentDate"))
                {
                    invoiceDetailsTable.Columns["PaymentDate"].HeaderText = "Payment Date";
                    invoiceDetailsTable.Columns["PaymentDate"].DefaultCellStyle.Format = "dd-MM-yyyy";
                }

                if (invoiceDetailsTable.Columns.Contains("Updated_By"))
                    invoiceDetailsTable.Columns["Updated_By"].HeaderText = "Updated By";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error formatting columns: " + ex.Message);
            }
        }
        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            LoadFilteredInvoices();
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            LoadFilteredInvoices();
        }

        private void LoadCustomerNames()
        {
            string query = "SELECT CustomerID, CustomerName FROM Customer";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    ComboBoxCustomer.Items.Clear();

                    ComboBoxCustomer.Items.Add(new CustomerItem
                    {
                        CustomerID = -1,
                        CustomerName = "-- Show All --"
                    });

                    while (reader.Read())
                    {
                        CustomerItem customer = new CustomerItem
                        {
                            CustomerID = Convert.ToInt32(reader["CustomerID"]),
                            CustomerName = reader["CustomerName"].ToString()
                        };
                        ComboBoxCustomer.Items.Add(customer);
                    }

                    reader.Close();
                    ComboBoxCustomer.SelectedIndex = 0;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Customer: " + ex.Message);
                }
            }
        }

        private void InvoiceDetails_Load(object sender, EventArgs e)
        {
            dtpFromDate.Value = DateTime.Today.AddMonths(-1);
            dtpToDate.Value = DateTime.Today;
        }

        private void invoiceDetailsTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                invoiceDetailsTable.ClearSelection();
                DataGridViewRow row = invoiceDetailsTable.Rows[e.RowIndex];
                row.Selected = true;

                txtInvoice.Text = row.Cells["InvoiceID"].Value?.ToString() ?? "";

                if (row.Cells["OutstandingAmount"].Value != null)
                {
                    decimal outstanding = Convert.ToDecimal(row.Cells["OutstandingAmount"].Value);
                    txtOutstandingAmount.Text = outstanding.ToString("N2");

                    txtPay.Text = outstanding.ToString("N2");
                }
            }
        }
        private void UpdateSummaryTotals()
        {
            try
            {
                decimal totalAmount = 0;
                decimal totalPaid = 0;
                decimal totalOutstanding = 0;

                foreach (DataGridViewRow row in invoiceDetailsTable.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        decimal rowTotal = row.Cells["TotalAmount"].Value != null ?
                            Convert.ToDecimal(row.Cells["TotalAmount"].Value) : 0;

                        decimal rowPaid = row.Cells["PaymentAmount"].Value != null ?
                            Convert.ToDecimal(row.Cells["PaymentAmount"].Value) : 0;

                        decimal rowOutstanding = row.Cells["OutstandingAmount"].Value != null ?
                            Convert.ToDecimal(row.Cells["OutstandingAmount"].Value) : 0;

                        totalAmount += rowTotal;
                        totalPaid += rowPaid;
                        totalOutstanding += rowOutstanding;
                    }
                }

                txtTotalAmount.Text = totalAmount.ToString("N2");
                txtPaymentAmount.Text = totalPaid.ToString("N2");
                txtOutstandingAmount.Text = totalOutstanding.ToString("N2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating totals: {ex.Message}",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }
        private void ComboBoxCustomer_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFilteredInvoices();
        }

        private void kryptonButton15_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            All_Invoices invoiceForm = new All_Invoices();

            panel1.Controls.Add(invoiceForm.panel1);

            panel2.Controls.Add(invoiceForm.panel2);

            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton4_Click(object sender, EventArgs e)
        {
        }

        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void kryptonButton12_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            Invoice invoiceForm = new Invoice();

            panel1.Controls.Add(invoiceForm.panel1);

            panel2.Controls.Add(invoiceForm.panel2);

            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void rjComboBox1_OnSelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void kryptonGroupBox2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (AppContext.CurrentUser == null)
                {
                    MessageBox.Show("Please login before recording payments.",
                                  "Authentication Required",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                if (invoiceDetailsTable.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select an invoice first by clicking on a row.",
                                  "No Invoice Selected",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                int invoiceId = Convert.ToInt32(invoiceDetailsTable.SelectedRows[0].Cells["InvoiceID"].Value);

                string paymentText = txtPay.Text.Replace(",", "");
                if (string.IsNullOrWhiteSpace(paymentText))
                {
                    MessageBox.Show("Please enter a payment amount.",
                                  "Payment Amount Required",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    txtPay.Focus();
                    return;
                }

                if (!decimal.TryParse(paymentText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal paymentAmount))
                {
                    MessageBox.Show("Please enter a valid numeric payment amount (e.g., 10000.00).",
                                  "Invalid Payment Amount",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    txtPay.SelectAll();
                    txtPay.Focus();
                    return;
                }

                if (paymentAmount <= 0)
                {
                    MessageBox.Show("Payment amount must be greater than zero.",
                                  "Invalid Payment Amount",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    txtPay.SelectAll();
                    txtPay.Focus();
                    return;
                }

                string outstandingText = txtOutstandingAmount.Text.Replace(",", "");
                if (string.IsNullOrWhiteSpace(outstandingText) ||
                    !decimal.TryParse(outstandingText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal outstandingAmount))
                {
                    MessageBox.Show("Cannot determine outstanding amount. Please select a valid invoice.",
                                  "Invalid Outstanding Amount",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    return;
                }

                if (paymentAmount > outstandingAmount)
                {
                    MessageBox.Show($"Payment amount ({paymentAmount:N2}) cannot exceed outstanding amount ({outstandingAmount:N2}).",
                                  "Payment Too Large",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    txtPay.Text = outstandingAmount.ToString("N2");
                    txtPay.SelectAll();
                    txtPay.Focus();
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertPaymentQuery = @"
                    INSERT INTO InvoicePayment 
                        (Invoice_ID, PaymentAmount, PaymentDate, Updated_By)
                    VALUES
                        (@InvoiceID, @PaymentAmount, @PaymentDate, @UpdatedBy)";

                            using (SqlCommand cmd = new SqlCommand(insertPaymentQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                                cmd.Parameters.AddWithValue("@PaymentAmount", paymentAmount);
                                cmd.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@UpdatedBy", AppContext.CurrentUser.UserID);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected != 1)
                                {
                                    throw new Exception("Failed to insert payment record.");
                                }
                            }

                            transaction.Commit();

                            LoadFilteredInvoices();

                            txtPay.Text = "";
                            txtPay.Focus();
                            invoiceDetailsTable.ClearSelection();

                            MessageBox.Show("Payment recorded successfully!",
                                        "Success",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        }
                        catch (SqlException sqlEx)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Database Error: {sqlEx.Message}\nError Number: {sqlEx.Number}",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Error: {ex.Message}",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

    }
}

