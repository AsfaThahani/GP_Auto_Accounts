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

using iTextSharp.text;
using iTextSharp.text.pdf;


namespace GP_Auto_Accounts
{
    public partial class purchaseReport : Form
    {
        public purchaseReport()
        {
            InitializeComponent();


        }
        private readonly string connectionString = "Server=FAZEEN\\SQLEXPRESS;Database=AMS_db;Integrated Security=True;";
        int Count;
        private void purchaseReport_Load(object sender, EventArgs e)
        {
        }

        private void kryptonLabel7_Click(object sender, EventArgs e)
        {

        }


        private void rjComboBox3_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            fillcombobox();

            rjComboBox2.Texts = "";
        }






        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void fillcombobox()
        {
            switch (rjComboBox3.SelectedIndex)
            {
                case 0:

                    {
                        kryptonDataGridView3.Visible = true;
                        kryptonDataGridView1.Visible = false;

                        rjComboBox2.Texts = "";
                        rjComboBox2.Items.Clear();
                        string query = "SELECT SupplierName FROM Supplier";
                        using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                        {
                            sqlConnection.Open();
                            SqlCommand cmd = new SqlCommand(query, sqlConnection);
                            SqlDataReader rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                rjComboBox2.Items.Add(rdr["SupplierName"].ToString());

                            }
                            rdr.Close();
                        }
                        dgvSupplierfill();


                        break;
                    }
                case 1:
                    {
                        kryptonDataGridView1.Visible = true;
                        kryptonDataGridView3.Visible = false;



                        rjComboBox2.Texts = "";
                        rjComboBox2.Items.Clear(); rjComboBox2.Items.Clear();
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
                        }
                        dgvProductfill();


                        break;

                    }


            }
        }


        private void kryptonButton13_Click(object sender, EventArgs e)
        {

        }
        private void dgvSupplierfill()
        {
            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;
            Decimal Sum;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query2 = @"
                                    SELECT 
                                        S.SupplierName,
                                        po.SubTotal,
                                        po.PurchaseorderID,
                                        po.TotalDiscount,
                                        (po.SubTotal-po.TotalDiscount) AS GrandTotal,
                                        pp.PaymentAmount,
                                        (GrandTotal-pp.PaymentAmount) AS OutstandingAmount,
                                        po.CreatedAt
                                        
                                    FROM 
                                        PurchaseOrder po
                                        
                                    JOIN 
                                        Supplier S ON po.Supplier_ID = S.SupplierID
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    WHERE 
                                        
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";


                SqlCommand invoiceCmd = new SqlCommand(query2, conn);


                invoiceCmd.Parameters.AddWithValue("@StartDate", startDate);
                invoiceCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataAdapter adapter = new SqlDataAdapter(invoiceCmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No invoices found for this customer.");
                    kryptonTextBox1.Text = "";
                    kryptonTextBox2.Text = "";
                    kryptonTextBox3.Text = "";
                    kryptonTextBox4.Text = "";
                    kryptonTextBox5.Text = "";
                }
                kryptonDataGridView3.AutoGenerateColumns = false;

                kryptonDataGridView3.DataSource = dt;
                LockDataGridView3();
                kryptonDataGridView3.Refresh();

                conn.Close();
            }
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(po.SubTotal) AS Sum 
                                    FROM
                                        PurchaseOrder po
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                    WHERE
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";

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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(po.TotalDiscount) AS Sum 
                                    FROM
                                        PurchaseOrder po
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                    WHERE
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";


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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();

                string query3 = @"SELECT  
                                              SUM(po.GrandTotal) AS Sum 
                                    FROM
                                        PurchaseOrder po
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                    WHERE
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";



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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(pp.PaymentAmount) AS Sum 
                                    FROM
                                        PurchaseOrder po
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                    WHERE
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";

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
                                              SUM(po.GrandTotal-pp.PaymentAmount) AS Sum 
                                    FROM
                                        PurchaseOrder po
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                    WHERE
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";

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


        private void dgvProductfill()
        {
            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;
            Decimal Sum;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query2 = @"
                                    SELECT 
                                        po.PurchaseorderID,
                                        p.ProductName,
                                        p.PurchaseUnitPrice,
                                        pod.Quantity,
                                        po.SubTotal,
                                        po.TotalDiscount,
                                        (po.SubTotal-po.TotalDiscount) AS GrandTotal,
                                        pp.PaymentAmount,
                                        ((po.SubTotal - po.TotalDiscount) - pp.PaymentAmount) AS OutstandingAmount,
                                        ((po.SubTotal - po.TotalDiscount) / NULLIF(pod.Quantity, 0)) AS RCPU,
                                        po.CreatedAt
                                        
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";


                SqlCommand invoiceCmd = new SqlCommand(query2, conn);


                invoiceCmd.Parameters.AddWithValue("@StartDate", startDate);
                invoiceCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataAdapter adapter = new SqlDataAdapter(invoiceCmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No PurchaseOrders found for this customer.");
                    kryptonTextBox1.Text = "";
                    kryptonTextBox2.Text = "";
                    kryptonTextBox3.Text = "";
                    kryptonTextBox4.Text = "";
                    kryptonTextBox5.Text = "";
                }
                kryptonDataGridView1.AutoGenerateColumns = false;

                kryptonDataGridView1.DataSource = dt;
                LockDataGridView1();
                kryptonDataGridView1.Refresh();

                conn.Close();
            }
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(po.SubTotal) AS Sum 
                                        
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";

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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(po.TotalDiscount) AS Sum 
                                         
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";


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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();

                string query3 = @"SELECT  
                                              SUM(po.GrandTotal) AS Sum 
                                        
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";



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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(pp.PaymentAmount) AS Sum 
                                       
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";

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
                                              SUM(po.GrandTotal-pp.PaymentAmount) AS Sum 
                                         
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";

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
        private void ExportStatementOfProduct(DataGridView yourDataGridView, string filePath)
        {
            string ProductName = rjComboBox2.SelectedItem?.ToString();

            int ProductID;
            string productName = "", ProductCategory = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"
                                                    SELECT 
                                                            ProductID
                                                    FROM 
                                                            Product 
                                                    WHERE 
                                                            ProductName = @ProductName"
                                                    , conn);
                    cmd.Parameters.AddWithValue("@ProductName", ProductName);

                    object SupplierIDObj = cmd.ExecuteScalar();

                    if (SupplierIDObj == null)
                    {
                        MessageBox.Show("Product not found.");
                        return;
                    }

                    ProductID = Convert.ToInt32(SupplierIDObj);

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

                Document doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

                doc.Open();

                string logoPath = @"C:\Fonts\logo.png";
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(180f, 180f);
                    logo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(logo);
                }

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


                Paragraph address = new Paragraph("99/1, Quarry Road, Dehiwala, Colombo | 9477-1199991 | 9477-8916404", fontText);
                address.Alignment = Element.ALIGN_CENTER;
                doc.Add(address);
                doc.Add(new Paragraph(" "));

                Paragraph title = new Paragraph("PRODUCT PURCHASE STATEMENT", fontTitle);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                doc.Add(title);

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 3f, 2f });

                PdfPCell leftCell = new PdfPCell { Border = PdfPCell.NO_BORDER };
                Paragraph ProductLabel = new Paragraph("Product Details", fontLabel);
                ProductLabel.SpacingAfter = 5f;
                leftCell.AddElement(ProductLabel);
                leftCell.AddElement(new Paragraph($"Name: {productName}", fontText));
                leftCell.AddElement(new Paragraph($"Category Name: {ProductCategory}", fontText));
                headerTable.AddCell(leftCell);

                PdfPCell rightCell = new PdfPCell { Border = PdfPCell.NO_BORDER, PaddingLeft = 85f };
                rightCell.AddElement(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}", fontText));
                headerTable.AddCell(rightCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                string[] columnKeys = { "PO_ID", "UnitPrice", "Quantity", "GrossTotal", "TotalDiscount", "GrandTotal", "PaymentAmount", "Outstanding_Amount", "CreatedAt" };

                string[] displayHeaders = { "PO ID", "Unit Price", "QTY", "Gross Total", "Discount", "Net Total", "Paid", "Outstanding", "Date" };
                float[] colWidths = { 1.2f, 2.1f, 1.5f, 2.3f, 2.2f, 2f, 2.3f, 2.5f, 2f };

                PdfPTable table = new PdfPTable(columnKeys.Length);
                table.WidthPercentage = 100;
                table.SetWidths(colWidths);

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




                foreach (DataGridViewRow row in yourDataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        for (int i = 0; i < columnKeys.Length; i++)
                        {
                            string colName = columnKeys[i];

                            if (yourDataGridView.Columns.Contains(colName))
                            {
                                object value = row.Cells[colName].Value ?? "";
                                string text = "";

                                if (colName == "CreatedAt" && DateTime.TryParse(value.ToString(), out DateTime dt))
                                {
                                    text = dt.ToString("dd/MM/yyyy");
                                }
                                else if (
                                    colName == "UnitPrice" ||
                                    colName == "GrossTotal" ||
                                    colName == "TotalDiscount" ||
                                    colName == "GrandTotal" ||
                                    colName == "PaymentAmount" ||
                                    colName == "Outstanding_Amount"
                                )
                                {
                                    if (decimal.TryParse(value.ToString(), out decimal num))
                                    {
                                        text = num.ToString("N2");
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
                            else
                            {
                                table.AddCell(new PdfPCell(new Phrase("", fontTableData))
                                {
                                    Border = PdfPCell.NO_BORDER,
                                    Padding = 5f
                                });
                            }
                        }

                    }



                    doc.Add(table);

                    doc.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f)));
                    doc.Add(new Paragraph(" "));


                    string grossTotal = TryFormatRupees(kryptonTextBox5.Text);
                    string TotalDiscount = TryFormatRupees(kryptonTextBox1.Text);
                    string GrandTotal = TryFormatRupees(kryptonTextBox2.Text);
                    string AmountPaid = TryFormatRupees(kryptonTextBox3.Text);
                    string OutstandingAMount = TryFormatRupees(kryptonTextBox4.Text);


                    PdfPTable summaryTable = new PdfPTable(2);
                    summaryTable.WidthPercentage = 50;
                    summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                    summaryTable.SetWidths(new float[] { 2f, 1f });

                    AddSummaryRow(summaryTable, "Gross Total:   ", grossTotal, fontSummaryLabel, fontSummaryValue);
                    AddSummaryRow(summaryTable, "Total Discount:", TotalDiscount, fontSummaryLabel, fontSummaryValue);
                    AddSummaryRow(summaryTable, "Grand Total:   ", GrandTotal, fontSummaryLabel, fontSummaryValue);



                    AddSummaryRow(summaryTable, "Amount Paid:    ", AmountPaid, fontSummaryLabel, fontSummaryValue);
                    AddSummaryRow(summaryTable, "Outstanding:     ", OutstandingAMount, fontSummaryLabel, fontSummaryValue);

                    doc.Add(summaryTable);

                    doc.Add(new Paragraph(" "));
                    doc.Add(new Paragraph("Thank you for your business!", fontLabel)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 20f
                    });




                    doc.Close();


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
            }
        }


        private void filldgv4Supplier(string SupplierName)
        {


            int SupplierID;
            Decimal Sum;
            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;





            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT SupplierID FROM Supplier WHERE SupplierName = @SupplierName", conn);
                cmd.Parameters.AddWithValue("@SupplierName", SupplierName);

                object SupplierIDObj = cmd.ExecuteScalar();
                SupplierID = Convert.ToInt32(SupplierIDObj);



                if (SupplierIDObj == null)
                {
                    MessageBox.Show("Supplier not found." + SupplierID);
                    return;
                }





                string query2 = @"
                                       SELECT 
                                           S.SupplierName,
                                           po.SubTotal,
                                           po.PurchaseorderID,
                                           po.TotalDiscount,
                                           (po.SubTotal-po.TotalDiscount) AS GrandTotal,
                                           pp.PaymentAmount,
                                           (GrandTotal-pp.PaymentAmount) AS OutstandingAmount,
                                           po.CreatedAt

                                       FROM 
                                           PurchaseOrder po

                                       JOIN 
                                           Supplier S ON po.Supplier_ID = S.SupplierID
                                       JOIN
                                           PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                       WHERE 
                                           po.Supplier_ID=@Sid AND
                                           pp.CreatedAt BETWEEN @StartDate AND @EndDate";


                SqlCommand invoiceCmd = new SqlCommand(query2, conn);

                invoiceCmd.Parameters.AddWithValue("@Sid", SupplierID);
                invoiceCmd.Parameters.AddWithValue("@StartDate", startDate);
                invoiceCmd.Parameters.AddWithValue("@EndDate", endDate);



                SqlDataAdapter adapter = new SqlDataAdapter(invoiceCmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No invoices found for this customer.");
                    kryptonTextBox1.Text = "";
                    kryptonTextBox2.Text = "";
                    kryptonTextBox3.Text = "";
                    kryptonTextBox4.Text = "";
                    kryptonTextBox5.Text = "";
                }
                kryptonDataGridView3.AutoGenerateColumns = false;

                kryptonDataGridView3.DataSource = dt;
                kryptonDataGridView3.Refresh();
                LockDataGridView3();
                conn.Close();
            }
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                                 SUM(po.SubTotal) AS Sum 
                                       FROM
                                           PurchaseOrder po
                                       JOIN
                                           PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                       WHERE
                                           po.Supplier_ID=@Sid AND
                                           pp.CreatedAt BETWEEN @StartDate AND @EndDate";

                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@Sid", SupplierID);



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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                                 SUM(po.TotalDiscount) AS Sum 
                                       FROM
                                           PurchaseOrder po
                                       JOIN
                                           PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                       WHERE
                                           po.Supplier_ID=@Sid AND
                                           pp.CreatedAt BETWEEN @StartDate AND @EndDate";


                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@Sid", SupplierID);



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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();

                string query3 = @"SELECT  
                                                 SUM(po.GrandTotal) AS Sum 
                                       FROM
                                           PurchaseOrder po
                                       JOIN
                                           PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                       WHERE
                                           po.Supplier_ID=@Sid AND
                                           pp.CreatedAt BETWEEN @StartDate AND @EndDate";



                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@Sid", SupplierID);


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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                                 SUM(pp.PaymentAmount) AS Sum 
                                       FROM
                                           PurchaseOrder po
                                       JOIN
                                           PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                       WHERE
                                           po.Supplier_ID=@Sid AND
                                           pp.CreatedAt BETWEEN @StartDate AND @EndDate";

                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@Sid", SupplierID);



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
                                                 SUM(po.GrandTotal-pp.PaymentAmount) AS Sum 
                                       FROM
                                           PurchaseOrder po
                                       JOIN
                                           PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                                       WHERE
                                           po.Supplier_ID=@Sid AND
                                           pp.CreatedAt BETWEEN @StartDate AND @EndDate";

                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@Sid", SupplierID);


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

        private void filldgv4Product(string ProductName)
        {

            int productID;
            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;
            Decimal Sum;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ProductID FROM Product WHERE ProductName = @ProductName", conn);
                cmd.Parameters.AddWithValue("@ProductName", ProductName);

                object productIDObj = cmd.ExecuteScalar();
                productID = Convert.ToInt32(productIDObj);


                if (productIDObj == null)
                {
                    MessageBox.Show("Product not found." + productID);
                    return;
                }

            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query2 = @"
                                    SELECT 
                                        po.PurchaseorderID,
                                        p.ProductName,
                                        p.PurchaseUnitPrice,
                                        pod.Quantity,
                                        po.SubTotal,
                                        po.TotalDiscount,
                                        (po.SubTotal-po.TotalDiscount) AS GrandTotal,
                                        pp.PaymentAmount,
                                        ((po.SubTotal - po.TotalDiscount) - pp.PaymentAmount) AS OutstandingAmount,
                                        ((po.SubTotal - po.TotalDiscount) / NULLIF(pod.Quantity, 0)) AS RCPU,
                                        po.CreatedAt
                                        
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        p.ProductID=@pID 
                                    AND
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";


                SqlCommand PurchaseCmd = new SqlCommand(query2, conn);


                PurchaseCmd.Parameters.AddWithValue("@StartDate", startDate);
                PurchaseCmd.Parameters.AddWithValue("@EndDate", endDate);
                PurchaseCmd.Parameters.AddWithValue("@pID", productID);


                SqlDataAdapter adapter = new SqlDataAdapter(PurchaseCmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No PurchaseOrders found for this customer.");
                    kryptonTextBox1.Text = "";
                    kryptonTextBox2.Text = "";
                    kryptonTextBox3.Text = "";
                    kryptonTextBox4.Text = "";
                    kryptonTextBox5.Text = "";
                }
                kryptonDataGridView1.AutoGenerateColumns = false;

                kryptonDataGridView1.DataSource = dt;
                kryptonDataGridView1.Refresh();
                LockDataGridView1();

                conn.Close();
            }
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(po.SubTotal) AS Sum 
                                        
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        p.ProductID=@pID 
                                    AND
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";

                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@pID", productID);



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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(po.TotalDiscount) AS Sum 
                                         
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        p.ProductID=@pID 
                                    AND                                        
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";


                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@pID", productID);



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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();

                string query3 = @"SELECT  
                                              SUM(po.GrandTotal) AS Sum 
                                        
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                        p.ProductID=@pID 
                                    AND
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";



                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@pID", productID);



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
            using (SqlConnection conn1 = new SqlConnection(connectionString))
            {
                conn1.Open();


                string query3 = @"SELECT  
                                              SUM(pp.PaymentAmount) AS Sum 
                                       
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                          p.ProductID=@pID 
                                    AND                                      
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";

                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@pID", productID);



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
                                              SUM(po.GrandTotal-pp.PaymentAmount) AS Sum 
                                         
                                    FROM 
                                         PurchaseOrder po           
                                    JOIN
                                        PurchasePayment pp ON po.PurchaseOrderID= pp.PurchaseOrder_ID
                                    JOIN
                                        PurchaseOrderDetail pod ON po.PurchaseOrderID= pod.PurchaseOrder_ID
                                    JOIN 
                                        Product p ON pod.Product_ID = p.ProductID
                                    WHERE 
                                           p.ProductID=@pID 
                                    AND                                     
                                        pp.CreatedAt BETWEEN @StartDate AND @EndDate";

                SqlCommand SumCmd = new SqlCommand(query3, conn1);

                SumCmd.Parameters.AddWithValue("@StartDate", startDate);
                SumCmd.Parameters.AddWithValue("@EndDate", endDate);
                SumCmd.Parameters.AddWithValue("@pID", productID);



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
        private void ExportStatementOfAccount(DataGridView yourDataGridView, string filePath)
        {
            string SupplierName = rjComboBox2.SelectedItem?.ToString();

            int SupplierID;
            string supplierName = "", supplierContact = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"
                                                    SELECT 
                                                            SupplierID
                                                    FROM 
                                                            Supplier 
                                                    WHERE 
                                                            SupplierName = @SupplierName"
                                                    , conn);
                    cmd.Parameters.AddWithValue("@SupplierName", SupplierName);

                    object SupplierIDObj = cmd.ExecuteScalar();

                    if (SupplierIDObj == null)
                    {
                        MessageBox.Show("Supplier not found.");
                        return;
                    }

                    SupplierID = Convert.ToInt32(SupplierIDObj);

                    string query2 = @"
                            SELECT 
                                S.SupplierName, S.ContactNo
                            FROM 
                                PurchaseOrder po
                            JOIN 
                                Supplier S ON po.Supplier_ID = S.SupplierID
                            JOIN 
                                PurchasePayment pp ON po.PurchaseOrderID = pp.PurchaseOrder_ID
                            WHERE 
                                po.Supplier_ID = @Sid";

                    SqlCommand Cmd = new SqlCommand(query2, conn);
                    Cmd.Parameters.AddWithValue("@Sid", SupplierID);

                    SqlDataReader reader = Cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        supplierName = reader["SupplierName"].ToString();
                        supplierContact = reader["ContactNo"].ToString();
                    }
                    reader.Close();
                }

                Document doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                string logoPath = @"C:\Fonts\logo.png";
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(180f, 180f);
                    logo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(logo);
                }

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



                Paragraph address = new Paragraph("99/1, Quarry Road, Dehiwala, Colombo | 9477-1199991 | 9477-8916404", fontText);
                address.Alignment = Element.ALIGN_CENTER;
                doc.Add(address);
                doc.Add(new Paragraph(" "));

                Paragraph title = new Paragraph("SUPPLIER PURCHASE STATEMENT", fontTitle);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                doc.Add(title);

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 3f, 2f });

                PdfPCell leftCell = new PdfPCell { Border = PdfPCell.NO_BORDER };
                Paragraph supplierLabel = new Paragraph("Supplier Details", fontLabel);
                supplierLabel.SpacingAfter = 5f;
                leftCell.AddElement(supplierLabel);
                leftCell.AddElement(new Paragraph($"Name: {supplierName}", fontText));
                leftCell.AddElement(new Paragraph(""));
                headerTable.AddCell(leftCell);

                PdfPCell rightCell = new PdfPCell { Border = PdfPCell.NO_BORDER, PaddingLeft = 85f };
                rightCell.AddElement(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}", fontText));
                headerTable.AddCell(rightCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                string[] columnKeys = { "POID", "Gross_Total", "Discount", "NetAmount", "AmountPaid", "OutstandingAmount", "Date" };

                string[] displayHeaders = { "PO ID", "Gross Total", "Discount", "Net Amount", "Paid", "Outstanding", "Purchase Date" };
                float[] colWidths = {  2f, 2.5f, 2.5f, 2.5f, 2.5f, 3f, 3f };

                PdfPTable table = new PdfPTable(columnKeys.Length);
                table.WidthPercentage = 100;
                table.SetWidths(colWidths);

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


                foreach (DataGridViewRow row in yourDataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        for (int i = 0; i < columnKeys.Length; i++)
                        {
                            string colName = columnKeys[i];

                            if (yourDataGridView.Columns.Contains(colName))
                            {
                                object value = row.Cells[colName].Value ?? "";
                                string text = "";

                                if (colName == "Date" && DateTime.TryParse(value.ToString(), out DateTime dt))
                                {
                                    text = dt.ToString("dd/MM/yyyy");
                                }
                                else if (
                                    
                                    colName == "Gross_Total" ||
                                    colName == "Discount" ||
                                    colName == "NetAmount" ||
                                    colName == "AmountPaid" ||
                                    colName == "OutstandingAmount"
                                )
                                {
                                    if (decimal.TryParse(value.ToString(), out decimal num))
                                    {
                                        text = num.ToString("N2");
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
                            else
                            {
                                table.AddCell(new PdfPCell(new Phrase("", fontTableData))
                                {
                                    Border = PdfPCell.NO_BORDER,
                                    Padding = 5f
                                });
                            }
                        }

                    }
                }

                doc.Add(table);

                doc.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f)));
                doc.Add(new Paragraph(" "));


                string grossTotal = TryFormatRupees(kryptonTextBox5.Text);
                string TotalDiscount = TryFormatRupees(kryptonTextBox1.Text);
                string GrandTotal = TryFormatRupees(kryptonTextBox2.Text);
                string AmountPaid = TryFormatRupees(kryptonTextBox3.Text);
                string OutstandingAMount = TryFormatRupees(kryptonTextBox4.Text);



                PdfPTable summaryTable = new PdfPTable(2);
                summaryTable.WidthPercentage = 50;
                summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                summaryTable.SetWidths(new float[] { 2f, 1f });

                AddSummaryRow(summaryTable, "Gross Total:   ", grossTotal, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Total Discount:", TotalDiscount, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Grand Total:   ", GrandTotal, fontSummaryLabel, fontSummaryValue);



                AddSummaryRow(summaryTable, "Amount Paid:    ", AmountPaid, fontSummaryLabel, fontSummaryValue);
                AddSummaryRow(summaryTable, "Outstanding:     ", OutstandingAMount, fontSummaryLabel, fontSummaryValue);

                doc.Add(summaryTable);

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
                MessageBox.Show("" + ex);
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
        public class SummaryFooter : PdfPageEventHelper
            {
                public PdfPTable SummaryTable { get; set; }

                public override void OnEndPage(PdfWriter writer, Document document)
                {
                    if (SummaryTable == null) return;

                    SummaryTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;

                    float yPos = document.BottomMargin + SummaryTable.TotalHeight + 10;

                    SummaryTable.WriteSelectedRows(0, -1, document.LeftMargin, yPos, writer.DirectContent);
                }
        }

        private void kryptonButton17_Click(object sender, EventArgs e)
        {
            switch (rjComboBox3.SelectedIndex)
            {
                case 0:
                    {
                        try
                        {
                            rjComboBox2.Texts = "";
                            if (rjComboBox2.SelectedItem == null)
                            {
                                MessageBox.Show("Pleaase Select a Name from the ComboBox");
                            }
                            else
                            {


                                DateTime DT = DateTime.Now;
                                string formattedDate = DT.ToString("yyyyMMdd_HHmmss");

                                string filePath = @"C:\reports\StatementofAccount_" + formattedDate + ".pdf";
                                ExportStatementOfAccount(kryptonDataGridView3, filePath);


                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("" + ex);
                        }
                        break;

                    }
                case 1:
                    {
                        try
                        {
                            rjComboBox2.Texts = "";

                            if (rjComboBox2.SelectedItem == null)
                            {
                                MessageBox.Show("Pleaase Select a Valid Name from the ComboBox");
                            }
                            else
                            {


                                DateTime DT = DateTime.Now;
                                string formattedDate = DT.ToString("yyyyMMdd_HHmmss");

                                string filePath = @"C:\reports\StatementofProduct_" + formattedDate + ".pdf";
                                ExportStatementOfProduct(kryptonDataGridView1, filePath);


                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("" + ex);
                        }
                        break;
                        
                    }
            }
        }

        private string TryFormatRupees(string input)
        {
            if (decimal.TryParse(input, out decimal amount))
            {
                return "Rs. " + amount.ToString("N2");
            }
            else
            {
                return input;
            }
        }


        private void rjComboBox2_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            switch (rjComboBox3.SelectedIndex)
            {
                case 0:
                    {

                        string SupplierName = rjComboBox2.SelectedItem.ToString();
                        filldgv4Supplier(SupplierName); ;
                        break;
                    }
                case 1:
                    {
                        string ProductName = rjComboBox2.SelectedItem.ToString();
                        filldgv4Product(ProductName) ; 
                        break;
                    }
            }
        }

        private void kryptonDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void LockDataGridView1()
        {
            kryptonDataGridView1.ReadOnly = true;

            kryptonDataGridView1.AllowUserToAddRows = false;
            kryptonDataGridView1.AllowUserToDeleteRows = false;

            kryptonDataGridView1.AllowUserToResizeRows = false;

            kryptonDataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            kryptonDataGridView1.DefaultCellStyle.SelectionBackColor = kryptonDataGridView1.DefaultCellStyle.BackColor;
            kryptonDataGridView1.DefaultCellStyle.SelectionForeColor = kryptonDataGridView1.DefaultCellStyle.ForeColor;

            kryptonDataGridView1.RowHeadersVisible = false;
            kryptonDataGridView1.EnableHeadersVisualStyles = false;
        }

        private void LockDataGridView3()
        {
            kryptonDataGridView3.ReadOnly = true;

            kryptonDataGridView3.AllowUserToAddRows = false;
            kryptonDataGridView3.AllowUserToDeleteRows = false;

            kryptonDataGridView3.AllowUserToResizeRows = false;

            kryptonDataGridView3.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            kryptonDataGridView3.DefaultCellStyle.SelectionBackColor = kryptonDataGridView3.DefaultCellStyle.BackColor;
            kryptonDataGridView3.DefaultCellStyle.SelectionForeColor = kryptonDataGridView3.DefaultCellStyle.ForeColor;

            kryptonDataGridView3.RowHeadersVisible = false;
            kryptonDataGridView3.EnableHeadersVisualStyles = false;
        }

        private void kryptonDataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

