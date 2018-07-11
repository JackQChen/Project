using System;
using System.Threading;
using System.Windows.Forms;
using PhysicsEngine.Base;
using PhysicsEngine.Core;

namespace TestPE
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private Engine engine;
        private Renderer render;
        private Runner runner;
        private Body box;

        private void Form1_Load(object sender, EventArgs e)
        {
            engine = new Engine();
            engine.Debug = false;

            double x = 10, y = 10, offset = 5, left = 0, right = 500, top = 0, bottom = 400, thick = 10;
            engine.World.Add(Factory.CreateRectangleBody(x + right / 2, y + top - offset, right, thick, true));
            var bottomBody = Factory.CreateRectangleBody(x + right / 2, y + bottom + offset, right + 2 * thick, thick, true);
            engine.World.Add(bottomBody);
            engine.World.Add(Factory.CreateRectangleBody(x + left - offset, y + bottom / 2, thick, bottom, true));
            engine.World.Add(Factory.CreateRectangleBody(x + right + offset, y + bottom / 2, thick, bottom, true));

            box = Factory.CreateCircleBody(100, 200, 30 );
            box.AngularVelocity = 0.1;
            //box = Factory.CreateCircleBody(100, 100, 50);
            //box.Density = 0.0001;

            engine.World.Add(box);

            var box2 = Factory.CreateRectangleBody(200, 200, 100, 100);
            engine.World.Add(box2);

            runner = new Runner(engine);
            render = new Renderer(engine, runner);
            pictureBox1.Image = render.Bitmap;

            new Thread(() =>
            {
                while (true)
                {
                    runner.Update(DateTime.Now.Ticks);
                    render.Render();
                    this.pictureBox1.Invoke(new Action(() =>
                    {
                        this.pictureBox1.Refresh();
                    }));
                    Thread.Sleep(10);
                }
            })
            { IsBackground = true }.Start();

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.box.Force.Y = -0.01;
                this.box.Force.X = 0.01 * (e.X > box.Position.X ? 1 : -1);
                this.box.AngularVelocity = 0.05;
            }
            else if (e.Button == MouseButtons.Right)
            {
                this.box.Position = new PhysicsEngine.Common.Point(e.X, e.Y);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.engine.World.Gravity.Direction.Y = 0;
        }
    }
}
