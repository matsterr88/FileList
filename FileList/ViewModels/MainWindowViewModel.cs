using FileList.Models;
using System.ComponentModel;

namespace FileList.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public FileCollector fc;

        public MainWindowViewModel()
        {
            fc = FileCollector.getInstance();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region 프로퍼티s
        /// <summary>
        /// 저장 폴더 경로
        /// </summary>
        public string SaveFolderPath {
            get { return fc.SaveFolderPath; }
            set { 
                fc.SaveFolderPath = value;
                OnPropertyChanged(nameof(SaveFolderPath));
                OnPropertyChanged(nameof(SavePath));
            }
        }

        private int _count;
        /// <summary>
        /// 진행 상태를 알려줄 카운트, 진행된 파일 수
        /// </summary>
        public int Count {
            get { return fc.Count; }
            set { 
                fc.Count = value;
                OnPropertyChanged(nameof(Count));
            }
        }

        private string _targetPath;
        /// <summary>
        /// 파일리스트 만들 대상 폴더
        /// </summary>
        public string TargetPath {
            get { return fc.TargetPath; }
            set {
                fc.TargetPath = value;
                OnPropertyChanged(nameof(TargetPath));
                OnPropertyChanged(nameof(SavePath));
            }
        }

        private string _savePath;
        /// <summary>
        /// 저장할 엑셀 파일 경로
        /// </summary>        
        public string SavePath {
            get { return fc.SavePath; }
            set { 
                fc.SavePath = value;
                OnPropertyChanged(nameof(SavePath));
            }
        }
        #endregion

        

        ///// <summary>
        ///// 파일 리스트 만들기
        ///// </summary>
        //public void MakeFileList()
        //{
        //    List<FileItem> fileItemList = new List<FileItem>();
        //    List<string> FileDataList = new List<string>();
        //    DataTable dt = new DataTable();

        //    // 대상 폴더 존재 여부 확인
        //    if (Directory.Exists(TargetPath) == false)
        //    {
        //        MessageBox.Show("폴더가 존재하지 않습니다");
        //        return;
        //    }

        //    fileItemList = ExportFileListToList(TargetPath); // 파일리스트 생성

        //    SetStatus("폴더 스캔 완료");

        //    dt = FileItemListToDataTable(fileItemList); //파일리스트 DataTable로 저장
        //    dt.TableName = "FileList";


        //    if (ExportDataTableToXLSX(dt, SaveTargetPath))
        //    {
        //        SetStatus("파일 리스트 생성 완료!");
        //    }
        //    else
        //    {
        //        SetStatus("파일 리스트 생성 실패");
        //    }
        //}





        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
