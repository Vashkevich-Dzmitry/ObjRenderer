using System.ComponentModel;
using System.Diagnostics;

namespace ObjRenderer.ViewModels
{
    public class FPSCounterViewModel : INotifyPropertyChanged
    {
        private readonly Stopwatch _stopwatch;

        private string fps;
        public string FPS
        {
            get => fps;
            set
            {
                fps = value;
                OnPropertyChanged(nameof(FPS));
            }
        }

        public FPSCounterViewModel() 
        {
            _stopwatch = new();
            fps = "0";
        }

        public void Start()
        {
            _stopwatch.Restart();
        }

        public void Stop()
        {
            _stopwatch.Stop();

            FPS = _stopwatch.ElapsedMilliseconds != 0 ? Math.Round(1000.0f / _stopwatch.ElapsedMilliseconds).ToString() : "inf";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
