using Drawing = System.Drawing;

namespace ObjRenderer.Maps
{
    public class DiffuseMap : Map<Drawing.Color>
    {
        public DiffuseMap(int width, int height) : base(width, height) { }

        public override Drawing.Color GetValue(float x, float y)
        {
            try
            {
                return _values[GetMapYCoordinate(y) * Width + GetMapXCoordinate(x)];
            }
            catch (Exception e)
            {
                Console.Write(e.StackTrace);
                return Drawing.Color.Pink;
            }
        }


        public override void SetValue(int x, int y, Drawing.Color value)
        {
            _values[y * Width + x] = value;
        }
    }
}
