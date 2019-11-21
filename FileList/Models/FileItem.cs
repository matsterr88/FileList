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
    public class FileItem
    {
        public int Index { get; set; }
        public string FullPath { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string FileNameOnly { get; set; }
        public string Extension { get; set; }
        public long Length { get; set; }
    }

    class FileItemComparer : Comparer<FileItem>
    {
        public override int Compare(FileItem x, FileItem y)
        {
            if (x.Path.Equals(y.Path))
            {
                if (x.FileName.Equals(y.FileName))
                {
                    return 0;
                }

                return x.FileName.CompareTo(y.FileName);
            }
            else
            {
                return x.Path.CompareTo(y.Path);
            }
            
        }
    }

    //class FileListComparerByFileName : Comparer<FileItem>
    //{
    //    public override int Compare(FileItem x, FileItem y)
    //    {
    //        if (x.FileName.Equals(y.FileName)) return 0;
    //        return x.FileName.CompareTo(y.FileName);
    //    }
    //}
}
