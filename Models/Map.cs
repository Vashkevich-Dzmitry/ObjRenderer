using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjRenderer.Models
{
    public abstract class Map<T>
    {
        public int Width;
        public int Height;

        protected T[] _values;

        public Map(int width, int height)
        {
            Width = width;
            Height = height;

            _values = new T[width * height];
        }

        public abstract T GetValue(float x, float y);

        public abstract void SetValue(int x, int y, T value);

        public int GetMapXCoordinate(float x)
        {
            int result = (int)(x * Width);

            return result;
        }

        public int GetMapYCoordinate(float y)
        {
            int result = (int)(y * Height);

            return result;
        }
    }
}
