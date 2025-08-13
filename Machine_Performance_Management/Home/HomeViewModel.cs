using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Machine_Performance_Management.Home
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private BitmapImage _imageSource;

        public BitmapImage ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        public HomeViewModel()
        {
            LoadImage();
        }

        private void LoadImage()
        {
            ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/partron.png")); // Thay đổi đường dẫn cho đúng
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
