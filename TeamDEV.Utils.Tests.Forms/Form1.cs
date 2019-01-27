using System;
using System.Windows.Forms;
using TeamDEV.Utils.Forms;

namespace TeamDEV.Utils.Tests.Forms {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            FormTable.Get<Form2>().label1.Text = "Accessing Form2.label1.Text on Form1";
            FormTable.Get<Form2>().Show();
        }
    }
}
