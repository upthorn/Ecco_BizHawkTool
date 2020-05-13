using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Cores.Consoles.Sega.gpgx;

namespace BizHawk.Tool.Ecco
{
    partial class Ecco2Tool
    {
        private int _prevX = 0;
        private int _prevY = 0;
        private int _destX = 0;
        private int _destY = 0;
        private int _snapPast = 0;
        private string _rowStateGuid = string.Empty;
		private readonly uint[] ULevelNamePtrOffsets =
		{
			0xECBD0, 0x106BC0, 0x10AF8C, 0x135A48,
			0x1558E8, 0x15F700, 0x16537C, 0x180B00,
			0x193920, 0x1B3ECC, 0x1D7A44, 0x1DBF70,
			0x2DF2, 0x2DF6, 0x2DFA, 0x2DFE
		};
		private void PreProcessMapDump()
        {
            uint _levelTime = Mem.ReadU16(AddrGlobal.LevelFrameCount);
            _levelID = Mem.ReadS8(AddrGlobal.LevelID);
            int[] nameGroupLengths =
            {
                7,1,11,6,
                4,3,3,3,
                7,1,2,1,
                0,0,0,0
            };
			uint[] levelNamePtrs = ULevelNamePtrOffsets;
            int nameGroup = 0;
            string name = "map";
            int i = _levelID;
            switch (_mapDumpState)
            {
                case 0:
                    if ((_levelTime > 1) && (_levelTime < 4))
                    {
                        _mapDumpState = 1;
                        _rowStateGuid = string.Empty;
                        _top = _bottom = _left = _right = 0;
                        ClientApi.SetGameExtraPadding(0, 0, 0, 0);
                    }
                    break;
                case 3:
                    while ((i >= 0) && (nameGroup < nameGroupLengths.Length))
                    {
                        i -= nameGroupLengths[nameGroup];
                        if (i >= 0) nameGroup++;
                    }
                    if (i < 0)
                    {
                        i += nameGroupLengths[nameGroup];
                        uint strOffset = Mem.ReadU32(levelNamePtrs[nameGroup] + 0x2E);
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
                    ClientApi.Screenshot($"{MapDumpFolder}{_levelID}_{name}_top.png");
                    _destX = _destY = 0;
                    ClientApi.SetGameExtraPadding(0, 0, 0, 0);
                    _mapDumpState++;
                    break;
                case 6:
                    while ((i >= 0) && (nameGroup < nameGroupLengths.Length))
                    {
                        i -= nameGroupLengths[nameGroup];
                        if (i >= 0) nameGroup++;
                    }
                    if (i < 0)
                    {
                        i += nameGroupLengths[nameGroup];
                        uint strOffset = Mem.ReadU32(levelNamePtrs[nameGroup] + 0x2E);
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
                    ClientApi.Screenshot($"{MapDumpFolder}{_levelID}_{name}_bottom.png");
                    _destX = _destY = 0;
                    _left = _right = 160;
                    _top = _bottom = 112;
                    ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
                    _mapDumpState = 0;
                    break;
            }

        }
        private void PostProcessMapDump()
        {
            PlayerObj player;
            ReadPlayerObj(Addr2D.PlayerObj, out player);
            Mem.WriteS16(Addr2D.PlayerObj + 0x42, 7); // Hide HP meter
            Mem.WriteS16(Addr2D.PlayerObj + 0x44, 56); // Hide air meter
            Point playerPos = GetScreenLoc(player.Mid);
            if ((playerPos.X < -64) || (playerPos.X > 384) || (playerPos.Y < -64) || (playerPos.Y > 288))
            {
                Mem.WriteByte(Addr2D.PlayerObj + 0x9C, 0xC); // set player invincibility
                Mem.WriteS16(0xFFA7CA, 1); // set flag that flashes invincible sprites invisible
            }
            else
            {
                Mem.WriteByte(Addr2D.PlayerObj + 0x9C, 0x0); // clear player invincibiility
                Mem.WriteS16(0xFFA7CA, 0); // clear flag that flashes invincible sprites invisible
            }
            int levelWidth = Mem.ReadS16(Addr2D.LevelWidth);
            int levelHeight = Mem.ReadS16(Addr2D.LevelHeight);
            var levelID = Mem.ReadByte(AddrGlobal.LevelID);
            GPGX.GPGXSettings s;
            switch (_mapDumpState)
            {
                case 1:
                    s = Emu.GetSettings() as GPGX.GPGXSettings;
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
                            ClientApi.Screenshot($"{MapDumpFolder}{levelID}\\{_destY}_{_destX}_top.png");
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
                    Mem.WriteS16(Addr2D.CamXDest, _destX);
                    Mem.WriteS16(Addr2D.CamYDest, _destY);
                    break;
                case 2:
                    if (_rowStateGuid != String.Empty)
                        MemSS.DeleteState(_rowStateGuid);
                    _rowStateGuid = String.Empty;
                    ClientApi.SetGameExtraPadding(levelWidth - 320, levelHeight - 224, 0, 0);
                    s = Emu.GetSettings() as GPGX.GPGXSettings;
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

                    Gui.DrawImageRegion($"{MapDumpFolder}{levelID}\\{levelHeight - 224}_{levelWidth - 320}_top.png", 2, 2, 318, 222, (levelWidth - 318), (levelHeight - 222), 318, 222);
                    for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                    {
                        var dx = (x == 0) ? 0 : 2;
                        Gui.DrawImageRegion($"{MapDumpFolder}{levelID}\\{levelHeight - 224}_{x}_top.png", dx, 2, 320 - dx, 222, x + dx, (levelHeight - 222), 320 - dx, 222);
                    }
                    for (int y = ((levelHeight - 224) / 111) * 111; y >= 0; y -= 111)
                    {
                        var dy = (y == 0) ? 0 : 2;
                        Gui.DrawImageRegion($"{MapDumpFolder}{levelID}\\{y}_{levelWidth - 320}_top.png", 2, dy, 318, 224 - 2, levelWidth - 318, y + dy, 318, 224 - dy);
                        for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                        {
                            var dx = (x == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"{MapDumpFolder}{levelID}\\{y}_{x}_top.png", dx, dy, 320 - dx, 224 - dy, x + dx, y + dy, 320 - dx, 224 - dy);
                        }
                    }
                    Gui.ToggleCompositingMode();
                    Gui.SetAttributes(new System.Drawing.Imaging.ImageAttributes());
                    Gui.DrawFinish();
                    _mapDumpState++;
                    break;
                case 4:
                    s = Emu.GetSettings() as GPGX.GPGXSettings;
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
                            ClientApi.Screenshot($"{MapDumpFolder}{levelID}\\{_destY}_{_destX}_bottom.png");
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
                    Mem.WriteS16(Addr2D.CamXDest, _destX);
                    Mem.WriteS16(Addr2D.CamYDest, _destY);
                    break;
                case 5:
                    if (_rowStateGuid != String.Empty)
                        MemSS.DeleteState(_rowStateGuid);
                    _rowStateGuid = String.Empty;
                    s = Emu.GetSettings() as GPGX.GPGXSettings;
                    s.DrawBGA = (levelID != 29);
                    s.DrawBGB = (levelID == 7);
                    s.DrawBGW = true;
                    s.DrawObj = true;
                    s.Backdrop = false;
                    Emu.PutSettings(s);
                    Gui.DrawImageRegion($"{MapDumpFolder}{levelID}\\{levelHeight - 224}_{levelWidth - 320}_bottom.png", 2, 2, 318, 222, (levelWidth - 318), (levelHeight - 222), 318, 222);
                    for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                    {
                        var dx = (x == 0) ? 0 : 2;
                        Gui.DrawImageRegion($"{MapDumpFolder}{levelID}\\{levelHeight - 224}_{x}_bottom.png", dx, 2, 320 - dx, 222, x + dx, (levelHeight - 222), 320 - dx, 222);
                    }
                    for (int y = ((levelHeight - 224) / 111) * 111; y >= 0; y -= 111)
                    {
                        var dy = (y == 0) ? 0 : 2;
                        Gui.DrawImageRegion($"{MapDumpFolder}{levelID}\\{y}_{levelWidth - 320}_bottom.png", 2, dy, 318, 224 - 2, levelWidth - 318, y + dy, 318, 224 - dy);
                        for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                        {
                            var dx = (x == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"{MapDumpFolder}{levelID}\\{y}_{x}_bottom.png", dx, dy, 320 - dx, 224 - dy, x + dx, y + dy, 320 - dx, 224 - dy);
                        }
                    }
                    Gui.DrawFinish();
                    _mapDumpState++;
                    break;
            }
            _prevX = _camX;
            _prevY = _camY;
        }
    }
}