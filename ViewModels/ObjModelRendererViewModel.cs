using ObjRenderer.Helpers;
using ObjRenderer.Models;
using ObjRenderer.Parsing;
using ObjRenderer.Rendering;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;
using static ObjRenderer.Helpers.CoordinateTransformer;
using static ObjRenderer.Parsing.ObjFileParser;

namespace ObjRenderer.ViewModels
{
    public class ObjModelRendererViewModel : INotifyPropertyChanged
    {
        private Vector3 DiffuseColor = new(System.Drawing.Color.Green.R, System.Drawing.Color.Green.G, System.Drawing.Color.Green.B);
        private Vector3 AmbientColor = new(System.Drawing.Color.Green.R, System.Drawing.Color.Green.G, System.Drawing.Color.Green.B);
        private Vector3 SpecularColor = new(System.Drawing.Color.White.R, System.Drawing.Color.White.G, System.Drawing.Color.White.B);


        private const float nearPlaneDistance = 1f;
        private const float farPlaneDistance = 1000f;

        private const float baseScale = 100f;

        private const float cameraAlpha = (float)Math.PI / 2;
        private const float cameraBeta = 0.0f;
        private const float cameraDistanceR = 150.0f;
        private const float cameraDistanceX = 0.0f;
        private const float cameraDistanceY = 0.0f;

        public Drawer Drawer { get; set; }
        public ObjModelParser ModelParser { get; set; }

        public System.Windows.Controls.Image Image { get; set; }
        public Model Model { get; set; }
        public CameraViewModel Camera { get; set; }
        public FPSCounterViewModel FPSCounter { get; set; }

        public Vector3 LightingVector { get; set; }
        public List<Vector4> VerticesToDraw { get; set; }
        public List<Face> FacesToDraw { get; set; }

        public int pixelWidth;
        public int pixelHeight;

        public ObjModelRendererViewModel(System.Windows.Controls.Image image, int pixelWidth, int pixelHeight, string modelDirectoryPath)
        {
            Drawer = new(pixelWidth, pixelHeight, DiffuseColor, AmbientColor, SpecularColor);
            ModelParser = new(modelDirectoryPath);

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

        public void ChangeModel(string modelDirectoryPath)
        {
            ModelParser.ModelDirectoryPath = modelDirectoryPath;
            LoadModel();
        }

        public void LoadModel()
        {
            Model model = ModelParser.ParseModel();

            Vector3 modelCenter = model.Center;

            Vector3 position = new(-modelCenter.X, -modelCenter.Y, -modelCenter.Z);
            Vector3 forward = -Vector3.UnitZ;
            Vector3 up = Vector3.UnitY;

            var worldMatrix = GetWorldMatrix(position, forward, up);

            Vector3 modelSize = model.Size;
            float scale = baseScale / Math.Max(modelSize.X, Math.Max(modelSize.Z, modelSize.Y));

            var scaleMatrix = GetScaleMatrix(scale);

            var transformationMatrix = worldMatrix * scaleMatrix;

            model.Vertices = model.Vertices.AsParallel().ApplyTransform(transformationMatrix).ToList();

            Model = model;

            ResetCamera();
        }

        public void ResetCamera()
        {
            Camera.PropertyChanged -= CameraChanged;
            Camera.Reset(cameraAlpha, cameraBeta, cameraDistanceR, cameraDistanceX, cameraDistanceY);
            Camera.PropertyChanged += CameraChanged;
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

            FacesToDraw = Model.Faces.Where(f =>
                VerticesToDraw.ElementAt(f.p0.index).W > 0 &&
                VerticesToDraw.ElementAt(f.p1.index).W > 0 &&
                VerticesToDraw.ElementAt(f.p2.index).W > 0)
                .ToList();

            VerticesToDraw = VerticesToDraw.AsParallel()
                .DivideByW()
                .ApplyTransform(viewportMatrix)
                .ToList();

            Image.Source = Drawer.DrawBitmap(FacesToDraw, VerticesToDraw, Model.Vertices, Model.VertexNormals, LightingVector, Camera.Eye).Source;

            FPSCounter.Stop();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
