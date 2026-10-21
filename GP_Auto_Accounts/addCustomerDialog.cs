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
using System.Xml.Linq;

namespace GP_Auto_Accounts
{
    public partial class addCustomerDialog : SmoothRoundedForm
    {
        public addCustomerDialog()
        {
            InitializeComponent();
           
        }

        private void addCustomerDialog_Load(object sender, EventArgs e)
        {

        }

        protected override void OnResize(EventArgs e)
        {
            
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtCustomerName.Text.Trim();
            string contact = txtContactNo.Text.Trim();
            string address = txtAddress.Text.Trim();
            string vehicleNo = txtVehicleNo.Text.Trim();
            string chassisNo = txtChassisNo.Text.Trim();
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(contact) || string.IsNullOrEmpty(vehicleNo))
            {
                MessageBox.Show("Customer name, phone number, and vehicle number are required.");
                return;
            }

            string connectionString = "Server=.\\SQLEXPRESS;Database=AMS;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                   
                string query = "INSERT INTO Customers (CustomerName, ContactNo, Address, VehicleNo, ChassisNo, Description, Updated_By ) VALUES (@CustomerName,@ContactNo, @Address, @VehicleNo, @ChassisNo, @Description, @UpdatedBy)";
         
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerName", name);
                    cmd.Parameters.AddWithValue("@ContactNo", contact);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);
                    cmd.Parameters.AddWithValue("@ChassisNo", chassisNo);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@UpdatedBy", "Admin");

                    conn.Open();
                     int rows = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (rows > 0)
                    {
                        MessageBox.Show("Customer added successfully.");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to add customer.");
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
