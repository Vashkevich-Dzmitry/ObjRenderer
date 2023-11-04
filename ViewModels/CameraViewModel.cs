using System.ComponentModel;
using System.Numerics;

namespace ObjRenderer.ViewModels
{
    public class CameraViewModel : INotifyPropertyChanged
    {
        private float alpha;
        private float beta;
        private float r;
        private float x;
        private float y;

        public CameraViewModel(float alpha, float beta, float r, float x, float y)
        {
            this.alpha = alpha;
            this.beta = beta;
            this.r = r;
            this.x = x;
            this.y = y;
        }

        public Vector3 Eye
        {
            get
            {
                double x = R * Math.Sin(Alpha) * Math.Sin(Beta) + X;
                double y = R * Math.Cos(Alpha) + Y;
                double z = R * Math.Sin(Alpha) * Math.Cos(Beta);

                return new Vector3((float)x, (float)y, (float)z);
            }
        }

        public Vector3 Target
        {
            get
            {
                return new Vector3(X, Y, 0);
            }
        }

        public float R
        {
            get => r;
            set
            {
                if (value >= 55 && value < 1000)
                {
                    r = value;
                    OnPropertyChanged(nameof(R));
                }
            }
        }

        public float X
        {
            get => x;
            set
            {
                if (value > -1000 && value < 1000)
                {
                    x = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public float Y
        {
            get => y;
            set
            {
                if (value > -1000 && value < 1000)
                {
                    y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }
        public float Alpha
        {
            get => alpha;
            set
            {
                if (value > 0 && value < Math.PI)
                {
                    alpha = value;
                    OnPropertyChanged(nameof(Alpha));
                }
            }
        }

        public float Beta
        {
            get => beta;
            set
            {
                beta = value % (2 * (float)Math.PI);
                OnPropertyChanged(nameof(Beta));
            }
        }

        public void Reset(float alpha, float beta, float r, float x, float y)
        {
            Alpha = alpha;
            Beta = beta;
            R = r;
            X = x;
            Y = y;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
