using System.ComponentModel;
using System.Numerics;

namespace ObjRenderer.Models
{
    public class Camera : INotifyPropertyChanged
    {
        private float alpha;
        private float beta;
        private float r;

        public Camera(float alpha, float beta, float r)
        {
            this.alpha = alpha;
            this.beta = beta;
            this.r = r;
        }

        public Vector3 Eye
        {
            get
            {
                double x = R * Math.Sin(Alpha) * Math.Sin(Beta);
                double y = R * Math.Cos(Alpha);
                double z = R * Math.Sin(Alpha) * Math.Cos(Beta);

                return new Vector3((float)x, (float)y, (float)z);
            }
        }
        public float R
        {
            get { return r; }
            set
            {
                if (value > 0 && value < 1000)
                {
                    r = value;
                    OnPropertyChanged(nameof(R));
                }
            }
        }

        public float Alpha
        {
            get { return alpha; }
            set
            {
                if (value >= 0 && value <= Math.PI)
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
                beta = value;
                OnPropertyChanged(nameof(Beta));
            }
        }

        public void ChangeAlphaIncrement(float delta)
        {
            Alpha += delta;
        }
        public void ChangeBetaIncrement(float delta)
        {
            beta = (beta + delta) % (2 * (float)Math.PI);
            if (beta < 0)
            {
                beta += 2 * (float)Math.PI;
            }
        }

        public void ChangeAlphaAssign(float angle)
        {
            Alpha = angle;
        }
        public void ChangeBetaAssign(float angle)
        {
            beta = (angle) % (2 * (float)Math.PI);
            if (beta < 0)
            {
                beta += 2 * (float)Math.PI;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
