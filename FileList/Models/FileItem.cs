using System.Collections.Generic;

namespace FileList.Models
{
    public enum FileItemInfo
    {
        Index = 1,
        FullPath = 2,
        Path = 3,
        FileName = 4,
        FileNameOnly = 5,
        Extension = 6,
        Length = 7
    }
    class FileItem
    {
        public int Index { get; set; }
        public string FullPath { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string FileNameOnly { get; set; }
        public string Extension { get; set; }
        public long Length { get; set; }
    }

    class FileListComparer : Comparer<FileItem>
    {
        public override int Compare(FileItem x, FileItem y)
        {
            if (x.FullPath.Equals(y.FullPath)) return 0;
            return x.Path.CompareTo(y.Path);
        }
    }
}
