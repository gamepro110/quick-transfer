using Core;

using Eto.Drawing;
using Eto.Forms;
using Eto.Forms.ThemedControls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar;

namespace quick_transfer_eto
{
    internal enum DirectoryType
    {
        Source,
        Target,
        other,
    }

    internal class MyForm : Form
    {
        private readonly Color grey = new Color(0.69f, 0.69f, 0.69f, 1);

#pragma warning disable IDE0044 // Add readonly modifier
        private string sourceDir;
        private string destinationDir;
        private int numThreads = 2;
        private bool overwrite = false;
        private TextBox sourceDirTextBox;
        private TextBox destinationDirTextBox;
        private List<Tuple<string, bool>> files = new List<Tuple<string, bool>>();
        private readonly Control filesBox;
        private List<int> MultithreadValues = new List<int>();
#pragma warning restore IDE0044 // Add readonly modifier

        public MyForm()
        {
            for (int i = 2; i < Environment.ProcessorCount + 1; i++)
            {
                MultithreadValues.Add(i);
            }

            MinimumSize = new Size(600, 400);
            Title = "quick-transfer";
            AutoSize = false;
            BackgroundColor = grey;

            Menu = new MenuBar
            {
                Items = {
                    new ButtonMenuItem
                    {
                        Text = "&File",
                        Items = {
                            new AboutCommand(this),
                            new QuitCommand(),
                        }
                    }
                }
            };

            var layout = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(5),
            };

            AddPathSelectRow(ref layout, DirectoryType.Source, ref sourceDirTextBox, "Source directory", "Source Path", "source directory");
            AddPathSelectRow(ref layout, DirectoryType.Target, ref destinationDirTextBox, "Target directory", "Target Path", "targets directory");

            #region options

            DropDown threadDropdown = new DropDown()
            {
                Cursor = Cursors.Pointer,
                SelectedIndex = 0,
                Height = 20
            };

            foreach (var item in MultithreadValues)
            {
                threadDropdown.Items.Add($"{item}");
            }

            threadDropdown.SelectedIndexChanged += ThreadDropdownSelectedIndexChanged;

            CheckBox overwriteCheckbox = new CheckBox()
            {
                Text = "Overwrite?",
            };

            overwriteCheckbox.CheckedChanged += OverwriteCheckboxValueChanged;

            layout.Rows.Add(
                new TableRow(
                    new Button(TransferButton_Click)
                    {
                        Text = "Start transfer",
                        Width = MinimumSize.Width / 3 * 2,
                        Height = 20,
                    },
                    threadDropdown,
                    overwriteCheckbox
                )
                {
                    ScaleHeight = false,
                }
            );

            layout.Rows.Add(
                new TableRow(
                    new Button((s, e) => ChangeSelectedFiles(true))
                    {
                        Text = "Select all",
                    },
                    new Button((s, e) => ChangeSelectedFiles(false))
                    {
                        Text = "Unselect all",
                    },
                    new TableRow()
                )
            );

            #endregion options

            #region display

            filesBox = new ListBox()
            {
                Width = MinimumSize.Width / 3 * 2,
            };

            filesBox.MouseUp += (object o, MouseEventArgs e) =>
            {
                if (o is not ListBox lbox)
                {
                    return;
                }

                var old = files[lbox.SelectedIndex];
                Tuple<string, bool> value = new(old.Item1, !old.Item2);
                files[lbox.SelectedIndex] = value;

                UpdateFilesListBox();
            };

            layout.Rows.Add(
                new TableRow(
                    filesBox
                )
            );

            #endregion display

            Content = layout;
        }

        private void ChangeSelectedFiles(bool selected)
        {
            int count = files.Count;
            for (int i = 0; i < count; i++)
            {
                Tuple<string, bool> file = new Tuple<string, bool>(files[i].Item1, selected);
                files[i] = file;
            }

            UpdateFilesListBox();
        }

        private void AddPathSelectRow(ref TableLayout layout, DirectoryType directoryType, ref TextBox textbox, string title, string dialogTitle, string placeholder = "...")
        {
            var newTextbox = new TextBox()
            {
                PlaceholderText = placeholder,
            };

            textbox = newTextbox;

            layout.Rows.Add(
                new TableRow(
                    textbox,
                    new Label
                    {
                        Text = title,
                    },
                    new Button((object sender, EventArgs e) => SetPathButtonCallback(dialogTitle, directoryType, newTextbox))
                    {
                        Text = "...",
                    }
                )
            );
        }

        private void SetPathButtonCallback(string dialogTitle, DirectoryType directoryType, TextBox textBox)
        {
            if (!DirDialog(dialogTitle, this, out string path))
            {
                return;
            }

            switch (directoryType)
            {
                case DirectoryType.Source:
                    {
                        UpdateFilesList(path);
                        sourceDir = path;
                        break;
                    }
                case DirectoryType.Target:
                    {
                        destinationDir = path;
                        break;
                    }
                case DirectoryType.other:
                default:
                    {
                        break;
                    }
            }

            textBox.Text = path;
            UpdateFilesListBox();
        }

        private void UpdateFilesList(string path)
        {
            files.Clear();
            string[] gatheredFiles = Directory.GetFiles(path);

            foreach (var item in gatheredFiles)
            {
                files.Add(new(item, true));
            }
        }

        private void UpdateFilesListBox()
        {
            if (filesBox is not ListBox fb)
            {
                return;
            }

            fb.Items.Clear();

            foreach (var item in files)
            {
                string txt = GetFileNameFromPath(item.Item1);
                fb.Items.Add($"[ {(item.Item2 ? "X" : "  ")} ]: {txt}\n");
            }
        }

        private void TransferButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(sourceDir))
            {
                MessageBox.Show("no source directory given...");
                return;
            }
            if (string.IsNullOrEmpty(destinationDir))
            {
                MessageBox.Show("no destination set...");
                return;
            }

            List<string> TransferingFiles = new List<string>();

            foreach (var file in files.Where(t => t.Item2))
            {
                TransferingFiles.Add(file.Item1);
            }

            string txt = "";
            TransferingFiles.ForEach(s => txt += $"{GetFileNameFromPath(s)}\t\n");
            txt += $"\n\n\nDestination: '{destinationDir}\\'";
            DialogResult questionResult = MessageBox.Show(txt, "are you sure u want to copy the following file(s)??", MessageBoxButtons.OKCancel, MessageBoxType.Question);
            if (DialogResult.Cancel == questionResult)
            {
                return;
            }

            bool res = Transferer.Transfer(TransferingFiles, destinationDir, numThreads, overwrite);

            string messageBoxText;
            MessageBoxType type;

            if (!res)
            {
                messageBoxText = "something went wrong during the transfer!!";
                type = MessageBoxType.Error;
            }
            else
            {
                messageBoxText = "transfered file(s) succesfully :)";
                type = MessageBoxType.Information;
                destinationDir = string.Empty;
            }
            MessageBox.Show(messageBoxText, type);
        }

        private void OverwriteCheckboxValueChanged(object sender, EventArgs e)
        {
            if (sender is not CheckBox check)
            {
                return;
            }

            overwrite = check.Enabled;
        }

        private void ThreadDropdownSelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is not DropDown drop)
            {
                return;
            }

            numThreads = MultithreadValues[drop.SelectedIndex];
        }

        private static bool DirDialog(string title, Window win, out string path)
        {
            SelectFolderDialog folderDialog = new SelectFolderDialog()
            {
                Title = title,
            };

            if (folderDialog.ShowDialog(win) != DialogResult.Ok)
            {
                path = "";
                return false;
            }

            path = folderDialog.Directory;
            return true;
        }

        private string GetFileNameFromPath(string input)
        {
            return input.Split("\\")[^1];
        }
    }

    public class QuitCommand : Command
    {
        public QuitCommand()
        {
            MenuText = "Quit";
            ToolBarText = "Quit";
            ToolTip = "Quit the App";
            Shortcut = Keys.Escape;
        }

        protected override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            Application.Instance.Quit();
        }
    }

    public class AboutCommand : Command
    {
        private readonly Window window;

        public AboutCommand(Window window)
        {
            this.window = window;
            MenuText = "About";
        }

        protected override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            new AboutDialog().ShowDialog(window);
        }
    }
}
