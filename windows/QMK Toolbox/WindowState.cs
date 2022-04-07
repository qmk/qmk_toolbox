using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QMK_Toolbox
{
    class WindowState : INotifyPropertyChanged
    {
        private bool _autoFlashEnabled = false;
        public bool AutoFlashEnabled
        {
            get => _autoFlashEnabled;
            set
            {
                if (_autoFlashEnabled != value)
                {
                    _autoFlashEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _showAllDevices = false;
        public bool ShowAllDevices
        {
            get => _showAllDevices;
            set
            {
                if (_showAllDevices != value)
                {
                    _showAllDevices = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canFlash = false;
        public bool CanFlash
        {
            get => _canFlash;
            set
            {
                if (_canFlash != value)
                {
                    _canFlash = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canReset = false;
        public bool CanReset
        {
            get => _canReset;
            set
            {
                if (_canReset != value)
                {
                    _canReset = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canClearEeprom = false;
        public bool CanClearEeprom
        {
            get => _canClearEeprom;
            set
            {
                if (_canClearEeprom != value)
                {
                    _canClearEeprom = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
