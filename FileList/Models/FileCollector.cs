using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;

namespace FileList.Models
{


    public class FileCollector
    {

        private readonly BackgroundWorker _worker;
        
        private FileCollector()
        {
            _worker = BWorker.getInstance();
            //dt = new DataTable("FileList");
            //FileItems = new List<FileItem>();

        }

        private static FileCollector fileCollector;

        public DoWorkEventArgs e;

        public static FileCollector getInstance()
        {
            if(fileCollector == null)
            {
                fileCollector = new FileCollector();
            }

            return fileCollector;
        }

        #region 프로퍼티s
        private string _saveFolderPath;
        /// <summary>
        /// 저장 폴더 경로
        /// </summary>
        public string SaveFolderPath {
            get { return _saveFolderPath; }
            set { 
                _saveFolderPath = value;
                SavePath = Path.Combine(SaveFolderPath, getSaveFileName());
            }
        }
        
        private int _count;
        /// <summary>
        /// 진행 상태를 알려줄 카운트, 진행된 파일 수
        /// </summary>
        public int Count {
            get { return _count; }
            set { _count = value; }
        }

        private string _targetPath;
        /// <summary>
        /// 파일리스트 만들 대상 폴더
        /// </summary>
        public string TargetPath {
            get { return _targetPath; }
            set {
                _targetPath = value;
                SavePath = Path.Combine(TargetPath, getSaveFileName());
            }
        }

        private string _savePath;

        /// <summary>
        /// 저장할 엑셀 파일 경로
        /// </summary>        
        public string SavePath {
            get { return _savePath; }
            set {
                _savePath = value;
            }
        }

        private string getSaveFileName()
        {
            return @"FileList_" + DateTime.Now.ToString("yyMMdd_HHmm") + ".xlsx";
        }

        List<FileItem> FileItems;
        DataTable dt;

        #endregion

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            Count = 0;
            dt = new DataTable("FileList");
            FileItems = new List<FileItem>();


            dt.Columns.Add(new DataColumn("순번"));
            dt.Columns.Add(new DataColumn("전체경로"));
            dt.Columns.Add(new DataColumn("폴더경로"));
            dt.Columns.Add(new DataColumn("파일명(full)"));
            dt.Columns.Add(new DataColumn("파일명(name only)"));
            dt.Columns.Add(new DataColumn("확장자"));
            dt.Columns.Add(new DataColumn("파일크기"));
        }

        public void StartProcess()
        {
            try
            {
                Init();
                if (_worker.CancellationPending)
                {
                    return;
                }
                CollectFiles();
                if (_worker.CancellationPending)
                {                    
                    return;
                }
                FileItemsToDt();
                if (_worker.CancellationPending)
                {
                    return;
                }
                WriteXlsx();
                if (_worker.CancellationPending)
                {
                    return;
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message, e);
            }
            
        }

        /// <summary>
        /// 대상폴더의 모든 파일의 리스트를 List<FileItem>으로 반환
        /// </summary>
        /// <param name="TargetPath"></param>
        /// <returns></returns>
        public IEnumerable<FileItem> CollectFiles()
        {
            if(TargetPath == null || SavePath == null)
            {
                throw new MissingFieldException("대상이 선택되지 않았습니다");
                //MessageBox.Show("대상을 입력하세요");                
            }

            if (!Directory.Exists(TargetPath))
            {
                throw new DirectoryNotFoundException();
            }

            if (File.Exists(SavePath))
            {
                throw new IOException("해당 경로에 파일을 쓸수 없습니다. 이미 파일이 존재할 수 있습니다");
            }

            try
            {
                FileIOPermission writePermission = new FileIOPermission(FileIOPermissionAccess.Write, SavePath);
                if (!SecurityManager.IsGranted(writePermission))
                {
                    throw new UnauthorizedAccessException("'{Savepath}' 경로에 대한 액세스가 거부되었습니다.");
                }

                FileStream fs = new FileStream(SavePath, FileMode.Create);                
                fs.Close();
                File.Delete(SavePath);
            }
            catch(Exception e)
            {
                throw new Exception(e.Message, e);
            }



            var dirInfo = new DirectoryInfo(TargetPath);

            //var EnumFileList_top = dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly);
            //var EnumFileList = EnumFileList_top.Concat(dirInfo.EnumerateDirectories().AsParallel().SelectMany(di => di.EnumerateFiles("*.*", SearchOption.AllDirectories)));
            var EnumFileList = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories);
            //var EnumFileList = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);

            foreach (var file in EnumFileList.Select((fl, index) => new { Info = fl, Index = (long)index }))
            {
                if (_worker.CancellationPending)
                {

                    return null;
                }

                FileItem fi = new FileItem();

                fi.FullPath = file.Info.FullName;
                fi.Path = file.Info.DirectoryName;
                fi.FileName = file.Info.Name;
                fi.FileNameOnly = RemoveExtension(file.Info);
                fi.Extension = file.Info.Extension;
                fi.Length = file.Info.Length;

                FileItems.Add(fi);

                Count += 1;
                ReportProg(fi.FileName);
       
            }
            FileItems.Sort(new FileItemComparer());


            foreach (var file in FileItems.Select((fl, index) => new { Item = fl, Index = index }))
            {
                file.Item.Index = file.Index+1;
            }

            return FileItems as IEnumerable<FileItem>;
        }


        /// <summary>
        /// FileItemList를 DataTable로 반환
        /// </summary>
        /// <param name="FileItems"></param>
        /// <returns></returns>
        public DataTable FileItemsToDt()
        {
            foreach (var item in FileItems)
            {
                if (_worker.CancellationPending)
                {
                    return null;
                }
                ReportProg("데이터 테이블 생성중...");
                DataRow row = dt.NewRow();
                row.ItemArray = new object[] { item.Index, item.FullPath, item.Path, item.FileName, item.FileNameOnly, item.Extension, item.Length };
                dt.Rows.Add(row);
            }

            return dt;
        }


        ///// <summary>
        ///// 데이터 테이블을 리스트로 반환
        ///// </summary>
        ///// <param name="dataTable"></param>
        ///// <returns></returns>
        //private List<string> DatatabeToList(DataTable dataTable)
        //{
        //    var lines = new List<string>();

        //    string[] columnNames = dataTable.Columns
        //        .Cast<DataColumn>()
        //        .Select(column => column.ColumnName)
        //        .ToArray();

        //    var header = string.Join(",", columnNames.Select(name => $"\"{name}\""));
        //    lines.Add(header);

        //    var valueLines = dataTable.AsEnumerable()
        //        .Select(row => string.Join(",", row.ItemArray.Select(val => $"\"{val}\"")));

        //    lines.AddRange(valueLines);

        //    return lines;

        //}

        /// <summary>
        /// 파일리스트(dataTable)을 엑셀파일로 저장
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="SavePath"></param>
        public bool WriteXlsx()
        {
            if (dt == null)
            {
                ReportProg("작업에 실패하였습니다.");
                return false;
            }

            if (File.Exists(SavePath))
            {
                ReportProg("작업에 실패. 이미 파일이 있습니다.");
                return false;
            }

            try
            {
                FileIOPermission writePermission = new FileIOPermission(FileIOPermissionAccess.Write, SavePath);
                if (!SecurityManager.IsGranted(writePermission))
                {
                    throw new UnauthorizedAccessException("'{Savepath}' 경로에 대한 액세스가 거부되었습니다.");
                }

                FileStream fs = new FileStream(SavePath, FileMode.Create);
                fs.Close();
                File.Delete(SavePath);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            using (ExcelPackage pck = new ExcelPackage(new FileInfo(SavePath)))
            {
                try
                {
                    ReportProg("파일 생성중...");
                    if (_worker.CancellationPending)
                    {
                        return false;
                    }
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("FileList");
                    ws.Cells["A1"].LoadFromDataTable(dt, true);
                    pck.Save();
                    ReportProg("파일 리스트 생성 완료!");
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message, e);
                }
                finally
                {
                    dt.Dispose();
                }
            }
        }

        private void ReportProg(string status)
        {
            _worker.ReportProgress(Count, status);
        }

        /// <summary>
        /// 파일명에서 확장자 제거
        /// </summary>
        /// <param name="fileinfo"></param>
        /// <returns></returns>
        private string RemoveExtension(FileInfo fileinfo)
        {
            string fileName = fileinfo.Name;
            string extension = fileinfo.Extension;
            int lenOfExtension = extension.Length;

            string nameOnly = fileName.Substring(0, fileName.Length - extension.Length);

            return nameOnly;
        }

    }
}
