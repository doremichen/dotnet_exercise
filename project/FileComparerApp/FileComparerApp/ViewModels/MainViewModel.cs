/** FileComparerApp - A simple application to compare files and folders.
 * 
 * This code is part of the FileComparerApp project, which allows users to compare files and folders.
 * The FolderNode class represents a folder in the comparison structure.
 * This class is main ViewModel for the FileComparerApp, handling the comparison logic and UI interactions.
 * 
 * Author: Adam Chen
 * Date: 2025/07/25
 */
using FileComparerApp.Models;
using FileComparerApp.Services;
using FileComparerApp.Utils;
using FileComparerApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileComparerApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // leftPath
        private string _leftPath = string.Empty;
        public string LeftPath
        {
            get => _leftPath;
            set
            {
                // Log _leftPath change
                Util.Log($"LeftPath changed from '{_leftPath}' to '{value}'");

                if (_leftPath != value)
                {
                    _leftPath = value;
                    OnPropertyChanged(nameof(LeftPath));
                    // raise compare button
                    ((RelayCommand)CompareCommand).RaiseCanExecuteChanged();
                }
            }
        }

        // rightPath
        private string _rightPath = string.Empty;
        public string RightPath
        {
            get => _rightPath;
            set
            {
                // Log _rightPath change
                Util.Log($"RightPath changed from '{_rightPath}' to '{value}'");

                if (_rightPath != value)
                {
                    _rightPath = value;
                    OnPropertyChanged(nameof(RightPath));
                    // raise compare button
                    ((RelayCommand)CompareCommand).RaiseCanExecuteChanged();
                }
            }
        }

        // ObservableCollection: compare tree
        public ObservableCollection<ICompareNode> CompareTree { get; set; } = new ObservableCollection<ICompareNode>();

        // selected compare node
        private CompareMode? _selectedMode;
        public CompareMode? SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (_selectedMode != value)
                {
                    _selectedMode = value;
                    OnPropertyChanged(nameof(SelectedMode));
                }
            }
        }

        // selected compare modes
        private ICompareNode? _selectedNode;
        public ICompareNode? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode != value)
                {
                    _selectedNode = value;
                    OnPropertyChanged(nameof(SelectedNode));

                    if (value is FileNode fileNode)
                    {
                        ShowDiffViewer(fileNode);
                    }
                }
            }
        }

        private void ShowDiffViewer(FileNode fileNode)
        {
            // DiffViewer view model
            var diffViewModel = new DiffViewerViewModel();

            diffViewModel.Load(fileNode.LeftPath, fileNode.RightPath);

            // create DiffViewer view
            var diffViewer = new DiffViewerWindow
            {
                DataContext = diffViewModel,
                Owner = Application.Current.MainWindow
            };
            diffViewer.Show();

        }

        public IEnumerable<CompareMode> CompareModes => Enum.GetValues(typeof(CompareMode)).Cast<CompareMode>();


        // compare command
        public ICommand CompareCommand { get; }

        /**
         * constructor - Initializes the MainViewModel.
         */
        public MainViewModel()
        {
            CompareCommand = new RelayCommand(async () => await CompareAsync(), CanCompare);
            // selectedMode = CompareMode.Text;
            SelectedMode = CompareMode.Text; // Default mode
        }

        private async Task CompareAsync()
        {

            var dialog = new ProgressDialog
            {
                Owner = Application.Current.MainWindow
            };

            try
            {

                dialog.Show();

                // async load tree from left path
                var leftNode = await Task.Run(() => TreeLoader.LoadFromPath(LeftPath));
                // async load tree from right path
                var rightNode = await Task.Run(() => TreeLoader.LoadFromPath(RightPath));
                // create compare strategy
                var strategy = CompareStrategyFactory.Create(SelectedMode);
                // async compare
                var result = await Task.Run(() => strategy.Compare(leftNode, rightNode));

                // clear compare tree
                CompareTree.Clear();
                // add result to compare tree
                CompareTree.Add(result.RootNode); // 可支援多樹根時用 AddRange

            }
            catch (Exception ex)
            {
                dialog.Close();

                // show message box with error
                MessageBox.Show($"比對失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dialog.Close();

                // Log completion of comparison
                Util.Log("Comparison completed.");
            }

        }

        private bool CanCompare()
        {
            // Log Dierectory.Exists check
            Util.Log($"Checking if directories exist: LeftPath='{LeftPath}', RightPath='{RightPath}'");
            bool leftExists = Directory.Exists(LeftPath);
            bool rightExists = Directory.Exists(RightPath);
            Util.Log($"Directory.Exists results: LeftExists={leftExists}, RightExists={rightExists}");

            return Directory.Exists(LeftPath) && Directory.Exists(RightPath);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
