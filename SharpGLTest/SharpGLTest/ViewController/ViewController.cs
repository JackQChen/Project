using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SharpGL;

namespace Test
{
    public class ViewController
    {
        public event EventHandler<CameraEventArgs> CameraChanged;
        private SharpGL.OpenGLControl openGLControl;
        private bool isStarted;
        private System.Windows.Forms.KeyEventHandler keyDownEvent;
        private System.Windows.Forms.KeyEventHandler keyUpEvent;
        private System.Windows.Forms.MouseEventHandler mouseDownEvent;
        private System.Windows.Forms.MouseEventHandler mouseMoveEvent;
        private System.EventHandler mouseEnterEvent;
        private System.EventHandler mouseLeaveEvent;
        private System.EventHandler resizedEvent;
        private Direction _keyDownDirection;
        private TriTuple eye;
        private TriTuple center;
        private TriTuple up;
        private PerspectiveParam perspective;
        Point currentLocation = Point.Empty;

        #region event handler functions

        void openGLControl_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if ((e.KeyCode) == Keys.W
                || (e.KeyCode) == Keys.Up)
            {
                _keyDownDirection |= Direction.Front;
            }
            if ((e.KeyCode) == Keys.S
                || (e.KeyCode) == Keys.Down)
            {
                _keyDownDirection |= Direction.Back;
            }
            if ((e.KeyCode) == Keys.A
                || (e.KeyCode) == Keys.Left)
            {
                _keyDownDirection |= Direction.Left;
            }
            if ((e.KeyCode) == Keys.D
                || (e.KeyCode) == Keys.Right)
            {
                _keyDownDirection |= Direction.Right;
            }
            if ((e.KeyCode) == Keys.Q
                || (e.KeyCode == Keys.PageUp))
            {
                _keyDownDirection |= Direction.Up;
            }
            if ((e.KeyCode) == Keys.E
                || (e.KeyCode == Keys.PageDown))
            {
                _keyDownDirection |= Direction.Down;
            }
        }

        void openGLControl_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if ((e.KeyCode) == Keys.W
                || (e.KeyCode) == Keys.Up)
            {
                _keyDownDirection &= ~Direction.Front;
            }
            if ((e.KeyCode) == Keys.S
                || (e.KeyCode) == Keys.Down)
            {
                _keyDownDirection &= ~Direction.Back;
            }
            if ((e.KeyCode) == Keys.A
                || (e.KeyCode) == Keys.Left)
            {
                _keyDownDirection &= ~Direction.Left;
            }
            if ((e.KeyCode) == Keys.D
                || (e.KeyCode) == Keys.Right)
            {
                _keyDownDirection &= ~Direction.Right;
            }
            if ((e.KeyCode) == Keys.Q
                || (e.KeyCode == Keys.PageUp))
            {
                _keyDownDirection &= ~Direction.Up;
            }
            if ((e.KeyCode) == Keys.E
                || (e.KeyCode == Keys.PageDown))
            {
                _keyDownDirection &= ~Direction.Down;
            }
        }

        void openGLControl_MouseDown(object sender, MouseEventArgs e)
        {
            currentLocation = e.Location;
        }

        void openGLControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var turnAngle = (e.Location.X - currentLocation.X) * TurnAngle;
                var staggerAngle = (e.Location.Y - currentLocation.Y) * StaggerAngle;
                currentLocation = e.Location;
                Turn(turnAngle);
                Stagger(-staggerAngle);
                UpdateView();
            }
        }


        void openGLControl_MouseEnter(object sender, EventArgs e)
        {
            this.currentLocation = Control.MousePosition;
            var item = this.openGLControl.Parent;
            Control preItem = this.openGLControl;
            while (item != null)
            {
                preItem = item;
                item = item.Parent;
            }
            this.currentLocation.Offset(preItem.Location);
        }

        void openGLControl_MouseLeave(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void openGLControl_Resized(object sender, EventArgs e)
        {
            this.UpdateView();
        }

        public void UpdateView()
        {
            if (openGLControl == null) return;

            if (openGLControl.InvokeRequired)
            {
                openGLControl.Invoke(updateViewDelegate);
            }
            else
            {
                var gl = openGLControl.OpenGL;
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();
                var perspective = this.perspective;
                //  Create a perspective transformation.
                gl.Perspective(perspective.fovy, perspective.aspect, perspective.zNear, perspective.zFar);
                //  Use the 'look at' helper function to position and aim the camera.
                var eye = this.eye; var center = this.center; var up = this.up;
                gl.LookAt(eye.X, eye.Y, eye.Z,
                    center.X, center.Y, center.Z,
                    up.X, up.Y, up.Z);
                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                if (this.CameraChanged != null)
                {
                    this.CameraChanged(this, new CameraEventArgs(eye, center, up, perspective));
                }
            }
        }

        #endregion event handler functions

        #region GoGoGo

        public void GoFront(double step)
        {
            var diff = new TriTuple(this.center.X - this.eye.X,
                0,
                this.center.Z - this.eye.Z);
            var length2 = diff.X * diff.X + 0 + diff.Z * diff.Z;
            var radio = Math.Sqrt(step * step / length2);
            var stepDiff = new TriTuple(diff.X * radio, 0, diff.Z * radio);

            this.eye.Add(stepDiff);
            this.center.Add(stepDiff);
        }

        public void GoBack(double step)
        {
            var diff = new TriTuple(this.center.X - this.eye.X,
                0,
                this.center.Z - this.eye.Z);
            var length2 = diff.X * diff.X + 0 + diff.Z * diff.Z;
            var radio = Math.Sqrt(step * step / length2);
            var stepDiff = new TriTuple(-diff.X * radio, 0, -diff.Z * radio);

            this.eye.Add(stepDiff);
            this.center.Add(stepDiff);
        }

        public void GoLeft(double step)
        {
            var diff = new TriTuple(this.center.X - this.eye.X,
                0,
                this.center.Z - this.eye.Z);
            var length2 = diff.X * diff.X + 0 + diff.Z * diff.Z;
            var radio = Math.Sqrt(step * step / length2);
            var stepDiff = new TriTuple(diff.Z * radio, 0, -diff.X * radio);

            this.eye.Add(stepDiff);
            this.center.Add(stepDiff);
        }

        public void GoRight(double step)
        {
            var diff = new TriTuple(this.center.X - this.eye.X,
                0,
                this.center.Z - this.eye.Z);
            var length2 = diff.X * diff.X + 0 + diff.Z * diff.Z;
            var radio = Math.Sqrt(step * step / length2);
            var stepDiff = new TriTuple(-diff.Z * radio, 0, diff.X * radio);

            this.eye.Add(stepDiff);
            this.center.Add(stepDiff);
        }

        public void GoUp(double step)
        {
            this.eye.Y = this.eye.Y + step;
            this.center.Y = this.center.Y + step;
        }

        public void GoDown(double step)
        {
            this.eye.Y = this.eye.Y - step;
            this.center.Y = this.center.Y - step;
        }

        /// <summary>
        /// 正数向右转，负数向左转
        /// </summary>
        /// <param name="turnAngle">正数向右转，负数向左转</param>
        public void Turn(double turnAngle)
        {
            var diff = new TriTuple(this.center.X - this.eye.X,
                0,
                this.center.Z - this.eye.Z);
            var cos = Math.Cos(turnAngle);
            var sin = Math.Sin(turnAngle);
            var centerDiff = new TriTuple(diff.X * cos - diff.Z * sin,
                0,
                diff.X * sin + diff.Z * cos);
            this.center.X = this.eye.X + centerDiff.X;
            this.center.Z = this.eye.Z + centerDiff.Z;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="staggerAngle"></param>
        private void Stagger(double staggerAngle)
        {
            var ceX = this.center.X - this.eye.X;
            var ceZ = this.center.Z - this.eye.Z;
            var distanceCE = Math.Sqrt(ceX * ceX + ceZ * ceZ);
            var diff = new TriTuple(distanceCE, this.center.Y - this.eye.Y, 0);
            var cos = Math.Cos(staggerAngle);
            var sin = Math.Sin(staggerAngle);
            var centerDiff = new TriTuple(diff.X * cos - diff.Y * sin,
                diff.X * sin + diff.Y * cos,
                0);
            this.center.Y = this.eye.Y + centerDiff.Y;
            var percent = centerDiff.X / distanceCE;
            this.center.X = this.eye.X + percent * ceX;
            this.center.Z = this.eye.Z + percent * ceZ;
        }

        #endregion GoGoGo


        public ViewController(OpenGLControl openGLControl)
        {
            // TODO: Complete member initialization
            Debug.Assert(openGLControl != null);
            this.Init(openGLControl, 100, 5, 5, 5, 0, 0, 0, 0, 1, 0, 60.0f,
                (double)(openGLControl.Width) / (double)(openGLControl.Height), 0.01, 100.0);
        }

        public ViewController(OpenGLControl openGLControl, int interval)
        {
            // TODO: Complete member initialization
            Debug.Assert(openGLControl != null);
            this.Init(openGLControl, interval, 5, 5, 5, 0, 0, 0, 0, 1, 0, 60.0f,
                (double)(openGLControl.Width) / (double)(openGLControl.Height), 0.01, 100.0);
        }
        public ViewController(SharpGL.OpenGLControl openGLControl, double eyex, double eyey, double eyez,
            double centerx, double centery, double centerz,
            double upx, double upy, double upz, double fovy, double aspect, double zNear, double zFar)
        {
            this.Init(openGLControl, 100, eyex, eyey, eyez, centerx, centery, centerz, upx, upy, upz, fovy, aspect, zNear, zFar);
        }

        public ViewController(SharpGL.OpenGLControl openGLControl, int interval, double eyex, double eyey, double eyez,
            double centerx, double centery, double centerz,
            double upx, double upy, double upz, double fovy, double aspect, double zNear, double zFar)
        {
            this.Init(openGLControl, interval, eyex, eyey, eyez, centerx, centery, centerz, upx, upy, upz, fovy, aspect, zNear, zFar);
        }

        private void Init(SharpGL.OpenGLControl openGLControl, int interval, double eyex, double eyey, double eyez,
            double centerx, double centery, double centerz,
            double upx, double upy, double upz, double fovy, double aspect, double zNear, double zFar)
        {
            this.openGLControl = openGLControl;
            this.interval = interval;
            this.eye = new TriTuple(eyex, eyey, eyez);
            this.center = new TriTuple(centerx, centery, centerz);
            this.up = new TriTuple(upx, upy, upz);
            this.perspective = new PerspectiveParam(fovy, aspect, zNear, zFar);
            this.updateViewDelegate = new Action(UpdateView);
        }

        public bool IsStarted()
        {
            return this.isStarted;
        }

        public Direction GetDirection()
        {
            return this._keyDownDirection;
        }

        public void Start()
        {
            if (!this.isStarted && this.openGLControl != null && !this.openGLControl.IsDisposed)
            {
                if (keyDownEvent == null)
                    keyDownEvent = new System.Windows.Forms.KeyEventHandler(openGLControl_KeyDown);
                this.openGLControl.KeyDown += keyDownEvent;
                if (keyUpEvent == null)
                    keyUpEvent = new System.Windows.Forms.KeyEventHandler(openGLControl_KeyUp);
                this.openGLControl.KeyUp += keyUpEvent;
                if (mouseDownEvent == null)
                    mouseDownEvent = new MouseEventHandler(openGLControl_MouseDown);
                this.openGLControl.MouseDown += mouseDownEvent;
                if (mouseMoveEvent == null)
                    mouseMoveEvent = new MouseEventHandler(openGLControl_MouseMove);
                this.openGLControl.MouseMove += mouseMoveEvent;
                if (mouseEnterEvent == null)
                    mouseEnterEvent = new EventHandler(openGLControl_MouseEnter);
                this.openGLControl.MouseEnter += mouseEnterEvent;
                if (mouseLeaveEvent == null)
                    mouseLeaveEvent = new EventHandler(openGLControl_MouseLeave);
                this.openGLControl.MouseLeave += mouseLeaveEvent;
                if (resizedEvent == null)
                    resizedEvent = new EventHandler(openGLControl_Resized);
                this.openGLControl.Resized += resizedEvent;
                this.isStarted = true;
                this.openGLControl_Resized(openGLControl, null);

                this.thrViewController = new Thread(viewControllerThreadFunc);
                this.thrViewController.IsBackground = true;
                this.thrViewController.Priority = ThreadPriority.Highest;
                this.thrViewController.Start();
            }
        }

        private void viewControllerThreadFunc()
        {
            while (true)
            {
                var moving = false;
                {
                    var frontBack = _keyDownDirection & (Direction.Front | Direction.Back);
                    if (frontBack != (Direction.Front | Direction.Back))
                    {
                        moving = true;
                        if (frontBack == Direction.Front)
                            this.GoFront(Step);
                        else if (frontBack == Direction.Back)
                            this.GoBack(Step);
                    }
                }
                {
                    var leftRight = _keyDownDirection & (Direction.Left | Direction.Right);
                    if (leftRight != (Direction.Left | Direction.Right))
                    {
                        moving = true;
                        if (leftRight == Direction.Left)
                            this.GoLeft(Step);
                        else if (leftRight == Direction.Right)
                            this.GoRight(Step);
                    }
                }
                {
                    var upDown = _keyDownDirection & (Direction.Up | Direction.Down);
                    if (upDown != (Direction.Up | Direction.Down))
                    {
                        moving = true;
                        if (upDown == Direction.Up)
                            this.GoUp(Step);
                        else if (upDown == Direction.Down)
                            this.GoDown(Step);
                    }
                }

                if (moving)
                    this.UpdateView();
                Thread.Sleep(interval);
            }
        }

        public override string ToString()
        {
            return string.Format("eye:{0},center:{1},up:{2},perspective:[{3}]", eye, center, up, perspective);
            //return base.ToString();
        }

        private double _step = 0.2;

        /// <summary>
        /// Step front/back, left/right. Or go up/down
        /// </summary>
        public double Step
        {
            get { return _step; }
            set { _step = value; }
        }

        private double _turnAngle = 0.005;

        /// <summary>
        /// Turn left/right
        /// </summary>
        public double TurnAngle
        {
            get { return _turnAngle; }
            set { _turnAngle = value; }
        }

        private double _staggerAngle = 0.005;
        private Thread thrViewController;

        /// <summary>
        /// Stagger angle
        /// </summary>
        public double StaggerAngle
        {
            get { return _staggerAngle; }
            set { _staggerAngle = value; }
        }

        private int interval = 100;
        private Action updateViewDelegate;

        public int Interval
        {
            get { return interval; }
            set { interval = value; }
        }

    }

}
