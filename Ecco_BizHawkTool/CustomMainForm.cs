using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Tool.Ecco;

namespace BizHawk.Client.EmuHawk
{
	//As you can see this is a dedicated attribute. You can write the name of your tool, a small description
	[ExternalTool(CustomMainForm.ToolName, Description = CustomMainForm.ToolDescription)]
	
	//and give an icon. The icon should be compiled as an embedded resource and you must give the entire path
	[ExternalToolEmbeddedIcon(CustomMainForm.IconPath)]

	//This attribute says what the tool is for
	//By setting this, your tool is contextualized, that mean you can't load it if emulator is in state you don't want
	//It avoid crash
	[ExternalToolApplicability.SingleSystem(CoreSystem.Genesis)]

	/// <summary>

	/// Here your first form
	/// /!\ it MUST be called CustomMainForm and implements IExternalToolForm
	/// Take also care of the namespace
	/// </summary>
	public partial class CustomMainForm : Form, IExternalToolForm
	{
        #region Fields
        /*
		The following stuff will be automatically filled
		by BizHawk runtime
		*/
        [RequiredApi]
        public IMem Mem { get; set; }

        [RequiredApi]
        public IGui Gui { get; set; }

        [RequiredApi]
        public IJoypad Joy { get; set; }

        [RequiredApi]
        public IEmu Emu { get; set; }

        [RequiredApi]
        public IGameInfo GI { get; set; }

        [RequiredApi]
        public IMemorySaveState MemSS { get; set; }

        /*
        Name for our external tool
        */
        public const string ToolName = "Ecco TAS Assistant";

        /*
        Description for our external tool
        */
        public const string ToolDescription = "Provides HUD, fastest-swim autofire, lag detection.";

        /*
        Icon for our external tool
        */
        public const string IconPath = "Ecco_icon.ico";

        private enum Modes { disabled, Ecco1, Ecco2 }
        private EccoToolBase _tool;
		private List<string> _statusLines = new List<string>();
		private List<Color?> _statusColors = new List<Color?>();
        #endregion

        #region cTor(s)

        public CustomMainForm()
		{
			InitializeComponent();
            ClientApi.StateLoaded += OnStateLoaded;
            FormClosed += CustomMainForm_FormClosed;
		}

		#endregion

		#region Winform Methods
		private void mapDumpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _tool.SetMapDumping(mapDumpCheckbox.Checked);
        }
		private void showNumbers_CheckedChanged(object sender, EventArgs e)
		{
			_tool.SetShowNumbers(showNumbersCheckbox.Checked);
		}
		private void autoFireCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            _tool.SetAutofire(autoFireCheckbox.Checked);
        }
        private void mapDumpFolderBrowse_Click(object sender, EventArgs e)
        {
            if (mapFolderBrowseDialog.ShowDialog() == DialogResult.OK)
            {
                mapDumpFolder.Text = mapFolderBrowseDialog.SelectedPath;
            }
        }
		public void CustomMainForm_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				SuspendLayout();
			}
			else
			{
				ResumeLayout();
			}
		}
		public void CustomMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Gui.DrawNew("emu");
            Gui.DrawFinish();
            ClientApi.SetGameExtraPadding(0);
        }
        //private void PostFrameCallback()
        #endregion
        #region Tool Methods
        public void SetStatusLine(string message, int line, Color? fg)
        {
			if (WindowState != FormWindowState.Minimized)
			{
				while (StatusTextBox.Lines.Length <= line)
				{
					StatusTextBox.AppendText(Environment.NewLine);
				}
				StatusTextBox.SelectionStart = StatusTextBox.GetFirstCharIndexFromLine(line);
				StatusTextBox.SelectionLength = StatusTextBox.Lines[line].Length;
				StatusTextBox.SelectionColor = fg ?? StatusTextBox.ForeColor;
				StatusTextBox.SelectedText = message;
			}
        }
        private void OnStateLoaded(object sender, StateLoadedEventArgs e)
        {
            Gui.DrawNew("emu");
            _tool.PreFrameCallback();
            _tool.PostFrameCallback();
            Gui.DrawFinish();
        }

        private void Init()
        {
            Mem.SetBigEndian();
            string gameName = GI.GetRomName();
            switch (gameName)
            {
                case "ECCO - The Tides of Time (J) [!]":
                    _tool = new Ecco2Tool(this, GameRegion.J);
                    break;
                case "ECCO - The Tides of Time (U) [!]":
                    _tool = new Ecco2Tool(this, GameRegion.U);
                    break;
                case "ECCO - The Tides of Time (E) [!]":
                    _tool = new Ecco2Tool(this, GameRegion.E);
                    break;
                case "ECCO The Dolphin (J) [!]":
                case "ECCO The Dolphin (UE) [!]":
                /*_tool = new EccoTool(this, GameRegion.UE);*/
                default:
                    Close();
                    break;
            }
        }
        #endregion

        #region BizHawk Required methods
		public bool AskSaveChanges()
		{
			return true;
		}

		/// <summary>
		/// This method is called instead of regular <see cref="UpdateValues"/>
		/// when emulator is running in turbo mode
		/// </summary>
		public void FastUpdate()
		{ }

        /// <summary>
        /// Restart is called the first time you call the form
        /// but also when you start playing a movie
        /// </summary>
        public void Restart()
        {
            Init();
        }

        /// <summary>
        /// New extensible update method
        /// </summary>
        public void UpdateValues(ToolFormUpdateType type)
        {
            switch (type)
            {
                case ToolFormUpdateType.General:
                    Init();
                    break;
                case ToolFormUpdateType.PreFrame:
					_tool.PreFrameCallback();
					ResumeLayout(true);
					SuspendLayout();
					break;
                case ToolFormUpdateType.PostFrame:
					_tool.PostFrameCallback();
					ResumeLayout(true);
					SuspendLayout();
                    break;
                default:
                    break;
            }
        }
		#endregion BizHawk Required methods
	}
}
