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
    public partial class addItemDialog: SmoothRoundedForm
    {
        public addItemDialog()
        {
            InitializeComponent();
        }

        private void addItemDialog_Load(object sender, EventArgs e)
        {

        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string item = txtItemName.Text.Trim();
            string partNo = txtPartNo.Text.Trim();
            string salePrice = txtSalePrice.Text.Trim();
            string purchasePrice = txtPurchasePrice.Text.Trim();
            string qty = txtQuantity.Text.Trim();
            string thresholdQty = txtThresholdQty.Text.Trim();
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(item))
            {
                MessageBox.Show("Item Name is required");
                return;
            }

            string connectionString = "Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                string query = "INSERT INTO Products (ProductName,ProductID,SaleUnitPrice,PurchaseUnitPrice,Quantity,ThresholdQty,Description, Updated_By ) VALUES (@itemName, @partNo, @salePrice, @purchasePrice, @qty, @thresholdQty, @description, @UpdatedBy )";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@itemName", item);
                    cmd.Parameters.AddWithValue("@partNo", partNo);
                    cmd.Parameters.AddWithValue("@salePrice", salePrice);
                    cmd.Parameters.AddWithValue("@purchasePrice", purchasePrice);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@thresholdQty", thresholdQty);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@UpdatedBy", 1);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (rows > 0)
                    {
                        MessageBox.Show("Item added successfully.");
                        this.Close(); // or clear fields
                    }
                    else
                    {
                        MessageBox.Show("Failed to add Item.");
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
