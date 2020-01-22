﻿using System;
using System.Windows;
using System.IO;
using System.Text;

namespace GuiClient
{
    public partial class FileDialog : Window
    {
        private const string pluginFolder = @"plugins\";
        private const string pluginExtension = @".dll";

        public FileDialog()
        {
            InitializeComponent();
            GetFileList();
        }

        public string pluginName;

        private void LoadPluginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pluginFiles.SelectedItem != null)
            {
                pluginName = pluginFiles.SelectedItem.ToString();
                DialogResult = true;
            }
        }

        private void GetFileList()
        {
            if (Directory.Exists(GetPluginsLocation()))
            {
                string[] files = GetFiles();

                foreach (string file in files)
                {
                    pluginFiles.Items.Add(file);
                }
            }
        }

        private string GetPluginsLocation()
        {
            string baseDir = AppContext.BaseDirectory;
            string[] explodedPath = baseDir.Split("GuiClient");

            StringBuilder pluginPath = new StringBuilder();
            pluginPath.Append(explodedPath[0]);
            pluginPath.Append(pluginFolder);

            return pluginPath.ToString();
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(GetPluginsLocation());
        }
    }
}