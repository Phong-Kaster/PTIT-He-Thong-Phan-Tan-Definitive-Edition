using System;
using System.Collections.Generic;

namespace SharedClass
{
    [Serializable]
    public partial class Dir
    {
        public String Name { get; set; }
        public String Path { get; set; }
        public List<Dir> SubDirectories { get; set; }
        public List<FileDir> SubFiles { get; set; }

        public Dir(String name, String path)
        {
            Name = name;
            Path = path;
            SubFiles = new List<FileDir>();
            SubDirectories = new List<Dir>();
        }

        public Dir() { }

    }

    [Serializable]
    public partial class FileDir
    {
        public String Name { get; set; }
        public String Path { get; set; }
        public FileDir(String name, String path)
        {
            Name = name;
            Path = path;
        }

        public FileDir() { }
    }
}
