using System;
using System.Collections.Generic;
using System.IO;

namespace AstroxEditor
{

    public class AstroxElement
    {
        public AstroxFile File;
        public string Tag;
        public List<AstroxFile.Entry> Entries = new List<AstroxFile.Entry>();
        public bool IsCollection => Entries.Count > 1;

        //public AstroxElement(AstroxFile File, string Tag)
        //{
        //    Entries = new();

        //    Entries.Add(entry);
        //}
        //public AstroxElement()
        //{
        //    Entries = new();
        //}
        public AstroxFile.Entry this[int index]
        {
            get => Entries[index];
            //set
            //{
            //    int indexInFile = File.Entries.FindIndex(x => x == Entries[index]);
            //    File.Entries[indexInFile] = 
            //}
        }
        public void Add(AstroxFile.Entry entry)
        {
            if (Entries.Count > 0)
            {
                int indexInFile = File.Entries.FindIndex(x => x == Entries[Entries.Count - 1]);
                File.Entries.Insert(indexInFile + 1, entry);
            }
            else
            {
                File.Entries.Add(entry);
            }
        }
    }
    public class AstroxFile
    {
        public SaveGame SaveGame;
        public string Filename;
        public List<Entry> Entries;
        public AstroxFile(SaveGame sg, string filename)
        {
            SaveGame = sg;
            Filename = filename;
        }
        public virtual void Load()
        {
            Entries = new List<Entry>();

            var strm = File.OpenText(Filename);
            while (!strm.EndOfStream)
            {
                var line = strm.ReadLine();
                var e = new Entry(line);
                Entries.Add(e);
            }
            strm.Close();

        }
        public virtual void Save()
        {
            SaveTo(Filename);
        }
        public virtual void SaveTo(string filename)
        {
            var strm = File.CreateText(filename);
            foreach (var l in Entries)
            {
                l.UpdateFullLine();
                strm.WriteLine(l.FullLine);
            }
            strm.Close();
        }
        public virtual void SaveToFolder(string folder)
        {
            SaveTo(folder + "\\" + Path.GetFileName(Filename));
        }
        public IEnumerable<Entry> AllOfTag(string tag)
        {
            foreach (var e in Entries)
            {
                if (e.Tag == tag) yield return e;
            }
        }

        Dictionary<string, AstroxElement> AstroxElements = new Dictionary<string, AstroxElement>();

        public AstroxElement GetElement(string tag)
        {
            if (AstroxElements.TryGetValue(tag, out var ae))
            {
                return ae;
            }

            var aeN = new AstroxElement();
            aeN.File = this;
            aeN.Tag = tag;
            foreach (var e in Entries)
            {
                if (e.Tag == tag)
                {
                    aeN.Entries.Add(e);
                }
            }
            AstroxElements.Add(tag, aeN);
            return ae;
        }
        public class Entry
        {
            public string FullLine;
            public string Tag;
            public String[] Values;
            bool IsDirty = false;
            public Entry(string line)
            {
                FullLine = line;
                Values = FullLine.Trim().Split(';');
                if (Values.Length > 0)
                {
                    if (Values[0].StartsWith("//"))
                    {
                        Tag = null;
                    }
                    else
                    {
                        Tag = Values[0].Trim();
                    }
                }
            }
            public bool SetColumnValue(int colIndex, string value)
            {
                if (colIndex < Values.Length)
                {
                    Values[colIndex] = value;
                    IsDirty = true;
                    return true;
                }
                return false;
            }
            public bool SetColumnValue(int colIndex, double value)
            {
                if (colIndex < Values.Length)
                {
                    Values[colIndex] = value.ToString();
                    IsDirty = true;
                    return true;
                }
                return false;
            }
            public bool SetColumnValue(int colIndex, int value)
            {
                if (colIndex < Values.Length)
                {
                    Values[colIndex] = value.ToString();
                    IsDirty = true;
                    return true;
                }
                return false;
            }
            public bool SetColumnValue(int colIndex, double3 value)
            {
                if (colIndex + 2 < Values.Length)
                {
                    Values[colIndex] = value.x.ToString();
                    Values[colIndex + 1] = value.y.ToString();
                    Values[colIndex + 2] = value.z.ToString();
                    IsDirty = true;
                    return true;
                }
                return false;
            }
            public bool TryGetColumnValue(int colIndex, out string value)
            {
                if (colIndex < Values.Length)
                {
                    value = Values[colIndex];
                    return true;
                }
                value = default;
                return false;
            }
            public bool TryGetColumnValue(int colIndex, out double value)
            {
                if (colIndex < Values.Length)
                {
                    return double.TryParse(Values[colIndex], out value);
                }
                value = default;
                return false;
            }
            public bool TryGetColumnValue(int colIndex, out int value)
            {
                if (colIndex < Values.Length)
                {
                    return int.TryParse(Values[colIndex], out value);
                }
                value = default;
                return false;
            }
            public bool TryGetColumnValue(int colIndex, out double3 value)
            {
                value = default;
                if (colIndex + 2 < Values.Length)
                {
                    if (!double.TryParse(Values[colIndex], out value.x)) return false;
                    if (!double.TryParse(Values[colIndex + 1], out value.y)) return false;
                    if (!double.TryParse(Values[colIndex + 2], out value.z)) return false;
                    return true;
                }
                return false;
            }
            public bool TryGetColumnRef(int colIndex, out LineRefString lr)
            {
                if (colIndex < Values.Length)
                {
                    lr = new LineRefString(this, colIndex);
                    return true;
                }
                lr = default;
                return false;
            }
            public bool TryGetColumnRef(int colIndex, out LineRefInt lr)
            {
                if (colIndex < Values.Length)
                {
                    lr = new LineRefInt(this, colIndex);
                    return true;
                }
                lr = default;
                return false;
            }
            public bool TryGetColumnRef(int colIndex, out LineRefDouble lr)
            {
                if (colIndex < Values.Length)
                {
                    lr = new LineRefDouble(this, colIndex);
                    return true;
                }
                lr = default;
                return false;
            }
            public bool TryGetColumnRef(int colIndex, out LineRefDouble3 lr)
            {
                if (colIndex + 2 < Values.Length)
                {
                    lr = new LineRefDouble3(this, colIndex);
                    return true;
                }
                lr = default;
                return false;
            }
            public void UpdateFullLine()
            {
                if (Tag != null && IsDirty)
                {
                    string fl = Tag;
                    for (int i = 1; i != Values.Length; ++i)
                    {
                        fl += ";" + Values[i];
                    }
                    FullLine = fl;
                    IsDirty = false;
                }

            }
        }


        public struct LineRef
        {
            public LineRef(Entry entry, int columnIndex)
            {
                Entry = entry;
                ColumnIndex = columnIndex;
            }
            public Entry Entry;
            public int ColumnIndex;


            public bool SetValue(string value) => Entry.SetColumnValue(ColumnIndex, value);
            public bool SetColumnValue(double value) => Entry.SetColumnValue(ColumnIndex, value);
            public bool SetColumnValue(int value) => Entry.SetColumnValue(ColumnIndex, value);
            public bool TryGetValue(out string value) => Entry.TryGetColumnValue(ColumnIndex, out value);
            public bool TryGetColumnValue(out double value) => Entry.TryGetColumnValue(ColumnIndex, out value);
            public bool TryGetColumnValue(out int value) => Entry.TryGetColumnValue(ColumnIndex, out value);
        }


        public struct LineRefInt
        {
            public LineRefInt(Entry entry, int columnIndex)
            {
                Entry = entry;
                ColumnIndex = columnIndex;
            }
            public Entry Entry;
            public int ColumnIndex;

            public int Value
            {
                get
                {
                    if (Entry.TryGetColumnValue(ColumnIndex, out int value)) return value;
                    return default;
                }
                set => Entry.SetColumnValue(ColumnIndex, value);
            }
            public override string ToString()
            {
                if (Entry.TryGetColumnValue(ColumnIndex, out string value)) return value;
                return null;
            }
            public bool SetColumnValue(int value) => Entry.SetColumnValue(ColumnIndex, value);
            public bool TryGetColumnValue(out int value) => Entry.TryGetColumnValue(ColumnIndex, out value);
        }
        public struct LineRefString
        {
            public LineRefString(Entry entry, int columnIndex)
            {
                Entry = entry;
                ColumnIndex = columnIndex;
            }
            public Entry Entry;
            public int ColumnIndex;

            public string Value
            {
                get
                {
                    if (Entry.TryGetColumnValue(ColumnIndex, out string value)) return value;
                    return default;
                }
                set => Entry.SetColumnValue(ColumnIndex, value);
            }

            public override string ToString()
            {
                if (Entry.TryGetColumnValue(ColumnIndex, out string value)) return value;
                return null;
            }
            public bool SetColumnValue(string value) => Entry.SetColumnValue(ColumnIndex, value);
            public bool TryGetColumnValue(out string value) => Entry.TryGetColumnValue(ColumnIndex, out value);
        }
        public struct LineRefDouble
        {
            public LineRefDouble(Entry entry, int columnIndex)
            {
                Entry = entry;
                ColumnIndex = columnIndex;
            }
            public Entry Entry;
            public int ColumnIndex;

            public string Value
            {
                get
                {
                    if (Entry.TryGetColumnValue(ColumnIndex, out string value)) return value;
                    return default;
                }
                set => Entry.SetColumnValue(ColumnIndex, value);
            }

            public override string ToString()
            {
                if (Entry.TryGetColumnValue(ColumnIndex, out string value)) return value;
                return null;
            }
            public bool SetColumnValue(string value) => Entry.SetColumnValue(ColumnIndex, value);
            public bool TryGetColumnValue(out string value) => Entry.TryGetColumnValue(ColumnIndex, out value);
        }
        public struct LineRefDouble3
        {
            public LineRefDouble3(Entry entry, int columnIndex)
            {
                Entry = entry;
                ColumnIndex = columnIndex;
            }
            public Entry Entry;
            public int ColumnIndex;

            public double3 Value
            {
                get
                {
                    if (Entry.TryGetColumnValue(ColumnIndex, out double3 value)) return value;
                    return default;
                }
                set => Entry.SetColumnValue(ColumnIndex, value);
            }

            public override string ToString()
            {
                var v = Value;
                return $"({v.x},{v.y},{v.z})";
            }
            public bool SetColumnValue(double3 value) => Entry.SetColumnValue(ColumnIndex, value);
            public bool TryGetColumnValue(out double3 value) => Entry.TryGetColumnValue(ColumnIndex, out value);
        }

    }
}