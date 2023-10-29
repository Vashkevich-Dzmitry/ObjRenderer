using System.ComponentModel;
using System.Numerics;

namespace ObjRenderer.Models
{
    public class Camera : INotifyPropertyChanged
    {
        private float alpha;
        private float beta;
        private float z;
        private float x;
        private float y;

        public Camera(float alpha, float beta, float r, float x, float y)
        {
            this.alpha = alpha;
            this.beta = beta;
            this.z = r;
            this.x = x;
            this.y = y;
        }

        public Vector3 Eye
        {
            get
            {
                double x = Z * Math.Sin(Alpha) * Math.Sin(Beta) + X;
                double y = Z * Math.Cos(Alpha) + Y;
                double z = Z * Math.Sin(Alpha) * Math.Cos(Beta);

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

        public float Z
        {
            get { return z; }
            set
            {
                if (value > 0 && value < 1000)
                {
                    z = value;
                    OnPropertyChanged(nameof(Z));
                }
            }
        }

        public float X
        {
            get { return x; }
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
            get { return y; }
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
            get { return alpha; }
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
            get { return beta; }
            set
            {
                beta = value % (2 * (float)Math.PI);
                OnPropertyChanged(nameof(Beta));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
