using ObjRenderer.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ObjRenderer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _defaultModelPath = @"./Data/model.obj";

        private const float _mouseButtonSmoothness = 0.005f;
        private const float _mouseWheelSmoothness = 0.005f;
        private const float _keyDistanceChange = 1.0f;
        private const float _keyBetaChange = (float)Math.PI / 12;
        private const float _keyAlphaChange = (float)Math.PI /36;
        private const float _controlScaleValue = 5.0f;

        private delegate void ModelPathChangedHandler(string modelPath);
        private event ModelPathChangedHandler ModelPathChanged;
        
        private bool _isMouseDown;
        private Point _lastMousePosition;
        
        private ObjRendererViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            viewModel = new ObjRendererViewModel(Image, (int)ImagePanel.ActualWidth, (int)ImagePanel.ActualHeight);
            DataContext = viewModel;

            viewModel.LoadModel(_defaultModelPath);

            ModelPathChanged += viewModel.LoadModel;
        }


        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    viewModel.Camera.Y += _keyDistanceChange;
                    break;
                case Key.S:
                    viewModel.Camera.Y -= _keyDistanceChange;
                    break;
                case Key.D:
                    viewModel.Camera.X += _keyDistanceChange;
                    break;
                case Key.A:
                    viewModel.Camera.X -= _keyDistanceChange;
                    break;
                case Key.Z:
                    viewModel.Camera.R += _keyDistanceChange;
                    break;
                case Key.X:
                    viewModel.Camera.R -= _keyDistanceChange;
                    break;
                case Key.Q:
                    viewModel.Camera.Beta += _keyBetaChange;
                    break;
                case Key.E:
                    viewModel.Camera.Beta -= _keyBetaChange;
                    break;
                case Key.F:
                    viewModel.Camera.Alpha += _keyAlphaChange;
                    break;
                case Key.R:
                    viewModel.Camera.Alpha -= _keyAlphaChange;
                    break;
            }
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isMouseDown = true;
                _lastMousePosition = e.GetPosition(this);
            }
        }

        private void WindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                _isMouseDown = false;
            }
        }

        private void WindowMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                Point currentMousePosition = e.GetPosition(this);
                float deltaX = (float)(currentMousePosition.X - _lastMousePosition.X);
                float deltaY = (float)(currentMousePosition.Y - _lastMousePosition.Y);

                if (Math.Abs(deltaX) > 0.1f || Math.Abs(deltaY) > 0.1f)
                {
                    bool isControlPressed = Keyboard.Modifiers == ModifierKeys.Control;

                    viewModel.Camera.Alpha += isControlPressed ? deltaY * _mouseButtonSmoothness * _controlScaleValue : deltaY * _mouseButtonSmoothness;
                    viewModel.Camera.Beta += isControlPressed ? deltaX * _mouseButtonSmoothness * _controlScaleValue : deltaX * _mouseButtonSmoothness;
                }

                _lastMousePosition = currentMousePosition;
            }
        }

        private void WindowMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool isControlPressed = Keyboard.Modifiers == ModifierKeys.Control;
            float delta = e.Delta >= 0 ? (float)Math.Ceiling(e.Delta * _mouseWheelSmoothness) : (float)Math.Floor(e.Delta * _mouseWheelSmoothness);
            viewModel.Camera.R -= isControlPressed ? delta * _controlScaleValue : delta;
        }

        private void ChangeModelButtonClick(object sender, RoutedEventArgs e)
        {
            
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Model"; 
            dialog.DefaultExt = ".obj"; 
            dialog.Filter = "Obj files(.obj)|*.obj"; 

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    ModelPathChanged(dialog.FileName);
                }
                catch 
                {
                    MessageBox.Show("Invalid .obj file", "Model parsing error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetCameraButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetCamera();
        }
    }
}