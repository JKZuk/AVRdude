using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using AVRdude_v1.Properties;

namespace AVRdude
{
    public class AVR
    {
        // Events
        public event MessageReceivedHandler MessageReceived;
        public event WorkerCompleteHandler WorkerComplete;
        public delegate void MessageReceivedHandler(string message);
        public delegate void WorkerCompleteHandler(string message, bool iserror = false);

        // Fields
        private System.Threading.Thread avrthread;

        // Properties
        public bool IsBusy
        {
            get
            {
                if (avrthread == null)
                    return false;
                return avrthread.IsAlive;
            }
        }

        // Public Methods
        public void Start(job job)
        {
            //First Restart the Thread if it is No Longer Alive
            if (!(avrthread == null))
                if (avrthread.IsAlive == false)
                    avrthread = null;

            if (avrthread == null)
            {
                switch (job)
                {
                    case AVRdude.AVR.job.bootloader:
                        avrthread = new System.Threading.Thread(new System.Threading.ThreadStart(bootloader_worker));
                        avrthread.Start();
                        break;
                    case AVRdude.AVR.job.sketch:
                        avrthread = new System.Threading.Thread(new System.Threading.ThreadStart(sketch_worker));
                        avrthread.Start();
                        break;
                    case AVRdude.AVR.job.erase:
                        avrthread = new System.Threading.Thread(new System.Threading.ThreadStart(erase_worker));
                        avrthread.Start();
                        break;
                    default:
                        WorkerComplete("Job Doesnt Exist", true);
                        break;
                }
            }
        }
        public void Stop()
        {
            if (avrthread == null) { return; }
            if (!avrthread.IsAlive)
            {
                avrthread.Abort();
                while (avrthread.IsAlive == true) { }
                WorkerComplete("Cancelled", true);
            }

        }

        // Private Methods
        private void sendCommand(string AVR_command, string AVRbinLocation)
        {
            //Thread.Sleep(10000); // You can Enable this to test the thread out

            string installDir = AVRbinLocation + "\\"; ;
            string dir = installDir.Replace("\\", "/");

            Process avrprog = new Process();
            StreamReader avrstdout, avrstderr;
            StreamWriter avrstdin;
            ProcessStartInfo psI = new ProcessStartInfo("cmd");

            psI.UseShellExecute = false;
            psI.RedirectStandardInput = true;
            psI.RedirectStandardOutput = true;
            psI.RedirectStandardError = true;
            psI.CreateNoWindow = true;

            avrprog.StartInfo = psI;
            avrprog.Start();

            avrstdin = avrprog.StandardInput;
            avrstdout = avrprog.StandardOutput;
            avrstderr = avrprog.StandardError;

            avrstdin.AutoFlush = true;

            avrstdin.WriteLine(installDir + AVR_command);
            avrstdin.Close();

            string message = avrstderr.ReadToEnd();

            // I think these could be if-else
            if (AVR_command.Contains("-B 1 -U flash:w:"))
                MessageReceived("::::::::::::BOOTLOADER/SKETCH::::::::::" + Environment.NewLine + message);

            if (AVR_command.Contains("hfuse"))
                MessageReceived("::::::::::::HIGH FUSE:::::::::" + Environment.NewLine + message);

            if (AVR_command.Contains("lfuse"))
                MessageReceived("::::::::::::LOW FUSE:::::::::" + Environment.NewLine + message);

            if (AVR_command.Contains("efuse"))
                MessageReceived("::::::::::::EXTENDED FUSE::::::::" + Environment.NewLine + message);

            // Catch Errors - I havent checked if this is a Reliable way of doing it
            if (message.Contains("The system cannot find the path specified."))
                throw new Exception(message);
        }

        private void bootloader_worker()
        {
            try
            {
                //sendCommand("avrdude -c " +  textSettingProgrammer.Text + " -p " + textSettingsChipType.Text + " -B 1 -U flash:w:" + textSettingsBootloader.Text + "", textSettingsAVRdudeBinLocation.Text);
                //sendCommand("avrdude -c " + textSettingProgrammer.Text + " -p " + textSettingsChipType.Text + " -U lfuse:w:" + textSettingsLowFuse.Text + ":m", textSettingsAVRdudeBinLocation.Text);
                //sendCommand("avrdude -c " + textSettingProgrammer.Text + " -p " + textSettingsChipType.Text + " -U hfuse:w:" + textSettingsHighFuse.Text + ":m", textSettingsAVRdudeBinLocation.Text);
                //sendCommand("avrdude -c " + textSettingProgrammer.Text + " -p " + textSettingsChipType.Text + " -U efuse:w:" + textSettingsExtendedFuse.Text + ":m", textSettingsAVRdudeBinLocation.Text);

                sendCommand("avrdude -c " + Settings.Default["Programmer"].ToString() + " -p " + Settings.Default["chipType"].ToString() + " -B 1 -U flash:w:" + Settings.Default["Bootloader"].ToString() + "", Settings.Default["binLocation"].ToString());
                sendCommand("avrdude -c " + Settings.Default["Programmer"].ToString() + " -p " + Settings.Default["chipType"].ToString() + " -U lfuse:w:" + Settings.Default["Bootloader"].ToString() + ":m", Settings.Default["binLocation"].ToString());
                sendCommand("avrdude -c " + Settings.Default["Programmer"].ToString() + " -p " + Settings.Default["chipType"].ToString() + " -U hfuse:w:" + Settings.Default["Bootloader"].ToString() + ":m", Settings.Default["binLocation"].ToString());
                sendCommand("avrdude -c " + Settings.Default["Programmer"].ToString() + " -p " + Settings.Default["chipType"].ToString() + " -U efuse:w:" + Settings.Default["Bootloader"].ToString() + ":m", Settings.Default["binLocation"].ToString());

                WorkerComplete("Bootloader Complete!", false);
            }
            catch (Exception ex)
            {
                WorkerComplete("Bootloader Errored! " + Environment.NewLine + ex.Message,true);
            }
        }
        private void sketch_worker()
        {
            try
            {
                //sendCommand("avrdude -c " + textSettingProgrammer.Text + " -p " + textSettingsChipType.Text + " -B 1 -U flash:w:" + textSettingsSketch.Text + "", textSettingsAVRdudeBinLocation.Text);
                sendCommand("avrdude -c " + Settings.Default["Programmer"].ToString() + " -p " + Settings.Default["chipType"].ToString() + " -B 1 -U flash:w:" + Settings.Default["Sketch"].ToString() + "", Settings.Default["binLocation"].ToString());
                WorkerComplete("Sketch Complete!", false);
            }
            catch (Exception ex)
            {
                WorkerComplete("Sketch Errored! " + Environment.NewLine + ex.Message, true);
            }
        }
        private void erase_worker()
        {
            try
            {
                //sendCommand("avrdude -p " + textSettingsChipType.Text + " -c " + textSettingProgrammer.Text + " -e", textSettingsAVRdudeBinLocation.Text);  
                sendCommand("avrdude -p " + Settings.Default["chipType"].ToString() + " -c " + Settings.Default["Programmer"].ToString() + " -e", Settings.Default["binLocation"].ToString());
                WorkerComplete("Erase Complete!", false);
            }
            catch (Exception ex)
            {
                WorkerComplete("Erase Errored! " + Environment.NewLine + ex.Message, true);
            }
        }

        // Enumerations
        public enum job
        {
            bootloader,
            sketch,
            erase,
        }

    }
}
