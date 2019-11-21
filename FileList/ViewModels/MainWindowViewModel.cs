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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
