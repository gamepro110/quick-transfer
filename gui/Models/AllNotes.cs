using CoreLib;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gui.Models
{
    internal class AllNotes
    {
        public ObservableCollection<Note> Notes = new ObservableCollection<Note>();

        public AllNotes() => LoadNotes();

        public void LoadNotes()
        {
            Notes.Clear();

            string dataPath = Directory.GetCurrentDirectory();

            IEnumerable<Note> notes = Directory
                .EnumerateFiles(dataPath, "*.notes.txt")
                .Select(file => new Note
                {
                    Filename = file,
                    Text = File.ReadAllText(file),
                    Date = File.GetCreationTime(file)
                })
                .OrderBy(note => note.Date);

            foreach (var item in notes)
            { Notes.Add(item); }
        }
    }
}