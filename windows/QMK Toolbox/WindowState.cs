using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
                this._autoFlashEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _canFlash = false;
        public bool CanFlash
        {
            get => this._canFlash;
            set
            {
                this._canFlash = value;
                OnPropertyChanged();
            }
        }

        private bool _canReset = false;
        public bool CanReset
        {
            get => this._canReset;
            set
            {
                this._canReset = value;
                OnPropertyChanged();
            }
        }

        private bool _canClearEeprom = false;
        public bool CanClearEeprom
        {
            get => this._canClearEeprom;
            set
            {
                this._canClearEeprom = value;
                OnPropertyChanged();
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
