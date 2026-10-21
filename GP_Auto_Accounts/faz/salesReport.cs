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
using System.Configuration;
using System.Runtime.InteropServices.ComTypes;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using RJCodeAdvance.RJControls;
using Krypton.Toolkit;
using System.Windows.Forms.DataVisualization.Charting;
using System.Web.UI;
using DocumentFormat.OpenXml.Bibliography;
using iTextSharp.text;
using iTextSharp.text.pdf;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
namespace GP_Auto_Accounts
{
    public partial class salesReport : Form
    {
        public salesReport()
        {
            InitializeComponent();
            //to make the datagrid visible accordingly
            kryptonDataGridView1.Visible = false;
            kryptonDataGridView2.Visible = true;

        }

        private readonly string connectionString = "Server=FAZEEN\\SQLEXPRESS;Database=AMS_db;Integrated Security=True;";
       



        private void kryptonGroupBox2_Paint(object sender, PaintEventArgs e)
        {

        }
        private void fillcombobox()

        {
            kryptonDataGridView2.AutoGenerateColumns = false;
            kryptonDataGridView1.AutoGenerateColumns = false;


            switch (rjComboBox1.SelectedIndex)
            {
                case 0:

                    {
                        //to make the datagrid visible accordingly
                        kryptonDataGridView2.Visible = true;
                        kryptonDataGridView1.Visible = false;

                        //filling rjcmbbox 2 when customers is selected 
                        rjComboBox2.Texts = "";
                        rjComboBox2.Items.Clear();

                        string query = "SELECT CustomerName FROM Customer";
                        using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                        {
                            sqlConnection.Open();
                            SqlCommand cmd = new SqlCommand(query, sqlConnection);
                            SqlDataReader rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                rjComboBox2.Items.Add(rdr["CustomerName"].ToString());

                            }
                            rdr.Close();


                        }
                        dgvCustomerFill();

                        break;
                    }
                case 1:

                    {
                        //to make the datagrid visible accordingly
                        kryptonDataGridView1.Visible = true;
                        kryptonDataGridView2.Visible = false;

                        //filling rjcmbbox 2 when customers is selected 
                        rjComboBox2.Texts = "";
                        rjComboBox2.Items.Clear();

                        string query = "SELECT ProductName FROM Product";
                        using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                        {
                            sqlConnection.Open();
                            SqlCommand cmd = new SqlCommand(query, sqlConnection);
                            SqlDataReader rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                rjComboBox2.Items.Add(rdr["ProductName"].ToString());

                            }
                            rdr.Close();


                            dgvProductFill();

                            break;
                        }
                        
                    }
            }
        }
        
        private void dgvCustomerFill()
        {
            Decimal subTotalSum;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {


                DateTime startDate = rjDatePicker1.Value;
                DateTime endDate = rjDatePicker2.Value;




                string query2 = @"
                                    SELECT 
                                        c.CustomerName,
                                        i.SubTotal,
                                        i.InvoiceID,
                                        i.TotalDiscount,
                                        (i.SubTotal-i.TotalDiscount) AS GrandTotal,
                                        ip.PaymentAmount,
                                        (GrandTotal-ip.PaymentAmount) AS OutstandingAmount,
                                        ip.PaymentDate
                                        
                                    FROM 
                                        Invoice i
                                        
                                    JOIN 
                                        Customer c ON i.Customer_ID = c.CustomerID
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID= ip.Invoice_ID
                                    WHERE
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate
                                    ";





                // Get invoices
                SqlCommand invoiceCmd = new SqlCommand(query2, conn);

                invoiceCmd.Parameters.AddWithValue("@StartDate", startDate);
                invoiceCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataAdapter adapter = new SqlDataAdapter(invoiceCmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No invoices found from customers.");
                }

                kryptonDataGridView2.DataSource = dt;
                kryptonDataGridView2.Refresh();

                conn.Close();
            }
            //filling the text boxes below as the customer is selected 
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT 
                                  SUM(i.SubTotal) AS subTotalSum
                                     
                                    FROM
                                        Invoice i

                                    JOIN
                                        Customer c ON i.Customer_ID = c.CustomerID
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
                                    WHERE
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate ";
                DateTime startDate = rjDatePicker1.Value;
                DateTime endDate = rjDatePicker2.Value;


                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        subTotalSum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox5.Text = subTotalSum.ToString("F2");
                    }


                }



                conn1.Close();
            }
            ///////////////////////////////////////////////////////////////////////////////////
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();
                Decimal Sum;

                string query3 = @"SELECT 
                                   SUM(i.TotalDiscount) AS TotalDiscountSum

                                    FROM
                                        Invoice i


                                    JOIN
                                        Customer c ON i.Customer_ID = c.CustomerID
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
                                    WHERE
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate ";
                DateTime startDate = rjDatePicker1.Value;
                DateTime endDate = rjDatePicker2.Value;


                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        Sum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox1.Text = Sum.ToString("F2");
                    }


                }



                conn1.Close();
            }
            ///////////////////////////////////////////////////////////////////
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();
                Decimal Sum;

                string query3 = @"SELECT 
                                   SUM(i.GrandTotal) AS GrandTotalSum

                                    FROM
                                        Invoice i

                                    JOIN
                                        Customer c ON i.Customer_ID = c.CustomerID
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
                                    WHERE
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate ";
                DateTime startDate = rjDatePicker1.Value;
                DateTime endDate = rjDatePicker2.Value;


                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        Sum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox2.Text = Sum.ToString("F2");
                    }


                }



                conn1.Close();
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();
                Decimal Sum;

                string query3 = @"SELECT  
                                              SUM(ip.PaymentAmount) AS Sum 
                                    FROM 
                                        Invoice i
                                        
                                    JOIN 
                                        Customer c ON i.Customer_ID = c.CustomerID
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID= ip.Invoice_ID
                                    WHERE
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate";
                DateTime startDate = rjDatePicker1.Value;
                DateTime endDate = rjDatePicker2.Value;


                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        Sum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox3.Text = Sum.ToString("F2");
                    }


                }



                conn1.Close();
            }
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();
                Decimal Sum;

                string query3 = @"SELECT  
                                              SUM(i.GrandTotal-ip.PaymentAmount) AS Sum 
                                        
                                    FROM 
                                        Invoice i
                                        
                                    JOIN 
                                        Customer c ON i.Customer_ID = c.CustomerID
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID= ip.Invoice_ID
                                    WHERE
                                        
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate";
                DateTime startDate = rjDatePicker1.Value;
                DateTime endDate = rjDatePicker2.Value;


                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        Sum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox4.Text = Sum.ToString("F2");
                    }


                }



                conn1.Close();
            }

        }
        private void dgvProductFill()
        {

            int productID;
            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;
            Decimal Sum;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query2 = @"
                                    SELECT 
                                        i.InvoiceID,
                                        p.ProductName,
                                        p.SaleUnitPrice,
                                        id.Quantity,
                                        i.SubTotal,
                                        i.TotalDiscount,
                                        (i.SubTotal-i.TotalDiscount) AS GrandTotal,
                                        ip.PaymentAmount,
                                        ((i.SubTotal - i.TotalDiscount) - ip.PaymentAmount) AS OutstandingAmount,
                                        ip.PaymentDate
                                        
                                   FROM 
                                        Invoice i           
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID= ip.Invoice_ID
                                    JOIN
                                        InvoiceDetail id ON i.InvoiceID= id.Invoice_ID

                                    JOIN 
                                        Product p ON id.Product_ID = p.ProductID
                                    WHERE                                         
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate";

                //rcpu is rough cost per unit

                // Get invoices
                SqlCommand Cmd = new SqlCommand(query2, conn);


                Cmd.Parameters.AddWithValue("@StartDate", startDate);
                Cmd.Parameters.AddWithValue("@EndDate", endDate);


                SqlDataAdapter adapter = new SqlDataAdapter(Cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No Invoices found for this customer.");
                    kryptonTextBox1.Text = "";
                    kryptonTextBox2.Text = "";
                    kryptonTextBox3.Text = "";
                    kryptonTextBox4.Text = "";
                    kryptonTextBox5.Text = "";
                }
                kryptonDataGridView1.AutoGenerateColumns = false;

                kryptonDataGridView1.DataSource = dt;
                kryptonDataGridView1.Refresh();

                conn.Close();
            }
            
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(i.SubTotal) AS Sum 
                                     FROM 
                                         Invoice i           
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID= ip.Invoice_ID
                                    JOIN
                                        InvoiceDetail id ON i.InvoiceID= id.Invoice_ID
                                    JOIN 
                                        Product p ON id.Product_ID = p.ProductID
                                    WHERE 

                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate";

                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        Sum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox5.Text = Sum.ToString("F2");
                    }


                }



                conn1.Close();
            }
            
            ///////////////////////////////////////////////////////////////////////////////////
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(i.TotalDiscount) AS Sum 
                                         
                                 FROM 
                                         Invoice i           
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID= ip.Invoice_ID
                                    JOIN
                                        InvoiceDetail id ON i.InvoiceID= id.Invoice_ID
                                    JOIN 
                                        Product p ON id.Product_ID = p.ProductID
                                    WHERE 
                                       
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate";


                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        Sum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox1.Text = Sum.ToString("F2");
                    }


                }



                conn1.Close();
            }
            ///////////////////////////////////////////////////////////////////
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();

                string query3 = @"SELECT  
                                              SUM(i.GrandTotal) AS Sum 
                                        
                                   FROM 
                                         Invoice i           
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID= ip.Invoice_ID
                                    JOIN
                                        InvoiceDetail id ON i.InvoiceID= id.Invoice_ID
                                    JOIN 
                                        Product p ON id.Product_ID = p.ProductID
                                    WHERE 
                                       
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate";



                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        Sum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox2.Text = Sum.ToString("F2");
                    }


                }



                conn1.Close();
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(ip.PaymentAmount) AS Sum 
                                   FROM 
                                         Invoice i           
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID= ip.Invoice_ID
                                    JOIN
                                        InvoiceDetail id ON i.InvoiceID= id.Invoice_ID
                                    JOIN 
                                        Product p ON id.Product_ID = p.ProductID
                                    WHERE 
                                       
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate"; ;

                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        Sum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox3.Text = Sum.ToString("F2");
                    }


                }



                conn1.Close();
            }
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(i.GrandTotal-ip.PaymentAmount) AS Sum 
                                   FROM 
                                         Invoice i           
                                    JOIN
                                        InvoicePayment ip ON i.InvoiceID= ip.Invoice_ID
                                    JOIN
                                        InvoiceDetail id ON i.InvoiceID= id.Invoice_ID
                                    JOIN 
                                        Product p ON id.Product_ID = p.ProductID
                                    WHERE 
                                       
                                        ip.PaymentDate BETWEEN @StartDate AND @EndDate";

                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataReader rdr = SumCmd.ExecuteReader();
                if (rdr.Read())
                {

                    if (!rdr.IsDBNull(0))
                    {
                        Sum = Convert.ToDecimal(rdr[0]);
                        kryptonTextBox4.Text = Sum.ToString("F2");
                    }


                }



                conn1.Close();
            }



            

        }


        private void rjComboBox1_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            fillcombobox();
               
            }



        private void customerNameSelected(string customerName)
        {
            decimal subTotalSum = 0, totalDiscount = 0, grandTotal = 0, paymentTotal = 0, outstandingAmount = 0;
            int customerID;
            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get CustomerID
                customerName = rjComboBox2.SelectedItem.ToString();
                using (SqlCommand cmd = new SqlCommand("SELECT CustomerID FROM Customer WHERE CustomerName = @name", conn))
                {
                    cmd.Parameters.AddWithValue("@name", customerName);
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        MessageBox.Show("Customer not found.");
                        return;
                    }
                    customerID = Convert.ToInt32(result);
                }

                // Load invoice data to DataGridView
                string invoiceQuery = @"
                        SELECT 
                            c.CustomerName,
                            i.SubTotal,
                            i.InvoiceID,
                            i.TotalDiscount,
                            (i.SubTotal - i.TotalDiscount) AS GrandTotal,
                            ip.PaymentAmount,
                            ((i.SubTotal - i.TotalDiscount) - ip.PaymentAmount) AS OutstandingAmount,
                            ip.PaymentDate
                        FROM Invoice i
                        JOIN Customer c ON i.Customer_ID = c.CustomerID
                        JOIN InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
                        WHERE i.Customer_ID = @Customerid AND ip.PaymentDate BETWEEN @StartDate AND @EndDate";

                using (SqlCommand invoiceCmd = new SqlCommand(invoiceQuery, conn))
                {
                    invoiceCmd.Parameters.AddWithValue("@Customerid", customerID);
                    invoiceCmd.Parameters.AddWithValue("@StartDate", startDate);
                    invoiceCmd.Parameters.AddWithValue("@EndDate", endDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(invoiceCmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                        MessageBox.Show("No invoices found for this customer.");

                    kryptonDataGridView2.DataSource = dt;
                    kryptonDataGridView2.Refresh();
                }

                // Combine all SUM queries into one
                string summaryQuery = @"
                            SELECT 
                                SUM(i.SubTotal) AS SubTotalSum,
                                SUM(i.TotalDiscount) AS TotalDiscountSum,
                                SUM(i.SubTotal - i.TotalDiscount) AS GrandTotalSum,
                                SUM(ip.PaymentAmount) AS PaymentAmountSum,
                                SUM((i.SubTotal - i.TotalDiscount) - ip.PaymentAmount) AS OutstandingAmountSum
                            FROM Invoice i
                            JOIN Customer c ON i.Customer_ID = c.CustomerID
                            JOIN InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
                            WHERE i.Customer_ID = @Customerid AND ip.PaymentDate BETWEEN @StartDate AND @EndDate";

                using (SqlCommand sumCmd = new SqlCommand(summaryQuery, conn))
                {
                    sumCmd.Parameters.AddWithValue("@Customerid", customerID);
                    sumCmd.Parameters.AddWithValue("@StartDate", startDate);
                    sumCmd.Parameters.AddWithValue("@EndDate", endDate);

                    SqlDataReader rdr = sumCmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        subTotalSum = rdr.IsDBNull(0) ? 0 : rdr.GetDecimal(0);
                        totalDiscount = rdr.IsDBNull(1) ? 0 : rdr.GetDecimal(1);
                        grandTotal = rdr.IsDBNull(2) ? 0 : rdr.GetDecimal(2);
                        paymentTotal = rdr.IsDBNull(3) ? 0 : rdr.GetDecimal(3);
                        outstandingAmount = rdr.IsDBNull(4) ? 0 : rdr.GetDecimal(4);
                    }
                }

                // Assign values to UI
                kryptonTextBox5.Text = subTotalSum.ToString("N2");
                kryptonTextBox1.Text = totalDiscount.ToString("N2");
                kryptonTextBox2.Text = grandTotal.ToString("N2");
                kryptonTextBox3.Text = paymentTotal.ToString("N2");
                kryptonTextBox4.Text = outstandingAmount.ToString("N2");
            }
        }

        private void ProductNameSelected(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName)) return;

            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;

            int productID;
            decimal subTotal = 0, discount = 0, grandTotal = 0, payment = 0, outstanding = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Step 1: Get Product ID
                using (SqlCommand getProductCmd = new SqlCommand("SELECT ProductID FROM Product WHERE ProductName = @ProductName", conn))
                {
                    getProductCmd.Parameters.AddWithValue("@ProductName", productName);
                    object result = getProductCmd.ExecuteScalar();

                    if (result == null)
                    {
                        MessageBox.Show("Product not found.");
                        return;
                    }

                    productID = Convert.ToInt32(result);
                }

                // Step 2: Load Invoice Details for Product
                string detailQuery = @"
            SELECT 
                i.InvoiceID,
                p.ProductName,
                p.SaleUnitPrice,
                id.Quantity,
                i.SubTotal,
                i.TotalDiscount,
                (i.SubTotal - i.TotalDiscount) AS GrandTotal,
                ip.PaymentAmount,
                ((i.SubTotal - i.TotalDiscount) - ip.PaymentAmount) AS OutstandingAmount,
                ip.PaymentDate
            FROM Invoice i
            JOIN InvoiceDetail id ON i.InvoiceID = id.Invoice_ID
            JOIN Product p ON id.Product_ID = p.ProductID
            JOIN InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
            WHERE p.ProductID = @ProductID AND ip.PaymentDate BETWEEN @StartDate AND @EndDate";

                using (SqlCommand detailCmd = new SqlCommand(detailQuery, conn))
                {
                    detailCmd.Parameters.AddWithValue("@ProductID", productID);
                    detailCmd.Parameters.AddWithValue("@StartDate", startDate);
                    detailCmd.Parameters.AddWithValue("@EndDate", endDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(detailCmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No purchase orders found for this product.");
                        kryptonTextBox1.Clear();
                        kryptonTextBox2.Clear();
                        kryptonTextBox3.Clear();
                        kryptonTextBox4.Clear();
                        kryptonTextBox5.Clear();
                    }

                    kryptonDataGridView1.AutoGenerateColumns = false;
                    kryptonDataGridView1.DataSource = dt;
                    kryptonDataGridView1.Refresh();
                }

                // Step 3: Get Summary Data in One Query
                string summaryQuery = @"
            SELECT 
                SUM(i.SubTotal) AS SubTotalSum,
                SUM(i.TotalDiscount) AS DiscountSum,
                SUM(i.SubTotal - i.TotalDiscount) AS GrandTotalSum,
                SUM(ip.PaymentAmount) AS PaymentSum,
                SUM((i.SubTotal - i.TotalDiscount) - ip.PaymentAmount) AS OutstandingSum
            FROM Invoice i
            JOIN InvoiceDetail id ON i.InvoiceID = id.Invoice_ID
            JOIN Product p ON id.Product_ID = p.ProductID
            JOIN InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
            WHERE p.ProductID = @ProductID AND ip.PaymentDate BETWEEN @StartDate AND @EndDate";

                using (SqlCommand summaryCmd = new SqlCommand(summaryQuery, conn))
                {
                    summaryCmd.Parameters.AddWithValue("@ProductID", productID);
                    summaryCmd.Parameters.AddWithValue("@StartDate", startDate);
                    summaryCmd.Parameters.AddWithValue("@EndDate", endDate);

                    SqlDataReader rdr = summaryCmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        subTotal = rdr.IsDBNull(0) ? 0 : rdr.GetDecimal(0);
                        discount = rdr.IsDBNull(1) ? 0 : rdr.GetDecimal(1);
                        grandTotal = rdr.IsDBNull(2) ? 0 : rdr.GetDecimal(2);
                        payment = rdr.IsDBNull(3) ? 0 : rdr.GetDecimal(3);
                        outstanding = rdr.IsDBNull(4) ? 0 : rdr.GetDecimal(4);
                    }
                }

                // Step 4: Display results
                kryptonTextBox5.Text = subTotal.ToString("N2");
                kryptonTextBox1.Text = discount.ToString("N2");
                kryptonTextBox2.Text = grandTotal.ToString("N2");
                kryptonTextBox3.Text = payment.ToString("N2");
                kryptonTextBox4.Text = outstanding.ToString("N2");
            }
        }



        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void rjComboBox2_OnSelectedIndexChanged(object sender, EventArgs e)
        {

            if (rjComboBox1.SelectedItem == "Customers")
            {
                string customerName = rjComboBox2.SelectedItem.ToString();

                customerNameSelected(customerName);
            }
            else
            {
                if (rjComboBox1.SelectedItem == "Products")

                {
                    string ProductName = rjComboBox2.SelectedItem.ToString();

                    ProductNameSelected(ProductName);
                }
                else
                {
                    MessageBox.Show("Select the CatogoryName");
                }
            }
        }
        private void interfaceCustomer()
        {
            kryptonTextBox1.Visible = true;
            kryptonTextBox2.Visible = false;
            kryptonTextBox3.Visible = false;
            kryptonTextBox4.Visible = false;
            kryptonTextBox5.Visible = true;

            kryptonLabel7.Visible = true;
            kryptonLabel7.Text = "ProductName";

            kryptonLabel8.Visible = true;
            kryptonLabel8.Text = "Unit Price";

            kryptonLabel9.Visible = false;
            kryptonLabel1.Visible = false;
            kryptonLabel2.Visible = false;








        }
        private void kryptonDataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

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
        private void ExportStatementOfAccount(DataGridView yourDataGridView, string filePath, string CustomerName)
        {

            if (string.IsNullOrEmpty(CustomerName))
            {
                MessageBox.Show("Please select a customer name.");
                return;
            }

            int CustomerID;
            string customerName = "";

            try
            {
                // Load customer info from DB
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"
                                              SELECT CustomerID FROM Customer WHERE CustomerName = @CustomerName", conn);
                    cmd.Parameters.AddWithValue("@CustomerName", CustomerName);

                    object CustomerIDObj = cmd.ExecuteScalar();

                    if (CustomerIDObj == null)
                    {
                        MessageBox.Show("Customer not found.");
                        return;
                    }

                    CustomerID = Convert.ToInt32(CustomerIDObj);

                    string query2 = @"
                                        SELECT c.CustomerName
                                        FROM Invoice i
                                        JOIN Customer c ON i.Customer_ID = c.CustomerID
                                        JOIN InvoicePayment ip ON i.InvoiceID = ip.Invoice_ID
                                        WHERE i.Customer_ID = @Customerid";

                    SqlCommand Cmd = new SqlCommand(query2, conn);
                    Cmd.Parameters.AddWithValue("@Customerid", CustomerID);

                    SqlDataReader reader = Cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        customerName = reader["CustomerName"].ToString();
                    }
                    reader.Close();
                }

                if (yourDataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("No data rows available to export.");
                    return;
                }

                // Column keys and display headers
                string[] columnKeys = { "InvoiceID", "total", "Discount", "NetAmount", "AmountPaid", "OutstandingAmount", "Date" };
                string[] displayHeaders = { "PO ID", "Gross Total", "Discount", "Net Amount", "Paid", "Outstanding", "Purchase Date" };
                float[] colWidths = { 2f, 2.5f, 2.5f, 2.5f, 2.5f, 3f, 3f };

                // Verify all columns exist
                foreach (string col in columnKeys)
                {
                    if (!yourDataGridView.Columns.Contains(col))
                    {
                        MessageBox.Show($"DataGridView is missing expected column: {col}");
                        return;
                    }
                }

                // Start PDF document
                Document doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // Add logo if exists
                string logoPath = @"C:\Fonts\logo.png";
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(180f, 180f);
                    logo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(logo);
                }

                // Load custom fonts
                string afacadBoldPath = @"C:\Fonts\AfacadFlux-Bold.ttf";
                string afacadThinPath = @"C:\Fonts\AfacadFlux-Regular.ttf";
                string bahnschriftPath = @"C:\Fonts\BS_Static_Bold.ttf";

                BaseFont afacadBold = BaseFont.CreateFont(afacadBoldPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                BaseFont afacadThin = BaseFont.CreateFont(afacadThinPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                BaseFont bahnschriftBold = BaseFont.CreateFont(bahnschriftPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(bahnschriftBold, 20);
                iTextSharp.text.Font fontLabel = new iTextSharp.text.Font(afacadBold, 14);
                iTextSharp.text.Font fontText = new iTextSharp.text.Font(afacadThin, 12);

                BaseFont arialBold = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                BaseFont arialNormal = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                iTextSharp.text.Font fontTableHeader = new iTextSharp.text.Font(arialBold, 8);
                iTextSharp.text.Font fontTableData = new iTextSharp.text.Font(arialNormal, 8);
                iTextSharp.text.Font fontSummaryLabel = new iTextSharp.text.Font(arialBold, 8);
                iTextSharp.text.Font fontSummaryValue = new iTextSharp.text.Font(arialNormal, 8);

                // Company info
                Paragraph address = new Paragraph("99/1, Quarry Road, Dehiwala, Colombo | 9477-1199991 | 9477-8916404", fontText);
                address.Alignment = Element.ALIGN_CENTER;
                doc.Add(address);
                doc.Add(new Paragraph(" "));

                // Title
                Paragraph title = new Paragraph("SUPPLIER PURCHASE STATEMENT", fontTitle);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                doc.Add(title);

                // Supplier Info Header
                PdfPTable headerTable = new PdfPTable(2)
                {
                    WidthPercentage = 100
                };
                headerTable.SetWidths(new float[] { 3f, 2f });

                PdfPCell leftCell = new PdfPCell { Border = PdfPCell.NO_BORDER };
                Paragraph supplierLabel = new Paragraph("Supplier Details", fontLabel);
                supplierLabel.SpacingAfter = 5f;
                leftCell.AddElement(supplierLabel);
                leftCell.AddElement(new Paragraph($"Name: {customerName}", fontText));
                leftCell.AddElement(new Paragraph(""));
                headerTable.AddCell(leftCell);

                PdfPCell rightCell = new PdfPCell { Border = PdfPCell.NO_BORDER, PaddingLeft = 85f };
                rightCell.AddElement(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}", fontText));
                headerTable.AddCell(rightCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                // Create table for data with split rows enabled for multiple pages
                PdfPTable table = new PdfPTable(columnKeys.Length)
                {
                    WidthPercentage = 100,
                    SplitRows = true
                };
                table.SetWidths(colWidths);

                // Add headers
                for (int i = 0; i < displayHeaders.Length; i++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(displayHeaders[i], fontTableHeader))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        Border = PdfPCell.NO_BORDER,
                        Padding = 5f,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                // Add rows
                foreach (DataGridViewRow row in yourDataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        for (int i = 0; i < columnKeys.Length; i++)
                        {
                            string colName = columnKeys[i];
                            object value = row.Cells[colName].Value ?? "";
                            string text = "";

                            if (colName == "Date" && DateTime.TryParse(value.ToString(), out DateTime dt))
                            {
                                text = dt.ToString("dd/MM/yyyy");
                            }
                            else if (colName == "Gross_Total" ||
                                     colName == "Discount" ||
                                     colName == "NetAmount" ||
                                     colName == "AmountPaid" ||
                                     colName == "OutstandingAmount")
                            {
                                if (decimal.TryParse(value.ToString(), out decimal num))
                                {
                                    text = num.ToString("N2"); // Format with commas + 2 decimals
                                }
                                else
                                {
                                    text = value.ToString();
                                }
                            }
                            else
                            {
                                text = value.ToString();
                            }

                            PdfPCell dataCell = new PdfPCell(new Phrase(text, fontTableData))
                            {
                                Padding = 5f,
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                Border = PdfPCell.NO_BORDER
                            };
                            table.AddCell(dataCell);
                        }
                    }
                }

                // Add the table to document after all rows added
                doc.Add(table);

                // Add a horizontal line separator
                doc.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f)));
                doc.Add(new Paragraph(" "));

                // Format summary values (assuming TryFormatRupees is a helper that formats currency strings)
                string grossTotal = TryFormatRupees(kryptonTextBox5.Text);
                string TotalDiscount = TryFormatRupees(kryptonTextBox1.Text);
                string GrandTotal = TryFormatRupees(kryptonTextBox2.Text);
                string AmountPaid = TryFormatRupees(kryptonTextBox3.Text);
                string OutstandingAmount = TryFormatRupees(kryptonTextBox4.Text);

                PdfPTable summaryTable = new PdfPTable(2)
                {
                    WidthPercentage = 50,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                summaryTable.SetWidths(new float[] { 2f, 1f });

                void AddSummaryRow(PdfPTable tableSum, string label, string value, iTextSharp.text.Font labelFont, iTextSharp.text.Font valueFont)
                {
                    tableSum.AddCell(new PdfPCell(new Phrase(label, labelFont)) { Border = PdfPCell.NO_BORDER, Padding = 5f, HorizontalAlignment = Element.ALIGN_LEFT });
                    tableSum.AddCell(new PdfPCell(new Phrase(value, valueFont)) { Border = PdfPCell.NO_BORDER, Padding = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                AddSummaryRow(summaryTable, "Gross Total:   ", grossTotal, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Total Discount:", TotalDiscount, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Grand Total:   ", GrandTotal, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Amount Paid:    ", AmountPaid, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Outstanding:     ", OutstandingAmount, fontSummaryLabel, fontSummaryValue);

                doc.Add(summaryTable);

                // Footer
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("Thank you for your business!", fontLabel)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 20f
                });

                doc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting PDF: " + ex.Message);
            }
        }


        private void kryptonButton13_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Create an instance of the InvoiceForm
            purchaseReport invoiceForm = new purchaseReport();

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
            salesReport invoiceForm = new salesReport();

            // Access panel1 from InvoiceForm and add it to panel1 in MainForm
            panel1.Controls.Add(invoiceForm.panel1);

            // Access panel2 from InvoiceForm and add it to panel2 in MainForm
            panel2.Controls.Add(invoiceForm.panel2);

            // Optionally, make sure the controls from InvoiceForm are correctly docked or sized within the MainForm panels
            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void rjComboBox3_OnSelectedIndexChanged(object sender, EventArgs e)
        {
        }

        public class PdfPageEvents : PdfPageEventHelper
        {
            private PdfPTable headerTable;
            private iTextSharp.text.Font headerFont;

            public PdfPageEvents(string[] headers, float[] widths, iTextSharp.text.Font font)
            {
                headerFont = font;

                headerTable = new PdfPTable(headers.Length);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(widths);
                headerTable.SpacingAfter = 10f;

                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5f,
                        Border = PdfPCell.NO_BORDER
                    };
                    headerTable.AddCell(cell);
                }
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                // Add footer with page number
                var cb = writer.DirectContent;
                cb.BeginText();
                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb.SetFontAndSize(bf, 9);
                string text = $"Page {writer.PageNumber}";
                float x = (document.Left + document.Right) / 2;
                float y = document.Bottom - 20;
                cb.ShowTextAligned(Element.ALIGN_CENTER, text, x, y, 0);
                cb.EndText();

                // Add header table at top of each page
                headerTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                headerTable.WriteSelectedRows(0, -1, document.LeftMargin, document.PageSize.Height - 10, cb);
            }
        }


        private void ExportStatementOfProduct(DataGridView yourDataGridView, string filePath, string ProductName)
        {
            ProductName = rjComboBox2.SelectedItem?.ToString();

            int ProductID;
            string productName = "", ProductCategory = "";

            try
            {
                // Load product info from DB
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"SELECT ProductID FROM Product WHERE ProductName = @ProductName", conn);
                    cmd.Parameters.AddWithValue("@ProductName", ProductName);

                    object ProductIDObj = cmd.ExecuteScalar();

                    if (ProductIDObj == null)
                    {
                        MessageBox.Show("Product not found.");
                        return;
                    }

                    ProductID = Convert.ToInt32(ProductIDObj);

                    string query2 = @"
                SELECT 
                    p.ProductName,
                    c.CategoryName
                FROM 
                    Product p
                JOIN 
                    Category c ON p.Category_ID = c.CategoryID
                WHERE 
                    p.ProductID = @Pid";

                    SqlCommand Cmd = new SqlCommand(query2, conn);
                    Cmd.Parameters.AddWithValue("@Pid", ProductID);

                    SqlDataReader reader = Cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        productName = reader["ProductName"].ToString();
                        ProductCategory = reader["CategoryName"].ToString();
                    }
                    reader.Close();
                }

                if (yourDataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("No data rows available to export.");
                    return;
                }

                // Define columns and headers
                string[] columnKeys = { "I_ID", "UnitPrice", "Quantity", "GrossTotal", "TotalDiscount", "GrandTotal", "PaymentAmount", "Outstanding_Amount", "CreatedAt" };
                string[] displayHeaders = { "inv. ID", "Unit Price", "Quantity", "Gross Total", "Discount", "Net Total", "Paid", "Outstanding", "Date" };
                float[] colWidths = { 1.2f, 2.1f, 1.5f, 2.3f, 2.2f, 2f, 2.3f, 2.5f, 2f };

                // Start PDF document and writer
                Document doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

               
                doc.Open();

                // Add logo
                string logoPath = @"C:\Fonts\logo.png";
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(180f, 180f);
                    logo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(logo);
                }

                // Load fonts
                string afacadBoldPath = @"C:\Fonts\AfacadFlux-Bold.ttf";
                string afacadThinPath = @"C:\Fonts\AfacadFlux-Regular.ttf";
                string bahnschriftPath = @"C:\Fonts\BS_Static_Bold.ttf";

                BaseFont afacadBold = BaseFont.CreateFont(afacadBoldPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                BaseFont afacadThin = BaseFont.CreateFont(afacadThinPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                BaseFont bahnschriftBold = BaseFont.CreateFont(bahnschriftPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(bahnschriftBold, 20);
                iTextSharp.text.Font fontLabel = new iTextSharp.text.Font(afacadBold, 14);
                iTextSharp.text.Font fontText = new iTextSharp.text.Font(afacadThin, 12);

                BaseFont arialBold = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                BaseFont arialNormal = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                iTextSharp.text.Font fontTableHeader = new iTextSharp.text.Font(arialBold, 8);
                iTextSharp.text.Font fontTableData = new iTextSharp.text.Font(arialNormal, 8);
                iTextSharp.text.Font fontSummaryLabel = new iTextSharp.text.Font(arialBold, 8);
                iTextSharp.text.Font fontSummaryValue = new iTextSharp.text.Font(arialNormal, 8);

                // Company info
                Paragraph address = new Paragraph("99/1, Quarry Road, Dehiwala, Colombo | 9477-1199991 | 9477-8916404", fontText);
                address.Alignment = Element.ALIGN_CENTER;
                doc.Add(address);
                doc.Add(new Paragraph(" "));

                // Title
                Paragraph title = new Paragraph("PRODUCT PURCHASE STATEMENT", fontTitle)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                doc.Add(title);

                // Product details table (header)
                PdfPTable headerTable = new PdfPTable(2)
                {
                    WidthPercentage = 100
                };
                headerTable.SetWidths(new float[] { 3f, 2f });

                PdfPCell leftCell = new PdfPCell { Border = PdfPCell.NO_BORDER };
                Paragraph productLabel = new Paragraph("Product Details", fontLabel);
                productLabel.SpacingAfter = 5f;
                leftCell.AddElement(productLabel);
                leftCell.AddElement(new Paragraph($"Name: {productName}", fontText));
                leftCell.AddElement(new Paragraph($"Category Name: {ProductCategory}", fontText));
                headerTable.AddCell(leftCell);

                PdfPCell rightCell = new PdfPCell { Border = PdfPCell.NO_BORDER, PaddingLeft = 85f };
                rightCell.AddElement(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}", fontText));
                headerTable.AddCell(rightCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                // Data table
                PdfPTable table = new PdfPTable(columnKeys.Length)
                {
                    WidthPercentage = 100,
                    SplitRows = true // important for multi-page tables
                };
                table.SetWidths(colWidths);

                // Add headers — page event also adds them, but keep here for first page
                for (int i = 0; i < displayHeaders.Length; i++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(displayHeaders[i], fontTableHeader))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        Border = PdfPCell.NO_BORDER,
                        Padding = 5f,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                // Add data rows
                foreach (DataGridViewRow row in yourDataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        for (int i = 0; i < columnKeys.Length; i++)
                        {
                            string colName = columnKeys[i];
                            object value = row.Cells[colName].Value ?? "";
                            string text = "";

                            if (colName == "CreatedAt" && DateTime.TryParse(value.ToString(), out DateTime dt))
                            {
                                text = dt.ToString("dd/MM/yyyy");
                            }
                            else if (colName == "UnitPrice" || colName == "GrossTotal" || colName == "TotalDiscount" || colName == "GrandTotal" || colName == "PaymentAmount" || colName == "Outstanding_Amount")
                            {
                                if (decimal.TryParse(value.ToString(), out decimal num))
                                {
                                    text = num.ToString("N2"); // format with commas and 2 decimals
                                }
                                else
                                {
                                    text = value.ToString();
                                }
                            }
                            else
                            {
                                text = value.ToString();
                            }

                            PdfPCell dataCell = new PdfPCell(new Phrase(text, fontTableData))
                            {
                                Padding = 5f,
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                Border = PdfPCell.NO_BORDER
                            };
                            table.AddCell(dataCell);
                        }
                    }
                }

                doc.Add(table);

                // Add line separator
                doc.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f)));
                doc.Add(new Paragraph(" "));

                // Format summary values — assuming you have this method to format currency strings
                string grossTotal = TryFormatRupees(kryptonTextBox5.Text);
                string totalDiscount = TryFormatRupees(kryptonTextBox1.Text);
                string grandTotal = TryFormatRupees(kryptonTextBox2.Text);
                string amountPaid = TryFormatRupees(kryptonTextBox3.Text);
                string outstandingAmount = TryFormatRupees(kryptonTextBox4.Text);

                // Summary table
                PdfPTable summaryTable = new PdfPTable(2)
                {
                    WidthPercentage = 50,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                summaryTable.SetWidths(new float[] { 2f, 1f });

                void AddSummaryRow(PdfPTable tbl, string label, string value, iTextSharp.text.Font labelFont, iTextSharp.text.Font valueFont)
                {
                    tbl.AddCell(new PdfPCell(new Phrase(label, labelFont)) { Border = PdfPCell.NO_BORDER, Padding = 5f, HorizontalAlignment = Element.ALIGN_LEFT });
                    tbl.AddCell(new PdfPCell(new Phrase(value, valueFont)) { Border = PdfPCell.NO_BORDER, Padding = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                AddSummaryRow(summaryTable, "Gross Total:   ", grossTotal, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Total Discount:", totalDiscount, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Grand Total:   ", grandTotal, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Amount Paid:    ", amountPaid, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Outstanding:     ", outstandingAmount, fontSummaryLabel, fontSummaryValue);

                doc.Add(summaryTable);

                // Footer message
                doc.Add(new Paragraph(" "));
               
                doc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting PDF: " + ex.Message);
            }
        }

        private void kryptonDataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void salesReport_Load(object sender, EventArgs e)
        {

        }
        private string TryFormatRupees(string input)
        {
            if (decimal.TryParse(input, out decimal amount))
            {
                return "Rs. " + amount.ToString("N2");  // Format: Rs. 1,234.56
            }
            else
            {
                return input;  // Return original if not numeric
            }
        }

        private void kryptonButton14_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Create an instance of the InvoiceForm
            InventoryReport invoiceForm = new InventoryReport();

            // Access panel1 from InvoiceForm and add it to panel1 in MainForm
            panel1.Controls.Add(invoiceForm.panel1);

            // Access panel2 from InvoiceForm and add it to panel2 in MainForm
            panel2.Controls.Add(invoiceForm.panel2);

            // Optionally, make sure the controls from InvoiceForm are correctly docked or sized within the MainForm panels
            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void kryptonButton22_Click(object sender, EventArgs e)
        {
         
            try
            {
                if (rjComboBox2.SelectedItem == null)
                {
                    MessageBox.Show("Please select a valid name from the ComboBox.");
                    return;
                }

                DateTime now = DateTime.Now;
                string formattedDate = now.ToString("yyyyMMdd_HHmmss");
                string filePath = @"C:\reports\";

                Directory.CreateDirectory(filePath); // Ensure folder exists

                if (rjComboBox1.SelectedItem == "Customers")
                {
                    filePath += $"StatementOfAccount_{formattedDate}.pdf";
                    string CustomerName = rjComboBox2.SelectedItem.ToString();
                    ExportStatementOfAccount(kryptonDataGridView2, filePath, CustomerName);
                    MessageBox.Show("PDF exported successfully!");
                    
                }
                else
                {
                    if (rjComboBox1.SelectedItem == "Products")

                    {
                        filePath += $"StatementOfProduct_{formattedDate}.pdf";
                        string ProductName = rjComboBox2.SelectedItem.ToString();
                        ExportStatementOfProduct(kryptonDataGridView1, filePath, ProductName);
                        MessageBox.Show("PDF exported successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Invalid selection.");
                    }
                }



                // Reset ComboBox
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred:\n" + ex.Message);
           
}
        }

        private void kryptonButton18_Click(object sender, EventArgs e)
        {

        }
    }
}
//

























