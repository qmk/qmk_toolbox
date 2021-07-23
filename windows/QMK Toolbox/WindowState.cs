using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QMK_Toolbox
{
    class WindowState : INotifyPropertyChanged
    {
        private bool _autoFlashEnabled = false;
        public bool AutoFlashEnabled
        {
            get => this._autoFlashEnabled;
            set
            {
                if (this._autoFlashEnabled != value)
                {
                    this._autoFlashEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canFlash = false;
        public bool CanFlash
        {
            get => this._canFlash;
            set
            {
                if (this._canFlash != value)
                {
                    this._canFlash = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canReset = false;
        public bool CanReset
        {
            get => this._canReset;
            set
            {
                if (this._canReset != value)
                {
                    this._canReset = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canClearEeprom = false;
        public bool CanClearEeprom
        {
            get => this._canClearEeprom;
            set
            {
                if (this._canClearEeprom != value)
                {
                    this._canClearEeprom = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
