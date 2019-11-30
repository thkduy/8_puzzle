using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _8_puzzel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            

            
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Load clicked");
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Help clicked");
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button New clicked");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Save clicked");
        }

        private void BtnChooseImg_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                var width = (int)(gamefieldCanvas.ActualWidth / 3);//tru di do rong cua border
                var height = (int)(gamefieldCanvas.ActualHeight / 3) - 1;//tru di do rong cua border

                var source = new BitmapImage(new Uri(screen.FileName, UriKind.Absolute));
                
                previewImage.Width = 360;
                previewImage.Height = 230;
                previewImage.Source = source;

                Canvas.SetLeft(previewImage, 10);
                Canvas.SetTop(previewImage, 10);

                // Bat dau cat thanh 9 manh

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (!((i == 2) && (j == 2)))
                        {
                            var h = (int)(source.Height / 3);//100
                            var w = (int)(source.Width / 3);//125
                            //MessageBox.Show($"{h}-{w}");
                            //Debug.WriteLine($"Len = {len}");
                            var rect = new Int32Rect(j * w, i * h, w, h);
                            var cropBitmap = new CroppedBitmap(source, rect);

                            var cropImage = new Image();
                            cropImage.Stretch = Stretch.Fill;
                            cropImage.Width = width;
                            cropImage.Height = height;
                            cropImage.Source = cropBitmap;
                            cropImage.Tag = new Tuple<int, int>(i, j);
                            gamefieldCanvas.Children.Add(cropImage);
                            Canvas.SetLeft(cropImage, j * (width + 2));
                            Canvas.SetTop(cropImage, i * (height + 2));
                            cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                            cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
                            cropImage.MouseMove += CropImage_MouseMove;
                        }
                    }
                }
                _currentIndexNoneImage.X = 2;
                _currentIndexNoneImage.Y = 2;

                _selectedPosition.X = -1;
                _selectedPosition.Y = -1;
            }
        }

        private void CropImage_MouseMove(object sender, MouseEventArgs e)
        {
            var width = (int)(gamefieldCanvas.ActualWidth / 3);//tru di do rong cua border
            var height = (int)(gamefieldCanvas.ActualHeight / 3) - 1;//tru di do rong cua border
            var position = e.GetPosition(this);

            int i = ((int)position.Y) / height;
            int j = ((int)position.X) / width;

            this.Title = $"{position.X} - {position.Y}, a[{i}][{j}]";

            if (_isDragging && i > -1 && i < 3 && j > -1 && j < 3)
            {
                var dx = position.X - _lastPosition.X;
                var dy = position.Y - _lastPosition.Y;

                var lastLeft = Canvas.GetLeft(_selectedBitmap);
                var lastTop = Canvas.GetTop(_selectedBitmap);
                Canvas.SetLeft(_selectedBitmap, lastLeft + dx);
                Canvas.SetTop(_selectedBitmap, lastTop + dy);

                _lastPosition = position;
            }
        }

        bool _isDragging = false;
        Image _selectedBitmap = null;
        Point _lastPosition;
        Point _currentIndexNoneImage;
        Point _selectedPosition;
        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(_selectedPosition.X == -1)
            {
                return;
            }
            var width = (int)(gamefieldCanvas.ActualWidth / 3);//tru di do rong cua border
            var height = (int)(gamefieldCanvas.ActualHeight / 3) - 1;//tru di do rong cua border

            //i là dòng, j là cột
            // lấy vị trí trong mảng của hình vừa được chọn
            //var image = sender as Image;
            //var (i, j) = image.Tag as Tuple<int, int>;
            //this.Title = $"a[{i}][{j}]";

            //this.Title = $"last position {_lastPosition.X} - {_lastPosition.Y}";

            //MessageBox.Show($"selected: {_selectedPosition.X} - {_selectedPosition.Y} current:{_currentIndexNoneImage.X} - {_currentIndexNoneImage.Y}");
            //this.Title = $"a[{i}][{j}]";

            //kiểm tra hợp lệ mới cho chuyển vị trí hình (trên, dưới, trái, phải của hình none)
            int last_i = ((int)_lastPosition.Y) / height;
            int last_j = ((int)_lastPosition.X) / width;
            int[] dong = { -1, 1, 0, 0 };
            int[] cot = { 0, 0, -1, 1 };
            bool verifyIndex = false;

            if (_currentIndexNoneImage.X == last_i && _currentIndexNoneImage.Y == last_j)
            {
                for (var k = 0; k < 4; k++)
                {
                    if ((_currentIndexNoneImage.X + dong[k]) == _selectedPosition.X && (_currentIndexNoneImage.Y + cot[k]) == _selectedPosition.Y)
                    {
                        Debug.WriteLine($"{(_currentIndexNoneImage.X + dong[k])} - {_currentIndexNoneImage.Y + cot[k]}\n");
                        verifyIndex = true;
                        break;
                    }
                }
            }

            //nếu không hợp lệ thì không cho đổi vị trí mà để lại vị trí cũ
            if (!verifyIndex)
            { 
                _isDragging = false;
                Canvas.SetLeft(_selectedBitmap, _selectedPosition.Y * (width + 2));
                Canvas.SetTop(_selectedBitmap, _selectedPosition.X * (height + 2));
                this.Title = $"lai vi tri cu! selected: {_selectedPosition.X} - {_selectedPosition.Y} current:{_currentIndexNoneImage.X} - {_currentIndexNoneImage.Y}";
                //reset
                _selectedBitmap = null;
                _selectedPosition.X = -1;
                _selectedPosition.Y = -1;
            }
            else
            {
                _isDragging = false;
                var position = e.GetPosition(this);
                
                //MessageBox.Show($" x,y {newI}-{newJ}");
                //set vị trí mới cho hình trên giao diện
                Canvas.SetLeft(_selectedBitmap, _currentIndexNoneImage.Y * (width + 2));
                Canvas.SetTop(_selectedBitmap, _currentIndexNoneImage.X * (height + 2));

                //doi lai vi tri cua o trong hien tai
                _currentIndexNoneImage.X = _selectedPosition.X;
                _currentIndexNoneImage.Y = _selectedPosition.Y;
                //MessageBox.Show($"current:{_currentIndexNoneImage.X} - {_currentIndexNoneImage.Y}");
                this.Title = $"chuyen vi tri moi  selected: {_selectedPosition.X} - {_selectedPosition.Y} current:{_currentIndexNoneImage.X} - {_currentIndexNoneImage.Y}";
                _selectedPosition.X = -1;
                _selectedPosition.Y = -1;
                //this.Title = $"a[{_currentIndexNoneImage.X}][{_currentIndexNoneImage.Y}]";
            }
            
        }

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var width = (int)(gamefieldCanvas.ActualWidth / 3);//tru di do rong cua border
            var height = (int)(gamefieldCanvas.ActualHeight / 3) - 1;//tru di do rong cua border
            _isDragging = true;
            _selectedBitmap = sender as Image;
            
            _lastPosition = e.GetPosition(this);
            _selectedPosition.X = ((int)_lastPosition.Y) / height;
            _selectedPosition.Y = ((int)_lastPosition.X) / width;
            this.Title = $"selected: [{_selectedPosition.X}][{_selectedPosition.Y}]";
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            btnPlay.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Hidden;
            MessageBox.Show("button Pause clicked");
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Left clicked");
        }

        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Right clicked");
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Down clicked");
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("button Up clicked");
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            btnPlay.Visibility = Visibility.Hidden;
            btnPause.Visibility = Visibility.Visible;
            MessageBox.Show("button play clicked;");
        }
    }
}
