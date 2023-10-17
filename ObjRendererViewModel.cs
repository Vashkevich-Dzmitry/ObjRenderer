using ObjRenderer.Helpers;
using ObjRenderer.Models;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;
using static ObjRenderer.Helpers.CoordinateTransformer;
using static ObjRenderer.Helpers.ObjParser;

namespace ObjRenderer
{
    public class ObjRendererViewModel : INotifyPropertyChanged
    {
        private const string path = @"./Data/model.obj";

        private const float nearPlaneDistance = 10.0f;
        private const float fatPlaneDistance = 1000.0f;


        public Model Model { get; set; }
        public Camera Camera { get; set; }
        public Image Image { get; set; }
        public List<Vector4> VerticesToDraw { get; set; }
        public List<IList<FaceDescription>> FacesToDraw { get; set; }


        public int pixelWidth;
        public int pixelHeight;


        public event EventHandler? ViewChanged;

        public ObjRendererViewModel(Image image, int pixelWidth, int pixelHeight)
        {
            Camera = new((float)Math.PI / 2, 0, 400);
            Camera.PropertyChanged += CameraChanged;

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

            var worldMatrix = GetWorldMatrix(new(-model.Size.XCenter, -model.Size.YCenter, -model.Size.ZCenter), Vector3.UnitZ, new(0, 1, 0));
            var scaleMatrix = GetScaleMatrix(100 / model.Size.ZSize);

            model.Vertices = model.Vertices.ApplyTransform(worldMatrix).ApplyTransform(scaleMatrix).ToList();

            Model = model;

            RenderModel();
        }

        public void RenderModel()
        {
            var translationMatrix = Matrix4x4.CreateTranslation(0, 0, 0);
            //var rotationMatrix = Matrix4x4.CreateRotationX(Camera.Alpha) * Matrix4x4.CreateRotationY(Camera.Beta);
            var viewMatrix = GetViewMatrix(Camera.Eye, Vector3.Zero, Vector3.UnitY);

            var projectionMatrix = GetProjectionMatrix(45, pixelWidth / pixelHeight, nearPlaneDistance, fatPlaneDistance);

            var viewportMatrix = GetViewPortMatrix(pixelWidth, pixelHeight);

            var matrix = translationMatrix * viewMatrix * projectionMatrix;

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
