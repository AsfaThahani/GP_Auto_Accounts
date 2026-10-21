using System;
using System.Collections;
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
    public partial class Purchase_Payment : Form
    {

        string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";


        public Purchase_Payment()
        {
            InitializeComponent();

            // Initialize the Action to point to the LoadSupplierNames method of this instance
            this.RefreshSupplierComboBox = new Action(LoadSupplierNames);

            LoadSupplierNames();

        }

        public Action RefreshSupplierComboBox { get; set; }


        public class SupplierItem
        {
            public int SupplierID { get; set; }
            public string SupplierName { get; set; }

            public override string ToString()
            {
                return SupplierName;
            }
        }





        private void LoadSupplierNames()
        {
            string query = "SELECT SupplierID, SupplierName FROM Supplier";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    comboBoxSupplier.Items.Clear();

                    while (reader.Read())
                    {
                        SupplierItem supplier = new SupplierItem
                        {
                            SupplierID = Convert.ToInt32(reader["SupplierID"]),
                            SupplierName = reader["SupplierName"].ToString()
                        };
                        comboBoxSupplier.Items.Add(supplier);
                    }
                    reader.Close();

                    //Add "Show All" option at the top
                    comboBoxSupplier.Items.Insert(0, new SupplierItem
                    {
                        SupplierID = -1,
                        SupplierName = "-- Show All --"
                    });

                    comboBoxSupplier.SelectedIndex = 0; // Set default selection to "Show All"
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading suppliers: " + ex.Message);
                }
            }
        }




        private void LoadFilteredPayment()
        {
            // Retrieve Supplier ID from ComboBox
            int supplierId = -1; // Default to "Show All"
            if (comboBoxSupplier.SelectedItem is SupplierItem selectedItem)
            {
                supplierId = selectedItem.SupplierID;
            }

            // Retrieve From Date from DateTimePicker
            DateTime? fromDate = dtpFromDate.Checked ? dtpFromDate.Value.Date : (DateTime?)null;

            // Retrieve To Date from DateTimePicker
            DateTime? toDate = dtpToDate.Checked ? dtpToDate.Value.Date : (DateTime?)null;

            // --- Optional: Basic date validation ---
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                MessageBox.Show("'From Date' cannot be after 'To Date'. Please adjust your selection.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // You might consider resetting one of the DateTimePickers here if you want to enforce validity immediately.
                return; // Stop execution if dates are invalid
            }
            // --- End Optional Validation ---


            string query = @"
        SELECT
            po.PurchaseOrderID,
            pp.PurchasePaymentID AS PaymentID,
            s.SupplierName,
            po.TotalAmount,
            pp.PaymentAmount,
            (po.TotalAmount - pp.PaymentAmount) AS OutstandingAmount,
            pp.PaymentDate,
            pp.Updated_By AS UpdatedBy
        FROM
            PurchasePayment pp
        INNER JOIN
            PurchaseOrder po ON pp.PurchaseOrderID = po.PurchaseOrderID
        INNER JOIN
            Supplier s ON po.Supplier_ID = s.SupplierID";

            List<string> whereClauses = new List<string>();

            if (supplierId != -1)
            {
                whereClauses.Add("s.SupplierID = @SupplierID");
            }

            if (fromDate.HasValue)
            {
                whereClauses.Add("pp.PaymentDate >= @FromDate");
            }

            if (toDate.HasValue)
            {
                whereClauses.Add("pp.PaymentDate <= @ToDate");
            }

            if (whereClauses.Any())
            {
                query += " WHERE " + string.Join(" AND ", whereClauses);
            }

            query += " ORDER BY pp.PaymentDate DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (supplierId != -1)
                {
                    cmd.Parameters.AddWithValue("@SupplierID", supplierId);
                }

                if (fromDate.HasValue)
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Value); // Value is already .Date from above
                }

                if (toDate.HasValue)
                {
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Value.AddDays(1).AddTicks(-1)); // Value is already .Date from above
                }

                try
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;


                    if (dt.Rows.Count > 0)
                    {
                        dataGridView1.Columns["PurchaseOrderID"].HeaderText = "Purchase Order No.";
                        dataGridView1.Columns["PaymentID"].HeaderText = "Payment ID";
                        dataGridView1.Columns["SupplierName"].HeaderText = "Supplier";
                        dataGridView1.Columns["TotalAmount"].HeaderText = "Total Amount";
                        dataGridView1.Columns["PaymentAmount"].HeaderText = "Paid Amount";
                        dataGridView1.Columns["OutstandingAmount"].HeaderText = "Outstanding";
                        dataGridView1.Columns["PaymentDate"].HeaderText = "Payment Date";
                        dataGridView1.Columns["UpdatedBy"].HeaderText = "Updated By";
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    }
                    else
                    {
                        dataGridView1.DataSource = null; // Clear previous data
                        MessageBox.Show("No purchase payment data found for the selected criteria.", "No Records", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading purchase payment data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void Purchase_Payment_Load(object sender, EventArgs e)
        {
            LoadSupplierNames();

            LoadSupplierPaymentData(-1); // -1 means "show all"

            dtpToDate.Value = DateTime.Today.AddMonths(-1); // example: set default range
            dtpToDate.Value = DateTime.Today;
            LoadFilteredPayment();
        }


        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            LoadFilteredPayment();
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            LoadFilteredPayment();
        }


        private void kryptonGroupBox4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void kryptonButton1_Click(object sender, EventArgs e)
        {

        }



        private void kryptonButton15_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Create an instance of the InvoiceForm
            All_Purchase invoiceForm = new All_Purchase();

            // Access panel1 from InvoiceForm and add it to panel1 in MainForm
            panel1.Controls.Add(invoiceForm.panel1);

            // Access panel2 from InvoiceForm and add it to panel2 in MainForm
            panel2.Controls.Add(invoiceForm.panel2);

            // Optionally, make sure the controls from InvoiceForm are correctly docked or sized within the MainForm panels
            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton4_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Create an instance of the InvoiceForm
            Customer invoiceForm = new Customer();

            // Access panel1 from InvoiceForm and add it to panel1 in MainForm
            panel1.Controls.Add(invoiceForm.panel1);

            // Access panel2 from InvoiceForm and add it to panel2 in MainForm
            panel2.Controls.Add(invoiceForm.panel2);

            // Optionally, make sure the controls from InvoiceForm are correctly docked or sized within the MainForm panels
            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton12_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Create an instance of the InvoiceForm
            NewPurchase purchaseForm = new NewPurchase();

            // Access panel1 from InvoiceForm and add it to panel1 in MainForm
            panel1.Controls.Add(purchaseForm.panel1);

            // Access panel2 from InvoiceForm and add it to panel2 in MainForm
            panel2.Controls.Add(purchaseForm.panel2);

            // Optionally, make sure the controls from InvoiceForm are correctly docked or sized within the MainForm panels
            purchaseForm.panel1.Dock = DockStyle.Top;
            purchaseForm.panel2.Dock = DockStyle.Fill;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0 && dataGridView1.Rows[e.RowIndex].Cells["TotalAmount"].Value != null)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                txtTotalAmount.Text = row.Cells["TotalAmount"].Value.ToString();
                txtPaymentAmount.Text = row.Cells["PaymentAmount"].Value.ToString();
                txtOutstandingAmount.Text = row.Cells["OutstandingAmount"].Value.ToString();
            }
        }





        private void LoadSupplierPaymentData(int supplierId)
        {
            string query = @"
    SELECT
        po.PurchaseOrderID,
        pp.PurchasePaymentID AS PaymentID,
        s.SupplierName,
        po.TotalAmount,
        pp.PaymentAmount,
        (po.TotalAmount - pp.PaymentAmount) AS OutstandingAmount,
        pp.PaymentDate,
        pp.Updated_By AS UpdatedBy
    FROM
        PurchasePayment pp
    INNER JOIN
        PurchaseOrder po ON pp.PurchaseOrderID = po.PurchaseOrderID
    INNER JOIN
        Supplier s ON po.Supplier_ID = s.SupplierID";

            if (supplierId != -1)
            {
                query += " WHERE s.SupplierID = @SupplierID";
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                

                if (supplierId != -1)
                {
                    cmd.Parameters.AddWithValue("@SupplierID", supplierId);
                }

                try
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No invoices found for the selected filters.", "No Records", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        dataGridView1.Columns["PurchaseOrderID"].HeaderText = "Purchase Order No.";
                        dataGridView1.Columns["PaymentID"].HeaderText = "Payment ID";
                        dataGridView1.Columns["SupplierName"].HeaderText = "Supplier";
                        dataGridView1.Columns["TotalAmount"].HeaderText = "Total Amount";
                        dataGridView1.Columns["PaymentAmount"].HeaderText = "Paid Amount";
                        dataGridView1.Columns["OutstandingAmount"].HeaderText = "Outstanding";
                        dataGridView1.Columns["PaymentDate"].HeaderText = "Payment Date";
                        dataGridView1.Columns["UpdatedBy"].HeaderText = "Updated By";
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    }
                    else
                    {
                        // Optional: Clear previous data if no rows are returned
                        dataGridView1.DataSource = null;
                        MessageBox.Show("No purchase payment data found for the selected supplier (or no data at all).");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading purchase payment data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



     






        private void kryptonButton12_Click_1(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Create an instance of the NewPurchase form
            NewPurchase newPurchaseForm = new NewPurchase();

            // Access panel1 from NewPurchase and add it to panel1 in All_Purchase
            panel1.Controls.Add(newPurchaseForm.panel1);

            // Access panel2 from NewPurchase and add it to panel2 in All_Purchase
            panel2.Controls.Add(newPurchaseForm.panel2);

            // Optionally, make sure the controls from NewPurchase are correctly docked or sized
            newPurchaseForm.panel1.Dock = DockStyle.Top;
            newPurchaseForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton14_Click(object sender, EventArgs e)
        {
            
        }

        private void kryptonButton15_Click_1(object sender, EventArgs e)
        {
            //  No implementation was provided in the original code.  You might have intended to open All_Purchase form itself?
            //  If so, and if this button is within the All_Purchase form, this is probably not what you want.
            //  If you want to open *another* instance of All_Purchase:
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            All_Purchase allPurchaseForm = new All_Purchase(); // Create a new instance.
            panel1.Controls.Add(allPurchaseForm.panel1);
            panel2.Controls.Add(allPurchaseForm.panel2);
            allPurchaseForm.panel1.Dock = DockStyle.Top;
            allPurchaseForm.panel2.Dock = DockStyle.Fill;
        }

        

        private void comboBoxSupplier_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSupplier.SelectedItem is SupplierItem selectedSupplier)
            {
                LoadSupplierPaymentData(selectedSupplier.SupplierID);
            }
        }

        private void kryptonGroupBox4_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void kryptonButton3_Click(object sender, EventArgs e)
        {

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

       
    }
}

