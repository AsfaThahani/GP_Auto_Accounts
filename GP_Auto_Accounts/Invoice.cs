using Krypton.Toolkit;
using System;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RJCodeAdvance;
using System.Runtime.Remoting.Lifetime;
using RJCodeAdvance.RJControls;
using System.Web.Services.Description;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Windows.Documents;
using Paragraph = iTextSharp.text.Paragraph;
using System.Globalization;
namespace GP_Auto_Accounts
{
    public partial class Invoice : SmoothRoundedForm
    {
        private Dictionary<string, int> customerDictionary = new Dictionary<string, int>();
        private bool isUpdating = false;
        private bool isProcessing;

        public Invoice()
        {
            InitializeComponent();
            rdoSales.Checked = true;
            LoadCustomerNames();
            LoadCategoryNames();
            LoadItemNames("");

            kryptonDataGridView1.CellEndEdit += kryptonDataGridView1_CellEndEdit;
            kryptonDataGridView1.EditingControlShowing += KryptonDataGridView1_EditingControlShowing;
            kryptonDataGridView1.CellBeginEdit += kryptonDataGridView1_CellBeginEdit;

        }

        public void Invoice_Load(object sender, EventArgs e)
        {
            LoadCustomerNames();
            LoadCategoryNames();
            LoadItemNames("");
        }


        private void LoadProductDetails(string productName)
        {
            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = "SELECT ProductID, SaleUnitPrice,PurchaseUnitPrice,ThresholdQty, Quantity FROM Product WHERE ProductName = @ProductName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ProductName", productName);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtPartNo.Text = reader["ProductID"].ToString();        // Part number
                        txtThresholdQty.Text = reader["ThresholdQty"].ToString(); // Unit price
                        txtAvailableQty.Text = reader["Quantity"].ToString();   // Available stock
                        txtSaleUnitPrice.Text = reader["SaleUnitPrice"].ToString();   // Available stock
                        txtPurchaseUnitPrice.Text = reader["PurchaseUnitPrice"].ToString();   // Available stock
                    }
                    else
                    {
                        txtPartNo.Text = "";
                        txtThresholdQty.Text = "";
                        txtAvailableQty.Text = "";
                        txtSaleUnitPrice.Text = "";
                        txtPurchaseUnitPrice.Text = "";
                    
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading product details: " + ex.Message);
                }
            }
        }

        private void rjComboBox3_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedProduct = rjComboBox3.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedProduct))
            {
                LoadProductDetails(selectedProduct);
            }
        }


        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void kryptonButton8_Click(object sender, EventArgs e)
        {
            signup signupForm = new signup();

            // Show the form
            signupForm.Show();
        }

        private void kryptonButton14_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Create an instance of the InvoiceForm
            InvoicePayment invoiceForm = new InvoicePayment();

            // Access panel1 from InvoiceForm and add it to panel1 in MainForm
            panel1.Controls.Add(invoiceForm.panel1);

            // Access panel2 from InvoiceForm and add it to panel2 in MainForm
            panel2.Controls.Add(invoiceForm.panel2);

            // Optionally, make sure the controls from InvoiceForm are correctly docked or sized within the MainForm panels
            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void LoadCustomerNames()
        {
            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = "SELECT CustomerID, CustomerName FROM Customer";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    rjComboBox1.Items.Clear();
                    customerDictionary.Clear();

                    while (reader.Read())
                    {
                        string name = reader["CustomerName"].ToString();
                        int id = Convert.ToInt32(reader["CustomerID"]);

                        rjComboBox1.Items.Add(name);
                        customerDictionary[name] = id;
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }



        private void LoadCategoryNames()
        {
            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = "SELECT CategoryName FROM Category";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    rjComboBox2.Items.Clear();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string categoryName = reader["CategoryName"].ToString();
                            rjComboBox2.Items.Add(categoryName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No category found in the database.");
                    }


                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }


        private void LoadServices()
        {
            // Clear existing data
            rjComboBox3.Items.Clear();

            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT ServiceName FROM Service", con);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    rjComboBox3.Items.Add(reader["ServiceName"].ToString());
                }

                reader.Close();
            }

            // Don't auto-select anything
            // rjComboBox3.SelectedIndex = 0;
        }


        private void rjComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rjComboBox2.SelectedItem != null)
            {
                string selectedCategory = rjComboBox2.SelectedItem.ToString();
                LoadItemNames(selectedCategory);
            }
        }

        private void CalculateItemTotalAndFinalAmount()
        {
            // Step 1: Get Quantity
            if (!int.TryParse(txtQuantity.Text.Trim(), out int qty))
            {
                txtItemTotal.Text = "0.00";
                txtAmount.Text = "0.00";
                return;
            }

            // Step 2: Determine which unit price to use
            double unitPrice = 0;

            if (rdoServices.Checked)
            {
                // Use Service Fee
                if (!double.TryParse(txtServiceFee.Text.Trim(), out unitPrice))
                {
                    txtItemTotal.Text = "0.00";
                    txtAmount.Text = "0.00";
                    return;
                }
            }
            else
            {
                // Use Sale Unit Price
                if (!double.TryParse(txtSaleUnitPrice.Text.Trim(), out unitPrice))
                {
                    txtItemTotal.Text = "0.00";
                    txtAmount.Text = "0.00";
                    return;
                }
            }

            // Step 3: Calculate item total (qty × unitPrice)
            double itemTotal = qty * unitPrice;
            txtItemTotal.Text = itemTotal.ToString("F2");

            // Step 4: Get Discount (optional)
            double discount = 0;
            if (!string.IsNullOrWhiteSpace(txtDiscount.Text))
            {
                double.TryParse(txtDiscount.Text.Replace("%", "").Trim(), out discount);
            }

            // Step 5: Apply discount
            double discountAmount = itemTotal * (discount / 100.0);
            double finalAmount = itemTotal - discountAmount;

            // Optional: prevent negative total
            if (finalAmount < 0)
            {
                finalAmount = 0;
            }

            // Step 6: Update final amount
            txtAmount.Text = finalAmount.ToString("F2");
        }

        private void LoadItemNames(string selectedCategory)
        {
            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query;

            if (string.IsNullOrWhiteSpace(selectedCategory))
            {
                query = "SELECT ProductName FROM Product";
            }
            else
            {
                query = @"SELECT p.ProductName 
                  FROM Product p
                  INNER JOIN Category c ON p.Category_ID = c.CategoryID
                  WHERE c.CategoryName = @CategoryName";
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);

                    if (!string.IsNullOrWhiteSpace(selectedCategory))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    rjComboBox3.Items.Clear();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string productName = reader["ProductName"].ToString();
                            rjComboBox3.Items.Add(productName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No products found.");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void kryptonButton15_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Create an instance of the InvoiceForm
            All_Invoices allinvoiceForm = new All_Invoices();

            // Access panel1 from InvoiceForm and add it to panel1 in MainForm
            panel1.Controls.Add(allinvoiceForm.panel1);

            // Access panel2 from InvoiceForm and add it to panel2 in MainForm
            panel2.Controls.Add(allinvoiceForm.panel2);

            // Optionally, make sure the controls from InvoiceForm are correctly docked or sized within the MainForm panels
            allinvoiceForm.panel1.Dock = DockStyle.Top;
            allinvoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void rjComboBox1_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            string customerName = rjComboBox1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(customerName))
            {
                LoadVehicleNumbersForCustomer(customerName);
            }
        }

        private void LoadVehicleNumbersForCustomer(string customerName)
        {
            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = "SELECT VehicleNo FROM Customer WHERE CustomerName = @CustomerName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CustomerName", customerName);

                    SqlDataReader reader = cmd.ExecuteReader();
                    rjComboBox4.Items.Clear();

                    while (reader.Read())
                    {
                        string vehicleNo = reader["VehicleNo"].ToString();
                        rjComboBox4.Items.Add(vehicleNo);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading vehicles: " + ex.Message);
                }
            }
        }

        private void btnAddCustomers_Click_1(object sender, EventArgs e)
        {

            addCustomerDialog dialog = new addCustomerDialog();

            // Show the form as a dialog (modal)
            dialog.ShowDialog();

            // Optionally, you can retrieve any values or inputs from the dialog form after it's closed.


            // Do something with the result, like updating the UI
            LoadCustomerNames();
        }

        private void kryptonButton20_Click(object sender, EventArgs e)
        {
            addCategoryDialog dialog = new addCategoryDialog();

            // Show the form as a dialog (modal)
            dialog.ShowDialog();

            // Optionally, you can retrieve any values or inputs from the dialog form after it's closed.


            // Do something with the result, like updating the UI
            LoadCategoryNames();
        }
        private void kryptonButton21_Click(object sender, EventArgs e)
{
    if (rdoServices.Checked)
    {
        addServiceDialog serviceDialog = new addServiceDialog();
        var result = serviceDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            LoadServices(); // Refresh RJComboBox3 with updated list
        }
    }
    else
    {
        addItemDialog itemDialog = new addItemDialog();
        var result = itemDialog.ShowDialog();

        if (result == DialogResult.OK && rjComboBox2.SelectedItem != null)
        {
            string selectedCategory = rjComboBox2.SelectedItem.ToString();
            LoadItemNames(selectedCategory);
        }
    }
}


        private void kryptonButton23_Click(object sender, EventArgs e)
        {
            addVehicleDialog dialog = new addVehicleDialog();

            // Show the form as a dialog (modal)
            dialog.ShowDialog();

            // Optionally, you can retrieve any values or inputs from the dialog form after it's closed.


            // Do something with the result, like updating the UI
        }




        private void kryptonButton17_Click(object sender, EventArgs e)
        {

        }


        private void rjComboBox4_OnSelectedIndexChanged(object sender, EventArgs e)
        {

            string vehicleNo = rjComboBox4.SelectedItem?.ToString();
            string customerName = rjComboBox1.SelectedItem?.ToString(); // optional, for safety

            if (!string.IsNullOrEmpty(vehicleNo))
            {
                LoadChassisNumber(vehicleNo);
            }
        }

        private void LoadChassisNumber(string vehicleNo)
        {
            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = "SELECT ChassisNo FROM Customer WHERE VehicleNo = @VehicleNo";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtChassisNo.Text = reader["ChassisNo"].ToString();
                    }
                    else
                    {
                        txtChassisNo.Text = "";
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading chassis number: " + ex.Message);
                }
            }
        }




        private void txtDiscount_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow numbers and backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            // Prevent entering more than 3 digits (0-100%)
            if (char.IsDigit(e.KeyChar) && txtDiscount.Text.Replace("%", "").Length >= 3)
            {
                e.Handled = true;
            }
        }

        private void CalculateSubTotal()
        {
            decimal subTotal = 0;

            foreach (DataGridViewRow row in kryptonDataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Cells[3].Value != null) // Item Total column
                {
                    if (decimal.TryParse(row.Cells[3].Value.ToString(), out decimal itemTotal))
                    {
                        subTotal += itemTotal;
                    }
                }
            }

            txtSubTotal.Text = subTotal.ToString("F2");
        }


        private void txtAmountPaid_TextChanged(object sender, EventArgs e)
        {
            CalculateOutstanding();
        }

        private void CalculateOutstanding()
        {
            try
            {
                // Parse grand total with proper decimal handling
                if (!decimal.TryParse(txtGrandTotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal grandTotal) || grandTotal < 0)
                {
                    txtOutstanding.Text = 0.ToString("F2");
                    return;
                }

                // Parse amount paid (treat invalid/empty as 0)
                if (!decimal.TryParse(txtAmountPaid.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal amountPaid) || amountPaid < 0)
                {
                    amountPaid = 0;
                }

                // Handle overpayment with better UX
                if (amountPaid > grandTotal)
                {
                    // Option 1: Show warning but allow overpayment (common in some business cases)
                    // MessageBox.Show("Note: Payment exceeds grand total. Change will be due.", 
                    //               "Overpayment", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Option 2: Auto-correct to exact amount (current behavior)
                    amountPaid = grandTotal;
                    txtAmountPaid.Text = amountPaid.ToString("F2");
                }

                decimal outstanding = grandTotal - amountPaid;
                txtOutstanding.Text = outstanding.ToString("F2");

                // Optional: Visual feedback for fully paid/overpaid
                if (outstanding <= 0)
                {
                    txtOutstanding.BackColor = Color.LightGreen;
                }
                else
                {
                    txtOutstanding.BackColor = SystemColors.Window;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating outstanding balance: {ex.Message}");
                txtOutstanding.Text = "0.00";
            }
        }

        private void chkFullyPaid_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFullyPaid.Checked)
            {
                if (double.TryParse(txtGrandTotal.Text, out double grandTotal))
                {
                    txtAmountPaid.TextChanged -= txtAmountPaid_TextChanged;
                    txtAmountPaid.Text = grandTotal.ToString("F2");
                    txtOutstanding.Text = "0.00";
                    txtAmountPaid.TextChanged += txtAmountPaid_TextChanged;
                }
            }
            else
            {
                txtAmountPaid.Text = "";
                txtOutstanding.Text = "";
            }
        }

        private void CalculateTotalDiscount()
        {
            decimal totalDiscount = 0;

            foreach (DataGridViewRow row in kryptonDataGridView1.Rows)
            {
                if (!row.IsNewRow &&
                    row.Cells[3].Value != null && // Item Total
                    row.Cells[4].Value != null)   // Discount
                {
                    if (decimal.TryParse(row.Cells[3].Value.ToString(), out decimal itemTotal))
                    {
                        string discountStr = row.Cells[4].Value.ToString().Replace("%", "").Trim();
                        if (decimal.TryParse(discountStr, out decimal discountPercent))
                        {
                            decimal discountAmount = itemTotal * (discountPercent / 100m);
                            totalDiscount += discountAmount;
                        }
                    }
                }
            }

            txtTotalDiscount.Text = totalDiscount.ToString("F2");
        }

        private void txtTotalDiscount_TextChanged(object sender, EventArgs e)
        {
            CalculateGrandTotal();
        }

        private void CalculateGrandTotal()
        {
            if (decimal.TryParse(txtSubTotal.Text, out decimal subTotal) &&
                decimal.TryParse(txtTotalDiscount.Text, out decimal totalDiscount))
            {
                decimal grandTotal = subTotal - totalDiscount;
                txtGrandTotal.Text = grandTotal.ToString("F2");
            }
            else
            {
                txtGrandTotal.Text = "0.00";
            }

            CalculateOutstanding();
        }

        private void KryptonDataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // For Quantity column (index 1)
            if (kryptonDataGridView1.CurrentCell.ColumnIndex == 1)
            {
                if (e.Control is TextBox tb)
                {
                    tb.KeyPress -= QuantityKeyPress;
                    tb.KeyPress += QuantityKeyPress;
                }
            }
            // For Unit Price column (index 2)
            else if (kryptonDataGridView1.CurrentCell.ColumnIndex == 2)
            {
                if (e.Control is TextBox tb)
                {
                    tb.KeyPress -= PriceKeyPress;
                    tb.KeyPress += PriceKeyPress;
                }
            }
        }

        private void QuantityKeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow numbers and backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void PriceKeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow numbers, decimal point, and backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Only allow one decimal point
            TextBox tb = sender as TextBox;
            if (e.KeyChar == '.' && tb.Text.Contains('.'))
            {
                e.Handled = true;
            }
        }

        private void txtGrandTotal_TextChanged(object sender, EventArgs e)
        {
            CalculateOutstanding(); // Recalculate outstanding when grand total changes
        }

        private void ClearInvoiceForm()
        {
            txtPartNo.Clear();
            txtQuantity.Clear();
            txtDescription.Clear();
            txtMileage.Clear();
            txtDiscount.Clear();
            txtItemTotal.Clear();
            txtAmount.Clear();
            txtSubTotal.Clear();
            txtTotalDiscount.Clear();
            txtGrandTotal.Clear();
            txtAmountPaid.Clear();
            txtOutstanding.Clear();
            txtAvailableQty.Clear();
            txtThresholdQty.Clear();
            txtSaleUnitPrice.Clear();
            txtPurchaseUnitPrice.Clear();
            txtChassisNo.Clear();


            rjComboBox1.SelectedIndex = -1;
            rjComboBox2.SelectedIndex = -1;
            rjComboBox3.SelectedIndex = -1;
            rjComboBox4.SelectedIndex = -1;

            kryptonDataGridView1.Rows.Clear(); // Clears all rows from the DataGridView

            chkFullyPaid.Checked = false;

            rdoSales.Checked = false;
            rdoServices.Checked = false;
        }

        private void kryptonButton17_Click_1(object sender, EventArgs e)
        {
            string description = txtDescription.Text.Trim();
            string mileageText = txtMileage.Text.Trim();

            // Validate mileage
            if (!int.TryParse(mileageText, out int mileageValue) || mileageValue < 0)
            {
                MessageBox.Show("Invalid mileage. Please enter a positive whole number.");
                return;
            }

            // Validate financial amounts
            if (!decimal.TryParse(txtGrandTotal.Text.Trim(), out decimal grandTotal) ||
                !decimal.TryParse(txtSubTotal.Text.Trim(), out decimal subTotal) ||
                !decimal.TryParse(txtTotalDiscount.Text.Trim(), out decimal totalDiscount))
            {
                MessageBox.Show("Invalid total amounts. Please check the values.");
                return;
            }

            // Validate payment amount
            if (!decimal.TryParse(txtAmountPaid.Text.Trim(), out decimal amountPaid))
            {
                MessageBox.Show("Please enter a valid payment amount.");
                return;
            }

            // Ensure payment doesn't exceed grand total
            amountPaid = Math.Min(amountPaid, grandTotal);

            string selectedName = rjComboBox1.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedName) || !customerDictionary.ContainsKey(selectedName))
            {
                MessageBox.Show("Please select a valid customer.");
                return;
            }

            string connectionString = "Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // 1. Insert Invoice
                    string insertInvoiceQuery = @"
                INSERT INTO Invoice (Customer_ID, Updated_By, SubTotal, TotalDiscount, GrandTotal, Description, Mileage)
                VALUES (@Customer_ID, @Updated_By, @SubTotal, @TotalDiscount, @GrandTotal, @Description, @Mileage);
                SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdInvoice = new SqlCommand(insertInvoiceQuery, con, transaction);
                    int selectedCustomerID = customerDictionary[selectedName];
                    cmdInvoice.Parameters.AddWithValue("@Customer_ID", selectedCustomerID);
                    cmdInvoice.Parameters.AddWithValue("@Updated_By", 1);
                    cmdInvoice.Parameters.AddWithValue("@SubTotal", subTotal);
                    cmdInvoice.Parameters.AddWithValue("@TotalDiscount", totalDiscount);
                    cmdInvoice.Parameters.AddWithValue("@GrandTotal", grandTotal);
                    cmdInvoice.Parameters.AddWithValue("@Description", description);
                    cmdInvoice.Parameters.AddWithValue("@Mileage", mileageValue);

                    int invoiceID = Convert.ToInt32(cmdInvoice.ExecuteScalar());

                    // 2. Insert InvoiceDetails
                    foreach (DataGridViewRow row in kryptonDataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string itemType = row.Cells["ItemType"].Value?.ToString();
                        string itemName = row.Cells[0].Value?.ToString(); // Item name from first column
                        int qty = int.Parse(row.Cells["quantity"].Value?.ToString());
                        decimal discount = decimal.Parse(row.Cells["discount"].Value?.ToString().Replace("%", ""));

                        // Get the actual ID from database
                        int? itemId = null;
                        if (itemType == "Product")
                        {
                            itemId = GetProductIdByName(itemName, con, transaction);
                        }
                        else if (itemType == "Service")
                        {
                            itemId = GetServiceIdByName(itemName, con, transaction);
                        }

                        if (!itemId.HasValue)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Could not find {itemType} '{itemName}' in database");
                            return;
                        }

                        SqlCommand cmdDetail;
                        if (itemType == "Product")
                        {
                            string insertProductQuery = @"
                        INSERT INTO InvoiceDetail (Invoice_ID, Product_ID, Quantity, DiscountPercent)
                        VALUES (@Invoice_ID, @Product_ID, @Quantity, @DiscountPercent)";
                            cmdDetail = new SqlCommand(insertProductQuery, con, transaction);
                            cmdDetail.Parameters.AddWithValue("@Product_ID", itemId.Value);
                        }
                        else
                        {
                            string insertServiceQuery = @"
                        INSERT INTO InvoiceDetail 
                            (Invoice_ID, Service_ID, Quantity, UnitPrice, DiscountPercent)
                        VALUES 
                            (@Invoice_ID, @Service_ID, @Quantity, @UnitPrice, @DiscountPercent)";

                            cmdDetail = new SqlCommand(insertServiceQuery, con, transaction);
                            cmdDetail.Parameters.AddWithValue("@Service_ID", itemId.Value);
                            decimal unitPrice = decimal.Parse(row.Cells["UnitPrice"].Value.ToString());
                            cmdDetail.Parameters.AddWithValue("@UnitPrice", unitPrice);
                        }

                        cmdDetail.Parameters.AddWithValue("@Invoice_ID", invoiceID);
                        cmdDetail.Parameters.AddWithValue("@Quantity", qty);
                        cmdDetail.Parameters.AddWithValue("@DiscountPercent", discount);
                        cmdDetail.ExecuteNonQuery();

                        // Update stock only for products
                        if (itemType == "Product")
                        {
                            string updateStockQuery = "UPDATE Product SET Quantity = Quantity - @Quantity WHERE ProductID = @ProductID";
                            SqlCommand cmdUpdate = new SqlCommand(updateStockQuery, con, transaction);
                            cmdUpdate.Parameters.AddWithValue("@Quantity", qty);
                            cmdUpdate.Parameters.AddWithValue("@ProductID", itemId.Value);
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }

                    // 3. Insert Payment (using actual amount paid, not grand total)
                    if (amountPaid > 0)
                    {
                        string insertPaymentQuery = @"
                    INSERT INTO InvoicePayment (Invoice_ID, PaymentAmount, PaymentDate)
                    VALUES (@Invoice_ID, @PaymentAmount, @PaymentDate)";

                        SqlCommand cmdPayment = new SqlCommand(insertPaymentQuery, con, transaction);
                        cmdPayment.Parameters.AddWithValue("@Invoice_ID", invoiceID);
                        cmdPayment.Parameters.AddWithValue("@PaymentAmount", amountPaid);
                        cmdPayment.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                        cmdPayment.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    MessageBox.Show("Invoice, details, and payment saved successfully!");
                    ClearInvoiceForm();
                    rdoSales.Checked = true;

                    var customerInfo = GetCustomerVehicleInfo(selectedCustomerID);
                    GenerateInvoicePDF(invoiceID, selectedName, customerInfo.ContactNo, customerInfo.VehicleNo, mileageValue.ToString(), description);
                }
                catch (Exception ex)
                {
                    try
                    {
                        transaction.Rollback();
                        MessageBox.Show("Transaction rolled back. Error: " + ex.Message);
                    }
                    catch (Exception rollbackEx)
                    {
                        MessageBox.Show("Rollback failed: " + rollbackEx.Message);
                    }
                }
            }
        }

        // Helper method to get Service ID by name
        private int? GetServiceIdByName(string serviceName, SqlConnection con, SqlTransaction transaction)
        {
            string query = "SELECT ServiceID FROM Service WHERE ServiceName = @ServiceName";
            using (SqlCommand cmd = new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.AddWithValue("@ServiceName", serviceName);
                object result = cmd.ExecuteScalar();
                return result != null ? (int)result : (int?)null;
            }
        }

        // Helper method to get Product ID by name
        private int? GetProductIdByName(string productName, SqlConnection con, SqlTransaction transaction)
        {
            string query = "SELECT ProductID FROM Product WHERE ProductName = @ProductName";
            using (SqlCommand cmd = new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.AddWithValue("@ProductName", productName);
                object result = cmd.ExecuteScalar();
                return result != null ? (int)result : (int?)null;
            }
        }

        // Helper class for customer info
        class CustomerVehicleInfo
        {
            public string ContactNo { get; set; }
            public string VehicleNo { get; set; }
        }

        // Method to get customer contact and vehicle no
        CustomerVehicleInfo GetCustomerVehicleInfo(int customerId)
        {
            CustomerVehicleInfo info = new CustomerVehicleInfo();

            string connectionString = "Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;";

            string query = @"
        SELECT ContactNo, VehicleNo
        FROM Customer
        WHERE CustomerID = @CustomerID";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        info.ContactNo = reader["ContactNo"]?.ToString() ?? "";
                        info.VehicleNo = reader["VehicleNo"]?.ToString() ?? "";
                    }
                }
            }
            return info;
        }

        DataTable GetInvoiceProductDetails(int invoiceID)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT p.ProductName AS [Item Name], id.Quantity, p.SaleUnitPrice AS [Unit Price],
                    (id.Quantity * p.SaleUnitPrice) AS [Item Total], 
                    id.DiscountPercent AS [Discount %],
                    (id.Quantity * p.SaleUnitPrice * (1 - id.DiscountPercent / 100.0)) AS [Final Amount]
             FROM InvoiceDetail id
             INNER JOIN Product p ON id.Product_ID = p.ProductID
             WHERE id.Invoice_ID = @InvoiceID";

            using (SqlConnection con = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceID);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            return dt;
        }

        DataTable GetInvoiceServiceDetails(int invoiceID)
        {
            DataTable dt = new DataTable();
            string query = @"
        SELECT 
            s.ServiceName AS [Item Name], 
            id.Quantity, 
            id.UnitPrice AS [Unit Price],  
            (id.Quantity * id.UnitPrice) AS [Item Total], 
            id.DiscountPercent AS [Discount %],
            (id.Quantity * id.UnitPrice * (1 - id.DiscountPercent / 100.0)) AS [Final Amount]
        FROM InvoiceDetail id
        INNER JOIN Service s ON id.Service_ID = s.ServiceID
        WHERE id.Invoice_ID = @InvoiceID";

            using (SqlConnection con = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceID);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            return dt;
        }
      
        void GenerateInvoicePDF(int invoiceID, string customerName, string customerContact, string vehicleNo, string mileage, string description)
        {
            DataTable productDetailsTable = GetInvoiceProductDetails(invoiceID);
            DataTable serviceDetailsTable = GetInvoiceServiceDetails(invoiceID);

            // Get the invoice summary (subtotal, discount, grand total)
            decimal subtotal = 0, totalDiscount = 0, grandTotal = 0;
            GetInvoiceSummary(invoiceID, out subtotal, out totalDiscount, out grandTotal);

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = $"Invoice_{invoiceID}.pdf";
            string filePath = Path.Combine(desktopPath, fileName);

            Document doc = new Document(PageSize.A4, 50, 50, 50, 50);

            try
            {
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // Load custom fonts with fallback to system fonts if custom fonts not found
                BaseFont titleFont = null;
                BaseFont labelFont = null;
                BaseFont textFont = null;
                BaseFont tableHeaderFont = null;
                BaseFont tableDataFont = null;

                try
                {
                    // Try to load custom fonts
                    string afacadBoldPath = @"C:\Fonts\AfacadFlux-Bold.ttf";
                    string afacadThinPath = @"C:\Fonts\AfacadFlux-Regular.ttf";
                    string bahnschriftPath = @"C:\Fonts\BS_Static_Bold.ttf";

                    titleFont = BaseFont.CreateFont(bahnschriftPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    labelFont = BaseFont.CreateFont(afacadBoldPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    textFont = BaseFont.CreateFont(afacadThinPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    tableHeaderFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                    tableDataFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                }
                catch
                {
                    // Fallback to system fonts if custom fonts fail
                    titleFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                    labelFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                    textFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                    tableHeaderFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                    tableDataFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                }

                // Create font objects with appropriate sizes
                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(titleFont, 20);
                iTextSharp.text.Font fontLabel = new iTextSharp.text.Font(labelFont, 14);
                iTextSharp.text.Font fontText = new iTextSharp.text.Font(textFont, 12);
                iTextSharp.text.Font fontTableHeader = new iTextSharp.text.Font(tableHeaderFont, 8);
                iTextSharp.text.Font fontTableData = new iTextSharp.text.Font(tableDataFont, 8);

                // Create Arial fonts for specific elements
                BaseFont arialBold = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                BaseFont arialRegular = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);

                // Font for Services label (Arial Bold 12pt)
                iTextSharp.text.Font fontServicesLabel = new iTextSharp.text.Font(arialBold, 10);

                // Fonts for summary section
                iTextSharp.text.Font fontSummaryLabel = new iTextSharp.text.Font(arialBold, 8);
                iTextSharp.text.Font fontSummaryValue = new iTextSharp.text.Font(arialRegular, 8);

                // Logo
                string logoPath = @"C:\Users\Umer Farook\Desktop\group project proposal\logo.png";
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(180f, 180f);
                    logo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(logo);
                }

                Paragraph address = new Paragraph("99/1, Quarry Road, Dehiwala, Colombo | 9477-1199991 | 9477-8916404", fontText);
                address.Alignment = Element.ALIGN_CENTER;
                doc.Add(address);
                doc.Add(new Paragraph(" "));

                Paragraph invoiceTitle = new Paragraph("INVOICE", fontTitle)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                doc.Add(invoiceTitle);

                // Customer + Invoice Header Table
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 3f, 2f });

                PdfPCell leftCell = new PdfPCell { Border = PdfPCell.NO_BORDER };
                leftCell.AddElement(new Paragraph("Customer Details", fontLabel));
                leftCell.AddElement(new Paragraph($"Name: {customerName}", fontText));
                leftCell.AddElement(new Paragraph($"Contact No: {customerContact}", fontText));
                leftCell.AddElement(new Paragraph($"Vehicle No: {vehicleNo}", fontText));
                leftCell.AddElement(new Paragraph($"Mileage: {mileage} km", fontText));
                headerTable.AddCell(leftCell);

                PdfPCell rightCell = new PdfPCell { Border = PdfPCell.NO_BORDER, PaddingLeft = 85f };
                rightCell.AddElement(new Paragraph($"Invoice ID: {invoiceID}", fontText));
                rightCell.AddElement(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}", fontText));
                headerTable.AddCell(rightCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                // Create a single table for all items
                PdfPTable mainTable = new PdfPTable(7); // 7 columns
                mainTable.WidthPercentage = 100;
                float[] columnWidths = { 0.7f, 5f, 1.2f, 1.5f, 1.5f, 1.2f, 1.8f };
                mainTable.SetWidths(columnWidths);

                // Add table headers only once
                PdfPCell noHeader = new PdfPCell(new Phrase("No.", fontTableHeader))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    Border = PdfPCell.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5f
                };
                mainTable.AddCell(noHeader);

                string[] headers = { "Item Name", "Qty", "Unit Price (Rs)", "Item Total (Rs)", "Discount", "Final Amount (Rs)" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, fontTableHeader))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        Border = PdfPCell.NO_BORDER,
                        Padding = 5f,
                        HorizontalAlignment = header == "Item Name" ? Element.ALIGN_LEFT :
                                             header == "Qty" ? Element.ALIGN_CENTER : Element.ALIGN_RIGHT
                    };
                    mainTable.AddCell(cell);
                }

                // Add products with numbering starting from 1
                int productCounter = 1;
                foreach (DataRow row in productDetailsTable.Rows)
                {
                    AddItemToTable(row, mainTable, fontTableData, ref productCounter);
                }

                // Add services with fresh numbering starting from 1
                if (serviceDetailsTable.Rows.Count > 0)
                {
                    // Add separator if there are products above
                    if (productDetailsTable.Rows.Count > 0)
                    {
                        // Add empty row as separator
                        for (int i = 0; i < 7; i++)
                        {
                            mainTable.AddCell(new PdfPCell(new Phrase(" ", fontTableData))
                            {
                                Border = PdfPCell.NO_BORDER,
                                Padding = 5f
                            });
                        }

                        // Add "Services" label with Arial Bold 12pt
                        PdfPCell servicesLabel = new PdfPCell(new Phrase("Services", fontServicesLabel))
                        {
                            Colspan = 7,
                            Border = PdfPCell.NO_BORDER,
                            PaddingTop = 5f,
                            PaddingBottom = 5f
                        };
                        mainTable.AddCell(servicesLabel);
                    }

                    // Add services with fresh numbering
                    int serviceCounter = 1;
                    foreach (DataRow row in serviceDetailsTable.Rows)
                    {
                        AddItemToTable(row, mainTable, fontTableData, ref serviceCounter);
                    }
                }

                doc.Add(mainTable);

                // Add horizontal line before summary section
                doc.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f)));
                doc.Add(new Paragraph(" ")); // Add some space

                // Create summary table (2 columns: label and value)
                PdfPTable summaryTable = new PdfPTable(2);
                summaryTable.WidthPercentage = 50;
                summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                summaryTable.SetWidths(new float[] { 2f, 1f });

                // Add summary rows with Arial Bold for labels and Arial Regular for values
                AddSummaryRow(summaryTable, "Subtotal:", subtotal.ToString("F2"), fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Total Discount:", totalDiscount.ToString("F2"), fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Grand Total:", grandTotal.ToString("F2"), fontSummaryLabel, fontSummaryValue);

                // Get payment information
                decimal amountPaid = GetAmountPaid(invoiceID);
                decimal outstanding = grandTotal - amountPaid;

                AddSummaryRow(summaryTable, "Amount Paid:", amountPaid.ToString("F2"), fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Outstanding:", outstanding.ToString("F2"), fontSummaryLabel, fontSummaryValue);

                doc.Add(summaryTable);

                if (!string.IsNullOrWhiteSpace(description))
                {
                    Paragraph recommendationLabel = new Paragraph("Recommendation:", fontSummaryLabel)
                    {
                        Alignment = Element.ALIGN_LEFT,
                        SpacingBefore = 15f,
                        SpacingAfter = 2f
                    };
                    doc.Add(recommendationLabel);

                    Paragraph recommendationText = new Paragraph(description, fontSummaryValue)
                    {
                        Alignment = Element.ALIGN_LEFT,
                        SpacingAfter = 10f
                    };
                    doc.Add(recommendationText);
                }

                // Add footer
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("Thank you for your business!", fontLabel)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 20f
                });

                doc.Close();
                MessageBox.Show($"Invoice PDF saved to desktop: {fileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF generation error: " + ex.Message);
            }
        }
        private void GetInvoiceSummary(int invoiceID, out decimal subtotal, out decimal totalDiscount, out decimal grandTotal)
        {
            subtotal = 0;
            totalDiscount = 0;
            grandTotal = 0;

            string query = "SELECT SubTotal, TotalDiscount, GrandTotal FROM Invoice WHERE InvoiceID = @InvoiceID";

            using (SqlConnection con = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceID);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        subtotal = Convert.ToDecimal(reader["SubTotal"]);
                        totalDiscount = Convert.ToDecimal(reader["TotalDiscount"]);
                        grandTotal = Convert.ToDecimal(reader["GrandTotal"]);
                    }
                }
            }
        }

        private decimal GetAmountPaid(int invoiceID)
        {
            string query = "SELECT SUM(PaymentAmount) FROM InvoicePayment WHERE Invoice_ID = @InvoiceID";

            using (SqlConnection con = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceID);
                con.Open();

                object result = cmd.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        private void AddSummaryRow(PdfPTable table, string label, string value, iTextSharp.text.Font labelFont, iTextSharp.text.Font valueFont)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont))
            {
                Border = PdfPCell.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 5f
            };

            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont))
            {
                Border = PdfPCell.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 5f
            };

            table.AddCell(labelCell);
            table.AddCell(valueCell);
        }

        private void AddItemToTable(DataRow row, PdfPTable table, iTextSharp.text.Font font, ref int serialNo)
        {
            // Add serial number
            table.AddCell(new PdfPCell(new Phrase(serialNo++.ToString(), font))
            {
                Border = PdfPCell.NO_BORDER,
                Padding = 5f,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            // Add item details
            string[] columns = { "Item Name", "Quantity", "Unit Price", "Item Total", "Discount %", "Final Amount" };
            foreach (string col in columns)
            {
                object item = row[col];
                string text;

                if (col == "Discount %" && item != DBNull.Value)
                {
                    // Format discount and append %
                    decimal discountValue = Convert.ToDecimal(item);
                    text = discountValue % 1 == 0
                        ? discountValue.ToString("0") + " %"
                        : discountValue.ToString("0.#") + " %";
                }
                else if ((col == "Unit Price" || col == "Item Total" || col == "Final Amount") && item != DBNull.Value)
                {
                    // Standard currency formatting with 2 decimals
                    text = decimal.TryParse(item.ToString(), out var val) ? val.ToString("F2") : item.ToString();
                }
                else
                {
                    // Default formatting for other columns
                    text = item?.ToString() ?? "";
                }

                PdfPCell dataCell = new PdfPCell(new Phrase(text, font))
                {
                    Border = PdfPCell.NO_BORDER,
                    Padding = 5f,
                    HorizontalAlignment = col == "Item Name" ? Element.ALIGN_LEFT :
                                         col == "Quantity" ? Element.ALIGN_CENTER : Element.ALIGN_RIGHT
                };
                table.AddCell(dataCell);
            }
        }
      

        private void kryptonDataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the clicked column is the delete button column
            if (e.ColumnIndex == kryptonDataGridView1.Columns["DeleteColumn"].Index && e.RowIndex >= 0)
            {
                DialogResult result = MessageBox.Show(
                    "Delete this row?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        isProcessing = true;
                        kryptonDataGridView1.Rows.RemoveAt(e.RowIndex);
                        CalculateSubTotal();
                        CalculateTotalDiscount();
                        CalculateGrandTotal();
                    }
                    finally
                    {
                        isProcessing = false;
                    }
                }
            }
        }

        private void kryptonLabel5_Click(object sender, EventArgs e)
        {

        }

        private void rdoSales_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSales.Checked)
            {
                // Clear all input fields
                ClearAllInputFields();

                rjComboBox3.SelectedIndex = -1;
                rjComboBox3.Items.Clear();

                LoadItemNames(""); // Load products

                // Update UI visibility for Sales mode
                rjComboBox2.Visible = true;
                txtPartNo.Visible = true;
                lblPartNo.Text = "Part Number";
                lblCategory.Visible = true;
                lblPartNo.Visible = true;
                kryptonButton20.Visible = true;
                txtServiceFee.Visible = false;
                txtSaleUnitPrice.Enabled = true;
                txtPurchaseUnitPrice.Enabled = true;
                txtAvailableQty.Enabled = true;
                txtThresholdQty.Enabled = true;
                txtServiceFee.Clear();

                // Explicitly clear calculated fields (even if they show 0.00)
                txtItemTotal.Text = "";
                txtAmount.Text = "";
                txtDiscount.Text = "";
                txtQuantity.Text = "";
            }
        }


        private void rdoServices_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoServices.Checked)
            {
                // Clear all input fields
                ClearAllInputFields();

                rjComboBox3.SelectedIndex = -1;
                rjComboBox3.Items.Clear();

                LoadServices(); // Load services instead of products

                // Update UI visibility for Services mode
                rjComboBox2.Visible = false;
                lblPartNo.Text = "Service Fee";
                lblCategory.Visible = false;
                kryptonButton20.Visible = false;
                txtPartNo.Enabled = false;
                txtServiceFee.Visible = true;
                txtSaleUnitPrice.Enabled = false;
                txtPurchaseUnitPrice.Enabled = false;
                txtAvailableQty.Enabled = false;
                txtThresholdQty.Enabled = false;

                // Explicitly clear calculated fields (even if they show 0.00)
                txtItemTotal.Text = "";
                txtAmount.Text = "";
                txtDiscount.Text = "";
                txtQuantity.Text = "";
            }
        }

        private void panel2_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void chkFullyPaid_CheckedChanged_1(object sender, EventArgs e)
        {

        }
        private void kryptonButton24_Click(object sender, EventArgs e)
        {
            // Flag to prevent auto-calculation during clearing
            bool isProcessing = true;

            try
            {
                if (rdoServices.Checked)
                {
                    // --- Service Logic ---
                    string serviceName = rjComboBox3.SelectedItem?.ToString();
                    string description = txtDescription.Text.Trim();

                    if (string.IsNullOrEmpty(serviceName))
                    {
                        MessageBox.Show("Please select a service.");
                        return;
                    }

                    // Check for duplicate service
                    foreach (DataGridViewRow row in kryptonDataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            string existingName = row.Cells["itemName"].Value?.ToString().Trim();
                            string existingType = row.Cells["ItemType"].Value?.ToString().Trim();

                            if (string.Equals(existingName, serviceName, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(existingType, "Service", StringComparison.OrdinalIgnoreCase))
                            {
                                MessageBox.Show("This service has already been added.");
                                return;
                            }
                        }
                    }

                    // Validate inputs
                    if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
                    {
                        MessageBox.Show("Quantity must be a positive number.");
                        return;
                    }

                    if (!decimal.TryParse(txtServiceFee.Text, out decimal unitPrice))
                    {
                        MessageBox.Show("Service fee must be a valid number.");
                        return;
                    }

                    // Parse discount
                    decimal discount = 0;
                    if (!string.IsNullOrEmpty(txtDiscount.Text))
                    {
                        string discountText = txtDiscount.Text.Replace("%", "").Trim();
                        if (!decimal.TryParse(discountText, out discount) || discount < 0 || discount > 100)
                        {
                            MessageBox.Show("Discount must be between 0-100%.");
                            return;
                        }
                    }

                    // Calculate and add to grid
                    decimal itemTotal = unitPrice * qty;
                    decimal finalAmount = itemTotal * (1 - discount / 100m);

                    kryptonDataGridView1.Rows.Add(
                       serviceName,
                       qty,
                       unitPrice.ToString("F2"),
                       itemTotal.ToString("F2"),
                       discount.ToString("0.##") + "%",
                       finalAmount.ToString("F2"),
                       "", // Keep empty string if needed for other columns
                       "Service"
                       );
                }
                else
                {
                    // --- Product Logic ---
                    string item = rjComboBox3.SelectedItem?.ToString();
                    string partNo = txtPartNo.Text.Trim();
                    string description = txtDescription.Text.Trim();

                    if (string.IsNullOrEmpty(item) || string.IsNullOrEmpty(partNo))
                    {
                        MessageBox.Show("Please select a valid item and part number.");
                        return;
                    }

                    // Check for duplicate product
                    foreach (DataGridViewRow row in kryptonDataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            string existingName = row.Cells["itemName"].Value?.ToString().Trim();
                            string existingType = row.Cells["ItemType"].Value?.ToString().Trim();
                            string existingPartNo = row.Cells["PartNo"].Value?.ToString().Trim();

                            if (string.Equals(existingName, item, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(existingType, "Product", StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(existingPartNo, partNo, StringComparison.OrdinalIgnoreCase))
                            {
                                MessageBox.Show("This product has already been added.");
                                return;
                            }
                        }
                    }

                    // Validate inputs
                    if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
                    {
                        MessageBox.Show("Quantity must be a positive number.");
                        return;
                    }

                    if (!decimal.TryParse(txtSaleUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
                    {
                        MessageBox.Show("Unit price must be a valid positive number.");
                        return;
                    }

                    // Parse discount
                    decimal discount = 0;
                    if (!string.IsNullOrEmpty(txtDiscount.Text))
                    {
                        string discountText = txtDiscount.Text.Replace("%", "").Trim();
                        if (!decimal.TryParse(discountText, out discount) || discount < 0 || discount > 100)
                        {
                            MessageBox.Show("Discount must be between 0-100%.");
                            return;
                        }
                    }

                    // Calculate and add to grid
                    decimal itemTotal = qty * unitPrice;
                    decimal finalAmount = itemTotal * (1 - discount / 100m);

                    kryptonDataGridView1.Rows.Add(
                        item,
                        qty,
                        unitPrice.ToString("F2"),
                        itemTotal.ToString("F2"),
                        discount.ToString("0.##") + "%",
                        finalAmount.ToString("F2"),
                        partNo,
                        "Product"
                    );
                }

                // Recalculate and clear
                CalculateSubTotal();
                CalculateTotalDiscount();
                CalculateGrandTotal();
                ClearAllInputFields();
            }
            finally
            {
                isProcessing = false;
            }
        }

        private void ClearAllInputFields()
        {
            // Temporarily disable event handlers
            txtQuantity.TextChanged -= txtQuantity_TextChanged;
            txtServiceFee.TextChanged -= txtServiceFee_TextChanged;
            txtDiscount.TextChanged -= txtDiscount_TextChanged;

            // Clear ALL fields
            txtQuantity.Clear();
            txtDiscount.Clear();
           
            txtItemTotal.Clear();
            txtAmount.Clear();
            txtServiceFee.Clear();
            txtSaleUnitPrice.Clear();
            txtPartNo.Clear();
            txtPurchaseUnitPrice.Clear();
            txtAvailableQty.Clear();
            txtThresholdQty.Clear();

            // Reset comboboxes
            rjComboBox3.SelectedIndex = -1;
            rjComboBox2.SelectedIndex = -1;

            // Re-enable event handlers
            txtQuantity.TextChanged += txtQuantity_TextChanged;
            txtServiceFee.TextChanged += txtServiceFee_TextChanged;
            txtDiscount.TextChanged += txtDiscount_TextChanged;

            // Set focus appropriately
            if (rdoServices.Checked)
            {
                rjComboBox3.Focus();
            }
            else
            {
                
            }
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            if (isProcessing) return;
            CalculateItemTotalAndFinalAmount();
        }

        private bool isFormattingServiceFee = false; // Add this class-level variable

        private void txtServiceFee_TextChanged(object sender, EventArgs e)
        {
            if (isFormattingServiceFee || isProcessing) return;

            string text = txtServiceFee.Text;
            int cursorPos = txtServiceFee.SelectionStart;

            // Skip if empty or already has decimal places
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    isFormattingServiceFee = true;

                    // Remove any non-numeric characters except decimal point
                    string cleanText = new string(text.Where(c => char.IsDigit(c) || c == '.').ToArray());

                    // If there's no decimal point, we'll add it
                    bool needsDecimalFormat = !cleanText.Contains('.');

                    if (decimal.TryParse(cleanText, out decimal value))
                    {
                        // Format with 2 decimal places
                        string newText = needsDecimalFormat ? value.ToString("0.00") : value.ToString("F2");

                        // Only update if the formatted text is different
                        if (newText != text)
                        {
                            txtServiceFee.Text = newText;

                            // Adjust cursor position
                            if (needsDecimalFormat)
                            {
                                // When adding .00, place cursor before the decimal
                                txtServiceFee.SelectionStart = Math.Min(cursorPos, txtServiceFee.Text.Length - 3);
                            }
                            else
                            {
                                // Normal adjustment for other cases
                                int newPos = cursorPos + (newText.Length - text.Length);
                                txtServiceFee.SelectionStart = Math.Max(0, Math.Min(newPos, txtServiceFee.Text.Length));
                            }
                        }
                    }
                    else if (string.IsNullOrEmpty(cleanText))
                    {
                        txtServiceFee.Text = "";
                    }
                }
                finally
                {
                    isFormattingServiceFee = false;
                }
            }

            CalculateItemTotalAndFinalAmount();
        }

        private void txtServiceFee_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow control keys (backspace, delete, etc.)
            if (char.IsControl(e.KeyChar)) return;

            // Allow only one decimal point
            if (e.KeyChar == '.' && txtServiceFee.Text.Contains('.'))
            {
                e.Handled = true;
                return;
            }

            // Allow only digits and decimal point
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }
        }

        private void txtServiceFee_KeyDown(object sender, KeyEventArgs e)
        {
            // Allow full selection deletion
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                if (txtServiceFee.SelectionLength == txtServiceFee.Text.Length)
                {
                    txtServiceFee.Text = "";
                    e.Handled = true;
                }
            }

            // Handle paste operation
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Let the paste occur, then TextChanged will handle formatting
                return;
            }
        }
        private void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            if (isProcessing) return;

            // Get current text and cursor position
            string currentText = txtDiscount.Text;
            int cursorPos = txtDiscount.SelectionStart;

            // Only process if not empty and doesn't already end with %
            if (!string.IsNullOrEmpty(currentText) && !currentText.EndsWith("%"))
            {
                // Remove any existing % signs to prevent duplicates
                string cleanText = currentText.Replace("%", "");

                // Add % at the end
                txtDiscount.Text = cleanText + "%";

                // Restore cursor position (adjust if we removed characters)
                txtDiscount.SelectionStart = cursorPos <= cleanText.Length ? cursorPos : cleanText.Length;
            }

            CalculateItemTotalAndFinalAmount();
        }


        private void kryptonDataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridViewCell cell = kryptonDataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            cell.Tag = cell.Style.BackColor; // Store original color

            // Highlight only the edited cell
            cell.Style.BackColor = Color.LightBlue;

            if (e.ColumnIndex == 2) // Unit Price column
            {
                string itemType = kryptonDataGridView1.Rows[e.RowIndex].Cells[7].Value?.ToString();

                // Allow editing only for Services
                if (itemType != "Service")
                {
                    e.Cancel = true;
                    ShowErrorMessage("Unit price can only be edited for Services");
                }
            }
        }


        private void kryptonDataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = kryptonDataGridView1.Rows[e.RowIndex];
            DataGridViewCell cell = row.Cells[e.ColumnIndex];

            // Restore original background color
            if (cell.Tag is Color originalColor)
                cell.Style.BackColor = originalColor;
            else
                cell.Style.BackColor = kryptonDataGridView1.DefaultCellStyle.BackColor;

            if (row.IsNewRow) return;

            string itemType = row.Cells[7].Value?.ToString();

            // Only validate Quantity (1), Unit Price (2), or Discount (4)
            if (e.ColumnIndex == 1 || e.ColumnIndex == 2 || e.ColumnIndex == 4)
            {
                string qtyStr = row.Cells[1].Value?.ToString();
                string priceStr = row.Cells[2].Value?.ToString();
                string discountStr = row.Cells[4].Value?.ToString().Replace("%", "").Trim();

                bool validQty = int.TryParse(qtyStr, out int qty) && qty > 0;
                bool validPrice = decimal.TryParse(priceStr, out decimal price) && price >= 0;
                bool validDiscount = decimal.TryParse(discountStr, out decimal discount) && discount >= 0 && discount <= 100;

                if (!validQty || !validPrice || !validDiscount)
                {
                    ShowErrorMessage("Enter valid values: Quantity > 0, Discount 0–100%, Unit Price ≥ 0.");
                    return;
                }

                // Recalculate
                decimal itemTotal = qty * price;
                decimal finalAmount = itemTotal * (1 - discount / 100m);

                row.Cells[3].Value = itemTotal.ToString("F2");             // Item Total
                row.Cells[4].Value = discount.ToString("0.##") + "%";      // Discount %
                row.Cells[5].Value = finalAmount.ToString("F2");           // Final Amount

                // Refresh totals
                CalculateSubTotal();
                CalculateTotalDiscount();
                CalculateGrandTotal();

                // ✅ Show update success message
                string message = itemType == "Service" ? "Service updated successfully!" : "Product updated successfully!";
                ShowSuccessMessage(message);
            }
        }

    

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void kryptonButton10_Click(object sender, EventArgs e)
        {

        }

        private void kryptonButton18_Click(object sender, EventArgs e)
        {

        }

        private void txtSaleUnitPrice_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtItemTotal_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
