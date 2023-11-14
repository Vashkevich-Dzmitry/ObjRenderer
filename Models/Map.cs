using System.Numerics;

namespace ObjRenderer.Models
{
    public struct Map
    {
        public int Width;
        public int Height;

        private Vector3[] _values;
        
        public Map(int width, int height)
        {
            Width = width;
            Height = height;

            _values = new Vector3[width * height];
        }

        public Vector3 GetValue(int x, int y)
        {
            return _values[y * Width + x];
        }

        public void SetValue(int x, int y, Vector3 value)
        {
            _values[y * Width + x] = value;
        }
    }
}
