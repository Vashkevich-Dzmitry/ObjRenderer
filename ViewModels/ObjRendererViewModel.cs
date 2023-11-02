using ObjRenderer.Helpers;
using ObjRenderer.Models;
using ObjRenderer.Rendering;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;
using static ObjRenderer.Helpers.CoordinateTransformer;
using static ObjRenderer.Parsing.ObjParser;

namespace ObjRenderer.ViewModels
{
    public class ObjRendererViewModel : INotifyPropertyChanged
    {
        private const string path = @"./Data/model.obj";

        private const float nearPlaneDistance = 1f;
        private const float farPlaneDistance = 1000f;

        private const float baseScale = 100f;

        private const float cameraAlpha = (float)Math.PI / 2;
        private const float cameraBeta = 0.0f;
        private const float cameraDistanceR = 150.0f;
        private const float cameraDistanceX = 0.0f;
        private const float cameraDistanceY = 0.0f;

        public Image Image { get; set; }
        public Model Model { get; set; }
        public CameraViewModel Camera { get; set; }
        public FPSCounterViewModel FPSCounter { get; set; }

        public Vector3 LightingVector { get; set; }
        public List<Vector4> VerticesToDraw { get; set; }
        public List<IList<FaceDescription>> FacesToDraw { get; set; }


        public int pixelWidth;
        public int pixelHeight;

        public ObjRendererViewModel(Image image, int pixelWidth, int pixelHeight)
        {
            FPSCounter = new();

            Camera = new(cameraAlpha, cameraBeta, cameraDistanceR, cameraDistanceX, cameraDistanceY);
            Camera.PropertyChanged += CameraChanged;

            LightingVector = Vector3.One;

            Image = image;

            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;
        }

        private void CameraChanged(object? sender, PropertyChangedEventArgs e)
        {
            RenderModel();
        }

        public void LoadModel()
        {
            Model model = ParseObj(path);

            Vector3 position = new(-model.Size.XCenter, -model.Size.YCenter, -model.Size.ZCenter);
            Vector3 forward = -Vector3.UnitZ;
            Vector3 up = Vector3.UnitY;

            float scale = baseScale / Math.Max(model.Size.XSize, Math.Max(model.Size.ZSize, model.Size.YSize));

            var worldMatrix = GetWorldMatrix(position, forward, up);
            var scaleMatrix = GetScaleMatrix(scale);

            var matrix = worldMatrix * scaleMatrix;

            model.Vertices = model.Vertices.AsParallel().ApplyTransform(matrix).ToList();

            Model = model;

            RenderModel();
        }

        public void RenderModel()
        {
            FPSCounter.Start();

            var viewMatrix = GetViewMatrix(Camera.Eye, Camera.Target, Vector3.UnitY);

            var projectionMatrix = GetProjectionMatrix(60, pixelWidth / pixelHeight, nearPlaneDistance, farPlaneDistance);

            var viewportMatrix = GetViewPortMatrix(pixelWidth, pixelHeight);

            var matrix = viewMatrix * projectionMatrix;

            VerticesToDraw = Model.Vertices.AsParallel()
                .ApplyTransform(matrix)
                .ToList();

            FacesToDraw = Model.Faces.AsParallel().Where(f => !f.Any(item => VerticesToDraw.ElementAt(item.VertexIndex).W < 0)).ToList();

            VerticesToDraw = VerticesToDraw.AsParallel()
                .DivideByW()
                .ApplyTransform(viewportMatrix)
                .ToList();

            Image.Source = Drawer.DrawBitmap(FacesToDraw, VerticesToDraw, Model.VertexNormals, pixelWidth, pixelHeight, System.Drawing.Color.Green, LightingVector).Source;

            FPSCounter.Stop();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
