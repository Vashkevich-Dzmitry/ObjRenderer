using ObjRenderer.Helpers;
using ObjRenderer.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ObjRenderer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const float _smoothness = 0.01f;
        private const float _rSmoothness = 0.05f;

        private ObjRendererViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            viewModel = new ObjRendererViewModel(Image, (int)ImagePanel.ActualWidth, (int)ImagePanel.ActualHeight);
            DataContext = viewModel;

            viewModel.ViewChanged += DrawModel;
            viewModel.LoadModel();
        }

        private Point lastMousePosition;

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                lastMousePosition = e.GetPosition(this);
            }
        }

        private void WindowMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentMousePosition = e.GetPosition(this);
                double deltaX = currentMousePosition.X - lastMousePosition.X;
                double deltaY = currentMousePosition.Y - lastMousePosition.Y;

                if (Math.Abs(deltaX) > 0.1 || Math.Abs(deltaY) > 0.1)
                {
                    viewModel.Camera.Alpha += (float)deltaY * _smoothness;
                    viewModel.Camera.Beta += (float)deltaX * _smoothness;
                }

                lastMousePosition = currentMousePosition;
            }
        }

        private void WindowMouseWheel(object sender, MouseWheelEventArgs e)
        {
            viewModel.Camera.R -= e.Delta * _rSmoothness;
        }

        internal void DrawModel(object? sender, EventArgs e)
        {
            Image.Source = Drawer.DrawBitmap(viewModel.FacesToDraw, viewModel.VerticesToDraw, viewModel.pixelWidth, viewModel.pixelHeight, System.Drawing.Color.DarkGreen).Source;
        }
    }
}