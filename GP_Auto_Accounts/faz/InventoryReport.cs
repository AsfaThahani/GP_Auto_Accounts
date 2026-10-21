using DocumentFormat.OpenXml.Office.Word;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices.ComTypes;
using DocumentFormat.OpenXml.Bibliography;

namespace GP_Auto_Accounts
{
    public partial class InventoryReport : Form
    {
        public InventoryReport()
        {
            InitializeComponent();
            fillProductcombobox();
            fillCatogoryComboBox();
            dgvLoad();
            rjDatePicker1.Visible= false;
            rjDatePicker2.Visible= false;
            kryptonLabel6.Visible=false;
            kryptonLabel10.Visible=false;
            


        }
        private readonly string connectionString = "Server=FAZEEN\\SQLEXPRESS;Database=AMS_db;Integrated Security=True;";

        private void rjComboBox1_OnSelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void fillProductcombobox()
        {

            try
            {
                //rjComboBox3.Texts = "";
                //rjComboBox3.Items.Clear();
                rjComboBox2.Items.Insert(0, "-All Categories-");

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

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }
        private void fillCatogoryComboBox()
        {

            try
            {
                //  rjComboBox3.Texts = "";
                // rjComboBox3.Items.Clear();
                rjComboBox3.Items.Insert(0, "-All Categories-");


                string query = "SELECT CategoryName FROM Category";
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    SqlCommand cmd = new SqlCommand(query, sqlConnection);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        rjComboBox3.Items.Add(rdr["CategoryName"].ToString());

                    }
                    rdr.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }












        }

        private void kryptonButton11_Click(object sender, EventArgs e)
        {
            Application.Exit();
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

        private void rjComboBox3_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            rjDatePicker1.Visible = true;
            rjDatePicker2.Visible = true;
            kryptonLabel6.Visible = true;
            kryptonLabel10.Visible = true;

            kryptonDataGridView2.Visible = false;
            kryptonDataGridView1.Visible = true;
            int selectedIndex = rjComboBox3.SelectedIndex;
            switch (selectedIndex)
            { 
                case 0 :
                    kryptonDataGridView2.Visible = true;
                    kryptonDataGridView1.Visible = false;
                    dgvLoad();





                    break;
                        
                default:

                    kryptonDataGridView2.Visible = false;
                    kryptonDataGridView1.Visible = true;
                    dgvLoadOnCategory();
                    rjComboBox2.Texts = "";
                    rjComboBox2.Items.Clear();
                    rjComboBox2.Items.Insert(0, "-All Products-");

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string categoryName = rjComboBox3.SelectedItem.ToString();
                        int CategoryID=0;

                        conn.Open();

                        SqlCommand cmd = new SqlCommand("SELECT CategoryID FROM Category WHERE CategoryName = @name", conn);//to fetch the customerID
                        cmd.Parameters.AddWithValue("@name", categoryName);

                        object categoryIDIDObj = cmd.ExecuteScalar();
                        CategoryID = Convert.ToInt32(categoryIDIDObj);


                        if (categoryIDIDObj == null)
                        {
                            MessageBox.Show("project not found." + CategoryID);
                            return;
                        }


                        string query = "SELECT ProductName FROM Product WHERE Category_ID=@CID";
                        using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                        {
                            sqlConnection.Open();
                            SqlCommand cmd1 = new SqlCommand(query, sqlConnection);
                            cmd1.Parameters.AddWithValue("@CID", CategoryID);
                            SqlDataReader rdr = cmd1.ExecuteReader();
                            while (rdr.Read())
                            {
                                rjComboBox2.Items.Add(rdr["ProductName"].ToString());

                            }
                            rdr.Close();
                        }
                    }

                        break;
            
            
           
            }
        }

        private void dgvLoad()
        {
            string Query = @"
                            SELECT 
                                ProductID,
                                ProductName,
                                PurchaseUnitPrice,
                                Quantity,
                                ThresholdQty
                            FROM Product
                        ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand sqlCommand = new SqlCommand(Query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);

                DataTable dt1 = new DataTable();
                adapter.Fill(dt1);

                // ✅ Add "Date" column to DataTable
                if (!dt1.Columns.Contains("Date"))
                {
                    dt1.Columns.Add("Date", typeof(DateTime));  // Important: must match DataPropertyName
                }

                // ✅ Fill each row with today's date
                foreach (DataRow row in dt1.Rows)
                {
                    row["Date"] = DateTime.Today;
                }

                conn.Close();

                // ✅ Set DataSource
                kryptonDataGridView2.AutoGenerateColumns = false;  // You will map each column
                kryptonDataGridView2.DataSource = dt1;

                // ✅ Map Columns to DataPropertyNames
                kryptonDataGridView2.Columns["productID"].DataPropertyName = "ProductID";
                kryptonDataGridView2.Columns["PName"].DataPropertyName = "ProductName";
                kryptonDataGridView2.Columns["PurchaseUnitPrice"].DataPropertyName = "PurchaseUnitPrice";
                kryptonDataGridView2.Columns["Qty"].DataPropertyName = "Quantity";
                kryptonDataGridView2.Columns["TQty"].DataPropertyName = "ThresholdQty";
                kryptonDataGridView2.Columns["Today"].DataPropertyName = "Date";

                // ✅ Optional: format Date column
                kryptonDataGridView2.Columns["Today"].DefaultCellStyle.Format = "yyyy-MM-dd";
                kryptonDataGridView2.Columns["Today"].ReadOnly = true;
            }

        }



        private void dgvLoadOnCategory()

        {
            string CategoryName = rjComboBox3.SelectedItem.ToString();

            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;
            int CategoryID=0;

            string Query2 = @"
                             SELECT
                                c.CategoryID 
                             FROM
                                Category c
                             WHERE
                                c.CategoryName=@CName
    
                            ";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand sqlCommand = new SqlCommand(Query2, con);
                sqlCommand.Parameters.AddWithValue("@CName", CategoryName);


                object ProductIDObj = sqlCommand.ExecuteScalar();
                CategoryID = Convert.ToInt32(ProductIDObj);
            }

            kryptonDataGridView1.AutoGenerateColumns = false;
            string query = @"
                                SELECT 
                                    Combined.ProductID,
                                    Combined.ProductName,
                                    Combined.CategoryName,
                                    CONVERT(date, Combined.CreatedAt) AS [Date],
                                    Combined.MovementType AS [Movement],
                                    Combined.PurchaseUnitPrice AS [UnitPrice],
                                    Combined.Quantity,
                                    CASE 
                                        WHEN Combined.MovementType = 'IN' THEN Quantity
                                        WHEN Combined.MovementType = 'OUT' THEN -Quantity
                                        ELSE 0
                                    END AS StockChange,
                                    Combined.ThresholdQty
                                FROM (
                                    -- IN transactions
                                    SELECT 
                                        p.ProductID,
                                        p.ProductName,
                                        c.CategoryName,
                                        pp.CreatedAt AS CreatedAt,
                                        p.PurchaseUnitPrice,
                                        pod.Quantity,
                                        'IN' AS MovementType,
                                        p.ThresholdQty
                                    FROM Product p
                                    JOIN Category c ON p.Category_ID = c.CategoryID
                                    JOIN PurchaseOrderDetail pod ON p.ProductID = pod.Product_ID
                                    JOIN PurchaseOrder po ON po.PurchaseOrderID = pod.PurchaseOrder_ID
                                    JOIN PurchasePayment pp ON pp.PurchaseOrder_ID = po.PurchaseOrderID
                                    WHERE pp.CreatedAt BETWEEN @StartDate AND @EndDate
                                      AND c.CategoryID = @CategoryID

                                    UNION ALL

                                    -- OUT transactions
                                    SELECT 
                                        p.ProductID,
                                        p.ProductName,
                                        c.CategoryName,
                                        ip.PaymentDate AS CreatedAt,
                                        p.PurchaseUnitPrice,
                                        id.Quantity,
                                        'OUT' AS MovementType,
                                        p.ThresholdQty
                                    FROM Product p
                                    JOIN Category c ON p.Category_ID = c.CategoryID
                                    JOIN InvoiceDetail id ON p.ProductID = id.Product_ID
                                    JOIN Invoice i ON i.InvoiceID = id.Invoice_ID
                                    JOIN InvoicePayment ip ON ip.Invoice_ID = i.InvoiceID
                                    WHERE ip.PaymentDate BETWEEN @StartDate AND @EndDate
                                      AND c.CategoryID = @CategoryID
                                ) AS Combined
                                ORDER BY Combined.ProductID, Combined.CreatedAt";


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand sqlCommand = new SqlCommand(query, conn);
                

                sqlCommand.Parameters.AddWithValue("@CategoryID", CategoryID);
                sqlCommand.Parameters.AddWithValue("@StartDate", startDate);
                sqlCommand.Parameters.AddWithValue("@EndDate", endDate);

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt1 = new DataTable();
                adapter.Fill(dt1);

                kryptonDataGridView1.DataSource = dt1;
                kryptonDataGridView1.Refresh();

                conn.Close();



            }
            // 1. Execute the SQL command using SqlCommand + SqlDataAdapter
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", CategoryID);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            // 2. Add RunningStock column manually
            if (!dt.Columns.Contains("RunningStock"))
                dt.Columns.Add("RunningStock", typeof(int));

            // 3. Calculate Running Stock per product
            string currentProductId = null;
            int runningStock = 0;

            foreach (DataRow row in dt.Rows)
            {
                string productId = row["ProductID"].ToString();

                if (productId != currentProductId)
                {
                    currentProductId = productId;
                    runningStock = 0;
                }

                int stockChange = Convert.ToInt32(row["StockChange"]);
                runningStock += stockChange;
                row["RunningStock"] = runningStock;
            }

            // 4. Bind to DataGridView
            kryptonDataGridView1.AutoGenerateColumns = false;
            kryptonDataGridView1.DataSource = dt;

            kryptonDataGridView1.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd";


            kryptonDataGridView1.Columns["Date"].DataPropertyName = "Date";
            kryptonDataGridView1.Columns["Movement"].DataPropertyName = "Movement";
            kryptonDataGridView1.Columns["ProductName"].DataPropertyName = "ProductName";
            kryptonDataGridView1.Columns["CategoryName"].DataPropertyName = "CategoryName";
            kryptonDataGridView1.Columns["UnitPrice"].DataPropertyName = "UnitPrice";
            kryptonDataGridView1.Columns["Quantity"].DataPropertyName = "Quantity";
            kryptonDataGridView1.Columns["RunningStock"].DataPropertyName = "RunningStock";
            kryptonDataGridView1.Columns["ThresholdQty"].DataPropertyName = "ThresholdQty";




        }


        private void dgvLoadOnOnlyCategory(string Productname)

        {
            int ProductID;
            
            string Query1 = @"
                             SELECT
                                p.ProductID 
                             FROM
                                Product p
                             WHERE
                                p.ProductName=@PName
    
                            ";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand sqlCommand = new SqlCommand(Query1, con);
                sqlCommand.Parameters.AddWithValue("@PName", Productname);


                object productIDObj = sqlCommand.ExecuteScalar();
                ProductID = Convert.ToInt32(productIDObj);
            }
            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;
            kryptonDataGridView1.AutoGenerateColumns = false;
            string query = @"
                            SELECT 
                                Combined.ProductID,
                                Combined.ProductName,
                                Combined.CategoryName,
                                Combined.CreatedAt AS [Date],
                                Combined.MovementType AS [Movement],
                                Combined.PurchaseUnitPrice AS [UnitPrice],
                                Combined.Quantity,
                                CASE 
                                    WHEN Combined.MovementType = 'IN' THEN Quantity
                                    WHEN Combined.MovementType = 'OUT' THEN -Quantity
                                    ELSE 0
                                END AS StockChange,
                                Combined.ThresholdQty
                            FROM (
                                -- IN transactions
                                SELECT 
                                    p.ProductID,
                                    p.ProductName,
                                    c.CategoryName,
                                    pp.CreatedAt AS CreatedAt,
                                    p.PurchaseUnitPrice,
                                    pod.Quantity,
                                    'IN' AS MovementType,
                                    p.ThresholdQty
                                FROM Product p
                                JOIN Category c ON p.Category_ID = c.CategoryID
                                JOIN PurchaseOrderDetail pod ON p.ProductID = pod.Product_ID
                                JOIN PurchaseOrder po ON po.PurchaseOrderID = pod.PurchaseOrder_ID
                                JOIN PurchasePayment pp ON pp.PurchaseOrder_ID = po.PurchaseOrderID
                                WHERE pp.CreatedAt BETWEEN @StartDate AND @EndDate
                                AND p.ProductID=@pid

                                UNION ALL

                                -- OUT transactions
                                SELECT 
                                    p.ProductID,
                                    p.ProductName,
                                    c.CategoryName,
                                    ip.PaymentDate AS CreatedAt,
                                    p.PurchaseUnitPrice,
                                    id.Quantity,
                                    'OUT' AS MovementType,
                                    p.ThresholdQty
                                FROM Product p
                                JOIN Category c ON p.Category_ID = c.CategoryID
                                JOIN InvoiceDetail id ON p.ProductID = id.Product_ID
                                JOIN Invoice i ON i.InvoiceID = id.Invoice_ID
                                JOIN InvoicePayment ip ON ip.Invoice_ID = i.InvoiceID
                                WHERE ip.PaymentDate BETWEEN @StartDate AND @EndDate
                                AND p.ProductID=@pid

                            ) AS Combined
                            ORDER BY Combined.ProductID, Combined.CreatedAt";


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand sqlCommand = new SqlCommand(query, conn);
                sqlCommand.Parameters.AddWithValue("@pid", ProductID);
                sqlCommand.Parameters.AddWithValue("@StartDate", startDate);
                sqlCommand.Parameters.AddWithValue("@EndDate", endDate);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt1 = new DataTable();
                adapter.Fill(dt1);

                kryptonDataGridView1.DataSource = dt1;
                kryptonDataGridView1.Refresh();

                conn.Close();



            }
            // 1. Execute the SQL command using SqlCommand + SqlDataAdapter
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@pid", ProductID);

                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            // 2. Add RunningStock column manually
            if (!dt.Columns.Contains("RunningStock"))
                dt.Columns.Add("RunningStock", typeof(int));

            // 3. Calculate Running Stock per product
            string currentProductId = null;
            int runningStock = 0;

            foreach (DataRow row in dt.Rows)
            {
                string productId = row["ProductID"].ToString();

                if (productId != currentProductId)
                {
                    currentProductId = productId;
                    runningStock = 0;
                }

                int stockChange = Convert.ToInt32(row["StockChange"]);
                runningStock += stockChange;
                row["RunningStock"] = runningStock;
            }

            // 4. Bind to DataGridView
            kryptonDataGridView1.AutoGenerateColumns = false;
            kryptonDataGridView1.DataSource = dt;

            kryptonDataGridView1.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd";


            kryptonDataGridView1.Columns["Date"].DataPropertyName = "Date";
            kryptonDataGridView1.Columns["Movement"].DataPropertyName = "Movement";
            kryptonDataGridView1.Columns["ProductName"].DataPropertyName = "ProductName";
            kryptonDataGridView1.Columns["CategoryName"].DataPropertyName = "CategoryName";
            kryptonDataGridView1.Columns["UnitPrice"].DataPropertyName = "UnitPrice";
            kryptonDataGridView1.Columns["Quantity"].DataPropertyName = "Quantity";
            kryptonDataGridView1.Columns["RunningStock"].DataPropertyName = "RunningStock";
            kryptonDataGridView1.Columns["ThresholdQty"].DataPropertyName = "THresholdQty";




        }

        private void dgvLoadOnCategoryAndProduct(string ProductName,string CategoryName)

        {
            int ProductID;
            int categoryID;

            //to find the ProductID
            string Query1 = @"
                             SELECT
                                p.ProductID 
                             FROM
                                Product p
                             WHERE
                                p.ProductName=@PName
    
                            ";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand sqlCommand = new SqlCommand(Query1, con);
                sqlCommand.Parameters.AddWithValue("@PName", ProductName);


                object productIDObj = sqlCommand.ExecuteScalar();
                ProductID = Convert.ToInt32(productIDObj);
            }

            //to find the CategoryID
            string Query2 = @"
                             SELECT
                                c.CategoryID 
                             FROM
                                Category c
                             WHERE
                                c.CategoryName=@CName
    
                            ";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand sqlCommand = new SqlCommand(Query2, con);
                sqlCommand.Parameters.AddWithValue("@CName", CategoryName);


                object ProductIDObj = sqlCommand.ExecuteScalar();
                categoryID = Convert.ToInt32(ProductIDObj);
            }


            DateTime startDate = rjDatePicker1.Value;
            DateTime endDate = rjDatePicker2.Value;
            kryptonDataGridView1.AutoGenerateColumns = false;
            string query = @"
                            SELECT 
                                Combined.ProductID,
                                Combined.ProductName,
                                Combined.CategoryName,
                                Combined.CreatedAt AS [Date],
                                Combined.MovementType AS [Movement],
                                Combined.PurchaseUnitPrice AS [UnitPrice],
                                Combined.Quantity,
                                CASE 
                                    WHEN Combined.MovementType = 'IN' THEN Quantity
                                    WHEN Combined.MovementType = 'OUT' THEN -Quantity
                                    ELSE 0
                                END AS StockChange,
                                Combined.ThresholdQty
                            FROM (
                                -- IN transactions
                                SELECT 
                                    p.ProductID,
                                    p.ProductName,
                                    c.CategoryName,
                                    pp.CreatedAt AS CreatedAt,
                                    p.PurchaseUnitPrice,
                                    pod.Quantity,
                                    'IN' AS MovementType,
                                    p.ThresholdQty
                                FROM Product p
                                JOIN Category c ON p.Category_ID = c.CategoryID
                                JOIN PurchaseOrderDetail pod ON p.ProductID = pod.Product_ID
                                JOIN PurchaseOrder po ON po.PurchaseOrderID = pod.PurchaseOrder_ID
                                JOIN PurchasePayment pp ON pp.PurchaseOrder_ID = po.PurchaseOrderID
                                WHERE pp.CreatedAt BETWEEN @StartDate AND @EndDate
                                AND p.ProductID=@pid
                                AND c.CategoryID=@cid

                                UNION ALL

                                -- OUT transactions
                                SELECT 
                                    p.ProductID,
                                    p.ProductName,
                                    c.CategoryName,
                                    ip.PaymentDate AS CreatedAt,
                                    p.PurchaseUnitPrice,
                                    id.Quantity,
                                    'OUT' AS MovementType,
                                    p.ThresholdQty
                                FROM Product p
                                JOIN Category c ON p.Category_ID = c.CategoryID
                                JOIN InvoiceDetail id ON p.ProductID = id.Product_ID
                                JOIN Invoice i ON i.InvoiceID = id.Invoice_ID
                                JOIN InvoicePayment ip ON ip.Invoice_ID = i.InvoiceID
                                WHERE ip.PaymentDate BETWEEN @StartDate AND @EndDate
                                AND p.ProductID=@pid
                                AND c.CategoryID=@cid

                            ) AS Combined
                            ORDER BY Combined.ProductID, Combined.CreatedAt";


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand sqlCommand = new SqlCommand(query, conn);
                sqlCommand.Parameters.AddWithValue("@pid", ProductID);
                sqlCommand.Parameters.AddWithValue("@cid", categoryID);
                sqlCommand.Parameters.AddWithValue("@StartDate", startDate);
                sqlCommand.Parameters.AddWithValue("@EndDate", endDate);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt1 = new DataTable();
                adapter.Fill(dt1);

                kryptonDataGridView1.DataSource = dt1;
                kryptonDataGridView1.Refresh();

                conn.Close();



            }
            // 1. Execute the SQL command using SqlCommand + SqlDataAdapter
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@pid", ProductID);
                    cmd.Parameters.AddWithValue("@cid", categoryID);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            // 2. Add RunningStock column manually
            if (!dt.Columns.Contains("RunningStock"))
                dt.Columns.Add("RunningStock", typeof(int));

            // 3. Calculate Running Stock per product
            string currentProductId = null;
            int runningStock = 0;

            foreach (DataRow row in dt.Rows)
            {
                string productId = row["ProductID"].ToString();

                if (productId != currentProductId)
                {
                    currentProductId = productId;
                    runningStock = 0;
                }

                int stockChange = Convert.ToInt32(row["StockChange"]);
                runningStock += stockChange;
                row["RunningStock"] = runningStock;
            }

            // 4. Bind to DataGridView
            kryptonDataGridView1.AutoGenerateColumns = false;
            kryptonDataGridView1.DataSource = dt;

            kryptonDataGridView1.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd";


            kryptonDataGridView1.Columns["Date"].DataPropertyName = "Date";
            kryptonDataGridView1.Columns["Movement"].DataPropertyName = "Movement";
            kryptonDataGridView1.Columns["ProductName"].DataPropertyName = "ProductName";
            kryptonDataGridView1.Columns["CategoryName"].DataPropertyName = "CategoryName";
            kryptonDataGridView1.Columns["UnitPrice"].DataPropertyName = "UnitPrice";
            kryptonDataGridView1.Columns["Quantity"].DataPropertyName = "Quantity";
            kryptonDataGridView1.Columns["RunningStock"].DataPropertyName = "RunningStock";
            kryptonDataGridView1.Columns["ThresholdQty"].DataPropertyName = "THresholdQty";




        }



        private void rjComboBox2_OnSelectedIndexChanged(object sender, EventArgs e)
        {

            rjDatePicker1.Visible = true;
            rjDatePicker2.Visible = true;
            kryptonLabel6.Visible = true;
            kryptonLabel10.Visible = true;

            kryptonDataGridView2.Visible = false;
            kryptonDataGridView1.Visible = true;
            int selectedIndex = rjComboBox2.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:

                    dgvLoadOnCategory();
                    break;

                default:

                    if (rjComboBox3.SelectedItem == null)
                    {
                        string Productname = rjComboBox2.SelectedItem.ToString();

                        dgvLoadOnOnlyCategory(Productname);
                    }
                    else
                    {
                        string Productname = rjComboBox2.SelectedItem.ToString();

                        string categoryName = rjComboBox3.SelectedItem.ToString();

                        dgvLoadOnCategoryAndProduct(Productname, categoryName);
                    }
                    break;
            }
        }

        private void ExportStatementOfInventorySummery(DataGridView yourDataGridView, string filePath)
        {
            try
            {
                // Start PDF creation
                iTextSharp.text.Document doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
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
                // Add logo


                // Company info
                Paragraph address = new Paragraph("99/1, Quarry Road, Dehiwala, Colombo | 9477-1199991 | 9477-8916404", fontText);
                address.Alignment = Element.ALIGN_CENTER;
                doc.Add(address);
                doc.Add(new Paragraph(" "));

                // Title
                Paragraph title = new Paragraph("INVENTORY SUMMERY REPORT", fontTitle);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                doc.Add(title);

                // Supplier Info Header
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 3f, 2f });

                PdfPCell leftCell = new PdfPCell { Border = PdfPCell.NO_BORDER };
                Paragraph ProductLabel = new Paragraph("Product Details", fontLabel);
                ProductLabel.SpacingAfter = 5f;
                leftCell.AddElement(ProductLabel);
                headerTable.AddCell(leftCell);

                PdfPCell rightCell = new PdfPCell { Border = PdfPCell.NO_BORDER, PaddingLeft = 85f };
                rightCell.AddElement(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}", fontText));
                headerTable.AddCell(rightCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                // Table headers
                // These are the keys used to match DataGridView column names
                string[] columnKeys = { "Today","ProductID","PName",  "PurchaseUnitPrice", "Qty", "TQty" };

                // These are the display names you want to show in the PDF
                string[] displayHeaders = { "Date ", "Product N0", "ProductName", "Unit Price", "Current Stock", "Threshold Stock" };
                float[] colWidths = { 4f, 2.5f, 4f, 2.5f, 2.5f, 2.5f };

                PdfPTable table = new PdfPTable(columnKeys.Length);
                table.WidthPercentage = 100;
                table.SetWidths(colWidths);

                // Table headers (your existing code)
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




                // Add data rows (fixed)
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

                                // Format dates
                                if (colName == "Date" && DateTime.TryParse(value.ToString(), out DateTime dt))
                                {
                                    text = dt.ToString("dd/MM/yyyy");
                                }
                                // Format numbers with commas for currency-like columns
                                else if (
                                    colName == "PurchaseUnitPrice"

                                )
                                {
                                    if (decimal.TryParse(value.ToString(), out decimal num))
                                    {
                                        text = num.ToString("N2"); // N2 = comma + 2 decimals
                                    }
                                    else
                                    {
                                        text = value.ToString(); // fallback if parsing fails
                                    }
                                }
                                else
                                {
                                    text = value.ToString(); // normal text
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

                    // Add horizontal line before summary section
                    doc.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f)));
                    doc.Add(new Paragraph(" "));




                    // Add footer
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

        private void ExportStatementOfCategory(DataGridView yourDataGridView, string filePath)
        {
            string ProductName = rjComboBox2.SelectedItem?.ToString();

            int ProductID;
            string productName = "", ProductCategory = "";

            try
            {
                // Load supplier info from DB
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();


                    ProductCategory=rjComboBox3.SelectedItem?.ToString();


                   
                }

                // Start PDF creation
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
                // Add logo


                // Company info
                Paragraph address = new Paragraph("99/1, Quarry Road, Dehiwala, Colombo | 9477-1199991 | 9477-8916404", fontText);
                address.Alignment = Element.ALIGN_CENTER;
                doc.Add(address);
                doc.Add(new Paragraph(" "));

                // Title
                Paragraph title = new Paragraph("CATEGORY BASED INVENTORY STATEMENT", fontTitle);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                doc.Add(title);

                // Supplier Info Header
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 3f, 2f });

                PdfPCell leftCell = new PdfPCell { Border = PdfPCell.NO_BORDER };
                Paragraph ProductLabel = new Paragraph("Product Details", fontLabel);
                ProductLabel.SpacingAfter = 5f;
                leftCell.AddElement(ProductLabel);
                leftCell.AddElement(new Paragraph($"Category Name: {ProductCategory}", fontText));
                headerTable.AddCell(leftCell);

                PdfPCell rightCell = new PdfPCell { Border = PdfPCell.NO_BORDER, PaddingLeft = 85f };
                rightCell.AddElement(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}", fontText));
                headerTable.AddCell(rightCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                // Table headers
                // These are the keys used to match DataGridView column names
                string[] columnKeys = { "Date","Movement","ProductName", "CategoryName", "UnitPrice", "Quantity","RunningStock", "ThresholdQty" };

                // These are the display names you want to show in the PDF
                string[] displayHeaders = { "Date", "Movement", "Product Name", "Category Name", "Unit Price", "Quantity", "Running Stock", "Threshold Stock" };
                float[] colWidths = { 3f, 1.5f, 3f, 3f, 2.5f, 3f,1f,1f };

                PdfPTable table = new PdfPTable(columnKeys.Length);
                table.WidthPercentage = 100;
                table.SetWidths(colWidths);

                // Table headers (your existing code)
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




                // Add data rows (fixed)
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

                                // Format dates
                                if (colName == "Date" && DateTime.TryParse(value.ToString(), out DateTime dt))
                                {
                                    text = dt.ToString("dd/MM/yyyy");
                                }
                                // Format numbers with commas for currency-like columns
                                else if (
                                    colName == "UnitPrice"

                                )
                                {
                                    if (decimal.TryParse(value.ToString(), out decimal num))
                                    {
                                        text = num.ToString("N2"); // N2 = comma + 2 decimals
                                    }
                                    else
                                    {
                                        text = value.ToString(); // fallback if parsing fails
                                    }
                                }
                                else
                                {
                                    text = value.ToString(); // normal text
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

                    // Add horizontal line before summary section
                    doc.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f)));
                    doc.Add(new Paragraph(" "));




                    // Add footer
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

        private void ExportStatementOfProduct(DataGridView yourDataGridView, string filePath)
        {
            string ProductName = rjComboBox2.SelectedItem?.ToString();

            string productName = "", ProductCategory = "";

            
            try
            {
               ;


                ProductCategory = rjComboBox3.SelectedItem?.ToString();
                productName=rjComboBox2.SelectedItem?.ToString();   


                

                // Start PDF creation
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
                // Add logo


                // Company info
                Paragraph address = new Paragraph("99/1, Quarry Road, Dehiwala, Colombo | 9477-1199991 | 9477-8916404", fontText);
                address.Alignment = Element.ALIGN_CENTER;
                doc.Add(address);
                doc.Add(new Paragraph(" "));

                // Title
                Paragraph title = new Paragraph("PRODUCT BASED INVENTORY STATEMENT", fontTitle);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                doc.Add(title);

                // Supplier Info Header
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 3f, 2f });

                PdfPCell leftCell = new PdfPCell { Border = PdfPCell.NO_BORDER };
                Paragraph ProductLabel = new Paragraph("Product Details", fontLabel);
                ProductLabel.SpacingAfter = 5f;
                leftCell.AddElement(ProductLabel);
                leftCell.AddElement(new Paragraph($"Category Name: {ProductCategory}", fontText));
                leftCell.AddElement(new Paragraph($"Product Name: {productName}", fontText));
                headerTable.AddCell(leftCell);

                PdfPCell rightCell = new PdfPCell { Border = PdfPCell.NO_BORDER, PaddingLeft = 85f };
                rightCell.AddElement(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}", fontText));
                headerTable.AddCell(rightCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                // Table headers
                // These are the keys used to match DataGridView column names
                string[] columnKeys = { "Date", "Movement", "ProductName", "CategoryName", "UnitPrice", "Quantity", "RunningStock", "ThresholdQty" };

                // These are the display names you want to show in the PDF
                string[] displayHeaders = { "Date", "Movement", "Product Name", "Category Name", "Unit Price", "Quantity", "Running Stock", "Threshold Stock" };
                float[] colWidths = { 3f, 1.5f, 3f, 3f, 2.5f, 3f, 1f, 1f };

                PdfPTable table = new PdfPTable(columnKeys.Length);
                table.WidthPercentage = 100;
                table.SetWidths(colWidths);

                // Table headers (your existing code)
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




                // Add data rows (fixed)
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

                                // Format dates
                                if (colName == "Date" && DateTime.TryParse(value.ToString(), out DateTime dt))
                                {
                                    text = dt.ToString("dd/MM/yyyy");
                                }
                                // Format numbers with commas for currency-like columns
                                else if (
                                    colName == "UnitPrice"

                                )
                                {
                                    if (decimal.TryParse(value.ToString(), out decimal num))
                                    {
                                        text = num.ToString("N2"); // N2 = comma + 2 decimals
                                    }
                                    else
                                    {
                                        text = value.ToString(); // fallback if parsing fails
                                    }
                                }
                                else
                                {
                                    text = value.ToString(); // normal text
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

                // Add horizontal line before summary section
                doc.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0f)));
                doc.Add(new Paragraph(" "));




                // Add footer
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

        private void kryptonDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void kryptonGroupBox2_Panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void kryptonButton18_Click(object sender, EventArgs e)
        {
           int selectedindex= rjComboBox3.SelectedIndex;
            int selectedindex1 = rjComboBox2.SelectedIndex;

            if (rjComboBox2.SelectedItem == null && rjComboBox3.SelectedItem == null||selectedindex==0)
            {
                DateTime DT = DateTime.Now;
                string formattedDate = DT.ToString("yyyyMMdd_HHmmss");

                string filePath = @"C:\reports\InventoryReport\SummeryReport" + formattedDate + ".pdf";
                try
                {
                    ExportStatementOfInventorySummery(kryptonDataGridView2, filePath);
                    MessageBox.Show("PDF exported successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(""+ex);

                }
            }
            else
            {
                if (rjComboBox2.SelectedItem == null || selectedindex1 == 0)
                {
                    DateTime DT = DateTime.Now;
                    string formattedDate = DT.ToString("yyyyMMdd_HHmmss");

                    string filePath = @"C:\reports\InventoryReport\CategoryBasedReport" + formattedDate + ".pdf";
                    try
                    {
                        ExportStatementOfCategory(kryptonDataGridView1, filePath);
                        MessageBox.Show("PDF exported successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("" + ex);

                    }
                }
                else
                if (rjComboBox3.SelectedItem == null)
                {
                    DateTime DT = DateTime.Now;
                    string formattedDate = DT.ToString("yyyyMMdd_HHmmss");

                    string filePath = @"C:\reports\InventoryReport\ProductBasedReport" + formattedDate + ".pdf";
                    try
                    {
                        ExportStatementOfProduct(kryptonDataGridView1, filePath);
                        MessageBox.Show("PDF exported successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("" + ex);

                    }
                }
                else
                {
                    DateTime DT = DateTime.Now;
                    string formattedDate = DT.ToString("yyyyMMdd_HHmmss");

                    string filePath = @"C:\reports\InventoryReport\ProductBasedReport" + formattedDate + ".pdf";
                    try
                    {
                        ExportStatementOfProduct(kryptonDataGridView1, filePath);
                        MessageBox.Show("PDF exported successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("" + ex);

                    }


                }
            }
        }

        private void kryptonDataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void rjDatePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
