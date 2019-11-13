using FileList.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileList.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            StatusString = "대상 폴더 끌어놓기";
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private string _targetPath;
        private string _saveTargetPath;
        private string _statusString;

        /// <summary>
        /// 진행상태를 알려줄  텍스트
        /// </summary>
        public string StatusString {
            get { return _statusString; }
            set {
                _statusString = value;
                OnPropertyChanged("StatusString");
            }
        }

        /// <summary>
        /// 파일리스트 만들 대상 폴더
        /// </summary>
        public string TargetPath {
            get { return _targetPath; }
            set {
                _targetPath = value;
                SaveTargetPath = _targetPath + @"\FileList_" + DateTime.Now.ToString("yyMMdd_ms") + ".xlsx";
                OnPropertyChanged("TargetPath");
                OnPropertyChanged("SaveTargetPath");
            }
        }

        /// <summary>
        /// 저장할 엑셀 파일 경로
        /// </summary>        
        public string SaveTargetPath {
            get { return _saveTargetPath; }
            set {
                _saveTargetPath = value;
                OnPropertyChanged("SaveTargetPath");
            }
        }

        /// <summary>
        /// 파일 리스트 만들기
        /// </summary>
        public void MakeFileList()
        {
            List<FileItem> fileItemList = new List<FileItem>();
            List<string> FileDataList = new List<string>();
            DataTable dt = new DataTable();

            // 대상 폴더 존재 여부 확인
            if (Directory.Exists(TargetPath) == false)
            {
                MessageBox.Show("폴더가 존재하지 않습니다");
                return;
            }

            fileItemList = ExportFileListToList(TargetPath); // 파일리스트 생성

            SetStatus("폴더 스캔 완료");

            dt = FileItemListToDataTable(fileItemList); //파일리스트 DataTable로 저장
            dt.TableName = "FileList";


            if (ExportDataTableToXLSX(dt, SaveTargetPath))
            {
                SetStatus("파일 리스트 생성 완료!");
            }
            else
            {
                SetStatus("파일 리스트 생성 실패");
            }
        }


        /// <summary>
        /// 대상폴더의 모든 파일의 리스트를 List<FileItem>으로 반환
        /// </summary>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        private List<FileItem> ExportFileListToList(string targetPath)
        {

            // 파일리스트 저장할 리스트 선언
            List<FileItem> FileList = new List<FileItem>();

            var dirInfo = new DirectoryInfo(targetPath);

            // 파일리스트 생성 및 메모리에 저장

            var EnumFileList_top = dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly);
            var EnumFileList = EnumFileList_top.Concat(dirInfo.EnumerateDirectories().AsParallel().SelectMany(di => di.EnumerateFiles("*.*", SearchOption.AllDirectories)));

            foreach (var file in EnumFileList.Select((fl, index) => new { Info = fl, Index = index }))
            {
                FileItem fi = new FileItem();

                //fi.Index = file.Index;
                fi.FullPath = file.Info.FullName;
                fi.Path = file.Info.DirectoryName;
                fi.FileName = file.Info.Name;
                fi.FileNameOnly = RemoveExtension(file.Info);
                fi.Extension = file.Info.Extension;
                fi.Length = file.Info.Length;

                FileList.Add(fi);
            }
            //FileList.Sort(new FileListComparerByFileName());
            FileList.Sort(new FileListComparer());

            foreach (var file in FileList.Select((fl, index) => new { Item = fl, Index = index }))
            {
                file.Item.Index = file.Index;
            }

            return FileList;
        }


        /// <summary>
        /// FileItemList를 DataTable로 반환
        /// </summary>
        /// <param name="fileItems"></param>
        /// <returns></returns>
        private DataTable FileItemListToDataTable(List<FileItem> fileItems)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(new DataColumn("순번"));
            dt.Columns.Add(new DataColumn("전체경로"));
            dt.Columns.Add(new DataColumn("폴더경로"));
            dt.Columns.Add(new DataColumn("파일명(full)"));
            dt.Columns.Add(new DataColumn("파일명(name only)"));
            dt.Columns.Add(new DataColumn("확장자"));
            dt.Columns.Add(new DataColumn("파일크기"));

            foreach (var item in fileItems)
            {
                DataRow row = dt.NewRow();
                row.ItemArray = new object[] { item.Index, item.FullPath, item.Path, item.FileName, item.FileNameOnly, item.Extension, item.Length };
                dt.Rows.Add(row);
            }

            return dt;
        }


        /// <summary>
        /// 데이터 테이블을 리스트로 반환
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<string> DatatabeToList(DataTable dataTable)
        {
            var lines = new List<string>();

            string[] columnNames = dataTable.Columns
                .Cast<DataColumn>()
                .Select(column => column.ColumnName)
                .ToArray();

            var header = string.Join(",", columnNames.Select(name => $"\"{name}\""));
            lines.Add(header);

            var valueLines = dataTable.AsEnumerable()
                .Select(row => string.Join(",", row.ItemArray.Select(val => $"\"{val}\"")));

            lines.AddRange(valueLines);

            return lines;

        }

        /// <summary>
        /// 파일리스트(dataTable)을 엑셀파일로 저장
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath"></param>
        private bool ExportDataTableToXLSX(DataTable dataTable, string filePath)
        {
            using (ExcelPackage pck = new ExcelPackage(new FileInfo(filePath)))
            {
                try
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("FileList");
                    ws.Cells["A1"].LoadFromDataTable(dataTable, true);
                    pck.Save();
                    return true;
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("이미 파일이 존재합니다.", "에러");
                    return false;
                }
            }
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


        /// <summary>
        /// 프로세스 상태 메시지 업데이트
        /// </summary>
        /// <param name="statusStr"></param>
        private void SetStatus(string statusStr)
        {
            StatusString = statusStr;
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
