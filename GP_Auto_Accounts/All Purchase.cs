using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GP_Auto_Accounts
{
    public partial class All_Purchase: Form
    {
        public All_Purchase()
        {
            InitializeComponent();
        }

        private void All_Purchase_Load(object sender, EventArgs e)
        {

        }

        private void kryptonButton3_Click(object sender, EventArgs e)
        {

        }

        private void kryptonButton14_Click(object sender, EventArgs e)
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

        private void kryptonButton4_Click(object sender, EventArgs e)
        {

        }

        private void kryptonDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void kryptonButton8_Click(object sender, EventArgs e)
        {

        }
    }
}
