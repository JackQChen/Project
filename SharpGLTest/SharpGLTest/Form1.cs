using System;
using System.Windows.Forms;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Lighting;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ViewController vc = new ViewController(this.sceneControl);
            vc.Start();
            vc.Step = 0.01;
            vc.Interval = 10;
            vc.CameraChanged += new EventHandler<CameraEventArgs>(vc_CameraChanged);
            //this.sceneControl.OpenGL.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, new float[] { -1, 21, -6, 1 });
            var lights = this.sceneControl.Scene.SceneContainer.Children[1];
            lights.Children.Clear();
            Light light = new Light();
            light.On = true;
            light.GLCode = OpenGL.GL_LIGHT0;
            lights.AddChild(light);
        }

        void vc_CameraChanged(object sender, CameraEventArgs e)
        {
            var camera = this.sceneControl.Scene.CurrentCamera as LookAtCamera;
            camera.Position = new Vertex((float)e.eye.X, (float)e.eye.Y, (float)e.eye.Z);
            camera.Target = new Vertex((float)e.center.X, (float)e.center.Y, (float)e.center.Z);
            camera.UpVector = new Vertex((float)e.up.X, (float)e.up.Y, (float)e.up.Z);
        }

        float g3 = 0;

        private void Form1_Load(object sender, System.EventArgs e)
        {
            g3 = (float)Math.Sqrt(3d);
            Sphere sInner = new Sphere();
            sInner.Radius = 1;
            sInner.Slices = 100;
            sInner.Transformation.TranslateX = g3;
            sInner.Transformation.TranslateY = g3;
            sInner.Transformation.TranslateZ = g3;

            Sphere sOuter = new Sphere();
            sOuter.Transformation.TranslateX = g3;
            sOuter.Transformation.TranslateY = g3;
            sOuter.Transformation.TranslateZ = g3;
            sOuter.Radius = g3;
            sOuter.Slices = 50;
            sOuter.QuadricDrawStyle = DrawStyle.Line;
            Cube cube = new Cube();
            cube.Transformation.TranslateX = g3;
            cube.Transformation.TranslateY = g3;
            cube.Transformation.TranslateZ = g3;
            cube.Transformation.ScaleX = 1;
            cube.Transformation.ScaleY = 1;
            cube.Transformation.ScaleZ = 1;
            cube.Faces.Remove(cube.Faces[5]);
            this.sceneControl.Scene.SceneContainer.AddChild(sOuter);
            this.sceneControl.Scene.SceneContainer.AddChild(sInner);
            this.sceneControl.Scene.SceneContainer.AddChild(cube);
            this.sceneControl.Scene.RenderBoundingVolumes = false;
            //this.sceneControl.Scene.SceneContainer.Children[0].IsEnabled = false;
            //this.sceneControl.Scene.SceneContainer.Children[1].IsEnabled = false;
        }

        private void sceneControl_MouseClick(object sender, MouseEventArgs e)
        {
            var ctls = this.sceneControl.Scene.DoHitTest(e.X, e.Y);
            var es = ctls.GetEnumerator();
            if (es.MoveNext())
                this.propertyGrid1.SelectedObject = es.Current;
            else
                this.propertyGrid1.SelectedObject = null;
        }

    }
}
