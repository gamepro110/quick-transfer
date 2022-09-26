using Eto.Drawing;
using Eto.Forms;
using Eto.Forms.ThemedControls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace quick_transfer_eto
{
    // Remake this in eto-xml form?
    // TODO add recursive option
    internal class MyForm : Form
    {
        private readonly Color grey = new Color(0.69f, 0.69f, 0.69f, 1);

        private string sourceDir;
        private string destinationDir;
        private List<Tuple<string, bool>> files = new List<Tuple<string, bool>>();

        private List<int> numbers = new List<int> { 1, 5, 8, 8, 5, 6, 8, 4, 1, 2, 3, 3, 5 };

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
                Padding = new Padding(10, 10, 10, 10),
            };

            addPathSelectRow(ref layout, ref sourceDir, "Source directory", "Source Path", "source directory");
            addPathSelectRow(ref layout, ref destinationDir, "Target directory", "Target Path", "Targets directory");

            layout.Rows.Add(
                new TableRow(
                )
            );

            foreach (var item in files)
            {
                layout.Rows.Add(
                    new TableRow(
                        new Label { Text = item.Item1 },
                        new CheckBox { Checked = item.Item2 }
                    )
                );
            }

            //if (!string.IsNullOrEmpty(sourceDir))
            //{
            //    fillRows(Directory.GetFiles(sourceDir).ToList()).ForEach(n => { layout.Rows.Add(n); });
            //}

            Content = layout;
        }

        private void addPathSelectRow(ref TableLayout layout, ref string targetDir, string title, string dialogTitle, string placeholder = "...")
        {
            layout.Rows.Add(
                new TableRow(
                    new TextBox()
                    {
                        PlaceholderText = placeholder,
                    },
                    new Label
                    {
                        Text = title,
                    },
                    new Button(buttonCallback)
                    {
                        Text = "...",
                        Width = 20
                    }
                    //new Button((sender, e) =>
                    //{
                    //    DirDialog(dialogTitle, this, out targetDir);
                    //    files.Clear();
                    //    string[] gatheredFiles = Directory.GetFiles(targetDir);
                    //
                    //    foreach (var item in gatheredFiles)
                    //    {
                    //        files.Add(new(item, true));
                    //    }
                    //})
                    //{
                    //    Text = "...",
                    //    Width = 20
                    //}
                )
            );

            void buttonCallback(object sender, EventArgs e)
            {
                DirDialog(dialogTitle, this, out string path);
                //targetDir = path;
                files.Clear();
                string[] gatheredFiles = Directory.GetFiles(path);

                foreach (var item in gatheredFiles)
                {
                    files.Add(new(item, true));
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