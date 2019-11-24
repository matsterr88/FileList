using FileList.Models;
using FileList.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace FileList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly FileCollector _fileCollector;
        private readonly BackgroundWorker _worker;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = this.DataContext as MainWindowViewModel;
            _fileCollector = FileCollector.getInstance();
            _worker = BWorker.getInstance();

            _worker.DoWork += workerDoWork;
            _worker.RunWorkerCompleted += workerRunWorkerCompleted;
            _worker.ProgressChanged += workerProgressChanged;

            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
        }

        private void workerProgressChanged(object sender, ProgressChangedEventArgs e)
        {            
            textBlockCount.Text = e.ProgressPercentage.ToString();
            textBoxCurrent.Text = e.UserState.ToString();
        }

        private void workerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                textBoxCurrent.Text = "작업이 취소되었습니다";
            }
            else if (e.Error != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(e.Error.Message + "\n" + e.ToString());
                });
                textBoxCurrent.Text = "작업 실패";
            }
            btnMakeList.IsEnabled = true;
            btnSelectSave.IsEnabled = true;
            btnSelectTarget.IsEnabled = true;
            btnCancel.IsEnabled = false;

        }

        private void workerDoWork(object sender, DoWorkEventArgs e)
        {
            //_fileCollector.Init();
            //if (_worker.CancellationPending)
            //{
            //    e.Cancel = true;
            //    return;
            //}
            //_fileCollector.CollectFiles();
            //if (_worker.CancellationPending)
            //{
            //    e.Cancel = true;
            //    return;
            //}
            //_fileCollector.FileItemsToDt();
            //if (_worker.CancellationPending)
            //{
            //    e.Cancel = true;
            //    return;
            //}
            //_fileCollector.WriteXlsx();

            _fileCollector.StartProcess();

            if (_worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }


        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dataString = (string[])e.Data.GetData(DataFormats.FileDrop);

                _viewModel.TargetPath = dataString[0];
            }        
        }


        private void SelectFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dlg = new CommonOpenFileDialog())
            {
                dlg.IsFolderPicker = true;
                CommonFileDialogResult result = dlg.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    _viewModel.TargetPath = dlg.FileName;
                }
            }

        }

        private async void MakeListBtn_Click(object sender, RoutedEventArgs e)
        {
            btnMakeList.IsEnabled = false;
            btnSelectSave.IsEnabled = false;
            btnSelectTarget.IsEnabled = false;
            btnCancel.IsEnabled = true;

            _worker.RunWorkerAsync();
        }

        private void SelectSaveFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dlg = new CommonOpenFileDialog())
            {
                dlg.IsFolderPicker = true;
                CommonFileDialogResult result = dlg.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    _viewModel.SaveFolderPath = dlg.FileName;
                }
            }
        }

        private void CalcelBtn_Click(object sender, RoutedEventArgs e)
        {
            _worker.CancelAsync();
        }
    }
}
