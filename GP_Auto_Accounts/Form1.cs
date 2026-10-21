using Krypton.Toolkit;
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
using System.Windows.Forms.DataVisualization.Charting;

namespace GP_Auto_Accounts
{
    public partial class Form1 : SmoothRoundedForm
    {
      
        public Form1()
        {
            InitializeComponent();
            LoadMonthlySalesChart();
            LoadMonthlyPurchasesChart();
            SetActiveKryptonButton(kryptonButton1);
            LoadExpenseTypePieChart();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadMonthlySalesChart();
            LoadMonthlyPurchasesChart();
            SetActiveKryptonButton(kryptonButton1);
            LoadExpenseTypePieChart();
        }

        private KryptonButton currentActiveButton = null;

        private void SetActiveKryptonButton(KryptonButton clickedButton)
        {
            if (currentActiveButton != null && currentActiveButton != clickedButton)
            {
                currentActiveButton.StateCommon.Back.Color1 = Color.Transparent;
                currentActiveButton.StateCommon.Back.Color2 = Color.Transparent;
                currentActiveButton.OverrideDefault.Back.Color1 = Color.Transparent;
                currentActiveButton.OverrideDefault.Back.Color2 = Color.Transparent;
                currentActiveButton.StateCommon.Content.ShortText.Color1 = Color.FromArgb(0, 0, 192);
            }

            clickedButton.StateCommon.Back.Color1 = Color.DeepSkyBlue;
            clickedButton.StateCommon.Back.Color2 = Color.FromArgb(0, 0, 192);
            clickedButton.StateCommon.Back.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            clickedButton.StateCommon.Back.ColorAngle = 0f;
            clickedButton.OverrideDefault.Back.Color1 = Color.DeepSkyBlue;
            clickedButton.OverrideDefault.Back.Color2 = Color.FromArgb(0, 0, 192);
            clickedButton.StateCommon.Content.ShortText.Color1 = Color.White;


            currentActiveButton = clickedButton;

        }


        private void LoadMonthlySalesChart()
        {
            string query = @"SELECT FORMAT(CreatedAt, 'yyyy-MM') AS [Month], 
                            SUM(GrandTotal) AS [Total] 
                     FROM Invoice 
                     GROUP BY FORMAT(CreatedAt, 'yyyy-MM') 
                     ORDER BY [Month];";

            using (SqlConnection con = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                chartSales.Series.Clear();
                chartSales.ChartAreas[0].AxisX.Title = "Month";
                chartSales.ChartAreas[0].AxisY.Title = "Sales (Rs)";
                chartSales.ChartAreas[0].AxisX.LabelStyle.Angle = -45;

                var series = new Series("Monthly Sales");
                series.ChartType = SeriesChartType.Column;
                series.Color = Color.MediumSlateBlue;
                series.IsValueShownAsLabel = true;

                while (reader.Read())
                {
                    string month = reader["Month"].ToString();
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    series.Points.AddXY(month, total);
                }

                chartSales.Series.Add(series);
            }
        }

        private void LoadMonthlyPurchasesChart()
        {
            string query = @"
        SELECT FORMAT(CreatedAt, 'yyyy-MM') AS [Month], 
               SUM(GrandTotal) AS [Total] 
        FROM PurchaseOrder 
        GROUP BY FORMAT(CreatedAt, 'yyyy-MM') 
        ORDER BY [Month];";

            using (SqlConnection con = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                chartPurchases.Series.Clear();
                chartPurchases.ChartAreas[0].AxisX.Title = "Month";
                chartPurchases.ChartAreas[0].AxisY.Title = "Purchases (Rs)";
                chartPurchases.ChartAreas[0].AxisX.LabelStyle.Angle = -45;

                var series = new Series("Monthly Purchases");
                series.ChartType = SeriesChartType.Column;
                series.Color = Color.DarkSeaGreen;
                series.IsValueShownAsLabel = true;

                while (reader.Read())
                {
                    string month = reader["Month"].ToString();
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    series.Points.AddXY(month, total);
                }

                chartPurchases.Series.Add(series);
            }
        }

        private void LoadExpenseTypePieChart()
        {
            chartExpenses.Series.Clear();
            chartExpenses.Titles.Clear();

            string query = @"
        SELECT EC.CategoryName, SUM(E.TotalAmount) AS TotalAmount
        FROM Expense E
        INNER JOIN ExpenseCategory EC ON E.Category_ID = EC.CategoryID
        GROUP BY EC.CategoryName
        ORDER BY TotalAmount DESC;";

            using (SqlConnection conn = new SqlConnection("Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                Series series = new Series("Expenses");
                series.ChartType = SeriesChartType.Pie;
                series["PieLabelStyle"] = "Outside";
                series.BorderColor = Color.Black;

                foreach (DataRow row in dt.Rows)
                {
                    string category = row["CategoryName"].ToString();
                    decimal total = Convert.ToDecimal(row["TotalAmount"]);
                    series.Points.AddXY(category, total);
                }

                chartExpenses.Series.Add(series);
                chartExpenses.Legends[0].Enabled = true;
                chartExpenses.Titles.Add("Expenses by Category");
            }
        }

    



        private void kryptonButton1_Click(object sender, EventArgs e)
        {
            SetActiveKryptonButton(kryptonButton1);

            panel1.Controls.Clear();
            panel2.Controls.Clear();

            Form1 dashboard = new Form1();

            panel1.Controls.Add(dashboard.panel1);

            panel2.Controls.Add(dashboard.panel2);

            dashboard.panel1.Dock = DockStyle.Top;
            dashboard.panel2.Dock = DockStyle.Fill;
        }




        private void kryptonButton2_Click(object sender, EventArgs e)
        {
            SetActiveKryptonButton((KryptonButton)sender);
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            Invoice invoiceForm = new Invoice();

            panel1.Controls.Add(invoiceForm.panel1);

            panel2.Controls.Add(invoiceForm.panel2);

            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;

        }

      


        private void kryptonButton3_Click_1(object sender, EventArgs e)
        {
            SetActiveKryptonButton((KryptonButton)sender);

            panel1.Controls.Clear();
            panel2.Controls.Clear();

           
            NewPurchase purchaseForm = new NewPurchase();

 
            panel1.Controls.Add(purchaseForm.panel1);

            panel2.Controls.Add(purchaseForm.panel2);

            
            purchaseForm.panel1.Dock = DockStyle.Top;
            purchaseForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton4_Click_1(object sender, EventArgs e)
        {
            SetActiveKryptonButton((KryptonButton)sender);

            panel1.Controls.Clear();
            panel2.Controls.Clear();

        
            Customer purchaseForm = new Customer();

         
            panel1.Controls.Add(purchaseForm.panel1);

         
            panel2.Controls.Add(purchaseForm.panel2);

       
            purchaseForm.panel1.Dock = DockStyle.Top;
            purchaseForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton5_Click_1(object sender, EventArgs e)
        {
            SetActiveKryptonButton((KryptonButton)sender);

            panel1.Controls.Clear();
            panel2.Controls.Clear();

       
            Suppliers invoiceForm = new Suppliers();

         
            panel1.Controls.Add(invoiceForm.panel1);

            panel2.Controls.Add(invoiceForm.panel2);

            
            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton6_Click_1(object sender, EventArgs e)
        {
            SetActiveKryptonButton((KryptonButton)sender);

            panel1.Controls.Clear();
            panel2.Controls.Clear();

          
            Products invoiceForm = new Products();

        
            panel1.Controls.Add(invoiceForm.panel1);

          
            panel2.Controls.Add(invoiceForm.panel2);

       
            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }


        private void kryptonButton8_Click_1(object sender, EventArgs e)
        {
            SetActiveKryptonButton((KryptonButton)sender);

            panel1.Controls.Clear();
            panel2.Controls.Clear();

          
            Settings purchaseForm = new Settings();

            
            panel1.Controls.Add(purchaseForm.panel1);

           
            panel2.Controls.Add(purchaseForm.panel2);

            purchaseForm.panel1.Dock = DockStyle.Top;
            purchaseForm.panel2.Dock = DockStyle.Fill;
        }


        private void kryptonButton13_Click(object sender, EventArgs e)
        {
            SetActiveKryptonButton((KryptonButton)sender);

            panel1.Controls.Clear();
            panel2.Controls.Clear();

          
            Expense expenseForm = new Expense();

            panel1.Controls.Add(expenseForm.panel1);

            panel2.Controls.Add(expenseForm.panel2);

        
            expenseForm.panel1.Dock = DockStyle.Top;
            expenseForm.panel2.Dock = DockStyle.Fill;
        }



        private void kryptonButton11_Click_2(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void kryptonButton7_Click(object sender, EventArgs e)
        {
            SetActiveKryptonButton((KryptonButton)sender);

            panel1.Controls.Clear();
            panel2.Controls.Clear();

            salesReport salesReportForm = new salesReport();



            panel1.Controls.Add(salesReportForm.panel1);

            panel2.Controls.Add(salesReportForm.panel2);

            salesReportForm.panel1.Dock = DockStyle.Top;
            salesReportForm.panel2.Dock = DockStyle.Fill;
        }

    }
}