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
    // Remake this in eto-xml form?
    // TODO add recursive option
    internal class MyForm : Form
    {
        private readonly Color grey = new Color(0.69f, 0.69f, 0.69f, 1);

        private string sourceDir;
        private string destinationDir;
        private TextBox sourceDirTextBox;
        private TextBox destinationDirTextBox;
        private List<Tuple<string, bool>> files = new List<Tuple<string, bool>>();
        private readonly Control FileBox;

        internal string SrcDir
        {
            get => sourceDir;
            set => sourceDir = value;
        }

        public MyForm()
        {
            MinimumSize = new Size(600, 400);
            Title = "quick-transfer";
            AutoSize = true;
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

            addPathSelectRow(ref layout, ref sourceDir, ref sourceDirTextBox, "Source directory", "Source Path", "source directory");
            addPathSelectRow(ref layout, ref destinationDir, ref destinationDirTextBox, "Target directory", "Target Path", "targets directory", false);

            FileBox = new TextArea()
            {
                AcceptsReturn = true,
                AcceptsTab = false,
                ReadOnly = true,
                Width = this.MinimumSize.Width,
            };

            layout.Rows.Add(
                new TableRow(
                    FileBox
                )
            );

            Content = layout;
        }

        private void addPathSelectRow(ref TableLayout layout, ref string targetDir, ref TextBox textbox, string title, string dialogTitle, string placeholder = "...", bool updateTextArea = true)
        {
            var newTextbox = new TextBox()
            {
                PlaceholderText = placeholder,
                DataContext = targetDir,
            };

            textbox = newTextbox;

            layout.Rows.Add(
                new TableRow(
                    textbox,
                    new Label
                    {
                        Text = title,
                    },
                    new Button(buttonCallback)
                    {
                        Text = "...",
                    }
                )
            );

            void buttonCallback(object sender, EventArgs e)
            {
                DirDialog(dialogTitle, this, out string path);
                SrcDir = path;
                newTextbox.Text = path;

                if (!updateTextArea)
                { return; }

                files.Clear();

                string[] gatheredFiles = Directory.GetFiles(path);

                foreach (var item in gatheredFiles)
                {
                    files.Add(new(item, true));
                }

                var fb = FileBox as TextArea;
                fb.Text = string.Empty;
                foreach (var item in files)
                {
                    string txt = item.Item1.Split("\\")[^1];
                    fb.Text += $"[{(item.Item2 ? "X" : " ")}]: {txt}\n";
                }
            }
        }

        private List<TableRow> fillRows<T>(List<T> list)
        {
            List<TableRow> rows = new List<TableRow>();

            foreach (var item in list)
            {
                rows.Add(
                    new TableRow(
                        new Label()
                        {
                            Text = $"{item}"
                        }
                    )
                );
            }

            return rows;
        }

        private void DirDialog(string title, Window win, out string path)
        {
            SelectFolderDialog folderDialog = new SelectFolderDialog()
            {
                Title = title,
            };

            if (folderDialog.ShowDialog(win) != DialogResult.Ok)
            {
                path = "";
                return;
            }

            path = folderDialog.Directory;
        }

        private static bool IsPath(string dir)
        {
            return
                Path.EndsInDirectorySeparator(dir) &&
                Directory.Exists(dir) &&
                Uri.IsWellFormedUriString(dir, UriKind.Absolute);
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
        private Window window;

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