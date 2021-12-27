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

        const int HeightRectLog = 20;

        List<Color> LogColors = new List<Color>() {
            Color.LightBlue,
            Color.Orange,
            Color.Red,
            Color.DarkSeaGreen,
            Color.Green
           
        };

        
        public Form1()
        {

            p = Parcer.GetInstance(@"C:\1", 10);
            InitializeComponent();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            int y = 0;
            int x = 30;
            int i = 0;
            foreach (var pr in p.ReadProcesses)
            {
                DrawString($"R{i}",x-25,y,e);
                DrawProcessLog(x, y, pr, e);
                y += HeightRectLog;
                i++;
            }
            y += HeightRectLog;
            i = 0;
            foreach (var pr in p.WriteProcesses)
            {
                DrawString($"W{i}", x - 25, y, e);
                DrawProcessLog(x, y, pr, e);
                y += HeightRectLog;
                i++;
            }


        }

        private void DrawProcessLog(int x, int y, LogProcess log, PaintEventArgs e)
        {
            int countRecords = log.Records.Count;
            int tempX = x;
            int shift = 0;
            for (int i = 0; i < countRecords; i++)
            {
                LogProcTimes logProc = log.Records[i];
                int[] times = new int[3];
                
                times[0] = logProc.takeSem;
                times[1] = logProc.takeMut;
                times[2] = logProc.work;
              



                for (int j = 0; j < times.Length; j++)
                {
                    shift = (int)(Math.Round(Math.Log((double)times[j], 5.0), 1)*20 );
                    DrawRectangel(tempX, y, LogColors[j], shift, e);
                    tempX +=shift;
                }
                
            }
           
        }



        private void DrawRectangel(int x, int y, Color color, int width, PaintEventArgs e)
        {
            
            SolidBrush blueBrush = new SolidBrush(color);
            Rectangle rect = new Rectangle(x, y, width, HeightRectLog);
            // Fill rectangle to screen.
            e.Graphics.FillRectangle(blueBrush, rect);
        }

        public void DrawString(string drawString,int x, int y,  PaintEventArgs e)
        {


            // Create font and brush.
            Font drawFont = new Font("Arial", 10);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

          
            // Set format of string.
            StringFormat drawFormat = new StringFormat();
            

            // Draw string to screen.
            e.Graphics.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
        }
    }
}
