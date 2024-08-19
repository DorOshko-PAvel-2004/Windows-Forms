using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab3_2
{
    internal class Car
    {
        internal PictureBox CarElement;
        internal bool LeftRight;//true/false
        internal bool stopping;
        public Car(Point point, bool moving = false)
        {
            CarElement = new PictureBox
            {
                Size = new Size(50, 50),
                BackColor = Color.Red,
                Location = point
            }; 
            LeftRight = moving;
        }
    }
}
