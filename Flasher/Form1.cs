using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using RegawMOD.Android;

namespace Flasher
{
    public partial class Form1 : Form
    {
        public string args;
        AndroidController android = AndroidController.Instance;
        bool blu;
        string pdn;
        public Form1()
        {
            InitializeComponent();            
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            this.Text = "Fastboot Flasher";
            fw.Text = "Select";
            fl.Text = "Flash";
            inf.Text = "Info";
            textBox1.Text = "Double click to load firmware";
            comboBox1.Enabled = false;
            checkBox2.Checked = false;
            checkBox1.Checked = false;            
            richTextBox2.AppendText("Developed By : ",Color.Yellow);
            richTextBox2.AppendText("Kyaw Khant Zaw",Color.LimeGreen);
        }

        private void ParseFlashBat()
        {
            richTextBox2.Clear();
            checkedListBox1.Items.Clear();
            using (StreamReader r = new StreamReader(textBox1.Text+"\\flash_all.bat"))
            {
                string line;
                int i = 0;
                while ((line = r.ReadLine()) != null)
                {
                    if(line.Contains("flash") && !line.Contains("NONE"))
                    {                       
                        string[] ls = line.Split('|');
                        string sf=ls[0].Replace(" %* "," ");
                        string fs = sf.Replace(" %~dp0"," ");
                        string l = fs.Replace("fastboot ", " ");
                        string fn = l.TrimStart();
                        fn = fn.Replace("images","=");
                        if (fn.Contains(":"))
                        {
                            fn = fn.Replace(":", " ");
                            fn = fn.TrimStart();
                        }
                        if (fn.Contains("\\"))
                        {
                            fn = fn.Replace("\\", " ");
                        }
                        string[] final = fn.Split('=');
                        string cmd = final[0].TrimStart().TrimEnd()+" \\images\\"+final[1].TrimStart().TrimEnd();                                           
                        checkedListBox1.Items.Add(cmd);
                        checkedListBox1.SetItemChecked(i, true);
                        i++;
                    }                    
                }
            }
        }

        private void FBInfo()
        {
            android.UpdateDeviceList();
            if (android.HasConnectedDevices)
            {
                var device = android.GetConnectedDevice(android.ConnectedDevices[0]);
                richTextBox2.AppendText(device.State.ToString(),Color.Yellow);
                switch (device.State.ToString())
                {
                    case "FASTBOOT":
                        if (File.Exists(@"C:\Users\" + Environment.UserName + @"\Local\Temp\RegawMOD\AndroidLib\f.txt"))
                        {
                            File.Delete(@"C:\Users\" + Environment.UserName + @"\Appdata\Local\Temp\RegawMOD\AndroidLib\f.txt");
                        }
                        File.WriteAllText(@"C:\Users\"+Environment.UserName+ @"\Appdata\Local\Temp\RegawMOD\AndroidLib\fb.cmd", Properties.Resources.fb);                        
                        Process fd=new Process();
                        fd.StartInfo.FileName = "cmd.exe";
                        fd.StartInfo.Arguments = "/c fb.cmd";
                        fd.StartInfo.UseShellExecute = false;
                        fd.StartInfo.CreateNoWindow = true;
                        fd.StartInfo.WorkingDirectory = @"C:\Users\"+Environment.UserName+ @"\Appdata\Local\Temp\RegawMOD\AndroidLib";
                        fd.StartInfo.RedirectStandardOutput = true;
                        fd.Start();
                        fd.WaitForExit();                                               
                        File.Delete(@"C:\Users\" + Environment.UserName + @"\Appdata\Local\Temp\RegawMOD\AndroidLib\fb.cmd");
                        using (StreamReader r = new StreamReader(@"C:\Users\" + Environment.UserName + @"\Appdata\Local\Temp\RegawMOD\AndroidLib\f.txt"))
                        {
                            string line;
                            int i = 0;
                            while ((line = r.ReadLine()) != null)
                            {                                
                                if (line.Contains("unlocked"))
                                {
                                    line = line.Replace("(bootloader) ", " ").TrimStart();
                                    string[] pd = line.Split(':');
                                    richTextBox2.AppendText(Environment.NewLine+"Unlocked : ", Color.Yellow);
                                    if (pd[1].Contains("yes"))
                                    {
                                        richTextBox2.AppendText(pd[1].TrimStart().TrimEnd().ToUpper(), Color.LimeGreen);
                                        blu = true;
                                    }
                                    else
                                    {
                                        richTextBox2.AppendText(pd[1].TrimStart().TrimEnd(), Color.Red);
                                        blu = false;
                                    }
                                }
                                if (line.Contains("product"))
                                {
                                    line = line.Replace("(bootloader) ", " ").TrimStart();
                                    string[] pd = line.Split(':');
                                    richTextBox2.AppendText(Environment.NewLine + "Product : ", Color.Yellow);
                                    richTextBox2.AppendText(pd[1].TrimStart().TrimEnd(), Color.LimeGreen);
                                    pdn = pd[1].TrimStart().TrimEnd();
                                }
                                if (line.Contains("slot-count"))
                                {
                                    line = line.Replace("(bootloader) ", " ").TrimStart();
                                    string[] pd = line.Split(':');
                                    richTextBox2.AppendText(Environment.NewLine + pd[0]+" : ", Color.Yellow);
                                    richTextBox2.AppendText(pd[1].TrimStart().TrimEnd(), Color.LimeGreen);
                                    string slc = pd[1].TrimStart().TrimEnd();
                                    if (!slc.Contains("0"))
                                    {
                                        checkBox2.Checked = true;
                                    }
                                }
                                i++;                               
                            }                            
                        }
                        richTextBox2.AppendText(Environment.NewLine);
                        break;
                    default:
                        richTextBox2.AppendText("Device not in fastboot.",Color.Red);
                        break;
                }
                android.Dispose();
            }
            else
            {
                richTextBox2.AppendText("No Device Found!", Color.Red);
                android.Dispose();
            }
        }

        private void FBFlash()
        {
            this.Cursor = Cursors.WaitCursor;
            int i = 0;
            while (i < checkedListBox1.Items.Count)
            {                
                if(checkedListBox1.GetItemChecked(i) == true)
                {
                    args = checkedListBox1.Items[i].ToString();
                    string []argsp=args.Split('\\');                    
                    string log = argsp[0].Replace("flash ", " ");
                    richTextBox2.AppendText(Environment.NewLine+"Flashing "+log.TrimStart().TrimEnd()+"...",Color.Yellow);
                    Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(argsp[0],"\""+textBox1.Text+"\\"+argsp[1]+"\\"+argsp[2]+"\""));                 
                    richTextBox2.AppendText("Done",Color.LimeGreen);                    
                    richTextBox2.ScrollToCaret();
                    checkedListBox1.SetItemChecked(i,false);
                }
                i++;
            }
            if (checkBox1.Checked == true)
            {
                richTextBox2.AppendText(Environment.NewLine + "Disabling Verified Boot...",Color.Yellow);
                Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand("--disable-verity --disable-verification flash vbmeta","\""+ textBox1.Text + "\\images\\vbmeta.img"+"\""));
                richTextBox2.AppendText("Done",Color.LimeGreen);
            }
            if (checkBox2.Checked == true)
            {
                richTextBox2.AppendText(Environment.NewLine + "Setting Active to " + comboBox1.SelectedItem.ToString(), Color.Yellow);
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand("set_active a"));                        
                        break;
                        case 1:
                        Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand("set_active b"));
                        break;
                }
                richTextBox2.AppendText("Done", Color.Green);
            }
            richTextBox2.AppendText(Environment.NewLine+Environment.NewLine+"Developed By :",Color.Yellow);
            richTextBox2.AppendText("Kyaw Khant Zaw", Color.LimeGreen);
            this.Cursor = Cursors.Default;            
        }

        private void fwbrose()
        {
            var f = new FolderBrowserDialog();
            if (f.ShowDialog() == DialogResult.OK)
            {
                string p = f.SelectedPath;
                if (!File.Exists(p + "\\flash_all.bat") && (!Directory.Exists(p + "\\images")))
                {
                    MessageBox.Show("Flash script not found!");
                }
                else
                {
                    textBox1.Text = p;
                    ParseFlashBat();
                }
            }
        }
        private void fw_Click(object sender, EventArgs e)
        {
            fwbrose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int i = 0;
            while (i<checkedListBox1.Items.Count)
            {
                checkedListBox1.SetItemChecked(i, false);
                i++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            while (i < checkedListBox1.Items.Count)
            {
                checkedListBox1.SetItemChecked(i, true);
                i++;
            }
        }

        private void fl_Click(object sender, EventArgs e)
        {
            if(textBox1.TextLength>5 && checkedListBox1.Items.Count>1 && !textBox1.Text.Contains("Double"))
            {                
                richTextBox2.Clear();
                android.UpdateDeviceList();
                if (android.HasConnectedDevices)
                {
                    var device = android.GetConnectedDevice(android.ConnectedDevices[0]);
                    switch (device.State.ToString())
                    {
                        case "FASTBOOT":
                            FBInfo();
                            if (blu)
                            {
                                this.Text = "Fastboot Flasher (Flashing : Don't touch me)";
                                FBFlash();                                
                                DialogResult d = MessageBox.Show("Operation Done!" + Environment.NewLine + "Do you want to reboot?", "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);                                
                                if (d == DialogResult.Yes)
                                {
                                    
                                    device.FastbootReboot();
                                }
                                this.Text = "Fastboot Flasher";
                            }
                            else
                            {
                                richTextBox2.AppendText(Environment.NewLine + "Bootloader is locked. Aboaring!", Color.Red);
                            }
                            break;
                        default:
                            MessageBox.Show("Device not in fastboot.");
                            break;
                    }
                    android.Dispose();
                }
                else
                {
                    richTextBox2.AppendText("No Device Found!", Color.Red);
                    android.Dispose();
                }
            }
            else
            {
                MessageBox.Show("Select firmware first", "Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }           
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                comboBox1.Enabled = true;
                comboBox1.SelectedIndex = 0;
            }
            else
            {
                comboBox1.Enabled = false;
            }
        }

        private void inf_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            FBInfo();
        }

        private void tb1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            fwbrose();
        }
    }
    public static class RichTextBoxExtension
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
