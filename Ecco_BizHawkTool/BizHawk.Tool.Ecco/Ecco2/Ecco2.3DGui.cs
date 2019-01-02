using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

using BizHawk.Client.ApiHawk;
using BizHawk.Emulation.Common.WorkingTypes;
using BizHawk.Emulation.Cores.Consoles.Sega.gpgx;

namespace BizHawk.Tool.Ecco
{
    partial class Ecco2Tool : EccoToolBase
    {
        #region Object Type Helpers
        private enum Obj3DType
        {
            Player,
            Shell,
            PoisonBubble,
            Ring,
            Vine,
            JellyFish,
            Shark,
            Eagle,
            SonarBlast,
            HomingBubble,
            GfxBubble1,
            GfxBubble2,
            GfxSplash,
            unknown
        }
        private abstract class Obj3DTypeProvider
        {
            public abstract Obj3DType GetType(uint addr);
        }
        private class J3DProvider : Obj3DTypeProvider
        {
            private Dictionary<uint, Obj3DType> _typeMap;
            public J3DProvider()
            {
            }
            public override Obj3DType GetType(uint addr)
            {
                if (_typeMap.ContainsKey(addr))
                    return _typeMap[addr];
                else return Obj3DType.unknown;
            }
        }
        private class U3DProvider : Obj3DTypeProvider
        {
            private Dictionary<uint, Obj3DType> _typeMap;
            public U3DProvider()
            {
                _typeMap = new Dictionary<uint, Obj3DType>();
                _typeMap[0xD6B6E] = Obj3DType.Player;
                _typeMap[0xD4DBA] = Obj3DType.Shell;
                _typeMap[0xD4AB8] = Obj3DType.PoisonBubble;
                _typeMap[0xD817E] = Obj3DType.Ring;
                _typeMap[0xD49CC] = Obj3DType.Vine;
                _typeMap[0xD3B40] = Obj3DType.JellyFish;
                _typeMap[0xD3DB2] = Obj3DType.Shark;
                _typeMap[0xD4028] = Obj3DType.Eagle;
                _typeMap[0xD4432] = Obj3DType.SonarBlast;
                _typeMap[0xD463A] = Obj3DType.HomingBubble;
                _typeMap[0xD37CE] = Obj3DType.GfxBubble1;
                _typeMap[0xD3808] = Obj3DType.GfxBubble2;
                _typeMap[0xD4214] = Obj3DType.GfxSplash;
            }
            public override Obj3DType GetType(uint addr)
            {
                if (_typeMap.ContainsKey(addr))
                    return _typeMap[addr];
                else return Obj3DType.unknown;
            }
        }
        private class E3DProvider : Obj3DTypeProvider
        {
            private Dictionary<uint, Obj3DType> _typeMap;
            public E3DProvider()
            {
            }
            public override Obj3DType GetType(uint addr)
            {
                if (_typeMap.ContainsKey(addr))
                    return _typeMap[addr];
                else return Obj3DType.unknown;
            }
        }
        private Obj3DTypeProvider _3DTypeProvider;
        #endregion

        #region Definitions and Object Loaders
        private unsafe struct Obj3D
        {
            public uint PtrNext;       // Ptr to next object in RAM
            public wushort Flags;      // Bitwise bool array. Unsure what values mean but all 0s is marker for deletion
            public int XPos;           // Horizontal center of object
            public int ZPos;           // Depth-wise center of object
            public int YPos;           // Vertical center of object
            public int Width;          // Width of hitbox or radius of octagonal hitbox
            public int Depth;          // Depth of hitbox
            public int Height;         // Height of hitbox
            public int prevSpeed;      // Scratch variable that happens to store what Ecco's speed was prior to beginning a charge
            public int XOrig;          // Original horizontal center of object
            public int ZOrig;          // Original depth-wize center of object
            public int YOrig;          // Original vertical center of object
            public int XVel;           // Horizontal movement speed
            public int ZVel;           // Depth-wise movement speed
            public int YVel;           // Vertical movement speed
            public int XUnk;
            public int ZUnk;
            public int YUnk;
            public fixed int unkArr[5];
            public uint PLC;           // Pointer to sprite information structure
            public wshort Sprite;       // Which frame of animation to display
            public wshort SpriteScale;  // How big is the sprite based on zdist from camera
            public uint PtrMainFunc;   // Ptr to main function address. Proxy for object type, because it's the only way the game tracks 3D Object type
            public wshort unks0;
            public wshort unks1;
            public wshort Mode;
            public wbyte AccelCounter;  // Mostly just for Ecco, counts where he is in acceleration cycle. Mostly useless because the first frame is fastest
            public wbyte PrevControls;  // Mostly just for Ecco, what buttons were pressed last frame
            public wbyte CurControls;   // Mostly just for Ecco, what buttons are pressed now
            public wbyte ChargeCtr;     // Mostly just for Ecco, countdown until next charge possible
            public wbyte LastWasCharge; // Mostly just for Ecco, was charge the last button touched
            public wbyte InvincCount;   // Mostly just for Ecco, countdown until next vulnerable
            public int unk5;
            public int Damage;
            public wbyte State;
            public wbyte ControlsLocked;
            public wbyte unkb0;
            public wbyte BreachCtr;     // Mostly just for Ecco, how long until B is charge again instead of super-breach
            public int ScreenX;        // Object 2d-projected horizontal coord
            public int ScreenY;        // Object 2d-projected vertical coord
            public wbyte unkb1;
            public wbyte unkb2;
        }
        private void ReadObj3D(uint addr, out Obj3D obj)
        {
            obj = new Obj3D();
            obj.PtrNext = ReadPtrAndAdvance(ref addr);
            obj.Flags = ReadU16AndAdvance(ref addr);
            obj.XPos = ReadS32AndAdvance(ref addr);
            obj.ZPos = ReadS32AndAdvance(ref addr);
            obj.YPos = ReadS32AndAdvance(ref addr);
            obj.Width = ReadS32AndAdvance(ref addr);
            obj.Depth = ReadS32AndAdvance(ref addr);
            obj.Height = ReadS32AndAdvance(ref addr);
            obj.prevSpeed = ReadS32AndAdvance(ref addr);
            obj.XOrig = ReadS32AndAdvance(ref addr);
            obj.ZOrig = ReadS32AndAdvance(ref addr);
            obj.YOrig = ReadS32AndAdvance(ref addr);
            obj.XVel = ReadS32AndAdvance(ref addr);
            obj.ZVel = ReadS32AndAdvance(ref addr);
            obj.YVel = ReadS32AndAdvance(ref addr);
            obj.XUnk = ReadS32AndAdvance(ref addr);
            obj.ZUnk = ReadS32AndAdvance(ref addr);
            obj.YUnk = ReadS32AndAdvance(ref addr);
            addr += 12;
            obj.PLC = ReadPtrAndAdvance(ref addr);
            obj.Sprite = ReadS16AndAdvance(ref addr);
            obj.SpriteScale = ReadS16AndAdvance(ref addr);
            obj.PtrMainFunc = ReadPtrAndAdvance(ref addr);
            obj.unks0 = ReadS16AndAdvance(ref addr);
            obj.unks1 = ReadS16AndAdvance(ref addr);
            obj.Mode = ReadS16AndAdvance(ref addr);
            obj.AccelCounter = ReadByteAndAdvance(ref addr);
            obj.PrevControls = ReadByteAndAdvance(ref addr);
            obj.CurControls = ReadByteAndAdvance(ref addr);
            obj.ChargeCtr = ReadByteAndAdvance(ref addr);
            obj.LastWasCharge = ReadByteAndAdvance(ref addr);
            obj.InvincCount = ReadByteAndAdvance(ref addr);
            obj.unk5 = ReadS32AndAdvance(ref addr);
            obj.Damage = ReadS32AndAdvance(ref addr);
            obj.State = ReadByteAndAdvance(ref addr);
            obj.ControlsLocked = ReadByteAndAdvance(ref addr);
            obj.unkb0 = ReadByteAndAdvance(ref addr);
            obj.BreachCtr = ReadByteAndAdvance(ref addr);
            obj.ScreenX = ReadS32AndAdvance(ref addr);
            obj.ScreenY = ReadS32AndAdvance(ref addr);
            obj.unkb1 = ReadByteAndAdvance(ref addr);
            obj.unkb2 = ReadByteAndAdvance(ref addr);
        }
        private static class Addr3D
        {
            public const uint CamX = 0xFFD5E0;
            public const uint CamY = 0xFFD5E8;
            public const uint CamZ = 0xFFD5E4;
            public const uint CamZ2 = 0xFFD5F0;
            public const uint RingSpawnX = 0xFFD856;
            public const uint RingSpawnY = 0xFFD85A;
            public const uint PlayerObj = 0xFFB134;
            public const uint AnimLLHead = 0xFFD4C0;
        }
        #endregion

        private void GetEcco3DScreenCoords(Obj3D obj, int camX, int camY, int camZ, out int X, out int Y, out int Z)
        {
            X = 160 + ((obj.XPos >> 0xC) - camX);
            Y = 112 - ((obj.YPos >> 0xC) - camY);
            Z = _top + 112 - ((obj.ZPos >> 0xC) - camZ);
        }
        private void Draw3DHud()
        {
            int CamX = (Mem.ReadS32(Addr3D.CamX) >> 0xC) - _left;
            int CamY = (Mem.ReadS32(Addr3D.CamY) >> 0xC) + _top;
            int CamZ = (Mem.ReadS32(Addr3D.CamZ) >> 0xC) + _top;
            Obj3D player;
            ReadObj3D(Addr3D.PlayerObj, out player);
            Obj3D curObj;
            uint Addr = ReadPtr(Addr3D.AnimLLHead);
            while (Addr != 0)
            {
                ReadObj3D(Addr, out curObj);
                Obj3DType type = _3DTypeProvider.GetType(curObj.PtrMainFunc);
                int X, Y, Z;
                GetEcco3DScreenCoords(curObj, CamX, CamY, CamZ, out X, out Y, out Z);
                int radius, width, height, depth = height = width = 0;
                switch (type)
                {
                    case Obj3DType.Player:
                        DrawBoxMWH(X, Y, 1, 1, Color.Orange);
                        DrawBoxMWH(X, Z, 1, 1, Color.Red);
                        break;
                    case Obj3DType.Shell:
                    case Obj3DType.Eagle:
                        DrawBoxMWH(X, Y, 1, 1, Color.Lime, 0);
                        DrawBoxMWH(X, Z, 1, 1, Color.Blue, 0);
                        break;
                    case Obj3DType.PoisonBubble:
                        depth = 0x10;
                        radius = width = 8;
                        DrawOct(X, Y, radius, Color.Lime);
                        DrawBoxMWH(X, Z, width, depth, Color.Blue);
                        DrawBoxMWH(X, Y, 1, 1, Color.Lime, 0);
                        DrawBoxMWH(X, Z, 1, 1, Color.Blue, 0);
                        break;
                    case Obj3DType.Ring:
                        depth = 8;
                        if (player.ZVel < 0x1800) depth = 4;
                        radius = 32;
                        width = radius;
                        DrawOct(X, Y, radius, (curObj.Mode == 0) ? Color.Orange : Color.Gray);
                        DrawBoxMWH(X, Z, width, depth, Color.Red);
                        DrawBoxMWH(X, Y, 1, 1, Color.Orange, 0);
                        DrawBoxMWH(X, Z, 1, 1, Color.Red, 0);
                        TickerText($"{curObj.XPos / 4096.0:0.######}:{curObj.YPos / 4096.0:0.######}:{curObj.ZPos / 2048.0:0.######}:{curObj.State}", Color.Lime);
                        break;
                    case Obj3DType.Vine: // Vine collisions are based on draw position, which is a fucking pain in the ass to calculate
                        {
                            int Xvel = (curObj.XUnk - curObj.XPos);
                            int Zvel = (curObj.ZUnk - curObj.ZPos);
                            int dx = Mem.ReadS32(Addr3D.CamX) - Mem.ReadS32(0xFFD5C8) >> 3;
                            int dy = Mem.ReadS32(Addr3D.CamY) - Mem.ReadS32(0xFFD5D0);
                            dy = (dy >> 4) - (dy >> 6);
                            int dz = Mem.ReadS32(Addr3D.CamZ) - Mem.ReadS32(0xFFD5CC);
                            var chargeCount = player.ChargeCtr;
                            if (chargeCount == 0)
                            {
                                dz >>= 2;
                            }
                            else if ((chargeCount > 0x20) || (chargeCount <= 0x10))
                            {
                                dz >>= 3;
                            }
                            else if (chargeCount > 0x10)
                            {
                                dz >>= 4;
                            }
                            if (curObj.AccelCounter == 0)
                            {
                                Xvel >>= 0xA;
                                Zvel >>= 9;
                            }
                            else
                            {
                                Xvel >>= 9;
                                Zvel >>= 0xA;
                            }
                            Xvel += curObj.XVel;
                            Zvel += curObj.ZVel;
                            int Zpos = (curObj.ZOrig + dz - Mem.ReadS32(Addr3D.CamZ)) >> 0xB;
                            if ((Zpos < 0x600) && (Zpos > 0))
                            {
                                Zpos += 0x20;
                                int Xcur, Xmax, Ycur, Ymax;
                                int Zpos2 = (curObj.ZPos + Zvel + dz - Mem.ReadS32(Addr3D.CamZ)) >> 0xB;
                                Zpos2 = Math.Max(Zpos2 + 0x20, 1);
                                if (curObj.Mode != 0)
                                {
                                    int Xmid = curObj.XPos + dx + Xvel - Mem.ReadS32(Addr3D.CamX);
                                    if (Math.Abs(Xmid) > 0x400000)
                                        continue;
                                    int Xpos = curObj.XOrig + dx - Mem.ReadS32(Addr3D.CamX);
                                    if (Math.Abs(Xpos) > 0x400000)
                                        continue;
                                    Xcur = (Xmid << 2) / Zpos2 + (Xmid >> 5) + 0xA000 + (Xmid >> 5);
                                    Xmax = (Xpos << 2) / Zpos + (Xpos >> 5) + 0xA000 + (Xpos >> 5);
                                }
                                else
                                {
                                    Xcur = 0;
                                    Xmax = 256;
                                }
                                int Ymid = Mem.ReadS32(Addr3D.CamY) + dy - curObj.YPos;
                                Ycur = ((Ymid << 3) / Zpos2) + 0x6000;
                                int Ypos = Mem.ReadS32(Addr3D.CamY) + dy - curObj.YOrig;
                                Ymax = ((Ypos << 3) / Zpos) + 0x6000;
                                dx = Xmax - Xcur;
                                dy = Ymax - Ycur;
                                wshort ang = Ecco2Asin(dx,dy);
                                Xcur += Mem.ReadS8(0x2CC8 + ang) << 6;
                                Ycur += Mem.ReadS8(0x2BC8 + ang) << 6;
                                int rad = Math.Max(((OctRad(dx, dy) >> 8) + 0x1F) >> 5, 1);
                                dx /= rad;
                                dy /= rad;
                                int Zmid = (curObj.ZPos + curObj.ZOrig) >> 1;
                                Zmid >>= 0xC;
                                Zmid = 112 + _top - (Zmid - CamZ);
                                do
                                {
                                    rad--;
                                    DrawRhomb((Xcur >> 8) + _left, (Ycur >> 8) + _top, 8, Color.Lime);
                                    DrawBoxMWH((Xcur >> 8) + _left, Zmid, 8, 0x10, Color.Blue);
                                    Xcur += dx;
                                    Ycur += dy;
                                } while (rad >= 0);
                                DrawBoxMWH((Mem.ReadS32(player.ScreenX) >> 8) + _left, (player.ScreenY >> 8) + _top, 1, 1, Color.Lime, 0);
                            }
                        }
                        break;
                    case Obj3DType.Shark:
                    case Obj3DType.JellyFish:
                        width = (curObj.Width >> 0xC);
                        height = (curObj.Height >> 0xC);
                        depth = (curObj.Depth >> 0xC);
                        DrawBoxMWH(X, Y, width, height, Color.Lime);
                        DrawBoxMWH(X, Z, width, depth, Color.Blue);
                        DrawBoxMWH(X, Y, 1, 1, Color.Lime, 0);
                        DrawBoxMWH(X, Z, 1, 1, Color.Blue, 0);
                        break;
                    case Obj3DType.SonarBlast:
                        DrawOct(X, Y, 48, Color.Orange);
                        DrawOct(X, Y, 32, Color.Lime);
                        DrawBoxMWH(X, Z, 32, 32, Color.Blue);
                        DrawBoxMWH(X, Y, 1, 1, Color.Lime, 0);
                        DrawBoxMWH(X, Z, 1, 1, Color.Blue, 0);
                        break;
                    case Obj3DType.HomingBubble:
                        DrawOct(X, Y, 32, Color.Lime);
                        DrawBoxMWH(X, Z, 32, 32, Color.Blue);
                        DrawBoxMWH(X, Y, 1, 1, Color.Lime, 0);
                        DrawBoxMWH(X, Z, 1, 1, Color.Blue, 0);
                        break;
                    case Obj3DType.GfxBubble1:
                    case Obj3DType.GfxBubble2:
                    case Obj3DType.GfxSplash:
                        break;
                    default:
                        DrawBoxMWH(X, Y, 1, 1, Color.Lime, 0);
                        DrawBoxMWH(X, Z, 1, 1, Color.Blue, 0);
                        PutText(curObj.PtrMainFunc.ToString("X8"), X, Y - 4, 1, 1, -1, -9, Color.White, Color.Blue);
                        PutText(Addr.ToString("X8"), X, Y + 4, 1, 9, -1, -1, Color.White, Color.Blue);
                        break;
                }
                Addr = curObj.PtrNext;
            }
        }
        private void Ecco3DPredictSpawn()
        {
            string valueTicker;
            int SpawnZ = Mem.ReadS32(Addr3D.CamZ2) + 0x180000;
            int nextRingZ = SpawnZ;
            ReadObj3D(Addr3D.PlayerObj, out Obj3D player);
            while (((nextRingZ >> 17) & 0xF) != 0)
            {
                nextRingZ += 0x20000;
            }
            valueTicker = $"{Mem.ReadS32(Addr3D.RingSpawnX) / 4096.0:0.######}:{Mem.ReadS32(Addr3D.RingSpawnY) / 4096.0:0.######}:{(nextRingZ - 0x160000) / 2048.0:0.######}:{nextRingZ / 2048.0:0.######}";
            TickerText(valueTicker);
            var levelId = -1 - Mem.ReadS16(0xFFA79E);
            bool spawn = false;
            bool firstRand = true;
            int SpawnX, SpawnY, z;
            int CamX = (Mem.ReadS32(Addr3D.CamX) >> 0xC) - _left;
            int CamY = (Mem.ReadS32(Addr3D.CamY) >> 0xC) + _top;
            int CamZ = (Mem.ReadS32(Addr3D.CamY) >> 0xC) + _top;
            while (!spawn)
            {
                var temp = (SpawnZ >> 17) & 0xFF;
                var controlList = Mem.ReadS32(0x7B54 + (levelId << 2));
                temp = Mem.ReadS16(controlList + (temp << 1));
                var v = temp & 0xFF;
                var num = (temp >> 8) + v;
                temp = v;
                spawn = (num > 2);
                if (spawn) for (; temp < num; temp++)
                    {
                        switch (temp)
                        {
                            case 0:
                            case 1:
                            case 13:
                                // Nothing important spawns
                                break;
                            case 2:
                                // Jellyfish
                                SpawnX = player.XPos + 0x40000 - (Rand(firstRand) << 3);
                                firstRand = false;
                                SpawnY = -0xC0000 + (Rand() << 3);
                                z = SpawnZ + 0x20000;
                                valueTicker = $"{SpawnX / 4096.0:0.######}:{SpawnY / 4096.0:0.######}:{(z - 0x180000) / 2048.0:0.######}:{z / 2048.0:0.######}";
                                TickerText(valueTicker);
                                SpawnX = 160 + ((SpawnX >> 0xC) - CamX);
                                SpawnY = 112 - ((SpawnY >> 0xC) - CamY);
                                z = _top + 112 - ((z >> 0xC) - CamZ);
                                DrawBoxMWH(SpawnX, SpawnY, 1, 1, Color.Gray);
                                DrawBoxMWH(SpawnX, z, 1, 1, Color.Gray);
                                break;
                            case 3:
                                // Eagle
                                SpawnX = player.XPos + 0x40000 - (Rand(firstRand) << 3);
                                firstRand = false;
                                SpawnY = 0x50000;
                                z = SpawnZ - 0x40000 + 0x20000;
                                valueTicker = $"{SpawnX / 4096.0:0.######}:{SpawnY / 4096.0:0.######}:{(z - 0x180000) / 2048.0:0.######}:{z / 2048.0:0.######}";
                                TickerText(valueTicker);
                                SpawnX = 160 + ((SpawnX >> 0xC) - CamX);
                                SpawnY = 112 - ((SpawnY >> 0xC) - CamY);
                                z = _top + 112 - ((z >> 0xC) - CamZ);
                                DrawBoxMWH(SpawnX, SpawnY, 1, 1, Color.Gray);
                                DrawBoxMWH(SpawnX, z, 1, 1, Color.Gray);
                                break;
                            case 4:
                                // Shark
                                bool left = (Rand(firstRand) > 0x8000);
                                firstRand = false;
                                var xdiff = 0xC0000 + (Rand() << 3);
                                SpawnX = player.XPos + (left ? -xdiff : xdiff);
                                SpawnY = Math.Min(Mem.ReadS32(0xFFB142), -0x10000) - (Rand() + 0x10000);
                                z = SpawnZ + 0x20000;
                                valueTicker = $"{SpawnX / 4096.0:0.######}:{SpawnY / 4096.0:0.######}:{(z - 0x180000) / 2048.0:0.######}:{z / 2048.0:0.######}";
                                TickerText(valueTicker);
                                SpawnX = 160 + ((SpawnX >> 0xC) - CamX);
                                SpawnY = 112 - ((SpawnY >> 0xC) - CamY);
                                z = _top + 112 - ((z >> 0xC) - CamZ);
                                DrawBoxMWH(SpawnX, SpawnY, 1, 1, Color.Gray);
                                DrawBoxMWH(SpawnX, z, 1, 1, Color.Gray);
                                break;
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                // Vine
                                Rand(firstRand);
                                firstRand = false;
                                if ((temp & 1) == 1) Rand();
                                Rand();
                                break;
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                // Unknown, possibly just rand incrementation?
                                Rand(firstRand);
                                firstRand = false;
                                if ((temp & 1) == 1) Rand();
                                break;
                            case 14:
                                // Shell
                                SpawnX = player.XPos - 0x20000 + (Rand(firstRand) << 2);
                                firstRand = false;
                                SpawnY = -0x80000;
                                z = SpawnZ + 0x20000;
                                Rand();
                                valueTicker = $"{SpawnX / 4096.0:0.######}:{SpawnY / 4096.0:0.######}:{(z - 0x180000) / 2048.0:0.######}:{(z - 0x80000) / 2048.0:0.######}";
                                TickerText(valueTicker);
                                SpawnX = 160 + ((SpawnX >> 0xC) - CamX);
                                SpawnY = 112 - ((SpawnY >> 0xC) - CamY);
                                z = _top + 112 - ((z >> 0xC) - CamZ);
                                DrawBoxMWH(SpawnX, SpawnY, 1, 1, Color.Gray);
                                DrawBoxMWH(SpawnX, z, 1, 1, Color.Gray);
                                break;
                        }
                    }
                SpawnZ += 0x20000;
            }
        }
        private void Update3DTickers()
        {
            int CamX = Mem.ReadS32(Addr3D.CamX);
            int CamY = Mem.ReadS32(Addr3D.CamY);
            int CamZ = Mem.ReadS32(Addr3D.CamZ);
            Obj3D player;
            ReadObj3D(Addr3D.PlayerObj, out player);
            TickerText($"CamX: {CamX / 4096.0:0.######}\t CamY: {CamY / 4096.0:0.######}\t CamZ: {CamZ / 2048.0:0.######}");
            TickerText($"Player Pos X: {player.XPos / 4096.0:0.######}\t Y: {player.YPos / 4096.0:0.######}\t Z: {player.ZPos / 2048.0:0.######}");
            TickerText($"Player Vel X: {player.XVel / 4096.0:0.######}\t Y: {player.YVel / 4096.0:0.######}\t Z: {player.ZVel / 2048.0:0.######}");
            TickerText($"Charge Count: {player.ChargeCtr}\t Player State: {player.State} Player Breach Count: {player.BreachCtr}");
        }
    }
}