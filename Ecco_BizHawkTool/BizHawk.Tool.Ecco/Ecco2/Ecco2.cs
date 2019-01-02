using System.Collections.Generic;
using System.Drawing;

using BizHawk.Client.ApiHawk;
using BizHawk.Client.EmuHawk;

namespace BizHawk.Tool.Ecco
{
    partial class Ecco2Tool : EccoToolBase
    {
        #region Type Defs and Loaders
        private static class AddrGlobal
        {
            public const uint LevelID = 0xFFA7D0;
            public const uint LevelFrameCount = 0xFFA7C8;
            public const uint FrameCount = 0xFFA520;
            public const uint NonLagFrameCount = 0xFFA524;
            public const uint GameMode = 0xFFA555;
            public const uint TextYSpeed = 0xFFF342;
        }
        #endregion

        #region fields
        private int _camX = 0;
        private int _camY = 0;
        private uint _prevF = 0;
        private int _top = 0;
        private int _bottom = 0;
        private int _left = 0;
        private int _right = 0;
        private const int _signalAlpha = 255;
        private int _prevX = 0;
        private int _prevY = 0;
        private int _destX = 0;
        private int _destY = 0;
        private int _snapPast = 0;
        private string _rowStateGuid = string.Empty;
        private Color[] _turnSignalColors =
        {
            Color.FromArgb(_signalAlpha, 127, 127,   0),
            Color.FromArgb(_signalAlpha, 255,   0,   0),
            Color.FromArgb(_signalAlpha, 192,   0,  63),
            Color.FromArgb(_signalAlpha,  63,   0, 192),
            Color.FromArgb(_signalAlpha,   0,   0, 255),
            Color.FromArgb(_signalAlpha,   0,  63, 192),
            Color.FromArgb(_signalAlpha,   0, 192,  63),
            Color.FromArgb(_signalAlpha,   0, 255,   0)
        };
        #endregion
        #region Methods
        void AutoFire(bool on)
        {
            //Modif N - ECCO HACK - make caps lock (weirdly) autofire player 1's C key
            uint charge;
            int frameCount = Emu.FrameCount();
            int lagCount = Emu.LagCount();
            Joy.Set("Start", on, 1);
            switch (Mem.ReadU8(AddrGlobal.GameMode))
            {
                case 0x00:
                    if (on)
                    {
                        if (Mem.ReadS16(AddrGlobal.TextYSpeed) < 0)
                            Joy.Set("C", true, 1);
                        else
                            Joy.Set("C", false, 1);
                    }
                    break;
                case 0xE6:
                    if (Mem.ReadU16(0xFFD5E8) == 0x00000002)
                    {
                        Dictionary<string, bool> buttons = new Dictionary<string, bool>();
                        buttons["B"] = buttons["C"] = true;
                        Joy.Set(buttons, 1);
                    }
                    else
                    {
                        Dictionary<string, bool> buttons = new Dictionary<string, bool>();
                        buttons["B"] = buttons["C"] = false;
                        Joy.Set(buttons, 1);
                    }
                    break;
                case 0xF6:
                    charge = Mem.ReadU8(0xFFB19B);
                    if (on)
                    {
                        if ((charge <= 1) && ((Mem.ReadU8(0xFFB1A6) == 0) || (Mem.ReadU8(0xFFB1A9) != 0)))
                            Joy.Set("B", true, 1);
                        else if (charge > 1)
                            Joy.Set("B", false, 1);
                        Joy.Set("C", (Mem.ReadU16(AddrGlobal.LevelFrameCount) % 2) == 0, 1);
                    }
                    break;
                case 0x20:
                case 0x28:
                case 0xAC:
                    if (on)
                    {
                        if ((Mem.ReadU8(0xFFAB72) & 3) == 0)
                            Joy.Set("C", (Mem.ReadS8(0xFFAA6E) < 11), 1);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        public Ecco2Tool(CustomMainForm f, GameRegion r) : base(f, r)
        {
            _top = _bottom = 112;
            _left = _right = 160;
            ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
            switch (r)
            {
                case GameRegion.J:
                    _3DTypeProvider = new J3DProvider();
                    break;
                case GameRegion.U:
                    _3DTypeProvider = new U3DProvider();
                    _2DTypeProvider = new U2DProvider();
                    break;
                case GameRegion.E:
                    _3DTypeProvider = new E3DProvider();
                    break;
                default:
                    break;
            }
        }
        public override void PreFrameCallback()
        {
        Gui.ClearText();
        if (!Gui.HasGUISurface)
            Gui.DrawNew("emu");
            _camX = Mem.ReadS16(Addr2D.CamX) - _left;
            _camY = Mem.ReadS16(Addr2D.CamY) - _top;
            AutoFire(_autofireEnabled);
            if (!_mapDumpingEnabled || (_mapDumpState == 0))
            {
                Color bg = BackdropColor();
                Gui.DrawRectangle(0, 0, _left + 320 + _right, _top, bg, bg);
                Gui.DrawRectangle(0, 0, _left, _top + 224 + _bottom, bg, bg);
                Gui.DrawRectangle(_left + 320, 0, _left + 320 + _right, _top + 224 + _bottom, bg, bg);
                Gui.DrawRectangle(0, _top + 224, _left + 320 + _right, _top + 224 + _bottom, bg, bg);
            }
            switch (Mem.ReadU8(AddrGlobal.GameMode))
            {
                case 0x20:
                case 0x28:
                case 0xAC:
                    if (_mapDumpState <= 1) Draw2DHud();
                    if (_mapDumpingEnabled)
                    {
                        PreProcessMapDump();
                    }
                    break;
                case 0xF6:
                    Draw3DHud();
                    break;
                default:
                    break;
            }
            _prevF = Mem.ReadU32(AddrGlobal.NonLagFrameCount);
        }
        public override void PostFrameCallback()
        {
            uint frame = Mem.ReadU32(AddrGlobal.NonLagFrameCount);
            if ((frame <= _prevF) && !Emu.IsLagged())
            {
                Emu.SetIsLagged(true);
                Emu.SetLagCount(Emu.LagCount() + 1);
            }
            uint mode = Mem.ReadByte(AddrGlobal.GameMode);
            _tickerY = 48;
            TickerText($"Frames: {Mem.ReadU32(AddrGlobal.FrameCount)}\tNonlag Frames: {Mem.ReadU32(AddrGlobal.NonLagFrameCount)}\tLevel Frames:{Mem.ReadU16(AddrGlobal.LevelFrameCount)}\tGameMode: {mode:X2}");
            switch (mode)
            {
                case 0x20:
                case 0x28:
                case 0xAC:
                    Update2DTickers();
                    if (_mapDumpState != 0)
                    {
                        PostProcessMapDump();
                    }
                    _prevX = _camX;
                    _prevY = _camY;
                    int levelTime = Mem.ReadS16(AddrGlobal.LevelFrameCount);
                    var color = _turnSignalColors[levelTime & 7];
                    Gui.DrawRectangle(_left - 48, _top - 112, 15, 15, color, color);
                    break;
                case 0xF6:
                    Update3DTickers();
                    break;
            }
            Joy.Set("C", null, 1);
            Joy.Set("Start", null, 1);
            Gui.DrawFinish();
        }
    }
}
