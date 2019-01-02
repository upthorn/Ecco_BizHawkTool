using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BizHawk.Tool.Ecco
{
    abstract partial class EccoToolBase
    {
        protected int _tickerY = 48;
        protected void DrawRhomb(int x, int y, int radius, Color color, int fillAlpha = 63)
        {
            Point[] rhombus = {
                new Point(x, y - radius),
                new Point(x + radius, y),
                new Point(x, y + radius),
                new Point(x - radius, y)
            };
            Color? fillColor = null;
            if (fillAlpha > 0) fillColor = Color.FromArgb(fillAlpha, color);
            Gui.DrawPolygon(rhombus, color, fillColor);
        }
        protected void DrawEccoRhomb_scaled(int x, int y, int width, int height, int rscale, int bscale, int lscale, int tscale, Color color, int fillAlpha = 63)
        {
            Point[] rhombus = {
                new Point(x + (width << rscale), y),
                new Point(x, y + (height << bscale)),
                new Point(x - (width << lscale), y),
                new Point(x, y - (height << tscale))
            };
            Color? fillColor = null;
            if (fillAlpha > 0) fillColor = Color.FromArgb(fillAlpha, color);
            Gui.DrawPolygon(rhombus, color, fillColor);
        }
        protected void DrawOct(int x, int y, int r, Color color, int fillAlpha = 63)
        {
            var octOff = (int)(Math.Sqrt(2) * r) >> 1;
            Point[] octagon = {
                new Point(x, y - r),
                new Point(x + octOff, y - octOff),
                new Point(x + r, y),
                new Point(x + octOff, y + octOff),
                new Point(x, y + r),
                new Point(x - octOff, y + octOff),
                new Point(x - r, y),
                new Point(x - octOff, y - octOff)
            };
            Color? fillColor = null;
            if (fillAlpha > 0) fillColor = Color.FromArgb(fillAlpha, color);
            Gui.DrawPolygon(octagon, color, fillColor);
        }
        protected void DrawEccoOct_scaled(int x, int y, int xscale, int yscale, int r, Color color, int fillAlpha = 63)
        {
            var octOff = (int)(Math.Sin(Math.PI / 4) * r);
            var xoctOff = octOff << xscale;
            var yoctOff = octOff << yscale;
            var xr = r << xscale;
            var yr = r << yscale;
            Point[] octagon = {
                new Point(x, y - yr),
                new Point(x + xoctOff, y - yoctOff),
                new Point(x + xr, y),
                new Point(x + xoctOff, y + yoctOff),
                new Point(x, y + yr),
                new Point(x - xoctOff, y + yoctOff),
                new Point(x - xr, y),
                new Point(x - xoctOff, y - yoctOff)
            };
            Color? fillColor = null;
            if (fillAlpha > 0) fillColor = Color.FromArgb(fillAlpha, color);
            Gui.DrawPolygon(octagon, color, fillColor);
        }
        protected Point? Intersection(Point start1, Point end1, Point start2, Point end2)
        {
            if ((Math.Max(start1.X, end1.X) < Math.Min(start2.X, end2.X))
             || (Math.Min(start1.X, end1.X) > Math.Max(start2.X, end2.X))
             || (Math.Max(start1.Y, end1.Y) < Math.Min(start2.Y, end2.Y))
             || (Math.Min(start1.Y, end1.Y) > Math.Max(start2.Y, end2.Y)))
                return null;


            double ay_cy, ax_cx, px, py;
            double dx_cx = end2.X - start2.X,
                dy_cy = end2.Y - start2.Y,
                bx_ax = end1.X - start1.X,
                by_ay = end1.Y - start1.Y;

            double de = (bx_ax) * (dy_cy) - (by_ay) * (dx_cx);

            if (Math.Abs(de) < 0.01)
                return null;

            ax_cx = start1.X - start2.X;
            ay_cy = start1.Y - start2.Y;
            double r = ((ay_cy) * (dx_cx) - (ax_cx) * (dy_cy)) / de;
            double s = ((ay_cy) * (bx_ax) - (ax_cx) * (by_ay)) / de;
            px = start1.X + r * (bx_ax);
            py = start1.Y + r * (by_ay);
            if ((px < Math.Min(start1.X, end1.X)) || (px > Math.Max(start1.X, end1.X))
             || (px < Math.Min(start2.X, end2.X)) || (px > Math.Max(start2.X, end2.X))
             || (py < Math.Min(start1.Y, end1.Y)) || (py > Math.Max(start1.Y, end1.Y))
             || (py < Math.Min(start2.Y, end2.Y)) || (py > Math.Max(start2.Y, end2.Y)))
                return null;
            return new Point((int)px, (int)py);
        }
        protected void DrawRectRhombusIntersection(Point rectMid, Point rhombMid, int rw, int rh, int r, Color color, int fillAlpha = 63) // Octagon provided by the intersection of a rectangle and a rhombus
        {
            Point[] rect =
            {
                new Point(rectMid.X - rw, rectMid.Y + rh),
                new Point(rectMid.X - rw, rectMid.Y - rh),
                new Point(rectMid.X + rw, rectMid.Y - rh),
                new Point(rectMid.X + rw, rectMid.Y + rh)
            };
            Point[] rhombus =
            {
                new Point(rhombMid.X - r, rhombMid.Y),
                new Point(rhombMid.X, rhombMid.Y - r),
                new Point(rhombMid.X + r, rhombMid.Y),
                new Point(rhombMid.X, rhombMid.Y + r)
            };
            List<Point> finalShape = new List<Point>();
            foreach (Point p in rect)
            {
                if (Math.Abs(p.X - rhombMid.X) + Math.Abs(p.Y - rhombMid.Y) <= r)
                    finalShape.Add(p);
            }
            foreach (Point p in rhombus)
            {
                if ((Math.Abs(p.X - rectMid.X) <= rw) && (Math.Abs(p.Y - rectMid.Y) <= rh))
                    finalShape.Add(p);
            }
            for (int i = 0; i < 5; i++)
            {
                Point? p = Intersection(rhombus[i & 3], rhombus[(i + 1) & 3], rect[i & 3], rect[(i + 1) & 3]);
                if (p.HasValue) finalShape.Add(p.Value);
                p = Intersection(rhombus[i & 3], rhombus[(i + 1) & 3], rect[(i + 1) & 3], rect[(i + 2) & 3]);
                if (p.HasValue) finalShape.Add(p.Value);
            }
            double mX = 0;
            double my = 0;
            foreach (Point p in finalShape)
            {
                mX += p.X;
                my += p.Y;
            }
            mX /= finalShape.ToArray().Length;
            my /= finalShape.ToArray().Length;
            Color? fillColor = null;
            if (fillAlpha > 0) fillColor = Color.FromArgb(fillAlpha, color);
            Gui.DrawPolygon(finalShape.OrderBy(p => Math.Atan2(p.Y - my, p.X - mX)).ToArray(), color, fillColor);
        }
        protected void DrawEccoTriangle(int x1, int y1, int x2, int y2, int x3, int y3, Color color, int fillAlpha = 63)
        {
            Color? fillColor = null;
            Point[] triPoints =
            {
                new Point(x1, y1),
                new Point(x2, y2),
                new Point(x3, y3)
            };
            if (fillAlpha > 0) fillColor = Color.FromArgb(fillAlpha, color);
            Gui.DrawPolygon(triPoints, color, fillColor);
        }
        protected void DrawBoxMWH(int x, int y, int w, int h, Color color, int fillAlpha = 63)
        {
            Color? fillColor = null;
            if (fillAlpha > 0) fillColor = Color.FromArgb(fillAlpha, color);
            Gui.DrawRectangle(x - w, y - h, w << 1, h << 1, color, fillColor);
        }
        protected void DrawBox(int x, int y, int x2, int y2, Color color, int fillAlpha = 63)
        {
            Color? fillColor = null;
            if (fillAlpha > 0) fillColor = Color.FromArgb(fillAlpha, color);
            Gui.DrawBox(x, y, x2, y2, color, fillColor);
        }
        protected void Print_Text(string message, int x, int y, Color color)
        {
            Gui.DrawText(x, y, message, color, null);
        }
        protected void PutText(string message, int x, int y, int xl, int yl, int xh, int yh, Color bg, Color fg)
        {
            xl = Math.Max(xl, 0);
            yl = Math.Max(yl, 0);
            xh = Math.Min(xh + 639, 639);
            yh = Math.Min(yh + 441, 441);
            xh -= 4 * message.Length;
            x = x - ((5 * (message.Length - 1)) / 2);
            y -= 3;
            int[] xOffset = { -1, -1, -1, 0, 1, 1, 1, 0 };
            int[] yOffset = { -1, 0, 1, 1, 1, 0, -1, -1 };
            for (int i = 0; i < 8; i++)
                Print_Text(message, x + xOffset[i], y + yOffset[i], bg);
            Print_Text(message, x, y, fg);
        }
        protected void TickerText(string message, Color? fg = null)
        {
            if (!_mapDumpingEnabled)
                Gui.Text(10, _tickerY, message, fg);
            _tickerY += 16;
        }
        protected Color BackdropColor()
        {
            uint color = Mem.ReadU16(0, "CRAM");
            int r = (int)((color & 0x7) * 0x22);
            int g = (int)(((color >> 3) & 0x7) * 0x22);
            int b = (int)(((color >> 6) & 0x7) * 0x22);
            return Color.FromArgb(r, g, b);
        }
    }
}