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
    public partial class NewPurchase: SmoothRoundedForm
    {
        private Dictionary<string, int> supplierDictionary = new Dictionary<string, int>();


        string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";

        private DataTable productDataTable;

        public NewPurchase()
        {
            InitializeComponent();
            LoadSupplierNames();
            LoadCategoryNames();


           
        }

        private void NewPurchase_Load(object sender, EventArgs e)
        {

            LoadSupplierNames();
            LoadCategoryNames();




        }

        private void LoadItemNames(string selectedCategory)
        {
            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = @"SELECT p.ProductName 
                     FROM Product p
                     INNER JOIN Category c ON p.Category_ID = c.CategoryID
                     WHERE c.CategoryName = @CategoryName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                    SqlDataReader reader = cmd.ExecuteReader();

                    comboBoxItemName.Items.Clear();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string productName = reader["ProductName"].ToString();
                            comboBoxItemName.Items.Add(productName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No products found for this category.");
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
            string query = "SELECT CategoryID, CategoryName FROM Category";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBoxCategory.DisplayMember = "CategoryName";
                    comboBoxCategory.ValueMember = "CategoryID";
                    comboBoxCategory.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading categories: " + ex.Message);
                }
                comboBoxCategory.SelectedIndex = -1;
            }
        }
        private void LoadSupplierNames()
        {
            supplierDictionary.Clear(); // Make sure it's fresh each time

            string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";
            string query = "SELECT SupplierID, SupplierName FROM Supplier";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    comboBoxSupplier.Items.Clear();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string supplierName = reader["SupplierName"].ToString();
                            int supplierId = Convert.ToInt32(reader["SupplierID"]);

                            comboBoxSupplier.Items.Add(supplierName);
                            supplierDictionary[supplierName] = supplierId; // 🔑 Important!
                        }
                    }
                    else
                    {
                        MessageBox.Show("No Supplier found in the database.");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading suppliers: " + ex.Message);
                }
            }
        }



        // Calculate Total Amount
        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (row.Cells[3].Value != null)
                {
                    total += Convert.ToDecimal(row.Cells[3].Value);
                }
            }
            txtAmount.Text = total.ToString("0.00");
        }



        private void ClearNewPurchaseForm()
        {
            comboBoxSupplier.SelectedIndex = -1;
            comboBoxCategory.SelectedIndex = -1;
            comboBoxItemName.SelectedIndex = -1;


            txtPartNo.Clear();
            txtQuantity.Clear();
            txtDescription.Clear();

            txtDiscount.Clear();
            txtItemTotal.Clear();
            txtAmount.Clear();
            txtSubTotal.Clear();
            txtTotalDiscount.Clear();
            txtGrandTotal.Clear();
            txtAmountPaid.Clear();
            txtOutstanding.Clear();
            txtStock.Clear();
            txtThresholdQty.Clear();
            txtSaleUnitPrice.Clear();
            txtPurchaseUnitPrice.Clear();


            dgvCart.Rows.Clear(); // Clears all rows from the DataGridView

            chkFullyPaid.Checked = false;
        }

        private void LoadProductDetails(string productName)
        {
            string query = "SELECT ProductID, SaleUnitPrice, Quantity, PurchaseUnitPrice, ThresholdQty FROM Product WHERE ProductName = @ProductName";
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
                        txtPartNo.Text = reader["ProductID"].ToString();
                        txtSaleUnitPrice.Text = reader["SaleUnitPrice"].ToString();
                        txtStock.Text = reader["Quantity"].ToString();
                        txtPurchaseUnitPrice.Text = reader["PurchaseUnitPrice"].ToString();
                        txtThresholdQty.Text = reader["ThresholdQty"].ToString();

                        // Recalculate item total if quantity is already entered
                        CalculateItemTotal(); // Safer than manually calling a TextChanged event
                    }
                    else
                    {
                        // Clear all fields if no data found
                        txtPartNo.Text = "";
                        txtSaleUnitPrice.Text = "";
                        txtStock.Text = "";
                        txtPurchaseUnitPrice.Text = "";
                        txtThresholdQty.Text = "";
                        txtItemTotal.Text = "";

                        MessageBox.Show("Product details not found.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading product details: " + ex.Message);
                }
            }
        }

        private void CalculateItemTotal()
        {
            if (decimal.TryParse(txtPurchaseUnitPrice.Text.Trim(), out decimal unitPrice) &&
                int.TryParse(txtQuantity.Text.Trim(), out int qty))
            {
                decimal total = unitPrice * qty;
                txtItemTotal.Text = total.ToString("0.00");
            }
            else
            {
                txtItemTotal.Text = "0.00";
            }
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            CalculateItemTotal();
        }
        private void kryptonButton14_Click_1(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Create an instance of the InvoiceForm
            Purchase_Payment invoiceForm = new Purchase_Payment();

            // Access panel1 from InvoiceForm and add it to panel1 in MainForm
            panel1.Controls.Add(invoiceForm.panel1);

            // Access panel2 from InvoiceForm and add it to panel2 in MainForm
            panel2.Controls.Add(invoiceForm.panel2);

            // Optionally, make sure the controls from InvoiceForm are correctly docked or sized within the MainForm panels
            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void chkFullyPaid_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the "Fully Paid" checkbox is currently checked
            if (chkFullyPaid.Checked)
            {
                // Attempt to parse the text from txtGrandTotal into a double (or decimal, if you prefer)
                if (double.TryParse(txtGrandTotal.Text, out double grandTotal))
                {
                    // Temporarily remove the TextChanged event handler for txtAmountPaid.
                    // This prevents an infinite loop or unnecessary recalculations when you programmatically set txtAmountPaid.Text.
                    txtAmountPaid.TextChanged -= txtAmountPaid_TextChanged;

                    // Set the txtAmountPaid textbox to the value of grandTotal, formatted to two decimal places.
                    txtAmountPaid.Text = grandTotal.ToString("F2");

                    // Set the txtOutstanding textbox to "0.00" directly, as the balance is fully paid.
                    txtOutstanding.Text = "0.00";

                    // Re-add the TextChanged event handler for txtAmountPaid.
                    // This ensures that manual changes to txtAmountPaid still trigger the outstanding balance calculation later.
                    txtAmountPaid.TextChanged += txtAmountPaid_TextChanged;
                }
            }
            else // If the "Fully Paid" checkbox is unchecked
            {
                // Clear the txtAmountPaid textbox
                txtAmountPaid.Text = "";
                // Clear the txtOutstanding textbox
                txtOutstanding.Text = "";
            }
        }

        private void txtAmountPaid_TextChanged(object sender, EventArgs e)
        {
            CalculateOutstanding();
        }

        private void txtGrandTotal_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtTotalDiscount_TextChanged(object sender, EventArgs e)
        {
            CalculateOutstanding();
        }

        private void btnAddSupplier_Click(object sender, EventArgs e)
        {
            var result = ShowSupplierInputDialog();

            if (result != null && !string.IsNullOrWhiteSpace(result["Name"]))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Supplier (SupplierName, ContactNo, Address, Description, Updated_By, CreatedAt) " +
                                   "VALUES (@name, @contact, @address, @description, @updatedBy, @createdAt)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", result["Name"]);
                    cmd.Parameters.AddWithValue("@contact", result["ContactNo"]);
                    cmd.Parameters.AddWithValue("@address", result["Address"]);
                    cmd.Parameters.AddWithValue("@description", result["Description"]);
                    cmd.Parameters.AddWithValue("@updatedBy", 1); // replace with actual user ID
                    cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Supplier added successfully!");
                LoadSupplierNames(); // refresh ComboBox
            }
        }

        public static Dictionary<string, string> ShowSupplierInputDialog()
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 300,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Add New Supplier",
                StartPosition = FormStartPosition.CenterScreen
            };

            // Create Labels & TextBoxes
            Label lblName = new Label() { Text = "Name", Left = 20, Top = 20 };
            TextBox txtName = new TextBox() { Left = 120, Top = 20, Width = 230 };

            Label lblContact = new Label() { Text = "Contact No", Left = 20, Top = 60 };
            TextBox txtContact = new TextBox() { Left = 120, Top = 60, Width = 230 };

            Label lblAddress = new Label() { Text = "Address", Left = 20, Top = 100 };
            TextBox txtAddress = new TextBox() { Left = 120, Top = 100, Width = 230 };

            Label lblDescription = new Label() { Text = "Description", Left = 20, Top = 140 };
            TextBox txtDescription = new TextBox() { Left = 120, Top = 140, Width = 230 };

            Button btnOK = new Button() { Text = "Add", Left = 260, Width = 90, Top = 190, DialogResult = DialogResult.OK };
            btnOK.Click += (sender, e) => { prompt.Close(); };

            // Add controls
            prompt.Controls.Add(lblName); prompt.Controls.Add(txtName);
            prompt.Controls.Add(lblContact); prompt.Controls.Add(txtContact);
            prompt.Controls.Add(lblAddress); prompt.Controls.Add(txtAddress);
            prompt.Controls.Add(lblDescription); prompt.Controls.Add(txtDescription);
            prompt.Controls.Add(btnOK);
            prompt.AcceptButton = btnOK;

            // Show dialog and return results
            if (prompt.ShowDialog() == DialogResult.OK)
            {
                return new Dictionary<string, string>
        {
            { "Name", txtName.Text.Trim() },
            { "ContactNo", txtContact.Text.Trim() },
            { "Address", txtAddress.Text.Trim() },
            { "Description", txtDescription.Text.Trim() }
        };
            }
            return null;
        }

        private void comboBoxItemName_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            // Ensure an item is actually selected
            if (comboBoxItemName.SelectedItem != null)
            {
                string selectedProductName = comboBoxItemName.SelectedItem.ToString();

                // Add a check to prevent calling LoadProductDetails with an empty string
                if (!string.IsNullOrWhiteSpace(selectedProductName))
                {
                    // Debugging line: Check what product name is being passed
                    System.Diagnostics.Debug.WriteLine($"Selected Product Name: '{selectedProductName}'");

                    // Directly call LoadProductDetails to get the full product info from the DB
                    LoadProductDetails(selectedProductName);
                }
                else
                {
                    // Clear textboxes if for some reason an empty product name is selected
                    ClearProductDetailsTextBoxes();
                }
            }
            else
            {
                // No item selected, clear the textboxes
                ClearProductDetailsTextBoxes();
            }
        }

        private void ClearProductDetailsTextBoxes()
        {
            txtPartNo.Text = "";
            txtSaleUnitPrice.Text = "";
            txtStock.Text = "";
            txtPurchaseUnitPrice.Text = "";
            txtThresholdQty.Text = "";
        }

        private void CalculateTotalDiscount()
        {
            decimal totalDiscountAmount = 0;

            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (row.IsNewRow) continue;

                // Need Quantity (Index 1), Unit Price (Index 2), and Discount Percentage (Index 4)
                int quantityColumnIndex = 1;
                int unitPriceColumnIndex = 2;
                int discountPercentColumnIndex = 4; // The 'Description' column

                if (row.Cells.Count > discountPercentColumnIndex && // Ensure Discount column exists
                    row.Cells[quantityColumnIndex].Value != null &&
                    row.Cells[unitPriceColumnIndex].Value != null &&
                    row.Cells[discountPercentColumnIndex].Value != null)
                {
                    int qty = 0;
                    decimal unitPrice = 0;
                    decimal discountPercent = 0;

                    string qtyString = row.Cells[quantityColumnIndex].Value.ToString().Trim();
                    string unitPriceString = row.Cells[unitPriceColumnIndex].Value.ToString().Trim();
                    string discountPercentString = row.Cells[discountPercentColumnIndex].Value.ToString().Replace("%", "").Trim(); // Remove '%'

                    if (int.TryParse(qtyString, out qty) &&
                        decimal.TryParse(unitPriceString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out unitPrice) &&
                        decimal.TryParse(discountPercentString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out discountPercent))
                    {
                        // Recalculate the gross amount for this line item
                        decimal itemTotalGross = qty * unitPrice;

                        // Calculate and add this row's actual discount amount to the total
                        totalDiscountAmount += itemTotalGross * (discountPercent / 100M);
                    }
                    // else { System.Diagnostics.Debug.WriteLine($"Failed to parse data for discount calc. Qty: '{qtyString}', Price: '{unitPriceString}', Discount: '{discountPercentString}'"); }
                }
            }
            txtTotalDiscount.Text = totalDiscountAmount.ToString("F2");
        }

        private void CalculateOutstanding()
        {
            decimal grandTotal = 0;
            decimal amountPaid = 0;

            // Safely parse Grand Total
            if (!decimal.TryParse(txtGrandTotal.Text, out grandTotal))
            {
                grandTotal = 0; // Default to 0 if parsing fails
            }

            // Safely parse Amount Paid
            if (!decimal.TryParse(txtAmountPaid.Text, out amountPaid))
            {
                amountPaid = 0; // Default to 0 if parsing fails or textbox is empty
            }

            decimal outstanding = grandTotal - amountPaid;
            txtOutstanding.Text = outstanding.ToString("F2");

        }



        private void CalculateSubTotal()
        {
            decimal subTotal = 0;

            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (row.IsNewRow) continue; // Skip the empty new row at the bottom

                // Need Quantity (Index 1) and Unit Price (Index 2) from DGV to recalculate gross amount
                int quantityColumnIndex = 1;
                int unitPriceColumnIndex = 2;

                if (row.Cells.Count > unitPriceColumnIndex && // Ensure UnitPrice column exists
                    row.Cells[quantityColumnIndex].Value != null &&
                    row.Cells[unitPriceColumnIndex].Value != null)
                {
                    int qty = 0;
                    decimal unitPrice = 0;

                    string qtyString = row.Cells[quantityColumnIndex].Value.ToString().Trim();
                    string unitPriceString = row.Cells[unitPriceColumnIndex].Value.ToString().Trim();

                    if (int.TryParse(qtyString, out qty) &&
                        decimal.TryParse(unitPriceString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out unitPrice))
                    {
                        subTotal += (qty * unitPrice); // Add the PRE-DISCOUNT amount to SubTotal
                    }
                    // else { System.Diagnostics.Debug.WriteLine($"Failed to parse Qty or UnitPrice for SubTotal: Qty='{qtyString}', Price='{unitPriceString}'"); }
                }
            }
            txtSubTotal.Text = subTotal.ToString("F2");
        }


        private void CalculateFinalAmount()
        {
            decimal itemTotal = 0;
            decimal discountPercent = 0;

            // 1. Get Item Total
            if (!decimal.TryParse(txtItemTotal.Text, out itemTotal))
            {
                txtAmount.Text = ""; // If Item Total is invalid, Amount should be clear
                return; // Stop here if we can't get a valid Item Total
            }

            // 2. Get Discount Percentage
            string discountText = txtDiscount.Text.Replace("%", "").Trim(); // Removes '%'
            if (!decimal.TryParse(discountText, out discountPercent))
            {
                discountPercent = 0; // If discount input is invalid, treat as 0%
            }

            // 3. Calculate the actual Discount Amount (e.g., if ItemTotal=100 and DiscountPercent=10, DiscountAmount=10)
            decimal discountAmount = itemTotal * (discountPercent / 100);

            // 4. Calculate the Final Amount (Item Total minus the calculated Discount Amount)
            decimal finalAmount = itemTotal - discountAmount;

            // 5. Handle potential negative amounts (if discount is somehow too large)
            if (finalAmount < 0)
            {
                MessageBox.Show("Discount cannot be greater than item total.");
                txtAmount.Text = "0.00";
                return;
            }

            // 6. Display the result
            txtAmount.Text = finalAmount.ToString("0.00");

        }


        



        private void chkFullyPaid_CheckedChanged_1(object sender, EventArgs e)
        {

        }

      

        private void btnPrint_Click(object sender, EventArgs e)
        {

        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Font headerFont = new Font("Arial", 14, FontStyle.Bold);
            Font contentFont = new Font("Arial", 10);
            int y = 100;

            e.Graphics.DrawString("Purchase Order Receipt", headerFont, Brushes.Black, new Point(100, y));
            y += 40;

            e.Graphics.DrawString($"Supplier: {comboBoxSupplier.Text}", contentFont, Brushes.Black, new Point(100, y));
            y += 25;
            e.Graphics.DrawString($"Date: {DateTime.Now}", contentFont, Brushes.Black, new Point(100, y));
            y += 30;

            e.Graphics.DrawString("Items:", contentFont, Brushes.Black, new Point(100, y));
            y += 25;

            // Table Header
            e.Graphics.DrawString("Item", contentFont, Brushes.Black, new Point(100, y));
            e.Graphics.DrawString("Qty", contentFont, Brushes.Black, new Point(250, y));
            e.Graphics.DrawString("Price", contentFont, Brushes.Black, new Point(300, y));
            e.Graphics.DrawString("Amount", contentFont, Brushes.Black, new Point(380, y));
            y += 20;

            // Table Body
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (row.Cells["Item Name"].Value != null)
                {
                    e.Graphics.DrawString(row.Cells["Item Name"].Value.ToString(), contentFont, Brushes.Black, new Point(100, y));
                    e.Graphics.DrawString(row.Cells["Quantity"].Value.ToString(), contentFont, Brushes.Black, new Point(250, y));
                    e.Graphics.DrawString(row.Cells["Unit Price"].Value.ToString(), contentFont, Brushes.Black, new Point(300, y));
                    e.Graphics.DrawString(row.Cells["Amount"].Value.ToString(), contentFont, Brushes.Black, new Point(380, y));
                    y += 20;
                }
            }

            y += 30;
            e.Graphics.DrawString($"Total: {txtAmount.Text}", contentFont, Brushes.Black, new Point(100, y));
            y += 20;
            e.Graphics.DrawString($"Paid: {txtAmountPaid.Text}", contentFont, Brushes.Black, new Point(100, y));
            y += 20;
            e.Graphics.DrawString($"Outstanding: {txtOutstanding.Text}", contentFont, Brushes.Black, new Point(100, y));
        }


       

        public static string ShowInputDialog(string title, string prompt)
        {
            Form promptForm = new Form()
            {
                Width = 450,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Text = prompt, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 360 };
            Button confirmation = new Button() { Text = "Add", Left = 270, Width = 90, Top = 80, DialogResult = DialogResult.OK };

            confirmation.Click += (sender, e) => { promptForm.Close(); };

            promptForm.Controls.Add(textLabel);
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(confirmation);
            promptForm.AcceptButton = confirmation;

            return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : null;
        }

        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            string categoryName = ShowInputDialog("Add New Category", "Enter Category Name:");

            if (!string.IsNullOrEmpty(categoryName))
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Category (CategoryName, Updated_By, CreatedAt) VALUES (@name, @updatedBy, @createdAt)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@name", categoryName);
                    cmd.Parameters.AddWithValue("@updatedBy", 1);
                    cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Category added successfully!");

                // Reload categories into ComboBox
                LoadCategoryNames();

            }
        }

        private void txtTotalDiscount_TextChanged_1(object sender, EventArgs e)
        {
            CalculateGrandTotal();
        }

        private void CalculateGrandTotal()
        {
            decimal subTotal = 0;
            decimal totalDiscount = 0;

            // Safely parse values from textboxes (which were updated by previous calculations)
            if (!decimal.TryParse(txtSubTotal.Text, out subTotal)) { subTotal = 0; }
            if (!decimal.TryParse(txtTotalDiscount.Text, out totalDiscount)) { totalDiscount = 0; }

            // Grand Total is now calculated as Gross SubTotal minus Total Discounts
            decimal grandTotal = subTotal - totalDiscount;

            txtGrandTotal.Text = grandTotal.ToString("F2");
        }

        private void txtGrandTotal_TextChanged_1(object sender, EventArgs e)
        {
            CalculateOutstanding(); // Recalculate outstanding when grand total changes
        }




        private bool isUpdating = false; // Flag to prevent re-entrancy

        private void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating) return; // Prevents the event from firing recursively

            isUpdating = true; // Set the flag to indicate we're programmatically changing the text

            string text = txtDiscount.Text;

            // 1. Remove existing '%' if present, so we're working with just the number
            if (text.EndsWith("%"))
                text = text.Substring(0, text.Length - 1);

            // 2. Filter out non-digit characters (except for the potential decimal point if you allow it,
            //    but your KeyPress suggests only digits for now)
            text = new string(text.Where(c => char.IsDigit(c)).ToArray());

            // 3. Handle empty input after filtering
            if (text == "")
            {
                txtDiscount.Text = ""; // Clear the textbox
                isUpdating = false;
                CalculateFinalAmount(); // Call calculation to reflect no discount
                return;
            }

            // 4. Re-add the '%' symbol to the end of the filtered number
            txtDiscount.Text = text + "%";

            // 5. Place the cursor before the '%' for a better user experience
            // This is important because changing .Text moves the cursor to the beginning
            // Ensure text length is > 0 before trying to set SelectionStart
            if (txtDiscount.Text.Length > 0) // Should always be true here if 'text' is not empty
            {
                txtDiscount.SelectionStart = txtDiscount.Text.Length - 1;
            }

            isUpdating = false; // Reset the flag

            // 6. Finally, trigger the calculation of the final amount
            CalculateFinalAmount();
        }
        private void txtDiscount_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits and control keys (e.g., backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Block non-digit input
            }
        }


        // Declare this at the class level (outside of any method):
        private int _currentSelectedSupplierId = -1; // Initialize with a default invalid ID


        private void comboBoxSupplier_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedName = comboBoxSupplier.SelectedItem?.ToString().Trim();

            if (supplierDictionary.ContainsKey(selectedName))
            {
                _currentSelectedSupplierId = supplierDictionary[selectedName];
            }
            else
            {
                _currentSelectedSupplierId = -1; // Reset if no valid supplier is selected (e.g., cleared selection)
            }
        }

        private void comboBoxCategory_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            // Make sure an item is actually selected and not null
            if (comboBoxCategory.SelectedItem != null)
            {
                // Get the DataRowView from the selected item
                DataRowView selectedRow = comboBoxCategory.SelectedItem as DataRowView;

                // Ensure it's a DataRowView before proceeding
                if (selectedRow != null)
                {
                    // Get the value of the "CategoryName" column (your DisplayMember)
                    string selectedCategory = selectedRow["CategoryName"].ToString();

                    // *** Crucial Debugging Step: Verify the value here ***
                    System.Diagnostics.Debug.WriteLine($"Category selected in C#: '{selectedCategory}'");

                    // Now call your method with the correct category name
                    LoadItemNames(selectedCategory);
                }
                else
                {
                    // Handle case where SelectedItem is not a DataRowView (unlikely if DataSource is DataTable)
                    System.Diagnostics.Debug.WriteLine("Warning: comboBoxCategory.SelectedItem is not a DataRowView.");
                    comboBoxItemName.Items.Clear();
                }
            }
            else
            {
                // No item selected, clear the product names
                comboBoxItemName.Items.Clear();
                System.Diagnostics.Debug.WriteLine("No item selected in comboBoxCategory.");
            }
        }

   
        private void btnClear_Click(object sender, EventArgs e)
        {
            comboBoxSupplier.SelectedIndex = -1;
            comboBoxCategory.SelectedIndex = -1;
            comboBoxItemName.SelectedIndex = -1;


            txtPartNo.Clear();
            txtQuantity.Clear();
            txtDescription.Clear();

            txtDiscount.Clear();
            txtItemTotal.Clear();
            txtAmount.Clear();
            txtSubTotal.Clear();
            txtTotalDiscount.Clear();
            txtGrandTotal.Clear();
            txtAmountPaid.Clear();
            txtOutstanding.Clear();
            txtStock.Clear();
            txtThresholdQty.Clear();
            txtSaleUnitPrice.Clear();
            txtPurchaseUnitPrice.Clear();


            dgvCart.Rows.Clear(); // Clears all rows from the DataGridView

            chkFullyPaid.Checked = false;
        }
    

       

     

        private void kryptonButton12_Click(object sender, EventArgs e)
        {
            
        }

        private void kryptonButton24_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void kryptonButton22_Click(object sender, EventArgs e)
        {

        }

        

        

       

        

        private void txtQuantity_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            // --- Input Validation ---
            string itemName = comboBoxItemName.SelectedItem?.ToString();
            string qtyText = txtQuantity.Text.Trim();
            string discountText = txtDiscount.Text.Trim();
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(itemName))
            {
                MessageBox.Show("Please select an item.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(qtyText, out int qty))
            {
                MessageBox.Show("Quantity must be a valid integer.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(txtSaleUnitPrice.Text, out decimal unitPrice))
            {
                MessageBox.Show("Unit price is not valid.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal discountPercent = 0;
            if (!string.IsNullOrEmpty(discountText))
            {
                if (!decimal.TryParse(discountText.Replace("%", "").Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out discountPercent))
                {
                    MessageBox.Show("Discount must be a valid number (e.g., 5 or 5%).", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // --- Get Product ID (PartNo) ---
            string productID = GetProductIDByName(itemName);
            if (string.IsNullOrEmpty(productID))
            {
                MessageBox.Show("Could not find Product ID for selected item.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- Calculations ---
            decimal itemTotalGross = qty * unitPrice;
            decimal discountAmount = itemTotalGross * (discountPercent / 100M);
            decimal finalAmount = itemTotalGross - discountAmount;

            if (finalAmount < 0)
            {
                MessageBox.Show("Discount cannot result in a negative item amount. Please adjust.", "Discount Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- Add to DataGridView (8 columns) ---
            dgvCart.Rows.Add(
                itemName,                                // 0: Item name
                qty,                                     // 1: Quantity
                unitPrice.ToString("F2"),                // 2: Unit Price
                itemTotalGross.ToString("F2"),           // 3: Gross total
                discountPercent.ToString("0.##") + "%",  // 4: Discount
                finalAmount.ToString("F2"),              // 5: Final amount
                description,                             // 6: Description
                productID                                // 7: Part No (Product ID)
            );

            // --- Display in textboxes ---
            txtItemTotal.Text = itemTotalGross.ToString("F2");
            txtAmount.Text = finalAmount.ToString("F2");

            // --- Update Totals ---
            CalculateSubTotal();
            CalculateTotalDiscount();
            CalculateGrandTotal();
            CalculateOutstanding();
        }

        private string GetProductIDByName(string productName)
        {
            string productId = null;
            string query = "SELECT ProductID FROM Product WHERE ProductName = @ProductName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProductName", productName);
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    productId = result.ToString();
                }
            }

            return productId;
        }
        private void btnSave_Click_1(object sender, EventArgs e)
        {

            // --- 1. Validate and Parse Main Totals from TextBoxes ---
            decimal grandTotal;
            decimal subTotal;
            decimal totalDiscount;

            if (!decimal.TryParse(txtGrandTotal.Text, out grandTotal))
            {
                MessageBox.Show("Grand Total is not a valid number. Please ensure items are added and totals calculated.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!decimal.TryParse(txtSubTotal.Text, out subTotal))
            {
                MessageBox.Show("Sub Total is not a valid number. Please ensure items are added and totals calculated.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!decimal.TryParse(txtTotalDiscount.Text, out totalDiscount))
            {
                MessageBox.Show("Total Discount is not a valid number. Please ensure items are added and totals calculated.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string description = txtDescription.Text.Trim();

            // Get selected SupplierName
            string supplierName = comboBoxSupplier.SelectedItem?.ToString();


            // --- 3. Establish Database Connection and Transaction ---
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction(); // Start a transaction for atomicity

                try
                {
                    // --- 4. Insert into PurchaseOrder (Main Order Header) ---
                    string insertPurchaseOrderQuery = @"
                INSERT INTO PurchaseOrder (Supplier_ID, Updated_By, SubTotal, TotalDiscount, GrandTotal, Description)
                VALUES (@Supplier_ID, @Updated_By, @SubTotal, @TotalDiscount, @GrandTotal, @Description);
                SELECT SCOPE_IDENTITY();"; // Get the newly generated PurchaseOrderID

                    SqlCommand cmdInsertOrder = new SqlCommand(insertPurchaseOrderQuery, con, transaction);

                    string selectedSupplierName = comboBoxSupplier.SelectedItem?.ToString().Trim();

                    if (string.IsNullOrEmpty(selectedSupplierName) || !supplierDictionary.ContainsKey(selectedSupplierName))
                    {
                        MessageBox.Show("Please select a valid Supplier from the dropdown list.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        transaction.Rollback(); // Rollback before returning on validation failure
                        return;
                    }

                    int selectedSupplier_ID = supplierDictionary[selectedSupplierName];

                    cmdInsertOrder.Parameters.AddWithValue("@Supplier_ID", selectedSupplier_ID);
                    cmdInsertOrder.Parameters.AddWithValue("@Updated_By", 1); // TODO: **IMPORTANT** Replace '1' with the actual logged-in user's ID
                    cmdInsertOrder.Parameters.AddWithValue("@SubTotal", subTotal);
                    cmdInsertOrder.Parameters.AddWithValue("@TotalDiscount", totalDiscount);
                    cmdInsertOrder.Parameters.AddWithValue("@GrandTotal", grandTotal);
                    // Use DBNull.Value for nullable string fields if they are empty
                    cmdInsertOrder.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);

                    // ExecuteScalar retrieves the single value generated by SCOPE_IDENTITY()
                    int purchaseOrderID = Convert.ToInt32(cmdInsertOrder.ExecuteScalar());


                    // --- 5. Insert into PurchaseOrderDetails and Update Product Stock for each item in the DGV ---
                    string insertPurchaseOrderDetailsQuery = @"
                INSERT INTO PurchaseOrderDetail (PurchaseOrder_ID, Product_ID, Quantity, DiscountPercent)
                VALUES (@PurchaseOrder_ID, @Product_ID, @Quantity, @DiscountPercent)";

                    // For a purchase order, stock typically INCREASES
                    string updateProductStockQuery = "UPDATE Product SET Quantity = Quantity + @Quantity WHERE ProductID = @ProductID";

                    foreach (DataGridViewRow row in dgvCart.Rows)
                    {
                        if (row.IsNewRow) continue; // Skip the empty row at the bottom of the DGV

                        // --- Access DGV Cell Values Safely ---
                        // IMPORTANT: You need to ensure 'PartNo' column exists in your DGV (even if hidden)
                        // and holds the ProductID. If not, adjust how ProductID is retrieved.
                        // For example, if ItemName is unique, you might look up ProductID from it,
                        // or store ProductID in a hidden DGV column or in the row's Tag property.
                        object partNoCell = row.Cells["PartNo"].Value; // Access by column name
                        if (partNoCell == null) // Check if the cell value itself is null
                        {
                            MessageBox.Show($"Product ID (Part No) missing for an item in row {row.Index + 1}. Cannot save.", "Data Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            transaction.Rollback();
                            return;
                        }
                        string productID = partNoCell.ToString(); // Assuming ProductID is stored as a string (PartNo)

                        int qty;
                        if (!int.TryParse(row.Cells[1].Value?.ToString(), out qty)) // Quantity is at DGV Index 1
                        {
                            MessageBox.Show($"Invalid Quantity '{row.Cells[1].Value}' for item '{row.Cells[0].Value}' in row {row.Index + 1}. Cannot save.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            transaction.Rollback();
                            return;
                        }

                        decimal discountPercent = 0;
                        // Discount is at DGV Index 4, usually stored as "X%"
                        if (row.Cells[4].Value != null)
                        {
                            string discountString = row.Cells[4].Value.ToString().Replace("%", "");
                            if (!decimal.TryParse(discountString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out discountPercent))
                            {
                                // Log or handle the case where discount percent is malformed, default to 0
                                // MessageBox.Show($"Invalid Discount Percentage '{row.Cells[4].Value}' for item '{row.Cells[0].Value}' in row {row.Index + 1}. Defaulting to 0%.", "Data Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                discountPercent = 0; // Default to 0 if parsing fails
                            }
                        }
                        // If row.Cells[4].Value is null, discountPercent remains 0, which is correct


                        // Execute INSERT for PurchaseOrderDetails
                        SqlCommand cmdDetail = new SqlCommand(insertPurchaseOrderDetailsQuery, con, transaction);
                        cmdDetail.Parameters.AddWithValue("@PurchaseOrder_ID", purchaseOrderID);
                        cmdDetail.Parameters.AddWithValue("@Product_ID", productID);
                        cmdDetail.Parameters.AddWithValue("@Quantity", qty);
                        cmdDetail.Parameters.AddWithValue("@DiscountPercent", discountPercent);
                        cmdDetail.ExecuteNonQuery();

                        // Execute UPDATE for Product Stock
                        SqlCommand cmdUpdateStock = new SqlCommand(updateProductStockQuery, con, transaction);
                        cmdUpdateStock.Parameters.AddWithValue("@Quantity", qty);
                        cmdUpdateStock.Parameters.AddWithValue("@ProductID", productID);
                        cmdUpdateStock.ExecuteNonQuery();
                    }

                    // --- 6. Insert PurchasePayment ---
                    string insertPurchasePaymentQuery = @"
                INSERT INTO PurchasePayment (PurchaseOrder_ID, PaymentAmount, PaymentDate)
                VALUES (@PurchaseOrder_ID, @PaymentAmount, @PaymentDate)";

                    SqlCommand cmdPayment = new SqlCommand(insertPurchasePaymentQuery, con, transaction);
                    cmdPayment.Parameters.AddWithValue("@PurchaseOrder_ID", purchaseOrderID); // Link payment to the new PurchaseOrder
                    cmdPayment.Parameters.AddWithValue("@PaymentAmount", grandTotal);       // Assuming the full grandTotal is the initial payment
                    cmdPayment.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                    cmdPayment.ExecuteNonQuery();

                    // --- 7. Commit Transaction and Provide Success Feedback ---
                    transaction.Commit();
                    MessageBox.Show("Purchase Order and Payment saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // --- 8. Clear Form for New Entry ---
                    ClearNewPurchaseForm(); // Call your existing method to clear all fields and DGV
                }
                catch (Exception ex)
                {
                    // --- 9. Rollback Transaction on Error ---
                    transaction.Rollback(); // Revert all changes made within the transaction
                    MessageBox.Show("An error occurred while saving the purchase order: " + ex.Message + "\n\nDetails: " + ex.ToString(), "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Optionally log the full exception (ex.ToString()) for debugging
                }
            }
        }

        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
