using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Numerics;

namespace ObjRenderer.Maps
{
    public class CubeMap
    {
        private DiffuseMap nxEdge, nyEdge, nzEdge, pxEdge, pyEdge, pzEdge;

        public CubeMap(DiffuseMap nxEdge, DiffuseMap nyEdge, DiffuseMap nzEdge, DiffuseMap pxEdge, DiffuseMap pyEdge, DiffuseMap pzEdge)
        {
            this.nxEdge = nxEdge;
            this.nyEdge = nyEdge;
            this.nzEdge = nzEdge;
            this.pxEdge = pxEdge;
            this.pyEdge = pyEdge;
            this.pzEdge = pzEdge;
        }

        private const float Distance = 1000f;
        private const float e = 0.000001f;

        private static Vector3 NormalNX = new(-1, 0, 0);
        private static Vector3 NormalPX = new(1, 0, 0);
        private static Vector3 NormalNY = new(0, -1, 0);
        private static Vector3 NormalPY = new(0, 1, 0);
        private static Vector3 NormalPZ = new(0, 0, -1);
        private static Vector3 NormalNZ = new(0, 0, 1);

        public System.Drawing.Color GetRedlectedEnvironmentColor(Vector3 pixelPoint, Vector3 pixelNormal, Vector3 eye)
        {
            System.Drawing.Color color;

            Vector3 eyeToPoint = Vector3.Normalize(pixelPoint - eye);
            Vector3 pointNormal = Vector3.Normalize(pixelNormal);

            Vector3 reflectedEyeToPoint = Vector3.Normalize(Vector3.Reflect(eyeToPoint, pointNormal));

            float xr = reflectedEyeToPoint.X;
            float yr = reflectedEyeToPoint.Y;
            float zr = reflectedEyeToPoint.Z;

            float xp = pixelPoint.X;
            float yp = pixelPoint.Y;
            float zp = pixelPoint.Z;

            float tpx = Math.Abs(xr) < e ? float.MaxValue : (Distance - xp) / xr;
            float tnx = Math.Abs(xr) < e ? float.MaxValue : (-Distance - xp) / xr;
            float tpy = Math.Abs(yr) < e ? float.MaxValue : (Distance - yp) / yr;
            float tny = Math.Abs(yr) < e ? float.MaxValue : (-Distance - yp) / yr;
            float tpz = Math.Abs(zr) < e ? float.MaxValue : (Distance - zp) / zr;
            float tnz = Math.Abs(zr) < e ? float.MaxValue : (-Distance - zp) / zr;

            tpx = tpx < 0 ? float.MaxValue : tpx;
            tnx = tnx < 0 ? float.MaxValue : tnx;
            tpy = tpy < 0 ? float.MaxValue : tpy;
            tny = tny < 0 ? float.MaxValue : tny;
            tpz = tpz < 0 ? float.MaxValue : tpz;
            tnz = tnz < 0 ? float.MaxValue : tnz;

            float minT = Math.Min(
                Math.Min(tpx, tnx),
                Math.Min(
                    Math.Min(tpy, tny),
                    Math.Min(tpz, tnz)
                )
            );

            float c1, c2;
            
            if (minT == tpx)        //PX
            {
                c1 = 0.5f - (tpx * zr + zp) / (2 * Distance);
                c2 = (tpx * yr + yp) / (2 * Distance) + 0.5f;

                color = pxEdge.GetValue(c1, c2);
            }
            else if (minT == tnx)   //NX
            {
                c1 = (tnx * zr + zp) / (2 * Distance) + 0.5f;
                c2 =(tnx * yr + yp) / (2 * Distance) + 0.5f;

                color = nxEdge.GetValue(c1, c2);
            }
            else if (minT == tpy)   //PY
            {
                c1 = 0.5f - (tpy * xr + xp) / (2 * Distance);
                c2 = (tpy * zr + zp) / (2 * Distance)+ 0.5f;

                color = pyEdge.GetValue(c1, c2);
            }
            else if (minT == tny)   //NY
            {
                c1 = 0.5f - (tny * xr + xp) / (2 * Distance);
                c2 = 0.5f - (tny * zr + zp) / (2 * Distance) ;

                color = nyEdge.GetValue(c1, c2);
            }
            else if (minT == tpz)   //PZ
            {
                c1 = (tpz * xr + xp) / (2 * Distance) + 0.5f;
                c2 = (tpz * yr + yp) / (2 * Distance) + 0.5f;

                color = pzEdge.GetValue(c1, c2);
            }
            else                    //NZ
            {
                c1 = 0.5f - (tnz * xr + xp) / (2 * Distance);
                c2 = (tnz * yr + yp) / (2 * Distance) + 0.5f;

                color = nzEdge.GetValue(c1, c2);
            }

            return color;
        }
    }
}
