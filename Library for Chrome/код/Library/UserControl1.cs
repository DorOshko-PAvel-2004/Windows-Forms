using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.AccessControl;
using System.IO;
using System.Security.Policy;
using System.Xml.Linq;

namespace Library
{

    [DefaultEvent(nameof(webBrowser1))]
    public partial class UserControl1: UserControl
    {
    internal string startPage = "https://google.com";
        internal Dictionary<string,string> pathes = new Dictionary<string, string>();
        public UserControl1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (filesBox.SelectedItems.Count != 0)
            {
                string name = filesBox.SelectedItems[0].ToString();
                string url = pathes[name];
                try
                {
                    using (FileStream stream = File.OpenRead(url))
                    {
                    webBrowser1.Url = new Uri(url);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    pathes.Remove(name);
                    filesBox.Items.RemoveAt(filesBox.SelectedIndex);
                    webBrowser1.Url = new Uri(startPage);
                }
            }
        }
        private void AddButton_Click(object sender, EventArgs e)
        {
            if(OFD.ShowDialog()==DialogResult.OK)
            {
                OFD_FileOk();
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (filesBox.Items.Count == 0)
                {
                    MessageBox.Show("There is no one page.");
                    return;
                }
                if (filesBox.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Choose one page for deleting.");
                    return;
                }
                pathes.Remove(filesBox.SelectedItem.ToString());
                filesBox.Items.RemoveAt(filesBox.SelectedIndex);
                webBrowser1.Url = new Uri(startPage);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (UrlText.Text.Trim() != "")
            {
                try
                {
                    webBrowser1.Url = new Uri(UrlText.Text.Trim());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    webBrowser1.Url = new Uri(startPage);
                }
            }
        }

        private void OFD_FileOk()
        {
            string path = OFD.FileName;
            string name = OFD.FileName.Split('\\').Last();
            while (true)
            {
                if(pathes.Values.Contains(path))
                {
                    MessageBox.Show("This file is already added.");
                    return;
                }
                if (!pathes.ContainsKey(name))
                {
                    pathes.Add(name,path);
                    filesBox.Items.Add(name);
                    return;
                }
                else if (pathes.ContainsKey(name))
                {
                    name = string.Concat('~', name);
                    continue;
                }
            }
        }

        private void UpdateBbutton_Click(object sender, EventArgs e)
        {
            //var keys = pathes.Keys.ToArray();
            for (int i = 0; i < pathes.Keys.ToArray().Length; i++) 
            { 
                string file = pathes[pathes.Keys.ToArray()[i]]; 
                if(File.Exists(file))
                {
                    try
                    {
                        using (FileStream stream = File.OpenRead(file))
                        {
                            webBrowser1.Url = new Uri(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        filesBox.Items.Remove(pathes.Keys.ToArray()[i]);
                        pathes.Remove(pathes.Keys.ToArray()[i]);
                        i--;
                    }
                }
                else
                {
                    MessageBox.Show($"File {file} doesn't exist.");
                    filesBox.Items.Remove(pathes.Keys.ToArray()[i]);
                    pathes.Remove(pathes.Keys.ToArray()[i]);
                    i--;
                }
            }
        }
        public void InsertUrl(string Url)
        {
            try
            {
                Uri site = new Uri(Url);
                webBrowser1.Url = site;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
