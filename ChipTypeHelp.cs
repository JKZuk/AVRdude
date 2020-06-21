using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AVRdude_v1
{
    public partial class ChipTypeHelp : Form
    {
        public ChipTypeHelp()
        {
            InitializeComponent();
        }

        private void ChipTypeHelp_Load(object sender, EventArgs e)
        {
            textChipTypeHelp.ReadOnly = true;
            textChipTypeHelp.SelectionStart = 0;
        }
    }
}
