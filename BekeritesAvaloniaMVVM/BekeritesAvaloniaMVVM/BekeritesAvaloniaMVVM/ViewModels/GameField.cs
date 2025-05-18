using CommunityToolkit.Mvvm.Input;

namespace BekeritesAvaloniaMVVM.ViewModels {
    public class GameField : ViewModelBase {
        private bool _isLocked;
        private bool _isPlayerOneColor;
        private bool _isPlayerTwoColor;

        public bool IsPlayerTwoColor {
            get { return _isPlayerTwoColor; }
            set {
                _isPlayerTwoColor = value;
                OnPropertyChanged();
            }
        }


        public bool IsPlayerOneColor {
            get { return _isPlayerOneColor; }
            set {
                _isPlayerOneColor = value;
                OnPropertyChanged();
            }
        }


        public bool IsLocked {
            get { return _isLocked; }
            set {
                if (_isLocked != value) {
                    _isLocked = value;
                    OnPropertyChanged();
                }
            }
        }

        public int X { get; set; }
        public int Y { get; set; }
        public (int, int) XY {
            get { return new(X, Y); }
        }

        public RelayCommand<(int, int)>? StepCommand { get; set; }
    }
}
