using System.ComponentModel;

namespace ArtTool.Core
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private bool toggleSwitch1;
        private bool toggleSwitch2;
        private bool toggleSwitch3;

        public bool ToggleSwitch1
        {
            get { return toggleSwitch1; }
            set
            {
                if (toggleSwitch1 != value)
                {
                    toggleSwitch1 = value;
                    OnPropertyChanged(nameof(ToggleSwitch1));
                }
            }
        }

        public bool ToggleSwitch2
        {
            get { return toggleSwitch2; }
            set
            {
                if (toggleSwitch2 != value)
                {
                    toggleSwitch2 = value;
                    OnPropertyChanged(nameof(ToggleSwitch2));
                }
            }
        }

        public bool ToggleSwitch3
        {
            get { return toggleSwitch3; }
            set
            {
                if (toggleSwitch3 != value)
                {
                    toggleSwitch3 = value;
                    OnPropertyChanged(nameof(ToggleSwitch3));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
