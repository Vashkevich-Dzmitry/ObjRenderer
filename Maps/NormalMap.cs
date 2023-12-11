using System.Numerics;

namespace ObjRenderer.Maps
{
    public class NormalMap : Map<Vector3>
    {
        public NormalMap(int width, int height) : base(width, height) { }

        public override Vector3 GetValue(float x, float y)
        {
            return _values[GetMapYCoordinate(y) * Width + GetMapXCoordinate(x)];
        }

        public override void SetValue(int x, int y, Vector3 value)
        {
            _values[y * Width + x] = value;
        }
    }
}
