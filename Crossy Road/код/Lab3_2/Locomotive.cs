using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab3_2
{
    internal class Locomotive
    {
        internal PictureBox carriage;
        internal bool Left = true;
        internal bool Right = false;
        internal bool Up = false;
        internal bool Down = false;
        private Panel Way;
        internal Locomotive(Point point, Panel way)
        {
            carriage = new PictureBox
            {
                Location = point,
                BackColor = Color.Silver,
                Size = new Size(30, 30)
            };
            Way = way;
        }
        internal void LocomotiveMoving()
        {
            if (Left && carriage.Location.X <= Way.Location.X && carriage.Location.Y <= Way.Location.Y)
            {
                Left = false;
                Down = true;
            }
            else if (Down && carriage.Location.Y >= Way.Location.Y+Way.Size.Height && carriage.Location.X <= Way.Location.X)
            {
                Down = false;
                Right = true;
            }
            else if (Right && carriage.Location.Y >= Way.Location.Y + Way.Size.Height && carriage.Location.X >= Way.Location.X+Way.Size.Width)
            {
                Right = false;
                Up = true;
            }
            else if (Up && carriage.Location.Y <= Way.Location.Y && carriage.Location.X <= Way.Location.X + Way.Size.Width)
            {
                Up = false;
                Left = true;
            }
            else
            {
                if (Up)
                {
                    carriage.Location = new Point(carriage.Location.X, carriage.Location.Y - 2);
                }
                if (Down)
                {
                    carriage.Location = new Point(carriage.Location.X, carriage.Location.Y + 2);
                }
                if (Left)
                {
                    carriage.Location = new Point(carriage.Location.X - 2, carriage.Location.Y);
                }
                if (Right)
                {
                    carriage.Location = new Point(carriage.Location.X + 2, carriage.Location.Y);
                }
            }
        }
    }
}
