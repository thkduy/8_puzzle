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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace _8_puzzel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer _timer;
        TimeSpan _time;
        public MainWindow()
        {
            InitializeComponent();
        }

        Image [,] _images;
        bool _isDragging = false;
        Image _selectedBitmap = null;
        Point _lastPosition;
        Point _currentIndexNoneImage;
        Point _selectedIndex;
        double startLeft, startTop;
        int[] dong = { -1, 1, 0, 0 };
        int[] cot = { 0, 0, -1, 1 };
        const int sizeX = 3;
        const int sizeY = 3;
        bool inGame = false;
        bool chooseImage = false;
        bool isShuffle = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _images = new Image[sizeX, sizeY];
            _time = TimeSpan.FromSeconds(180);
        }


        private void Load_Click(object sender, RoutedEventArgs e)
        {
            //Load...

            //Sau khi load
            _timer.Stop();
            TimerCountDown.Text = "00:03:00";
            _time = TimeSpan.FromSeconds(180);
            MessageBox.Show("button Load clicked");
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _time = _time - TimeSpan.FromSeconds(0);
            MessageBox.Show("button Help clicked");
            _timer.Start();   //Sau khi xem help
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("New Game?", "Notice", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    _timer.Stop();
                    TimerCountDown.Text = "00:03:00";
                    _time = TimeSpan.FromSeconds(180);
                    inGame = false;
                    btnPlay.Visibility = Visibility.Visible;
                    btnPause.Visibility = Visibility.Hidden;
                    chooseImage = false;
                    var none = new BitmapImage(new Uri("/Images/none.png", UriKind.Relative));
                    previewImage.Width = 360;
                    previewImage.Height = 230;
                    previewImage.Source = none;
                    for (int i = 0; i < sizeX; i++)
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            gamefieldCanvas.Children.Remove(_images[i, j]);
                            _images[i, j] = null;
                        }
                    }
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _time = _time - TimeSpan.FromSeconds(0);
            MessageBox.Show("button Save clicked");
            _timer.Start();   //Sau khi save - tiep tuc
        }

        private void BtnChooseImg_Click(object sender, RoutedEventArgs e)
        {
            if (chooseImage == false)
            {
                var screen = new OpenFileDialog();

                if (screen.ShowDialog() == true)
                {
                    chooseImage = true;
                    var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border //223
                    var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border //149

                    var source = new BitmapImage(new Uri(screen.FileName, UriKind.Absolute));
                    previewImage.Source = source;

                    Canvas.SetLeft(previewImage, 0);
                    Canvas.SetTop(previewImage, 0);

                    // Bat dau cat thanh 9 manh
                    var h = (int)(source.Height / sizeX);
                    var w = (int)(source.Width / sizeY);

                    for (int i = 0; i < sizeX; i++)
                    {
                        for (int j = 0; j < sizeY; j++)
                        {

                            if (!((i == sizeX - 1) && (j == sizeY - 1)))
                            {
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
                                _images[i, j] = cropImage; // tham chiếu tới crop image
                                gamefieldCanvas.Children.Add(cropImage);
                                Canvas.SetLeft(cropImage, j * (width + 2));
                                Canvas.SetTop(cropImage, i * (height + 2));

                                if (i == 0 && j == 0)
                                {
                                    startLeft = Canvas.GetLeft(cropImage);
                                    startTop = Canvas.GetTop(cropImage);
                                }

                                cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                                cropImage.MouseLeftButtonUp += CropImage_MouseLeftButtonUp;
                                cropImage.MouseMove += CropImage_MouseMove;
                            }
                        }
                    }
                    _currentIndexNoneImage.X = sizeX - 1;
                    _currentIndexNoneImage.Y = sizeY - 1;

                    _selectedIndex.X = -1;
                    _selectedIndex.Y = -1;
                }
            }
            else
            {
                MessageBox.Show("You have selected the photo. Please click new game");
            }
        }

        private void CropImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!inGame)
            {
                return;
            }

            var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border
            var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border
            var position = e.GetPosition(gamefieldCanvas);

            int i = ((int)position.Y) / height;
            int j = ((int)position.X) / width;

            //this.Title = $"{position.X} - {position.Y}, a[{i}][{j}]";

            if (_isDragging && i > -1 && i < sizeX && j > -1 && j < sizeY)
            {
                var dx = position.X - _lastPosition.X;
                var dy = position.Y - _lastPosition.Y;

                var lastLeft = Canvas.GetLeft(_selectedBitmap);
                var lastTop = Canvas.GetTop(_selectedBitmap);
                if(lastLeft + dx <= (sizeY - 1) * (width + 2) && lastLeft + dx >= startLeft )
                    Canvas.SetLeft(_selectedBitmap, lastLeft + dx);
                if(lastTop + dy <= (sizeX - 1) * (height + 2) && lastTop + dy >= startTop)
                    Canvas.SetTop(_selectedBitmap, lastTop + dy);

                _lastPosition = position;
            }
        }
       
        private void CropImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!inGame)
            {
                MessageBox.Show("Game has not been started");
                return;
            }

            if (_selectedIndex.X == -1)
            {
                return;
            }
            var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border
            var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border

            //i là dòng, j là cột
            // lấy vị trí trong mảng của hình vừa được chọn
            //var image = sender as Image;
            //var (i, j) = _images[(int)_selectedIndex.X,(int)_selectedIndex.Y].Tag as Tuple<int, int>;
            //this.Title = $"a[{i}][{j}]";

            //this.Title = $"last position {_lastPosition.X} - {_lastPosition.Y}";

            //MessageBox.Show($"selected: {_selectedPosition.X} - {_selectedPosition.Y} current:{_currentIndexNoneImage.X} - {_currentIndexNoneImage.Y}");
            //this.Title = $"a[{i}][{j}]";

            //kiểm tra hợp lệ mới cho chuyển vị trí hình (trên, dưới, trái, phải của hình none)
            int last_i = ((int)_lastPosition.Y) / height;
            int last_j = ((int)_lastPosition.X) / width;

            bool verifyIndex = false;

            if (_currentIndexNoneImage.X == last_i && _currentIndexNoneImage.Y == last_j)
            {
                for (var k = 0; k < 4; k++)
                {
                    if ((_currentIndexNoneImage.X + dong[k]) == _selectedIndex.X && (_currentIndexNoneImage.Y + cot[k]) == _selectedIndex.Y)
                    {
                        verifyIndex = true;
                        break;
                    }
                }
            }

            if (e.GetPosition(this).X > gamefieldCanvas.ActualWidth || e.GetPosition(this).Y > gamefieldCanvas.ActualHeight)
                verifyIndex = false;

            //nếu không hợp lệ thì không cho đổi vị trí mà để lại vị trí cũ
            if (!verifyIndex)
            { 
                _isDragging = false;
                Canvas.SetLeft(_selectedBitmap, _selectedIndex.Y * (width + 2));
                Canvas.SetTop(_selectedBitmap, _selectedIndex.X * (height + 2));
                //this.Title = $"lai vi tri cu! selected: {_selectedIndex.X} - {_selectedIndex.Y} current:{_currentIndexNoneImage.X} - {_currentIndexNoneImage.Y} a[{_selectedBitmap.Tag}]";
                //reset
                _selectedBitmap = null;
                _selectedIndex.X = -1;
                _selectedIndex.Y = -1;
            }
            else
            {
                _isDragging = false;
                //var position = e.GetPosition(gamefieldCanvas);
                
                //MessageBox.Show($" x,y {newI}-{newJ}");
                //set vị trí mới cho hình trên giao diện
                Canvas.SetLeft(_selectedBitmap, _currentIndexNoneImage.Y * (width + 2));
                Canvas.SetTop(_selectedBitmap, _currentIndexNoneImage.X * (height + 2));

                //thay đổi trong _images
                _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y] = _selectedBitmap;
                _images[(int)_selectedIndex.X, (int)_selectedIndex.Y] = null;

                //doi lai vi tri cua o trong hien tai
                _currentIndexNoneImage.X = _selectedIndex.X;
                _currentIndexNoneImage.Y = _selectedIndex.Y;

                //MessageBox.Show($"current:{_currentIndexNoneImage.X} - {_currentIndexNoneImage.Y}");
                //this.Title = $"chuyen vi tri moi  selected: {_selectedIndex.X} - {_selectedIndex.Y} current:{_currentIndexNoneImage.X} - {_currentIndexNoneImage.Y} a[{_selectedBitmap.Tag}]";
                _selectedIndex.X = -1;
                _selectedIndex.Y = -1;
                //this.Title = $"a[{_currentIndexNoneImage.X}][{_currentIndexNoneImage.Y}]";
            }
            
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!inGame)
            {
                return;
            }

            var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border
            var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border

            if (_selectedBitmap != null)
            {
                if (e.GetPosition(this).X > gamefieldCanvas.ActualWidth || e.GetPosition(this).Y > gamefieldCanvas.ActualHeight)
                {
                    _isDragging = false;
                    if (_selectedIndex.X != -1)
                    {
                        Canvas.SetLeft(_selectedBitmap, _selectedIndex.Y * (width + 2));
                        Canvas.SetTop(_selectedBitmap, _selectedIndex.X * (height + 2));
                        //this.Title = $"lai vi tri cu! selected: {_selectedIndex.X} - {_selectedIndex.Y} current:{_currentIndexNoneImage.X} - {_currentIndexNoneImage.Y} a[{_selectedBitmap.Tag}]";
                        //reset
                        _selectedBitmap = null;
                        _selectedIndex.X = -1;
                        _selectedIndex.Y = -1;
                    }
                }
            }
        }

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!inGame)
            {
                MessageBox.Show("Game has not been started");
                return;
            }

            var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border
            var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border
            _isDragging = true;
            _selectedBitmap = sender as Image;

            gamefieldCanvas.Children.Remove(_selectedBitmap);// remove image of canvas
            gamefieldCanvas.Children.Add(_selectedBitmap);
            _lastPosition = e.GetPosition(gamefieldCanvas);
            var newSelect_X = ((int)_lastPosition.Y) / height;
            var newSelect_Y = ((int)_lastPosition.X) / width;

           
            if (newSelect_X > -1 && newSelect_X < sizeX)
            {
                _selectedIndex.X = newSelect_X;
            }

            if (newSelect_Y > -1 && newSelect_Y < sizeY)
            {
                _selectedIndex.Y = newSelect_Y;
            }
            
            //this.Title = $"selected: [{_selectedIndex.X}][{_selectedIndex.Y}]";
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            inGame = false;
            btnPlay.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Hidden;
            MessageBox.Show("button Pause clicked");
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to exit?", "Notice", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Environment.Exit(1);
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (inGame == true)
            {
                var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border
                var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border
                if (_currentIndexNoneImage.Y + 1 < sizeY)
                {
                    _selectedBitmap = _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y + 1];

                    //trượt
                    var animation = new DoubleAnimation();
                    animation.From = (_currentIndexNoneImage.Y + 1)*(width + 2);
                    animation.To = _currentIndexNoneImage.Y * (width + 2);
                    animation.Duration = new Duration(TimeSpan.FromSeconds(0.3));

                    var story = new Storyboard();
                    story.Children.Add(animation);
                    Storyboard.SetTarget(animation, _selectedBitmap);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));
                    story.Begin(this);

                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y] = _selectedBitmap;
                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y + 1] = null;

                    _currentIndexNoneImage.Y += 1;
                }
            }
            else
            {
                MessageBox.Show("Game has not been started");
            }
        }

        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            if (inGame == true)
            {
                var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border
                var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border
                if (_currentIndexNoneImage.Y - 1 >= 0)
                {
                    _selectedBitmap = _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y - 1];

                    //trượt
                    var animation = new DoubleAnimation();
                    animation.From = (_currentIndexNoneImage.Y - 1) * (width + 2);
                    animation.To = _currentIndexNoneImage.Y * (width + 2);
                    animation.Duration = new Duration(TimeSpan.FromSeconds(0.3));

                    var story = new Storyboard();
                    story.Children.Add(animation);
                    Storyboard.SetTarget(animation, _selectedBitmap);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));
                    story.Begin(this);

                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y] = _selectedBitmap;
                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y - 1] = null;

                    _currentIndexNoneImage.Y -= 1;
                }
            }
            else
            {
                MessageBox.Show("Game has not been started");
            }
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (inGame == true)
            {
                var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border
                var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border
                if (_currentIndexNoneImage.X - 1 >= 0)
                {
                    _selectedBitmap = _images[(int)_currentIndexNoneImage.X - 1, (int)_currentIndexNoneImage.Y];

                    //trượt
                    var animation = new DoubleAnimation();
                    animation.From = (_currentIndexNoneImage.X - 1) * (height + 2);
                    animation.To = _currentIndexNoneImage.X * (height + 2);
                    animation.Duration = new Duration(TimeSpan.FromSeconds(0.3));

                    var story = new Storyboard();
                    story.Children.Add(animation);
                    Storyboard.SetTarget(animation, _selectedBitmap);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.TopProperty));
                    story.Begin(this);

                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y] = _selectedBitmap;
                    _images[(int)_currentIndexNoneImage.X - 1, (int)_currentIndexNoneImage.Y] = null;

                    _currentIndexNoneImage.X -= 1;
                }
            }
            else
            {
                
                MessageBox.Show("Game has not been started");
            }
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (inGame == true)
            {
                var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border
                var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border
                if (_currentIndexNoneImage.X + 1 < sizeX)
                {
                    _selectedBitmap = _images[(int)_currentIndexNoneImage.X + 1, (int)_currentIndexNoneImage.Y];

                    //trượt
                    var animation = new DoubleAnimation();
                    animation.From = (_currentIndexNoneImage.X + 1) * (height + 2);
                    animation.To = _currentIndexNoneImage.X * (height + 2);
                    animation.Duration = new Duration(TimeSpan.FromSeconds(0.3));

                    var story = new Storyboard();
                    story.Children.Add(animation);
                    Storyboard.SetTarget(animation, _selectedBitmap);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.TopProperty));
                    story.Begin(this);

                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y] = _selectedBitmap;
                    _images[(int)_currentIndexNoneImage.X + 1, (int)_currentIndexNoneImage.Y] = null;

                    _currentIndexNoneImage.X += 1;
                }
            }
            else
            {
                MessageBox.Show("Game has not been started");
            }
        }
     
        private bool CheckWinState()
        {

            return (!gamefieldCanvas.Children.Contains(_images[2, 2]));  //Nếu thắng - true. Chưa thắng - false
          
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (chooseImage == true)
            {
                inGame = true;
                btnPlay.Visibility = Visibility.Hidden;
                btnPause.Visibility = Visibility.Visible;
                //MessageBox.Show("button play clicked");
                //tráo đổi
                if (!isShuffle)
                {
                    var width = (int)(gamefieldCanvas.ActualWidth / sizeY);//tru di do rong cua border
                    var height = (int)(gamefieldCanvas.ActualHeight / sizeX) - 1;//tru di do rong cua border
                    var i = 0;
                    Random rnd = new Random();
                    while (i < 100)
                    {
                        switch (rnd.Next(1, 5)) // creates a number between 1 and 4
                        {
                            case 1: //left
                                if (_currentIndexNoneImage.Y + 1 < sizeY)
                                {
                                    _selectedBitmap = _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y + 1];

                                    Canvas.SetLeft(_selectedBitmap, _currentIndexNoneImage.Y * (width + 2));
                                    Canvas.SetTop(_selectedBitmap, _currentIndexNoneImage.X * (height + 2));

                                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y] = _selectedBitmap;
                                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y + 1] = null;

                                    _currentIndexNoneImage.Y += 1;
                                }
                                break;
                            case 2:
                                if (_currentIndexNoneImage.Y - 1 >= 0)
                                {
                                    _selectedBitmap = _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y - 1];

                                    Canvas.SetLeft(_selectedBitmap, _currentIndexNoneImage.Y * (width + 2));
                                    Canvas.SetTop(_selectedBitmap, _currentIndexNoneImage.X * (height + 2));

                                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y] = _selectedBitmap;
                                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y - 1] = null;

                                    _currentIndexNoneImage.Y -= 1;
                                }
                                break;
                            case 3:
                                if (_currentIndexNoneImage.X - 1 >= 0)
                                {
                                    _selectedBitmap = _images[(int)_currentIndexNoneImage.X - 1, (int)_currentIndexNoneImage.Y];

                                    Canvas.SetLeft(_selectedBitmap, _currentIndexNoneImage.Y * (width + 2));
                                    Canvas.SetTop(_selectedBitmap, _currentIndexNoneImage.X * (height + 2));

                                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y] = _selectedBitmap;
                                    _images[(int)_currentIndexNoneImage.X - 1, (int)_currentIndexNoneImage.Y] = null;

                                    _currentIndexNoneImage.X -= 1;
                                }
                                break;
                            case 4:
                                if (_currentIndexNoneImage.X + 1 < sizeX)
                                {
                                    _selectedBitmap = _images[(int)_currentIndexNoneImage.X + 1, (int)_currentIndexNoneImage.Y];

                                    Canvas.SetLeft(_selectedBitmap, _currentIndexNoneImage.Y * (width + 2));
                                    Canvas.SetTop(_selectedBitmap, _currentIndexNoneImage.X * (height + 2));

                                    _images[(int)_currentIndexNoneImage.X, (int)_currentIndexNoneImage.Y] = _selectedBitmap;
                                    _images[(int)_currentIndexNoneImage.X + 1, (int)_currentIndexNoneImage.Y] = null;

                                    _currentIndexNoneImage.X += 1;
                                }
                                break;

                        }
                        i++;
                    }
                    isShuffle = !isShuffle;
                }

                _time = _time - TimeSpan.FromSeconds(0);
                _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                {
                    TimerCountDown.Text = _time.ToString("c");
                    if (_time == TimeSpan.Zero)
                    {
                        _timer.Stop();
                        MessageBox.Show("You Loose !");
                        MessageBoxResult result = MessageBox.Show("New Game?", "Notice", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                inGame = false;
                                chooseImage = false;
                                var none = new BitmapImage(new Uri("/Images/none.png", UriKind.Relative));

                                previewImage.Width = 360;
                                previewImage.Height = 230;
                                previewImage.Source = none;
                                for (int i = 0; i < sizeX; i++)
                                {
                                    for (int j = 0; j < sizeY; j++)
                                    {
                                        gamefieldCanvas.Children.Remove(_images[i, j]);
                                        _images[i, j] = null;
                                    }
                                }
                                break;
                            case MessageBoxResult.No:
                                break;
                        }
                    }
                    _time = _time.Add(TimeSpan.FromSeconds(-1));
                }, Application.Current.Dispatcher);
                _timer.Start();
            }
            else
            {
               string s=  _images[2, 2].Tag.ToString();
                MessageBox.Show("You have not selected a photo");
            }
        }
    }
}
