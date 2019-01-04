using System;

using BizHawk.Emulation.Common.WorkingTypes;

namespace BizHawk.Tool.Ecco
{
    partial class Ecco2Tool
    {
        private wshort Ecco2Asin(int dx, int dy)
        {
            wshort angle;
            int abs_dx = Math.Abs(dx >> 6) & 0xFFFF;
            int abs_dy = Math.Abs(dy >> 6) & 0xFFFF;
            if (abs_dx == abs_dy)
            {
                if (dx > 0)
                {
                    if (dy > 0)
                    {
                        angle = 0x20;
                    }
                    else
                    {
                        angle = 0xE0;
                    }
                }
                else
                {
                    if (dy > 0)
                    {
                        angle = 0x60;
                    }
                    else
                    {
                        angle = 0xA0;
                    }
                }
            }
            else
            {
                if (abs_dx > abs_dy)
                {
                    abs_dy <<= 5;
                    abs_dy += abs_dx - 1;
                    abs_dy &= 0xFFFF;
                    abs_dy /= abs_dx;
                }
                else
                {
                    abs_dx <<= 5;
                    abs_dx += abs_dy - 1;
                    abs_dx &= 0xFFFF;
                    abs_dx /= abs_dy;
                    abs_dy = 0x40 - abs_dx;
                }
                if (dx > 0)
                {
                    if (dy > 0)
                    {
                        angle = abs_dy;
                    }
                    else
                    {
                        angle = 0xff - abs_dy;
                    }
                }
                else
                {
                    if (dy > 0)
                    {
                        angle = 0x7f - abs_dy;
                    }
                    else
                    {
                        angle = 0x81 + abs_dy;
                    }
                }
            }
            return angle;
        }
        private int OctRad(int dx, int dy)
        {
            int dSml = Math.Abs(dx);
            int dBig = Math.Abs(dy);
            if (dBig < dSml)
            {
                dSml ^= dBig;
                dBig ^= dSml;
                dSml ^= dBig;
            }
            return (dBig + (dSml >> 1) - (dSml >> 3));
        }
        private int _rseed = 1;
        private int Rand(bool refresh = false)
        {
            if (refresh)
            {
                _rseed = (int)(Mem.ReadU16(AddrGlobal.RandomSeed));
            }
            bool odd = (_rseed & 1) != 0;
            _rseed >>= 1;
            if (odd)
            {
                _rseed ^= 0xB400;
            }
            return _rseed;
        }
    }
}