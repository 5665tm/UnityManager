using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ListBox = System.Windows.Controls.ListBox;

namespace UnityManager
{
	public partial class MainWindow
	{
		private string _folderMain;
		private string _folderExtras;
		private readonly List<string> _filesReplace = new List<string>();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void BtFolderMain_Click(object sender, RoutedEventArgs e)
		{
			_folderMain = SelectFolder(LbFolderMain);
		}

		private void BtFolderExtras_Click(object sender, RoutedEventArgs e)
		{
			_folderExtras = SelectFolder(LbFolderExtras);
		}

		private static string SelectFolder(ContentControl lbInfo)
		{
			var folderBrowser = new FolderBrowserDialog();
			var dialogResult = folderBrowser.ShowDialog();
			if (dialogResult == System.Windows.Forms.DialogResult.OK)
			{
				string folderName = folderBrowser.SelectedPath;
				lbInfo.Content = folderName;
				return folderName;
			}
			return null;
		}

		private void BtStat_Click(object sender, RoutedEventArgs e)
		{
			ListBoxFiles.Items.Clear();
			List<string> directoryList = new List<string>();
			List<string> fileList = new List<string>();
			GetAllFilesAndDirectories(_folderMain, directoryList, fileList);

			CreateHeader("Файлы у которых нет метафайлa", ListBoxFiles);
			foreach (var file in fileList)
			{
				if (!file.EndsWith(".meta"))
				{
					if (!fileList.Contains(file + ".meta"))
					{
						ListBoxFiles.Items.Add(file);
					}
				}
			}

			CreateHeader("Папки у которых нет метафайлa", ListBoxFiles);
			foreach (var dir in directoryList)
			{
				string[] splitDirName = dir.Split('\\');
				string parentFolder = "";
				for (int i = 0; i < splitDirName.Length - 1; i++)
				{
					parentFolder += splitDirName[i] + "\\";
				}
				string dirName = splitDirName[splitDirName.Length - 1];
				if (!fileList.Contains(parentFolder + dirName + ".meta"))
				{
					ListBoxFiles.Items.Add(dir);
				}
			}

			CreateHeader("Метафайлы без пары", ListBoxFiles);
			foreach (var file in fileList)
			{
				if (file.EndsWith(".meta"))
				{
					string pairName = file.Substring(0, file.Length - 5);
					bool alone = !(fileList.Contains(pairName))
						&& !(directoryList.Contains(pairName));

					if (alone)
					{
						ListBoxFiles.Items.Add(file);
					}
				}
			}

			if (_folderExtras == null)
			{
				return;
			}

			List<string> directoryListExtras = new List<string>();
			List<string> fileListExtras = new List<string>();
			GetAllFilesAndDirectories(_folderExtras, directoryListExtras, fileListExtras);
			CreateHeader("Метафайлы которые есть в обоих проектах, но различаются по содержанию", ListBoxFiles);
			foreach (var file in fileList)
			{
				if (file.EndsWith(".meta"))
				{
					string pair = _folderExtras + file.Replace(_folderMain, "");
					if (fileListExtras.Contains(pair))
					{
						string mainStr = File.ReadAllText(file);
						string extStr = File.ReadAllText(pair);
						if (mainStr != extStr)
						{
							_filesReplace.Add(file);
							ListBoxFiles.Items.Add(file);
						}
					}
				}
			}
		}

		private void CreateHeader(string headerName, ListBox listBox)
		{
			listBox.Items.Add("");
			listBox.Items.Add("");
			listBox.Items.Add("=== " + headerName.ToUpperInvariant() + " ===");
			listBox.Items.Add("");
			listBox.Items.Add("");
		}

		private void GetAllFilesAndDirectories(string startDirectory, List<string> directoryList, List<string> fileList)
		{
			string[] directories = Directory.GetDirectories(startDirectory);
			directoryList.AddRange(directories);
			fileList.AddRange(Directory.GetFiles(startDirectory));
			if (directories.Length > 0)
			{
				foreach (string t in directories)
				{
					GetAllFilesAndDirectories(t + @"\", directoryList, fileList);
				}
			}
		}

		private void BtReplace_Click(object sender, RoutedEventArgs e)
		{
			foreach (var file in _filesReplace)
			{
				string pair = _folderExtras + file.Replace(_folderMain, "");
				File.Copy(pair, file, true);
			}
		}
	}
}