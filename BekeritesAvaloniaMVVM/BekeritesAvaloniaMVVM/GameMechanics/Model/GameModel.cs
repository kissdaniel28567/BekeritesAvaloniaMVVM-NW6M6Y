using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameMechanics.Persistence;
using System.Drawing;

namespace GameMechanics.Model
{ //view csak viewmodellel kommunikál!!
    public class GameModel {
        #region Fields
        private List<(int, int)> Steps = new List<(int, int)>();
        #endregion

        #region Properties
        public List<Player> Players { get; private set; }
        public int[][] _field { get; private set; }
        public int _nextPLayer { get; private set; }
        //public bool _isNuget { get; private set; }
        public int TableSize {
            get => _field.Length;
        }
        private IGameAccess _dataAccess;
        #region Events

        public event EventHandler<int[][]>? TableChanged;
        public event EventHandler<String>? GameEnded;
        public event EventHandler<int>? TableReady;
        public event EventHandler<PointsEventArgs>? PointsChanged;
        public event EventHandler<PlayerEventArgs>? PlayerChanged;
        #endregion

        //minek eltárolni? Elég lenne egy function
        //public Player currentPlayer { get; private set; }
        //<--------------!!!!!!!!!!!!!!!!!!!!!!!!!!!!FONTOS. LOAD FGV!!!!!!!!!!!!!!!!!!!!!!!!!!!!-------------->
        #endregion

        #region Constructor
        //console tester
        /*public Game(int[][] field) {
            this.field = field;
            Players = new List<Player>();
            nextPLayer = 0;
            isNuget = false;
        }*/

        public GameModel(IGameAccess dataAccess) {
            _dataAccess = dataAccess;
            this._field = new int[8][];
            Players = new List<Player>();
            _nextPLayer = 0;
            //_isNuget = false;
        }

        

        //public GameModel(int size, String PlayerOneName, String player1Color, String PlayerTwoName, String player2Color) {
        //    Players = new List<Player>();
        //    Players.Add(new Player(0, Color.FromName(player1Color), PlayerOneName));
        //    Players.Add(new Player(0, Color.FromName(player2Color), PlayerTwoName));
        //    _field = new int[size][];
        //    for (int i = 0; i < size; i++) {
        //        _field[i] = Enumerable.Repeat(0, size).ToArray();
        //    }
        //    _nextPLayer = 0;
        //    OnTableReady();
        //    OnTableChanged();
        //}
        #endregion

        #region Public methods

        public void NewGame(int size, String? PlayerOneName = null , String? playerOneColor = null, String? PlayerTwoName = null, String? playerTwoColor = null) {
            if (playerOneColor != null && PlayerOneName != null && PlayerTwoName != null && playerTwoColor != null) {
                //playervalidator
                Players.Clear();
                Players.Add(new Player(0, Color.FromName(playerOneColor), PlayerOneName));
                Players.Add(new Player(0, Color.FromName(playerTwoColor), PlayerTwoName));
                OnPointsChanged();
                OnPlayerChanged();
            } else {
                if (Players.Count == 0) throw new ArgumentException();
                foreach (Player player in Players) {
                    player.ClearPoints();
                    OnPointsChanged();
                }
            }
            _field = new int[size][];
            for (int i = 0; i < size; i++) {
                _field[i] = Enumerable.Repeat(0, size).ToArray();
            }
            OnTableReady();
            OnTableChanged();
        }


        public async Task LoadGameAsync(String path) {
            //DONT KNOW IF THIS WiLL WORK
            try {
                (_field, Players) = await _dataAccess.LoadAsync(path);
            } catch (Exception) {
                throw new IOException();
            }
            if (Players.Count != 2 && Players.Where(player => player.Name == null).Count() > 0) throw new Exception();
            OnTableReady();
            OnTableChanged();
            OnPlayerChanged();
            OnPointsChanged();
            EndGame();
        }
        
        public async Task LoadGameAsync(Stream stream) {
            //DONT KNOW IF THIS WiLL WORK
            try {
                (_field, Players) = await _dataAccess.LoadAsync(stream);
            } catch (Exception) {
                throw new IOException();
            }
            if (Players.Count != 2 && Players.Where(player => player.Name == null).Count() > 0) throw new Exception();
            OnTableReady();
            OnTableChanged();
            OnPlayerChanged();
            OnPointsChanged();
            EndGame();
        }

        public async Task SaveGameAsync(String path) {
            if (Players.Count != 2 && (_field == null || _field.GetLength(1) < 1)) throw new DataMisalignedException();

            await _dataAccess.SaveAsync(path, _field, Players);
        }
        
        public async Task SaveGameAsync(Stream stream) {
            if (Players.Count != 2 && (_field == null || _field.GetLength(1) < 1)) throw new DataMisalignedException();

            await _dataAccess.SaveAsync(stream, _field, Players);
        }

        //public void Clear(int size) {
        //    _field = new int[size][];
        //    for (int i = 0; i < size; i++) {
        //        _field[i] = Enumerable.Repeat(0, size).ToArray();
        //    }
        //    OnTableReady();
        //    OnTableChanged();
        //    foreach (Player player in Players) {
        //        player.ClearPoints();
        //        OnPointsChanged();
        //    }
        //}

        public int Step(int x, int y) {
            Steps.Add((x, y));
            if(Steps.Count == 2) {
                int x1 = Steps[0].Item1;
                int y1 = Steps[0].Item2;
                int x2 = Steps[1].Item1;
                int y2 = Steps[1].Item2;
                Steps.Clear();

                bool areAdjacent = (Math.Abs(x1 - x2) == 1 && y1 == y2) || (Math.Abs(y1 - y2) == 1 && x1 == x2);

                if (areAdjacent && _field[x1][y1] == 0 && _field[x2][y2] == 0) {
                    _field[x1][y1] = _nextPLayer + 1;
                    _field[x2][y2] = _nextPLayer + 1;
                    DetectAndFillRectangle(_nextPLayer + 1);
                    Players[_nextPLayer].AddToPoint();
                    Players[_nextPLayer].AddToPoint();
                    OnPointsChanged();

                    EndGame();

                    _nextPLayer++;
                    _nextPLayer %= 2;
                    return (_nextPLayer + 1) % 2 +  1;
                } else {
                    OnTableChanged();
                    throw new ArgumentException();
                }
            } else {
                return _nextPLayer + 1;
            }
        }

        public Color PlayerColor(int playerId) {
            return Players[playerId].Color;
        }

        public bool EndGame() {
            for (int row = 0; row < _field.Length; row++) {
                for (int col = 0; col < _field[row].Length; col++) {
                    if (_field[row][col] == 0) {
                        if (col + 1 < _field[row].Length && _field[row][col + 1] == 0) {
                            Console.WriteLine("Adjacent empty spaces found at: (" + row + ", " + col + ") and (" + row + ", " + (col + 1) + ")");
                            return false;
                        }

                        if (row + 1 < _field.Length && _field[row + 1][col] == 0) {
                            Console.WriteLine("Adjacent empty spaces found at: (" + row + ", " + col + ") and (" + (row + 1) + ", " + col + ")");
                            return false;
                        }
                    }
                }
            }
            OnGameEnded();
            return true;
        }

        private String Winner() {
            bool arePointsEqual = Players.Select(player => player.Points)
                             .Aggregate((acc, point) => acc == point ? acc : -1) != -1;
            if (arePointsEqual) {
                return "Draw";
            }
            return Players.Where(player => player.Points == Players.Max(player => player.Points)).First().Name;
        }

        #endregion

        #region Private methods
        //checks edges
        // ide kell az esemény tábla változásakor!!!
        // majd az endgame-hez
        private void DetectAndFillRectangle(int playerId) {
            for (int topRow = 0; topRow < _field.Length; topRow++) {
                for (int leftCol = 0; leftCol < _field[topRow].Length; leftCol++) {
                    if (_field[topRow][leftCol] == playerId) {
                        for (int bottomRow = topRow + 1; bottomRow < _field.Length; bottomRow++) {
                            for (int rightCol = leftCol + 1; rightCol < _field[bottomRow].Length; rightCol++) {
                                if (_field[bottomRow][leftCol] == playerId &&
                                    _field[topRow][rightCol] == playerId &&
                                    _field[bottomRow][rightCol] == playerId) {
                                    if (IsPerimeterFilled(topRow, leftCol, bottomRow, rightCol, playerId)) {
                                        FillRectangle(topRow, leftCol, bottomRow, rightCol, playerId);
                                        //Ebbe kell az event hívás
                                        OnTableChanged();
                                    }
                                } 
                            }
                        }
                    } 
                }
            }
        }

        //checks sides
        private bool IsPerimeterFilled(int topRow, int leftCol, int bottomRow, int rightCol, int playerId) {
            for (int col = leftCol; col <= rightCol; col++) {
                if (_field[topRow][col] != playerId || _field[bottomRow][col] != playerId) {
                    return false;
                }
            }

            for (int row = topRow; row <= bottomRow; row++) {
                if (_field[row][leftCol] != playerId || _field[row][rightCol] != playerId) {
                    return false;
                }
            }
            return true;
        }

        // Filelr
        private void FillRectangle(int topRow, int leftCol, int bottomRow, int rightCol, int playerId) {
            for (int i = topRow; i <= bottomRow; i++) {
                for (int j = leftCol; j <= rightCol; j++) {
                    if(_field[i][j] != playerId) {
                        _field[i][j] = playerId;
                        Players[playerId - 1].AddToPoint();
                        OnPointsChanged();
                    }
                    
                }
            }
        }
        #endregion

        #region Handlers
        private void OnGameEnded() {
            GameEnded?.Invoke(this, Winner());
        }
        private void OnPlayerChanged() {
            PlayerChanged?.Invoke(this,new PlayerEventArgs(Players[0].Name, Players[1].Name, Players[0].Color.Name, Players[1].Color.Name));   
        }
        private void OnTableChanged() {
            TableChanged?.Invoke(this, _field);
        }
        private void OnPointsChanged() {
            PointsChanged?.Invoke(this, new PointsEventArgs(Players[0].Points, Players[1].Points));
        }
        private void OnTableReady() {
            TableReady?.Invoke(this, _field.Length);
        }
        #endregion
    }
}
