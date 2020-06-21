using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using AVRdude_v1.Properties;

namespace AVRdude
{
    public partial class Form1 : Form
    {
        AVR AVR = new AVR();

        delegate void Delegate_SetTrace(string text);
        delegate void Delegate_SetWarning(bool boolean);

        public Form1()
        {
            // Add Event Handles
            AVR.MessageReceived += new AVR.MessageReceivedHandler(AVR_MessageReceived);
            AVR.WorkerComplete += new AVR.WorkerCompleteHandler(AVR_Complete);
            this.FormClosing += Form_Closing;
            this.Load += Form_Loading;
 
            InitializeComponent();
        }

        //AVR Handles
        private void AVR_MessageReceived(string message)
        {
            SetTrace(message);
        }
        private void AVR_Complete(string message, bool IsError)
        {
            if (IsError)
                MessageBox.Show(message,"AVR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show(message,"AVR", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SetWarning(false);
        }

        //Form Handles
        private void Button_Bootloader_Click(object sender, EventArgs e)
        {

            if (AVR.IsBusy == false)
            {
                SetTrace(null);
                SetWarning(true);
                AVR.Start(AVRdude.AVR.job.bootloader);
            }
            else
                MessageBox.Show("AVR Is Busy, Please Wait...", "AVR", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }
        private void Button_Sketch_Click(object sender, EventArgs e)
        {

            if (AVR.IsBusy == false)
            {
                SetTrace(null);
                SetWarning(true);
                AVR.Start(AVRdude.AVR.job.sketch);
            }
            else
                MessageBox.Show("AVR Is Busy, Please Wait...", "AVR", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }
        private void Button_Erase_Click(object sender, EventArgs e)
        {
            
            if (AVR.IsBusy == false)
            {
                SetTrace(null);
                SetWarning(true);
                AVR.Start(AVRdude.AVR.job.erase);
            }
            else
                MessageBox.Show("AVR Is Busy, Please Wait...", "AVR", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }
        private void Button_Clear_Click(object sender, EventArgs e)
        {
            SetTrace(null);
        }
        private void CheckBox_StayOnTop_CheckedChanged(object sender, EventArgs e)
        {       
            this.TopMost = CheckBox_StayOnTop.Checked;
        }
        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if (AVR.IsBusy == true)
            {
                MessageBox.Show("AVR Is Busy, Please Wait...", "AVR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }                 
        }
        private void Form_Loading(object sender, EventArgs e)
        {
            textBox_trace.ReadOnly = true;
            label_warning.Visible = false;

            textSettingProgrammer.Text = Settings.Default["Programmer"].ToString();
            textSettingsBootloader.Text = Settings.Default["Bootloader"].ToString();
            textSettingsLowFuse.Text = Settings.Default["lFuse"].ToString();
            textSettingsHighFuse.Text = Settings.Default["hFuse"].ToString();
            textSettingsExtendedFuse.Text = Settings.Default["eFuse"].ToString();
            textSettingsSketch.Text = Settings.Default["Sketch"].ToString();
            textSettingsAVRdudeBinLocation.Text = Settings.Default["binLocation"].ToString();
            textSettingsChipType.Text = Settings.Default["chipType"].ToString();
        }

        //Methods
        private void SetTrace(string message)
        {
            if (this.textBox_trace.InvokeRequired)
            {
                Delegate_SetTrace d = new Delegate_SetTrace(SetTrace);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                if (message == null)
                    this.textBox_trace.Text = "";
                else
                    this.textBox_trace.Text += message;
            }
        }
        private void SetWarning(bool boolean)
        {
            if (this.label_warning.InvokeRequired)
            {
                Delegate_SetWarning d = new Delegate_SetWarning(SetWarning);
                this.Invoke(d, new object[] { boolean });
            }
            else
            {
                this.label_warning.Visible = boolean;
            }
        }     
       
        #region Save TextBox Settings
        private void textSettingsLowFuse_TextChanged(object sender, EventArgs e)
        {
            Settings.Default["lFuse"] = textSettingsLowFuse.Text;
            Settings.Default.Save();
        }

        private void textSettingsHighFuse_TextChanged(object sender, EventArgs e)
        {
            Settings.Default["hFuse"] = textSettingsHighFuse.Text;
            Settings.Default.Save();
        }

        private void textSettingsExtendedFuse_TextChanged(object sender, EventArgs e)
        {
            Settings.Default["eFuse"] = textSettingsExtendedFuse.Text;
            Settings.Default.Save();
        }

        private void textSettingProgrammer_TextChanged(object sender, EventArgs e)
        {
            Settings.Default["Programmer"] = textSettingProgrammer.Text;
            Settings.Default.Save();
        }

        private void textSettingsChipType_TextChanged(object sender, EventArgs e)
        {
            Settings.Default["chipType"] = textSettingsChipType.Text;
            Settings.Default.Save();
        }
        #endregion

        private void btnSettingsUploadBootloader_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "HEX files|*.hex;";
                DialogResult dr = ofd.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    string sourcePath = ofd.FileName;
                    string name = Path.GetFileName(sourcePath);
                    textSettingsBootloader.Text = name;

                    Settings.Default["Bootloader"] = textSettingsBootloader.Text;
                    Settings.Default.Save();

                    string applicationPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + ofd.SafeFileName;
                    File.Copy(sourcePath, applicationPath, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSettingsUploadSketch_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "HEX files|*.hex;";
                DialogResult dr = ofd.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    string sourcePath = ofd.FileName;
                    string name = Path.GetFileName(sourcePath);
                    textSettingsSketch.Text = name;

                    Settings.Default["Sketch"] = textSettingsSketch.Text;
                    Settings.Default.Save();

                    string applicationPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + name;
                    File.Copy(sourcePath, applicationPath, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK)
            {
                textSettingsAVRdudeBinLocation.Text = fbd.SelectedPath+"\\";

                Settings.Default["binLocation"] = textSettingsAVRdudeBinLocation.Text;
                Settings.Default.Save();
            }
        }

        #region HELP Forms
        private void btnSettingsBinLocationHelp_Click(object sender, EventArgs e)
        {
            AVRdude_v1.AVRdudeBinHelp open = new AVRdude_v1.AVRdudeBinHelp();
            open.ShowDialog();

        }

        private void textSettingsChipTypeHelp_Click(object sender, EventArgs e)
        {
            AVRdude_v1.ChipTypeHelp open = new AVRdude_v1.ChipTypeHelp();
            open.ShowDialog();
        }

        private void btnSettingsProgrammerHelp_Click(object sender, EventArgs e)
        {
            AVRdude_v1.ProgrammerHelp open = new AVRdude_v1.ProgrammerHelp();
            open.ShowDialog();
        }

        private void btnSettingsSketchHelp_Click(object sender, EventArgs e)
        {
            AVRdude_v1.SketchHelp open = new AVRdude_v1.SketchHelp();
            open.ShowDialog();
        }
        #endregion

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("http://www.nongnu.org/avrdude/"); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label15.Visible = false;
        }

    }
}
