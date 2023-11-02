using ObjRenderer.Helpers;
using ObjRenderer.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using static ObjRenderer.Helpers.CoordinateTransformer;
using static ObjRenderer.Helpers.ObjParser;

namespace ObjRenderer
{
    public class ObjRendererViewModel : INotifyPropertyChanged
    {
        private const string path = @"./Data/model.obj";

        private const float nearPlaneDistance = 0.1f;
        private const float farPlaneDistance = 100.0f;

        private const float baseScale = 100f;

        private const float cameraAlpha = (float)Math.PI / 2;
        private const float cameraBeta = 0.0f;
        private const float cameraDistanceZ = 150.0f;
        private const float cameraDistanceX = 0.0f;
        private const float cameraDistanceY = 0.0f;

        public Model Model { get; set; }
        public Camera Camera { get; set; }
        public Image Image { get; set; }

        public Vector3 lightingVector { get; set; }
        public List<Vector4> VerticesToDraw { get; set; }
        public List<IList<FaceDescription>> FacesToDraw { get; set; }


        public int pixelWidth;
        public int pixelHeight;


        public event EventHandler? ViewChanged;

        public ObjRendererViewModel(Image image, int pixelWidth, int pixelHeight)
        {
            Camera = new(cameraAlpha, cameraBeta, cameraDistanceZ, cameraDistanceX, cameraDistanceY);
            Camera.PropertyChanged += CameraChanged;

            lightingVector = Vector3.One;

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
            Model model = Parse(path);

            Vector3 position = new(-model.Size.XCenter, -model.Size.YCenter, -model.Size.ZCenter);
            Vector3 forward = -Vector3.UnitZ;
            Vector3 up = Vector3.UnitY;

            float scale = baseScale / Math.Max(model.Size.XSize, Math.Max(model.Size.ZSize, model.Size.YSize));

            var worldMatrix = GetWorldMatrix(position, forward, up);
            var scaleMatrix = GetScaleMatrix(scale);
            
            var matrix = worldMatrix * scaleMatrix;

            model.Vertices = model.Vertices.ApplyTransform(matrix).ToList();

            Model = model;

            RenderModel();
        }

        public void RenderModel()
        {
            var viewMatrix = GetViewMatrix(Camera.Eye, Camera.Target, Vector3.UnitY);

            var projectionMatrix = GetProjectionMatrix(60, pixelWidth / pixelHeight, nearPlaneDistance, farPlaneDistance);

            var viewportMatrix = GetViewPortMatrix(pixelWidth, pixelHeight);

            var matrix = viewMatrix * projectionMatrix;

            VerticesToDraw = Model.Vertices
                .ApplyTransform(matrix)
                .ToList();

            FacesToDraw = Model.Faces.Where(f => !f.Any(item => VerticesToDraw.ElementAt(item.VertexIndex).W < 0)).ToList();

            VerticesToDraw = VerticesToDraw
                .DivideByW()
                .ApplyTransform(viewportMatrix)
                .ToList();

            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
