﻿using ObjRenderer.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ObjRenderer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const float mouseButtonSmoothness = 0.005f;
        private const float mouseWheelSmoothness = 0.005f;
        private const float keyDistanceChange = 1.0f;
        private const float keyBetaChange = (float)Math.PI / 12;
        private const float keyAlphaChange = (float)Math.PI /36;
        private const float controlScaleValue = 5.0f;

        private ObjRendererViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            viewModel = new ObjRendererViewModel(Image, (int)ImagePanel.ActualWidth, (int)ImagePanel.ActualHeight);
            DataContext = viewModel;

            viewModel.LoadModel();
        }

        private Point lastMousePosition;

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    viewModel.Camera.Y += keyDistanceChange;
                    break;
                case Key.S:
                    viewModel.Camera.Y -= keyDistanceChange;
                    break;
                case Key.D:
                    viewModel.Camera.X += keyDistanceChange;
                    break;
                case Key.A:
                    viewModel.Camera.X -= keyDistanceChange;
                    break;
                case Key.Z:
                    viewModel.Camera.R += keyDistanceChange;
                    break;
                case Key.X:
                    viewModel.Camera.R -= keyDistanceChange;
                    break;
                case Key.Q:
                    viewModel.Camera.Beta += keyBetaChange;
                    break;
                case Key.E:
                    viewModel.Camera.Beta -= keyBetaChange;
                    break;
                case Key.F:
                    viewModel.Camera.Alpha += keyAlphaChange;
                    break;
                case Key.R:
                    viewModel.Camera.Alpha -= keyAlphaChange;
                    break;
            }
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
                float deltaX = (float)(currentMousePosition.X - lastMousePosition.X);
                float deltaY = (float)(currentMousePosition.Y - lastMousePosition.Y);

                if (Math.Abs(deltaX) > 0.1f || Math.Abs(deltaY) > 0.1f)
                {
                    bool isControlPressed = Keyboard.Modifiers == ModifierKeys.Control;

                    viewModel.Camera.Alpha += isControlPressed ? deltaY * mouseButtonSmoothness * controlScaleValue : deltaY * mouseButtonSmoothness;
                    viewModel.Camera.Beta += isControlPressed ? deltaX * mouseButtonSmoothness * controlScaleValue : deltaX * mouseButtonSmoothness;
                }

                lastMousePosition = currentMousePosition;
            }
        }

        private void WindowMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool isControlPressed = Keyboard.Modifiers == ModifierKeys.Control;
            float delta = e.Delta >= 0 ? (float)Math.Ceiling(e.Delta * mouseWheelSmoothness) : (float)Math.Floor(e.Delta * mouseWheelSmoothness);
            viewModel.Camera.R -= isControlPressed ? delta * controlScaleValue : delta;
        }
    }
}