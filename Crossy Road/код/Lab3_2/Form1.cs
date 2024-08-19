using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab3_2
{
    public partial class Form1 : Form
    {
            bool IsWin = false;
        const int count = 5;
        Player player;
        Car[] objects = new Car[count];
        Locomotive[] Train = new Locomotive[count];
        Thread[] threads = new Thread[count];
        bool IsFinished = false;
        Point start;
        Thread Carriages;

        public Form1()
        {
            InitializeComponent();
            start = new Point(PlayerField.Width / 2, PlayerField.Height - 50);
            PaintObject();
            TrainCreate();
            CreateTreads();
            Carriages = new Thread(() => MovingTrain());
            Carriages.Name = "train";
            Carriages.IsBackground = false;
            Carriages.Start();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void PaintObject()//рисовка машин и игрока
        {
            for (int i = 0; i < count; i++)
            {
                Point point = new Point(this.Location.X, this.Location.Y + this.ClientSize.Height / 2 + 100 - i * 70);
                objects[i] = new Car(point, Convert.ToBoolean(i));
                Controls.Add(objects[i].CarElement);
            }
            player = new Player(start);
            Controls.Add(player.playerImage);
        }
        public void CreateTreads()//1 машина - 1 поток
        {
            for (int i = 0; i < count; i++)
            {
                int index = i;
                threads[i] = new Thread(() => Moving(index));
                threads[i].IsBackground = false;
                threads[i].Name = $"Car#{i}";
                threads[i].Start();
            }
        }
        private void Moving(int index)//движение машины
        {
            while (!IsFinished)
            {
                Rectangle rectangle = objects[index].CarElement.Bounds;
                rectangle.X = !objects[index].LeftRight ? rectangle.X + rectangle.Width : rectangle.X - rectangle.Width;
                foreach (var carriage in Train)
                {
                    if (rectangle.IntersectsWith(carriage.carriage.Bounds))
                    {
                        if ((carriage.Left && !objects[index].LeftRight) || (carriage.Right && objects[index].LeftRight))
                        {
                            objects[index].LeftRight = !objects[index].LeftRight;
                            break;
                        }
                        else
                        {
                            objects[index].stopping = true;
                            break;
                        }
                    }
                    objects[index].stopping = false;
                }

                if (objects[index].CarElement.InvokeRequired && 
                    (!player.playerImage.IsDisposed && objects[index].CarElement.IsHandleCreated && threads[index].IsAlive))
                {
                    objects[index].CarElement.BeginInvoke(new Action(()=>
                    {//логика движения 1 из машин
                        if (objects[index].CarElement.Bounds.IntersectsWith(player.playerImage.Bounds))
                        {
                            player.playerImage.Location = start;
                        }
                        Point startPosition = new Point(this.Location.X, objects[index].CarElement.Top);

                        if (!objects[index].stopping)
                        {
                            if ((objects[index].CarElement.Right < ClientSize.Width && !objects[index].LeftRight) ||
                            (objects[index].CarElement.Location.X > startPosition.X && objects[index].LeftRight))
                            {
                                switch (objects[index].LeftRight)
                                {
                                    case false:
                                        objects[index].CarElement.Location = new Point(objects[index].CarElement.Left + 5 * (index + 1), startPosition.Y); break;
                                    case true:
                                        objects[index].CarElement.Location = new Point(objects[index].CarElement.Left - 5 * (index + 1), startPosition.Y); break;
                                }

                            }
                            else if (objects[index].CarElement.Right >= ClientSize.Width && !objects[index].LeftRight)
                            {
                                objects[index].LeftRight = true;
                            }
                            else if (objects[index].CarElement.Location.X <= startPosition.X && objects[index].LeftRight)
                            {
                                objects[index].LeftRight = false;
                            }
                        }
                    }));
                }
                else if (player.playerImage.IsDisposed)
                {
                    return;
                }

                Thread.Sleep(15);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsFinished = true;
            Carriages.Join();
            foreach (Thread t in threads)
            {
                t.Join();
            }

        }

        private void TrainCreate()//создание локомотива и поездов
        {
            Point point = new Point(trainWay.Location.X, trainWay.Location.Y);
            for (int i = 0; i < count; i++)
            {
                Train[i] = new Locomotive(new Point(point.X + i * 30, point.Y),trainWay);
                Controls.Add(Train[i].carriage);
            }
        }
        private void MovingTrain()//движение всех вагонов одним потоком
        {
            bool stopping = false;
            while (!IsFinished)
            {
                Rectangle rectangle = Train[0].carriage.Bounds;
                //rectangle.Width = Train[0].Down ? rectangle.Width+Train[0].carriage.Size.Width : rectangle.Width - Train[0].carriage.Size.Width;
                rectangle.Y = Train[0].Down ? rectangle.Y + rectangle.Height : rectangle.Y - rectangle.Height;
                foreach (var car in objects)
                {
                    if ((Train[0].Down || Train[0].Up) && rectangle.IntersectsWith(car.CarElement.Bounds))
                    {
                        stopping = true;
                        break;
                    }
                    stopping = false;
                }

                foreach (var carriage in Train)
                {

                    if (carriage.carriage.InvokeRequired && (!player.playerImage.IsDisposed && carriage.carriage.IsHandleCreated && Carriages.IsAlive))
                    {
                        carriage.carriage.BeginInvoke(new Action(() => 
                        {
                            if (carriage.carriage.Bounds.IntersectsWith(player.playerImage.Bounds))
                            {
                                player.playerImage.Location = start;
                            }
                            if (!stopping)
                            {
                                carriage.LocomotiveMoving();
                            }
                        }));
                    }
                    else if(player.playerImage.IsDisposed)
                    {
                        return;
                    }
                    else { break; }
                }
                Thread.Sleep(15);
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)//движение игрока при нажатии кнопки
        {
            Point location = player.playerImage.Location;
            Size size = player.playerImage.Size;
            switch (e.KeyCode)
            {
                case Keys.W: case Keys.Up: case Keys.NumPad8: player.playerImage.Location = location.Y-size.Height<=PlayerField.Location.Y  ? new Point(location.X, PlayerField.Location.Y) :
                        new Point(location.X, location.Y - size.Height);
                    if(player.playerImage.Location.Y== PlayerField.Location.Y && !IsWin)
                    {
                        DialogResult result = MessageBox.Show("Вы смогли перебежать дорогу! Завершить игру?", "Победа!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes )
                        {
                            // Закрыть приложение
                            Close();
                        }
                        else
                        {
                            IsWin = !IsWin;
                        }
                    }
                    break;
                case Keys.S: case Keys.Down: case Keys.NumPad2: player.playerImage.Location = location.Y + size.Height>= PlayerField.Height ? new Point(location.X, PlayerField.Height- size.Height):
                       new Point(location.X, location.Y + size.Height); break;
                case Keys.A: case Keys.Left: case Keys.NumPad4: player.playerImage.Location = location.X - size.Width<= PlayerField.Location.X ? new Point(PlayerField.Location.X, location.Y):
                         new Point(location.X - size.Width, location.Y); break;
                case Keys.D: case Keys.Right: case Keys.NumPad6: player.playerImage.Location = location.X + size.Width >= PlayerField.Width ? new Point(PlayerField.Width - size.Width, location.Y) :
                        new Point(location.X + size.Width, location.Y); break;
                default: break;
            }
        }
    }
}
