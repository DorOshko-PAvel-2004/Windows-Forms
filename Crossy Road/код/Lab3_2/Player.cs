using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab3_2
{
    internal class Player
    {
        internal PictureBox playerImage;
        internal Player(Point point)
        {
            playerImage = new PictureBox();
            playerImage.Location = point;
            playerImage.Name = "Player";
            playerImage.Size = new Size(100, 50);
            playerImage.TabIndex = 0;
            playerImage.BackColor = Color.DarkRed;
            playerImage.TabStop = false;
        }

    }
}
