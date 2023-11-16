using Drawing = System.Drawing;

namespace ObjRenderer.Models
{
    public class DiffuseMap : Map<Drawing.Color>
    {
        public DiffuseMap(int width, int height) : base(width, height) { }

        public override Drawing.Color GetValue(float x, float y)
        {
            return _values[GetMapYCoordinate(y) * Width + GetMapXCoordinate(x)];
        }

        public override void SetValue(int x, int y, Drawing.Color value)
        {
            _values[y * Width + x] = value;
        }
    }
}
