using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

using BizHawk.Client.ApiHawk;
using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Cores.Consoles.Sega.gpgx;

namespace BizHawk.Tool.Ecco
{
    partial class Ecco2Tool : EccoToolBase
    {
        #region Type Defs and Loaders
        private static class AddrGlobal
        {
            public const uint LevelID = 0xFFA7D0;
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
        private int _mapDumpState = 0;
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
        void EccoAutofire(bool on)
        {
            //Modif N - ECCO HACK - make caps lock (weirdly) autofire player 1's C key
            uint charge;
            uint mode = Mem.ReadU8(0xFFA555);
            int frameCount = Emu.FrameCount();
            int lagCount = Emu.LagCount();
            Joy.Set("Start", on, 1);
            switch (mode)
            {
                case 0x00:
                    if (on)
                    {
                        if (Mem.ReadU16(0xFFF342) == 0xFFFD)
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
                        Joy.Set("C", (Mem.ReadU16(0xFFA7C8) % 2) == 0, 1);
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
            EccoAutofire(Joy.Get(1)["Start"]);
            if (_mapDumpState == 0)
            {
                Color bg = BackdropColor();
                Gui.DrawRectangle(0, 0, _left + 320 + _right, _top, bg, bg);
                Gui.DrawRectangle(0, 0, _left, _top + 224 + _bottom, bg, bg);
                Gui.DrawRectangle(_left + 320, 0, _left + 320 + _right, _top + 224 + _bottom, bg, bg);
                Gui.DrawRectangle(0, _top + 224, _left + 320 + _right, _top + 224 + _bottom, bg, bg);
            }
            uint mode = Mem.ReadByte(0xFFA555);
            switch (mode)
            {
                case 0x20:
                case 0x28:
                case 0xAC:
                    if (_mapDumpState <= 1) Draw2DHud();
                    if ((_mapDumpingEnabled) && (Mem.ReadU16(0xFFA7C8) > 1) && (Mem.ReadU16(0xFFA7C8) < 4))
                    {
                        _mapDumpState = 1;
                        _rowStateGuid = string.Empty;
                        _top = _bottom = _left = _right = 0;
                        ClientApi.SetGameExtraPadding(0, 0, 0, 0);
                    }
                    if (_mapDumpState == 3)
                    {
                        var levelID = Mem.ReadS8(0xFFA7D0);
                        int[] nameGroupLengths =
                        {
                            7,1,11,6,
                            4,3,3,3,
                            7,1,2,1,
                            0,0,0,0
                        };
                        int[] nameStringPtrOffsets =
                        {
                            0xECBD0, 0x106BC0, 0x10AF8C, 0x135A48,
                            0x1558E8, 0x15F700, 0x16537C, 0x180B00,
                            0x193920, 0x1B3ECC, 0x1D7A44, 0x1DBF70,
                            0x2DF2, 0x2DF6, 0x2DFA, 0x2DFE
                        };
                        int nameGroup = 0;
                        var i = levelID;
                        while ((i >= 0) && (nameGroup < nameGroupLengths.Length))
                        {
                            i -= nameGroupLengths[nameGroup];
                            if (i >= 0) nameGroup++;
                        }
                        string name = "map";
                        if (i < 0)
                        {
                            i += nameGroupLengths[nameGroup];
                            uint strOffset = Mem.ReadU32(nameStringPtrOffsets[nameGroup] + 0x2E);
                            Console.WriteLine($"{i}");
                            strOffset = Mem.ReadU32(strOffset + ((i << 3) + (i << 5)) + 0x22);
                            strOffset += 0x20;
                            List<byte> strTmp = new List<byte>();
                            byte c;
                            do
                            {
                                c = (byte)Mem.ReadByte(strOffset++);
                                if (c != 0)
                                    strTmp.Add(c);
                            } while (c != 0);
                            name = System.Text.Encoding.ASCII.GetString(strTmp.ToArray());
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                            name = textInfo.ToTitleCase(name).Replace(" ", string.Empty);
                        }
                        ClientApi.Screenshot($"c:\\Ecco2Maps\\{levelID}_{name}_top.png");
                        _destX = _destY = 0;
                        ClientApi.SetGameExtraPadding(0, 0, 0, 0);
                        _mapDumpState++;
                    }
                    if (_mapDumpState == 6)
                    {
                        var levelID = Mem.ReadS8(0xFFA7D0);
                        int[] nameGroupLengths =
                        {
                            7,1,11,6,
                            4,3,3,3,
                            7,1,2,1,
                            0,0,0,0
                        };
                        int[] nameStringPtrOffsets =
                        {
                            0xECBD0, 0x106BC0, 0x10AF8C, 0x135A48,
                            0x1558E8, 0x15F700, 0x16537C, 0x180B00,
                            0x193920, 0x1B3ECC, 0x1D7A44, 0x1DBF70,
                            0x2DF2, 0x2DF6, 0x2DFA, 0x2DFE
                        };
                        int nameGroup = 0;
                        var i = levelID;
                        while ((i >= 0) && (nameGroup < nameGroupLengths.Length))
                        {
                            i -= nameGroupLengths[nameGroup];
                            if (i >= 0) nameGroup++;
                        }
                        string name = "map";
                        if (i < 0)
                        {
                            i += nameGroupLengths[nameGroup];
                            uint strOffset = Mem.ReadU32(nameStringPtrOffsets[nameGroup] + 0x2E);
                            Console.WriteLine($"{i}");
                            strOffset = Mem.ReadU32(strOffset + ((i << 3) + (i << 5)) + 0x22);
                            strOffset += 0x20;
                            List<byte> strTmp = new List<byte>();
                            byte c;
                            do
                            {
                                c = (byte)Mem.ReadByte(strOffset++);
                                if (c != 0)
                                    strTmp.Add(c);
                            } while (c != 0);
                            name = System.Text.Encoding.ASCII.GetString(strTmp.ToArray());
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                            name = textInfo.ToTitleCase(name).Replace(" ", string.Empty);
                        }
                        ClientApi.Screenshot($"c:\\Ecco2Maps\\{levelID}_{name}_bottom.png");
                        _destX = _destY = 0;
                        _left = _right = 160;
                        _top = _bottom = 112;
                        ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
                        _mapDumpState = 0;
                    }
                    break;
                case 0xF6:
                    Draw3DHud();
                    break;
                default:
                    break;
            }
            _prevF = Mem.ReadU32(0xFFA524);
        }
        public override void PostFrameCallback()
        {
            uint frame = Mem.ReadU32(0xFFA524);
            if ((frame <= _prevF) && !Emu.IsLagged())
            {
                Emu.SetIsLagged(true);
                Emu.SetLagCount(Emu.LagCount() + 1);
            }
            uint mode = Mem.ReadByte(0xFFA555);
            _tickerY = 81;
            string valueTicker = $"{Mem.ReadU32(0xFFA520)}:{Mem.ReadU32(0xFFA524)}:{Mem.ReadU16(0xFFA7C8)}:{mode:X2}";
            TickerText(valueTicker);
            switch (mode)
            {
                case 0x20:
                case 0x28:
                case 0xAC:
                    valueTicker = $"{Mem.ReadS16(0xFFAD9C)}:{Mem.ReadS16(0xFFAD9E)}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFAA1A) / 65536.0:0.######}:{Mem.ReadS32(0xFFAA1E) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFAA32) / 65536.0:0.######}:{Mem.ReadU8(0xFFAA6D)}:{Mem.ReadU8(0xFFAA6E)}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFAA36) / 65536.0:0.######}:{Mem.ReadS32(0xFFAA3A) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFA9D6) / 65536.0:0.######}:{Mem.ReadS32(0xFFA9DA) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFAA3E) / 65536.0:0.######}:{Mem.ReadS32(0xFFAA42) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{(Mem.ReadS32(0xFFAA36) + Mem.ReadS32(0xFFA9D6) + Mem.ReadS32(0xFFAA3E)) / 65536.0:0.######}:" +
                                  $"{(Mem.ReadS32(0xFFAA3A) + Mem.ReadS32(0xFFA9DA) + Mem.ReadS32(0xFFAA42)) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadU8(0xFFAB72)}:{Mem.ReadU8(0xFFAB70)}:{(short)Mem.ReadS16(0xFFAA52):X4}:{(short)Mem.ReadS16(0xFFAA5A):X4}";
                    TickerText(valueTicker);
                    switch (Mem.ReadU8(0xFFA7D0))
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 30:
                        case 46:
                            var globeFlags = Mem.ReadU32(0xFFD434) >> 1;
                            var globeFlags2 = Mem.ReadU32(0xFFD438) >> 1;
                            int i, j = i = 0;
                            while (globeFlags > 0)
                            {
                                globeFlags >>= 1;
                                i++;
                            }
                            while (globeFlags2 > 0)
                            {
                                globeFlags2 >>= 1;
                                j++;
                            }
                            TickerText($"{i}:{j}", Color.Blue);
                            break;
                        default:
                            break;
                    }
                    if (_mapDumpState != 0)
                    {
                        Mem.WriteS16(0xFFAA16, 7);
                        Mem.WriteS16(0xFFAA18, 56);
                        int PlayerX = Mem.ReadS16(0xFFAA1A) - _camX;
                        int PlayerY = Mem.ReadS16(0xFFAA1E) - _camY;
                        if ((PlayerX < -64) || (PlayerX > 384) || (PlayerY < -64) || (PlayerY > 288))
                        {
                            Mem.WriteByte(0xFFAA70, 0xC);
                            Mem.WriteS16(0xFFA7CA, 1);
                        }
                        else
                        {
                            Mem.WriteByte(0xFFAA70, 0x0);
                            Mem.WriteS16(0xFFA7CA, 0);
                        }
                    }
                    if (_mapDumpState == 1)
                    {
                        int levelWidth = Mem.ReadS16(Addr2D.LevelWidth);
                        int levelHeight = Mem.ReadS16(Addr2D.LevelHeight);
                        var levelID = Mem.ReadByte(AddrGlobal.LevelID);
                        var s = Emu.GetSettings() as GPGX.GPGXSettings;
                        s.DrawBGA = false;
                        s.DrawBGB = false;
                        s.DrawBGW = false;
                        s.DrawObj = false;
                        s.Backdrop = true;
                        Emu.PutSettings(s);
                        if ((_camX == _destX) && (_camY == _destY))
                        {
                            if ((_prevX != _camX) || (_prevY != _camY))
                            {
                                if (_destX == 0)
                                {
                                    if (_rowStateGuid != string.Empty)
                                    {
                                        MemSS.DeleteState(_rowStateGuid);
                                    }
                                    _rowStateGuid = MemSS.SaveCoreStateToMemory();
                                }
                                _snapPast = 1;
                            }
                            else
                            {
                                _snapPast--;
                            }
                            if (_snapPast == 0)
                            {
                                ClientApi.Screenshot($"c:\\Ecco2Maps\\{levelID}\\{_destY}_{_destX}_top.png");
                                if (_destX >= levelWidth - 320)
                                {
                                    if (_destY < levelHeight - 224)
                                    {
                                        if (_rowStateGuid != string.Empty)
                                        {
                                            MemSS.LoadCoreStateFromMemory(_rowStateGuid);
                                        }
                                        _destX = 0;
                                        _destY = Math.Min(_destY + 111, levelHeight - 224);
                                    }
                                }
                                else
                                    _destX = Math.Min(_destX + 159, levelWidth - 320);
                                if ((_prevX == _destX) && (_prevY == _destY))
                                {
                                    ClientApi.SetGameExtraPadding(levelWidth - 320, levelHeight - 224, 0, 0);
                                    _mapDumpState++;
                                }
                            }
                        }
                        Mem.WriteS16(0xFFAD8C, _destX);
                        Mem.WriteS16(0xFFAD90, _destY);
                    }
                    else if (_mapDumpState == 2)
                    {
                        if (_rowStateGuid != String.Empty)
                            MemSS.DeleteState(_rowStateGuid);
                        _rowStateGuid = String.Empty;
                        int levelWidth = Mem.ReadS16(Addr2D.LevelWidth);
                        int levelHeight = Mem.ReadS16(0xFFA7AC);
                        ClientApi.SetGameExtraPadding(levelWidth - 320, levelHeight - 224, 0, 0);
                        var levelID = Mem.ReadS8(0xFFA7D0);
                        var s = Emu.GetSettings() as GPGX.GPGXSettings;
                        s.DrawBGA = false;
                        s.DrawBGB = false;
                        s.DrawBGW = false;
                        s.DrawObj = false;
                        s.Backdrop = true;
                        Emu.PutSettings(s);

                        var a = Gui.GetAttributes();
                        a.SetColorKey(Color.FromArgb(0, 0x11, 0x22, 0x33), Color.FromArgb(0, 0x11, 0x22, 0x33));
                        Gui.SetAttributes(a);
                        Gui.ToggleCompositingMode();

                        Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{levelHeight - 224}_{levelWidth - 320}_top.png", 2, 2, 318, 222, (levelWidth - 318), (levelHeight - 222), 318, 222);
                        for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                        {
                            var dx = (x == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{levelHeight - 224}_{x}_top.png", dx, 2, 320 - dx, 222, x + dx, (levelHeight - 222), 320 - dx, 222);
                        }
                        for (int y = ((levelHeight - 224) / 111) * 111; y >= 0; y -= 111)
                        {
                            var dy = (y == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{y}_{levelWidth - 320}_top.png", 2, dy, 318, 224 - 2, levelWidth - 318, y + dy, 318, 224 - dy);
                            for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                            {
                                var dx = (x == 0) ? 0 : 2;
                                Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{y}_{x}_top.png", dx, dy, 320 - dx, 224 - dy, x + dx, y + dy, 320 - dx, 224 - dy);
                            }
                        }

                        Gui.ToggleCompositingMode();
                        Gui.SetAttributes(new System.Drawing.Imaging.ImageAttributes());
                        Gui.DrawFinish();
                        _mapDumpState++;
                    }
                    else if (_mapDumpState == 4)
                    {
                        int levelWidth = Mem.ReadS16(Addr2D.LevelWidth);
                        int levelHeight = Mem.ReadS16(0xFFA7AC);
                        var levelID = Mem.ReadByte(0xFFA7D0);
                        var s = Emu.GetSettings() as GPGX.GPGXSettings;
                        s.DrawBGA = (levelID != 29);
                        s.DrawBGB = (levelID == 7);
                        s.DrawBGW = true;
                        s.DrawObj = true;
                        s.Backdrop = false;
                        Emu.PutSettings(s);
                        if ((_camX == _destX) && (_camY == _destY))
                        {
                            if ((_prevX != _camX) || (_prevY != _camY))
                            {
                                if (_destX == 0)
                                {
                                    if (_rowStateGuid != string.Empty)
                                    {
                                        MemSS.DeleteState(_rowStateGuid);
                                    }
                                    _rowStateGuid = MemSS.SaveCoreStateToMemory();
                                }
                                _snapPast = 1;
                            }
                            else
                            {
                                _snapPast--;
                            }
                            if (_snapPast == 0)
                            {
                                ClientApi.Screenshot($"c:\\Ecco2Maps\\{levelID}\\{_destY}_{_destX}_bottom.png");
                                if (_destX >= levelWidth - 320)
                                {
                                    if (_destY < levelHeight - 224)
                                    {
                                        if (_rowStateGuid != string.Empty)
                                        {
                                            MemSS.LoadCoreStateFromMemory(_rowStateGuid);
                                        }
                                        _destX = 0;
                                        _destY = Math.Min(_destY + 111, levelHeight - 224);
                                    }
                                }
                                else
                                    _destX = Math.Min(_destX + 159, levelWidth - 320);
                                if ((_prevX == _destX) && (_prevY == _destY))
                                {
                                    ClientApi.SetGameExtraPadding(levelWidth - 320, levelHeight - 224, 0, 0);
                                    _mapDumpState++;
                                }
                            }
                        }
                        Mem.WriteS16(0xFFAD8C, _destX);
                        Mem.WriteS16(0xFFAD90, _destY);
                    }
                    else if (_mapDumpState == 5)
                    {
                        if (_rowStateGuid != String.Empty)
                            MemSS.DeleteState(_rowStateGuid);
                        _rowStateGuid = String.Empty;
                        int levelWidth = Mem.ReadS16(Addr2D.LevelWidth);
                        int levelHeight = Mem.ReadS16(0xFFA7AC);
                        var levelID = Mem.ReadS8(0xFFA7D0);
                        var s = Emu.GetSettings() as GPGX.GPGXSettings;
                        s.DrawBGA = (levelID != 29);
                        s.DrawBGB = (levelID == 7);
                        s.DrawBGW = true;
                        s.DrawObj = true;
                        s.Backdrop = false;
                        Emu.PutSettings(s);
                        Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{levelHeight - 224}_{levelWidth - 320}_bottom.png", 2, 2, 318, 222, (levelWidth - 318), (levelHeight - 222), 318, 222);
                        for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                        {
                            var dx = (x == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{levelHeight - 224}_{x}_bottom.png", dx, 2, 320 - dx, 222, x + dx, (levelHeight - 222), 320 - dx, 222);
                        }
                        for (int y = ((levelHeight - 224) / 111) * 111; y >= 0; y -= 111)
                        {
                            var dy = (y == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{y}_{levelWidth - 320}_bottom.png", 2, dy, 318, 224 - 2, levelWidth - 318, y + dy, 318, 224 - dy);
                            for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                            {
                                var dx = (x == 0) ? 0 : 2;
                                Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{y}_{x}_bottom.png", dx, dy, 320 - dx, 224 - dy, x + dx, y + dy, 320 - dx, 224 - dy);
                            }
                        }
                        Gui.DrawFinish();
                        _mapDumpState++;
                    }
                    _prevX = _camX;
                    _prevY = _camY;
                    break;
                case 0xF6:
                    valueTicker = $"{Mem.ReadS32(0xFFD5E0) / 4096.0:0.######}:{Mem.ReadS32(0xFFD5E8) / 4096.0:0.######}:{Mem.ReadS32(0xFFD5E4) / 2048.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFB13A) / 4096.0:0.######}:{Mem.ReadS32(0xFFB142) / 4096.0:0.######}:{Mem.ReadS32(0xFFB13E) / 2048.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFB162) / 4096.0:0.######}:{Mem.ReadS32(0xFFB16A) / 4096.0:0.######}:{Mem.ReadS32(0xFFB166) / 2048.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadU8(0xFFB19B)}:{Mem.ReadU8(0xFFB1A6)}:{Mem.ReadU8(0xFFB1A9)}";
                    TickerText(valueTicker);
                    break;
            }
            Joy.Set("C", null, 1);
            Joy.Set("Start", null, 1);
            var color = _turnSignalColors[Mem.ReadS8(0xFFA7C9) & 7];
            Gui.DrawRectangle(_left - 48, _top - 112, 15, 15, color, color);
            Gui.DrawFinish();
        }
    }
}
