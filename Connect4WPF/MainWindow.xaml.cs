using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Connect4WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GameState gameState = new();
        private readonly Dictionary<Player, ImageSource> imageSources;
        private readonly Image[,] imageControls = new Image[7, 6];

        private readonly DoubleAnimation fadeOutAnimation = new()
        {
            Duration = TimeSpan.FromMilliseconds(600),
            From = 1,
            To = 0
        };

        private readonly DoubleAnimation fadeInAnimation = new()
        {
            Duration = TimeSpan.FromMilliseconds(600),
            From = 0,
            To = 1
        };

        public MainWindow()
        {
            InitializeComponent();

            imageSources = new()
            {
                {gameState.Players[0], new BitmapImage(new Uri("pack://application:,,,/Assets/Green.png")) },
                {gameState.Players[1], new BitmapImage(new Uri("pack://application:,,,/Assets/Red.png")) }
            };

            SetupGameGrid();
            SetupGame();

            gameState.MoveMade += OnMoveMade;
            gameState.ColumnFull += OnColumnFull;
            gameState.GameEnded += OnGameEnded;
            gameState.SwitchedPlayer += OnSwitchedPlayer;
            gameState.GameRestarted += OnGameRestarted;
        }

        private void OnColumnFull()
        {
            MessageBox.Show("Selected column is full, try another one. ", "Column full!!!");
        }

        private void SetupGameGrid()
        {
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 7; c++)
                {
                    Image imageControl = new();
                    GameGrid.Children.Add(imageControl);
                    imageControls[c, r] = imageControl;
                }
            }
        }

        private void SetupGame()
        {
            CurrentPlayer.Text = gameState.CurrentPlayer.Name;
            PlayerImage.Source = imageSources[gameState.CurrentPlayer];
        }

        private void OnMoveMade(int c)
        {
            PlayerImage.Source = imageSources[gameState.CurrentPlayer];
            imageControls[c, GameGrid.Rows - 1 - gameState.discs.Last(d => d.X == c).Y].Source = PlayerImage.Source;
        }

        private void OnSwitchedPlayer()
        {
            CurrentPlayer.Text = gameState.CurrentPlayer.Name;
            PlayerImage.Source = imageSources[gameState.CurrentPlayer];
        }

        private async void OnGameRestarted()
        {
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 7; c++)
                {
                    imageControls[c, r].Source = null;
                }
            }

            PlayerImage.Source = imageSources[gameState.CurrentPlayer];
            await TransitionToGameScreen();
        }

        private async void OnGameEnded(GameResult gameResult)
        {
            await Task.Delay(300);
            if (gameResult.Winner == null)
            {
                await TransitionToEndScreen("Its a tie", null!);
            }
            else
            {
                await ShowLine(gameResult.WinInfo!);
                await Task.Delay(1500);
                await TransitionToEndScreen($"{gameResult.Winner!.Name} wins in {gameState.TurnsPassed} turns", imageSources[gameResult.Winner]);
            }
        }

        private async Task TransitionToGameScreen()
        {
            await FadeOut(EndScreen);
            Line.Visibility = Visibility.Hidden;
            InLine.Visibility = Visibility.Hidden;
            await Task.WhenAll(FadeIn(TurnPanel), FadeIn(GameCanvas));
        }

        private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
        {
            await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas));
            ResultText.Text = text;
            WinnerImage.Source = winnerImage;
            await FadeIn(EndScreen);
        }

        private async Task FadeOut(UIElement e)
        {
            e.BeginAnimation(OpacityProperty, fadeOutAnimation);
            await Task.Delay(fadeOutAnimation.Duration.TimeSpan);
            e.Visibility = Visibility.Hidden;
        }

        private async Task FadeIn(UIElement e)
        {
            e.BeginAnimation(OpacityProperty, fadeInAnimation);
            await Task.Delay(fadeInAnimation.Duration.TimeSpan);
            e.Visibility = Visibility.Visible;
        }

        private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double squareSize = GameGrid.Width / 7;
            Point clickPosition = e.GetPosition(GameGrid);
            int col = (int)(clickPosition.X / squareSize);
            gameState.MakeMove(col);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            if (gameState.GameOver)
                gameState.Reset();
            SetupGame();
        }

        private async Task ShowLine(WinInfo winInfo)
        {
            (Point start, Point end) = FindLinePoints(winInfo);

            Line.X1 = start.X;
            Line.Y1 = start.Y;
            InLine.X1 = start.X;
            InLine.Y1 = start.Y;

            DoubleAnimation xAnimation = new()
            {
                Duration = TimeSpan.FromMilliseconds(300),
                From = start.X,
                To = end.X,
            };

            DoubleAnimation yAnimation = new()
            {
                Duration = TimeSpan.FromMilliseconds(300),
                From = start.Y,
                To = end.Y,
            };

            Line.Visibility = Visibility.Visible;
            InLine.Visibility = Visibility.Visible;
            Line.BeginAnimation(Line.X2Property, xAnimation);
            InLine.BeginAnimation(Line.X2Property, xAnimation);
            Line.BeginAnimation(Line.Y2Property, yAnimation);
            InLine.BeginAnimation(Line.Y2Property, yAnimation);
            await Task.Delay(xAnimation.Duration.TimeSpan);
        }

        private (Point, Point) FindLinePoints(WinInfo winInfo)
        {
            double wSize = GameGrid.Width / 7;
            double wmargin = wSize / 2;
            double hSize = GameGrid.Height / 6;
            double hmargin = hSize / 2;

            double x1 = (winInfo.StartCoordsOfWinningLine.Item1 * wSize) + wmargin + GameGrid.Margin.Left;
            double y1 = ((GameGrid.Rows - 1 - winInfo.StartCoordsOfWinningLine.Item2) * hSize) + hmargin + GameGrid.Margin.Top;
            double x2 = (winInfo.EndCoordsOfWinningLine.Item1 * wSize) + wmargin + GameGrid.Margin.Left;
            double y2 = ((GameGrid.Rows - 1 - winInfo.EndCoordsOfWinningLine.Item2) * hSize) + hmargin + GameGrid.Margin.Top;

            return (new Point(x1, y1), new Point(x2, y2));
        }
    }
}