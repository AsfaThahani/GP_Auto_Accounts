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

            Purchase_Payment invoiceForm = new Purchase_Payment();

            panel1.Controls.Add(invoiceForm.panel1);

            panel2.Controls.Add(invoiceForm.panel2);

            invoiceForm.panel1.Dock = DockStyle.Top;
            invoiceForm.panel2.Dock = DockStyle.Fill;
        }

        private void kryptonButton12_Click(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            NewPurchase purchaseForm = new NewPurchase();

            panel1.Controls.Add(purchaseForm.panel1);

            panel2.Controls.Add(purchaseForm.panel2);

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
