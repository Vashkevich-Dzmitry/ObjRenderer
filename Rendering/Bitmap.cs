using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ObjRenderer.Rendering
{
    public unsafe class Bitmap
    {
        private byte* BackBuffer { get; set; }
        private int BackBufferStride { get; set; }
        private int BytesPerPixel { get; set; }
        public WriteableBitmap Source { get; set; }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public Bitmap(int pixelWidth, int pixelHeight)
        {
            Source = new(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgra32, null);
            BackBuffer = (byte*)Source.BackBuffer;
            BackBufferStride = Source.BackBufferStride;
            BytesPerPixel = Source.Format.BitsPerPixel / 8;
            PixelWidth = Source.PixelWidth;
            PixelHeight = Source.PixelHeight;
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            byte* address = BackBuffer + y * BackBufferStride + x * BytesPerPixel;
            address[0] = b;
            address[1] = g;
            address[2] = r;
            address[3] = 255;
        }

    }
}
