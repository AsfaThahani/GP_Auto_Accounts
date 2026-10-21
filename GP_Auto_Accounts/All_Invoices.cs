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
using static GP_Auto_Accounts.InvoicePayment;

namespace GP_Auto_Accounts
{
    public partial class All_Invoices: SmoothRoundedForm
    {
        string connectionString = "Data Source=DESKTOP-KKN421P\\SQLEXPRESS;Initial Catalog=AMS;Integrated Security=True";

        public All_Invoices()
        {
           
            InitializeComponent();
            dgvAllInvoices.AutoGenerateColumns = false;

            LoadCustomerNames();
            LoadAllInvoices();
             
        }

        private void All_Invoices_Load(object sender, EventArgs e)
        {
            LoadCustomerNames();
            LoadAllInvoices();
        }


        private void rjComboBox1_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (rjComboBox1.SelectedItem is CustomerItem selectedCustomer)
            {
                if (selectedCustomer.CustomerID == -1)
                {
                    LoadAllInvoices();
                }
                else
                {
                    LoadInvoicesByCustomerID(selectedCustomer.CustomerID);
                }
            }
        }

        private void LoadInvoicesByCustomerID(int customerID)
        {
            string query = @"
    SELECT 
        i.InvoiceID,
        i.CreatedAt,
        c.CustomerName,
        i.GrandTotal,
        ISNULL(SUM(ip.PaymentAmount), 0) AS PaymentAmount,
        CASE 
            WHEN ISNULL(SUM(ip.PaymentAmount), 0) = 0 THEN 'Not Paid'
            WHEN ISNULL(SUM(ip.PaymentAmount), 0) < i.GrandTotal THEN 'Partial'
            ELSE 'Fully Paid'
        END AS PaymentStatus,
        i.Updated_By,
        i.Description
    FROM Invoice i
    JOIN Customer c ON i.Customer_ID = c.CustomerID
    LEFT JOIN InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
    WHERE c.CustomerID = @CustomerID
    GROUP BY i.InvoiceID, i.CreatedAt, c.CustomerName, i.GrandTotal, i.Updated_By, i.Description
    ORDER BY i.CreatedAt DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CustomerID", customerID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvAllInvoices.DataSource = dt;
            }
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

        private void LoadAllInvoices()
        {
            string query = @"
    SELECT 
        i.InvoiceID,
        i.CreatedAt,
        c.CustomerName,
        i.GrandTotal,
        ISNULL(SUM(ip.PaymentAmount), 0) AS PaymentAmount,
        CASE 
            WHEN ISNULL(SUM(ip.PaymentAmount), 0) = 0 THEN 'Not Paid'
            WHEN ISNULL(SUM(ip.PaymentAmount), 0) < i.GrandTotal THEN 'Partial'
            ELSE 'Fully Paid'
        END AS PaymentStatus,
        i.Updated_By,
        i.Description
    FROM Invoice i
    JOIN Customer c ON i.Customer_ID = c.CustomerID
    LEFT JOIN InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
    GROUP BY i.InvoiceID, i.CreatedAt, c.CustomerName, i.GrandTotal, i.Updated_By, i.Description
    ORDER BY i.CreatedAt DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvAllInvoices.DataSource = dt;
            }
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
                    rjComboBox1.Items.Clear();

                    rjComboBox1.Items.Add(new CustomerItem
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
                        rjComboBox1.Items.Add(customer);
                    }

                    reader.Close();
                    rjComboBox1.SelectedIndex = 0;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Customer: " + ex.Message);
                }
            }
        }


        private void kryptonButton8_Click(object sender, EventArgs e)
        {
            signup signupForm = new signup();

            signupForm.Show();
        }

        private void kryptonButton14_Click(object sender, EventArgs e)
        {

            panel1.Controls.Clear();
            panel2.Controls.Clear();

            InvoicePayment invoiceForm = new InvoicePayment();

            panel1.Controls.Add(invoiceForm.panel1);

            panel2.Controls.Add(invoiceForm.panel2);

            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void kryptonButton3_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            NewPurchase purchaseForm = new NewPurchase();

            panel1.Controls.Add(purchaseForm.panel1);

            panel2.Controls.Add(purchaseForm.panel2);

            purchaseForm.panel1.Dock = DockStyle.Top;
            purchaseForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton15_Click(object sender, EventArgs e)
        {

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

        private void dgvAllInvoices_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvAllInvoices_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
    

     
            invoiceDetails dialog = new invoiceDetails();

            dialog.ShowDialog();

        }
    }
}
