using GameMechanics.Model;
using GameMechanics.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;
//using System.Windows.Media;
//using System.Drawing;
using System.Windows;
using Color = System.Drawing.Color;
using MsBox.Avalonia;
using CommunityToolkit.Mvvm.Input;
using Avalonia;
using System.Reflection;

namespace BekeritesAvaloniaMVVM.ViewModels {
    public class MainViewModel : ViewModelBase, IDisposable {

        #region Fields

        private readonly string[] DefaultNames = { "Player One Name", "Player One Color", "Player Two Name", "Player Two Color" };
        private readonly Size GameBoardSize = new Size(400, 400);
        private GameModel? _model;
        private bool _disposed = false;
        private bool _isPlayerInputEnabled;
        private bool _isSizeInputEnabled;
        private int _boardSize;
        private String? _playerOneColor = null;
        private String? _playerTwoColor = null;
        private String? _radioSize = null;

       

        #endregion

        #region Commands
        public RelayCommand StartGame { get; set; }
        public RelayCommand LoadGameCommand { get; set; }
        public RelayCommand SaveGameCommand { get; set; }
        //public RelayCommand ButtonSelectedCommand { get; set; }
        public RelayCommand AddPlayerCommand { get; set; }

        #endregion

        #region Events
        //public event EventHandler<GameField>? ButtonSelected;
        public event EventHandler? SaveGame;
        public event EventHandler? LoadGame;
        public event EventHandler<string>? AddPlayersError;
        public event EventHandler? NewGameError;
        public event EventHandler? SaveGameError;
        public event EventHandler<string>? WinnerPrint;
        public event EventHandler? ButtonError;
        //public event EventHandler? AddPlayer;

        #endregion

        #region Properties
        public ObservableCollection<GameField> GameBoard { get; private set; }
        //public string testLabelText { get; set; }

        public int ButtonSize => 900 / BoardSize - 10;
        public int BoardSize
        {
            get => _boardSize;
            set
            {
                _boardSize = value;
                OnPropertyChanged(nameof(BoardSize));
                OnPropertyChanged(nameof(BoardRows));
                OnPropertyChanged(nameof(BoardColumns));
            }
        }

        public int BoardRows => BoardSize;
        public int BoardColumns => BoardSize;

        public String RadioSize {
            get { return _radioSize ?? ""; }
            set {
                _radioSize = value as string;
                int size = int.Parse(_radioSize.Split("X")[0]);
                if (BoardSize != size) {
                    BoardSize = size;
                }
            }
        }

        public bool IsPlayerInputEnabled {
            get => _isPlayerInputEnabled;
            set {
                _isPlayerInputEnabled = value;
                OnPropertyChanged(nameof(IsPlayerInputEnabled));
            }
        }
        public bool IsSizeInputEnabled {
            get => _isSizeInputEnabled;
            set {
                _isSizeInputEnabled = value;
                OnPropertyChanged(nameof(IsSizeInputEnabled));
            }
        }

        public String? PlayerOneColor {
            get => _playerOneColor;
            set {
                _playerOneColor = value;
                OnPropertyChanged();
            }
        }
        
        public String? PlayerTwoColor {
            get => _playerTwoColor;
            set {
                _playerTwoColor = value;
                OnPropertyChanged();
            }
        }

        public int PlayerOnePoint { get; set; }
        public int PlayerTwoPoint { get; set; }

        public string PlayerOneName { get; set; }
        public string PlayerTwoName { get; set; }


        public ObservableCollection<String> ColorPalette { get; }
        #endregion


        #region public functions

        // Color has been set, user will set the players at the beggining of the game and the player will set the new player onece the game ends
        public MainViewModel(GameModel model) {
            _model = model;
            AddEvents();
            IsPlayerInputEnabled = true;
            IsSizeInputEnabled = true;
            OnPropertyChanged(nameof(IsPlayerInputEnabled));
            OnPropertyChanged(nameof(IsSizeInputEnabled));
            PlayerOneName = DefaultNames[0];
            PlayerOneColor = DefaultNames[1];
            PlayerTwoName = DefaultNames[2];
            PlayerTwoColor = DefaultNames[3];
            RadioSize = "8X8";
            //testLabelText = "fasdlf";
            GameBoard = new ObservableCollection<GameField>();
            ColorPalette = new ObservableCollection<String>(typeof(Color)
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(Color))
                .Select(color => color.Name));

            BoardSize = 8;
            OnPropertyChanged(nameof(ButtonSize));
            StartGame = new RelayCommand(OnStartGameCommand);
            AddPlayerCommand = new RelayCommand(OnAddPLayerCommand);
            SaveGameCommand = new RelayCommand(OnSaveGameCommand);
            LoadGameCommand = new RelayCommand(OnLoadGameCommand);
        }

        public MainViewModel() : this(new GameModel(new GameFileDataAccess())) {

        }

        private void OnLoadGameCommand() {
            if (_model != null) {
                OnLoadGame();
            }
        }

        private void OnLoadGame() {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnSaveGameCommand() {
            if (_model == null) { throw new NullReferenceException(); }
            if (GameBoard.Count > 0) {
                OnSaveGame();

            } else {
                OnSaveGameError();
            }
        }

        private void OnSaveGameError() {
            SaveGameError?.Invoke(this, EventArgs.Empty);
        }

        private void OnAddPLayerCommand() {
            if (DefaultNames.Contains(PlayerOneName) && DefaultNames.Contains(PlayerTwoName)) {
                OnAddPlayersError("Don't leave playername default!");
                return;
            }
            if (DefaultNames.Contains(PlayerOneColor) && DefaultNames.Contains(PlayerTwoColor)) {
                OnAddPlayersError("Don't leave color default!");
                return;
            }
            if(PlayerOneColor == PlayerTwoColor) {
                OnAddPlayersError($"Player One and Two cannot be the same color!");
                return;
            }
            if (string.IsNullOrEmpty(PlayerOneName)) {
            }
            if (string.IsNullOrEmpty(PlayerTwoName)) {
                OnAddPlayersError("Don't leave player2 fields empty please!");
                return;
            }
            IsPlayerInputEnabled = false;
        }

        private void OnStartGameCommand() {
            try {
                if (IsPlayerInputEnabled) {
                    OnNewGameError();
                } else {
                    _model?.NewGame(BoardSize, PlayerOneName, PlayerOneColor, PlayerTwoName, PlayerTwoColor);
                    IsPlayerInputEnabled = false;
                    IsSizeInputEnabled = false;
                    OnPropertyChanged(nameof(IsPlayerInputEnabled));
                    OnPropertyChanged(nameof(IsSizeInputEnabled));
                    //AddEvents()
                }
            } catch (Exception) {
                OnNewGameError();
            }
        }

        private void OnSaveGame() {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnAddPlayersError(string message) {
            AddPlayersError?.Invoke(this, message);
        }

        public void LoadPropertieChange() {
            OnPropertyChanged(nameof(PlayerOneColor));
            OnPropertyChanged(nameof(PlayerTwoColor));
            OnPropertyChanged(nameof(PlayerOneName));
            OnPropertyChanged(nameof(PlayerTwoName));
            OnPropertyChanged(nameof(PlayerOnePoint));
            OnPropertyChanged(nameof(PlayerTwoPoint));
            OnPropertyChanged(nameof(BoardSize));
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region private functions
        private void OnNewGameError() {
            NewGameError?.Invoke(this, EventArgs.Empty);
        }



        private void AddEvents() {
            if (_model == null) throw new NullReferenceException();
            _model.PointsChanged += PointsChanged;
            _model.TableChanged += TableChanged;
            _model.TableReady += TableReady;
            _model.GameEnded += GameEnded;
            _model.PlayerChanged += PlayerChanged;
        }

        private void PlayerChanged(object? sender, PlayerEventArgs e) {
            if (e.PlayerOneName == null || e.PlayerTwoName == null || e.PlayerOneColor == null || e.PlayerTwoColor == null) return;
            PlayerOneName = e.PlayerOneName;
            PlayerTwoName = e.PlayerTwoName;
            OnPropertyChanged(nameof(PlayerOneName));
            OnPropertyChanged(nameof(PlayerTwoName));
            PlayerOneColor = e.PlayerOneColor;
            PlayerTwoColor = e.PlayerTwoColor;
            OnPropertyChanged(nameof(PlayerOneColor));
            OnPropertyChanged(nameof(PlayerTwoColor));
            IsSizeInputEnabled = false;
            IsPlayerInputEnabled = false;
            OnPropertyChanged(nameof(IsSizeInputEnabled));
            OnPropertyChanged(nameof(IsPlayerInputEnabled));
        }

        private void RemoveEvents() {
            if (_model == null) throw new NullReferenceException();
            _model.PointsChanged -= PointsChanged;
            _model.TableChanged -= TableChanged;
            _model.TableReady -= TableReady;
            _model.GameEnded -= GameEnded;
        }

        private void GameEnded(object? sender, string winner) {
            if (_model == null) throw new NullReferenceException();
            if (winner == "Draw") {
                OnWinnerPrint($"Game over. The winner is... Well, both of you");
            } else {
                OnWinnerPrint($"Game over. The winner is {winner}");
            }
            foreach (GameField field in GameBoard) {
                field.IsLocked = true;
            }

            IsPlayerInputEnabled = true;
            IsSizeInputEnabled = true;
            //OnPropertyChanged(nameof(IsPlayerInputEnabled));
            //OnPropertyChanged(nameof(IsSizeInputEnabled));
            ///_model.NewGame(BoardSize);
        }

        private void OnWinnerPrint(string message) {
            WinnerPrint?.Invoke(this, message);
        }

        private void TableReady(object? sender, int tableSize) {
            if (_model == null) throw new NullReferenceException();
            GameBoard.Clear();
            BoardSize = tableSize;
            OnPropertyChanged(nameof(BoardSize));
            for (int i = 0; i < tableSize; i++) {
                for (int j = 0; j < tableSize; j++) {
                    GameField field = new GameField {
                        IsLocked = false,
                        X = i,
                        Y = j,
                        StepCommand = new RelayCommand<(int, int)>(OnStepCommand)
                    };
                    GameBoard.Add(field);
                }
            }
            OnPropertyChanged(nameof(GameBoard));
            OnPropertyChanged(nameof(BoardSize));
            OnPropertyChanged(nameof(ButtonSize));
        }

        private void OnStepCommand((int x, int y) position) {
            StepGame(position.x, position.y);
        }

        private void StepGame(int x, int y) {
            if (_model == null) { throw new NullReferenceException(); }
            try {
                if (_model.Step(x, y) == 1) {
                    GameBoard[y + x * BoardSize].IsPlayerOneColor = true;
                } else {
                    GameBoard[y + x * BoardSize].IsPlayerTwoColor = true;
                }
            } catch (Exception) {
                OnButtonError();
            }
        }

        private void OnButtonError() {
            ButtonError?.Invoke(this, EventArgs.Empty);
        }

        private void TableChanged(object? sender, int[][] field) {
            if (_model == null) throw new NullReferenceException();
            int size = field.GetLength(0);
            for (int i = 0; i < size; i++) {
                for (int j = 0; j < size; j++) {
                    switch (field[i][j]) {
                        case 1:
                            GameBoard[j + i * size].IsPlayerOneColor = true;
                            break;
                        case 2:
                            GameBoard[j + i * size].IsPlayerTwoColor = true;
                            break;
                        default:
                            GameBoard[j + i * size].IsPlayerOneColor = false;
                            GameBoard[j + i * size].IsPlayerTwoColor = false;
                            break;
                    }
                    if (field[i][j] != 0) GameBoard[j + i * size].IsLocked = true;
                }
            }
            OnPropertyChanged(nameof(GameBoard));
        }

        private void PointsChanged(object? sender, PointsEventArgs points) {
            PlayerOnePoint = points.PlayerOnePoint;
            PlayerTwoPoint = points.PlayerTwoPoint;
            OnPropertyChanged(nameof(PlayerOnePoint));
            OnPropertyChanged(nameof(PlayerTwoPoint));
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;

            if (disposing) {
                if (_model != null) {
                    RemoveEvents();
                }
            }
            _disposed = true;
        }

        #endregion
    }
}
