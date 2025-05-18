using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using BekeritesAvaloniaMVVM.ViewModels;
using BekeritesAvaloniaMVVM.Views;
using GameMechanics.Model;
using GameMechanics.Persistence;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using Avalonia.Platform.Storage;
using System.Linq;

namespace BekeritesAvaloniaMVVM;

public partial class App : Application, IDisposable {
    //<<!--------------------Tedd at a messageboxokat-------------------!>> - done
    //<<!----------------Fájl kezelést old meg stream-el----------------!>> - done
    //<<!----------------------Android gomb meret-----------------------!>> - done
    //<<!------------------enemy field is not filling-------------------!>> - done
    #region fields
    private GameModel? _model = null;
    private MainViewModel? _mainViewModel = null;
    private bool _disposed = false;
    private TopLevel? TopLevel {
        get {
            return ApplicationLifetime switch {
                IClassicDesktopStyleApplicationLifetime desktop => TopLevel.GetTopLevel(desktop.MainWindow),
                ISingleViewApplicationLifetime singleViewPlatform => TopLevel.GetTopLevel(singleViewPlatform.MainView),
                _ => null
            };
        }
    }
    #endregion


    #region Initialization
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    private struct Person {
        public int age;
        public String name;
    }

    public override void OnFrameworkInitializationCompleted() {
        _model = new GameModel(new GameFileDataAccess());
        _mainViewModel = new MainViewModel(_model);

        Person[] people = new Person[2];

        //people.Where(p => p.age >= 18).Select(p => p.name).OrderBy(p => p);

        //AddedPlayers = new String[2];
        //Events go here
        //_mainViewModel.ButtonSelected += ButtonSelected;
        _mainViewModel.LoadGame += LoadGame;
        _mainViewModel.SaveGame += SaveGame;
        _mainViewModel.AddPlayersError += AddPlayersError;
        _mainViewModel.NewGameError += NewGameError;
        _mainViewModel.SaveGameError += SaveGameError;
        _mainViewModel.WinnerPrint += WinnerPrint;
        _mainViewModel.ButtonError += ButtonError;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow() {
                DataContext = _mainViewModel
            };
        } else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
            singleViewPlatform.MainView = new MainView() {
                DataContext = _mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    #endregion

    #region Public methods
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Private methods
    private async void ButtonError(object? sender, EventArgs e) {
        await MessageBoxManager.GetMessageBoxStandard(
            "Error!",
            "Click adjament buttons and do not step on the other fields!",
            ButtonEnum.Ok,
            Icon.Error
            ).ShowAsync();
    }

    private async void WinnerPrint(object? sender, string message) {
        await MessageBoxManager.GetMessageBoxStandard(
            "Error!",
            message,
            ButtonEnum.Ok,
            Icon.Error
            ).ShowAsync();
    }

    private async void SaveGameError(object? sender, EventArgs e) {
        await MessageBoxManager.GetMessageBoxStandard(
            "Error!",
            "Cannot save empty game!",
            ButtonEnum.Ok,
            Icon.Error).ShowAsync();
    }

    private async void NewGameError(object? sender, EventArgs e) {
        await MessageBoxManager.GetMessageBoxStandard(
                "Error!",
                "Add players first!",
                ButtonEnum.Ok,
                Icon.Error
            ).ShowAsync();
    }

    private async void AddPlayersError(object? sender, String message) {
        await MessageBoxManager.GetMessageBoxStandard(
            "Error!",
            message,
            ButtonEnum.Ok,
            Icon.Error
            ).ShowAsync();
    }

    private async void SaveGame(object? sender, EventArgs e) {
        //streames megoldas - done
        if (TopLevel == null) {
            await MessageBoxManager.GetMessageBoxStandard(
                    "File reading error!",
                    "File managing is not supported!",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
            return;
        }

        try {
            var file = await TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
                Title = "Save Game",
                FileTypeChoices = new[] {
                        new FilePickerFileType("TXT files") {
                            Patterns = new[] { "*.txt" }
                        }
                    }
            });

            if (file != null) {
                if (_model == null) throw new NullReferenceException();
                using (var stream = await file.OpenWriteAsync()) {
                    await _model.SaveGameAsync(stream);
                    await MessageBoxManager.GetMessageBoxStandard(
                        "Success",
                        "Game has saved successfully!",
                        ButtonEnum.Ok,
                        Icon.Success).ShowAsync();
                }
            } else {
                await MessageBoxManager.GetMessageBoxStandard(
                "File writing error!",
                "No file selected!",
                ButtonEnum.Ok,
                Icon.Error).ShowAsync();
            }
        } catch (Exception) {
            await MessageBoxManager.GetMessageBoxStandard(
                "File writing error!",
                "Something went wrong writing the file!",
                ButtonEnum.Ok,
                Icon.Error).ShowAsync();
        }

    }

    private async void LoadGame(object? sender, EventArgs e) {
        //csereld ki a sudokusra - done
        if (TopLevel == null) {
            await MessageBoxManager.GetMessageBoxStandard(
                    "File reading error!",
                    "File managing is not supported!",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
            return;
        }

        try {
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                Title = "Load Game",
                AllowMultiple = false,
                FileTypeFilter = new[] {
                        new FilePickerFileType("TXT files") {
                            Patterns = new[] { "*.txt" }
                        }
                    }
            });

            if (files.Count > 0) {
                if (_model == null) throw new NullReferenceException();
                using (var stream = await files[0].OpenReadAsync()) {
                    await _model.LoadGameAsync(stream);
                }

                await MessageBoxManager.GetMessageBoxStandard(
                    "Success",
                    "Game has loaded successfully!",
                    ButtonEnum.Ok,
                    Icon.Success).ShowAsync();
            } else {
                await MessageBoxManager.GetMessageBoxStandard(
                "File reading error!",
                "No file selected!",
                ButtonEnum.Ok,
                Icon.Error).ShowAsync();
            }
        } catch (Exception) {
            await MessageBoxManager.GetMessageBoxStandard(
                "File reading error!",
                "Something went wrong reading the file!",
                ButtonEnum.Ok,
                Icon.Error).ShowAsync();
        }
        
    }
    #endregion

    #region Public methods
    protected virtual void Dispose(bool disposing) {
        if (_disposed)
            return;

        if (disposing) {
            if (_mainViewModel != null) {
                _mainViewModel.LoadGame -= LoadGame;
                _mainViewModel.SaveGame -= SaveGame;
                _mainViewModel.Dispose();
            }
        }
        _disposed = true;
    }
    #endregion
}
