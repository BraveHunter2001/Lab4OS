using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawerProcess
{
    public partial class Form1 : Form
    {
        Parcer p;
        public Form1()
        {

            p = Parcer.GetInstance(@"C:\1", 6);
            InitializeComponent();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

            DrawRec(0, 0, 100, 10);
        }

        private void DrawRec(int x, int y, int width, int height)
        {
           
        }
    }
}
