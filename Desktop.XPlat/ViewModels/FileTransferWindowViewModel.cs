﻿using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using nexRemoteFree.Desktop.Core.Interfaces;
using nexRemoteFree.Desktop.Core.Services;
using nexRemoteFree.Desktop.Core.ViewModels;
using nexRemoteFree.Desktop.XPlat.Services;
using nexRemoteFree.Desktop.XPlat.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace nexRemoteFree.Desktop.XPlat.ViewModels
{
    public class FileTransferWindowViewModel : BrandedViewModelBase
    {
        private readonly IFileTransferService _fileTransferService;
        private readonly Viewer _viewer;
        private string _viewerConnectionId;
        private string _viewerName;

        public FileTransferWindowViewModel() { }

        public FileTransferWindowViewModel(
            Viewer viewer,
            IFileTransferService fileTransferService)
        {
            _fileTransferService = fileTransferService;
            _viewer = viewer;
            ViewerName = viewer.Name;
            ViewerConnectionId = viewer.ViewerConnectionID;
        }

        public ObservableCollection<FileUpload> FileUploads { get; } = new ObservableCollection<FileUpload>();

        public ICommand OpenFileUploadDialog => new Executor(async (param) =>
        {
            var initialDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (!Directory.Exists(initialDir))
            {
                initialDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "nex-RemoteFree_Shared")).FullName;
            }

            var ofd = new OpenFileDialog
            {
                Title = "Transfer Plików via nex-RemoteFree",
                AllowMultiple = true,
                Directory = initialDir
            };

            var result = await ofd.ShowAsync(param as FileTransferWindow);
            if (result?.Any() != true)
            {
                return;
            }
            foreach (var file in result)
            {
                if (File.Exists(file))
                {
                    await UploadFile(file);
                }
            }
        });

        public string ViewerConnectionId
        {
            get => _viewerConnectionId;
            set => this.RaiseAndSetIfChanged(ref _viewerConnectionId, value);
        }

        public string ViewerName
        {
            get => _viewerName;
            set => this.RaiseAndSetIfChanged(ref _viewerName, value);
        }

        public async Task UploadFile(string filePath)
        {
            var fileUpload = new FileUpload()
            {
                FilePath = filePath
            };

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                FileUploads.Add(fileUpload);
            });

            await _fileTransferService.UploadFile(fileUpload, _viewer, fileUpload.CancellationTokenSource.Token, async progress =>
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    fileUpload.PercentProgress = progress;
                });
            });
        }

        public ICommand RemoveFileUpload => new Executor((param) =>
        {
            if (param is FileUpload fileUpload)
            {
                FileUploads.Remove(fileUpload);
                fileUpload.CancellationTokenSource.Cancel();
            }
        });
    }
}
