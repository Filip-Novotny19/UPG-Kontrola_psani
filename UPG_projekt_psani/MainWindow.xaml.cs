using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace UPG_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> vety = new();
        int x = 0;
        int y;
        string nowveta = string.Empty;
        int chyba = 0;

        public IReadOnlyList<string> Vety => vety;

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private static string? GetSolutionDirectory()
        {
            try
            {
                var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                while (dir != null)
                {
                    var slnFiles = dir.GetFiles("*.sln").Concat(dir.GetFiles("*.slnx")).ToArray();
                    if (slnFiles.Length > 0)
                        return dir.FullName;

                    dir = dir.Parent;
                }
            }
            catch
            {
            }

            return null;
        }

        private void Start()
        {
            TextBlock welcome = new()
            {
                Text = "Vítej v programu pro trénování psaní na klávesnici! Klikni \"Start\" pro načtení vět.",
                FontSize = 30,
                TextWrapping = TextWrapping.Wrap,
                Width = 600,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(welcome, 80);
            Canvas.SetTop(welcome, 150);
            Button startButton = new()
            {
                Content = "Start",
                Background = Brushes.LightGray,
                Height = 58,
                Width = 80
            };
            startButton.Click += StartButton_Click;
            startButton.IsDefault = true;
            Canvas.SetLeft(startButton, 320);
            Canvas.SetTop(startButton, 250);

            Labels.Children.Add(startButton);
            Labels.Children.Add(welcome);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Labels.Children.Clear();

            if (timer == null)
            {
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += Timer_Tick;
            }
            elapsed = TimeSpan.Zero;

            LoadSentences();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            elapsed = elapsed.Add(TimeSpan.FromSeconds(1));
            if (timerLabel != null)
            {
                timerLabel.Content = $"Čas: {(int)elapsed.TotalSeconds} s";
            }
        }

        private TextBox? inputText;
        private DispatcherTimer? timer;
        private TimeSpan elapsed = TimeSpan.Zero;
        private Label? timerLabel;

        private void LoadSentences()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Open sentences file",
                Filter = "CSV files (*.csv)|*.csv",
                InitialDirectory = GetSolutionDirectory() ?? AppDomain.CurrentDomain.BaseDirectory
            };

            bool? result = dlg.ShowDialog();
            if (result == true && !string.IsNullOrEmpty(dlg.FileName))
            {
                vety = ReadSentences(dlg.FileName);
                vety = vety.OrderBy(_ => Random.Shared.Next()).ToList();
                if (vety != null)
                {
                    ShowSentences();
                }
            }
        }

        private List<string> ReadSentences(string path)
        {
            try
            {
                var lines = File.ReadAllLines(path, Encoding.UTF8)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l))
                    .ToList();

                return lines;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Nenačetli se věty!: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<string>();
            }
        }

        private void ShowSentences()
        {
            if (vety.Count > 0 && x <= 4)
            {
                Button nextbutton = new()
                {
                    Content = "Další",
                    Background = Brushes.White,
                    Height = 58,
                    Width = 63
                };
                nextbutton.Style = TryFindResource("PrimaryButton") as Style;
                nextbutton.Click += Next_Click;
                nextbutton.IsDefault = true;
                Canvas.SetLeft(nextbutton, 588);
                Canvas.SetTop(nextbutton, 290);

                Label or = new()
                {
                    Content = "Nebo zmáčkni Enter :)",
                    FontSize = 15,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Gray
                };
                or.Style = TryFindResource("BodyLabel") as Style;
                Canvas.SetLeft(or, 588);
                Canvas.SetTop(or, 365);

                inputText = new()
                {
                    Height = 38,
                    Width = 300
                };
                inputText.Style = TryFindResource("RoundedTextBox") as Style;
                Canvas.SetLeft(inputText, 195);
                Canvas.SetTop(inputText, 300);

                nowveta = vety[x];
                Label veta = new()
                {
                    Content = nowveta,
                    FontSize = 28,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Black
                };
                veta.Style = TryFindResource("BodyLabel") as Style;
                Canvas.SetLeft(veta, 0);
                Canvas.SetTop(veta, 160);

                Label chyby = new()
                {
                    Content = $"Počet chyb: {chyba}",
                    FontSize = 24,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Black
                };
                chyby.Style = TryFindResource("BodyLabel") as Style;
                Canvas.SetRight(chyby, 90);

                if (timerLabel == null)
                {
                    timerLabel = new Label
                    {
                        Content = "Čas: 0 s",
                        FontSize = 24,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = Brushes.Black
                    };
                    timerLabel.Style = TryFindResource("BodyLabel") as Style;
                }
                Canvas.SetRight(timerLabel, 90);
                Canvas.SetTop(timerLabel, 35);

                if (x == 0)
                {
                    elapsed = TimeSpan.Zero;
                    timerLabel.Content = "Čas: 0 s";
                    timer?.Start();
                }

                Labels.Children.Add(veta);
                Labels.Children.Add(nextbutton);
                Labels.Children.Add(inputText);
                Labels.Children.Add(chyby);
                Labels.Children.Add(timerLabel);
                Labels.Children.Add(or);

                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    try
                    {
                        inputText?.Focus();
                        Keyboard.Focus(inputText);
                        inputText?.SelectAll();
                    }
                    catch
                    {
                    }
                }));

                if (inputText != null)
                {
                    inputText.KeyDown += (s, e) =>
                    {
                        if (e.Key == Key.Enter)
                        {
                            Next_Click(nextbutton, new RoutedEventArgs());
                            e.Handled = true;
                        }
                    };
                }
            }
            else if (x > 4)
            {
                Labels.Children.Clear();
                timer?.Stop();
                Label end = new()
                {
                    Content = $"Konec! Počet chyb: {chyba}  Čas: {(int)elapsed.TotalSeconds} s",
                    FontSize = 40,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Black
                };
                end.Style = TryFindResource("BodyLabel") as Style;
                Canvas.SetLeft(end, 150);
                Canvas.SetTop(end, 150);
                Labels.Children.Add(end);
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var userText = inputText?.Text ?? string.Empty;
            SentenceCheck(userText);
            x++;
            Labels.Children.Clear();
            ShowSentences();
        }

        private void SentenceCheck(string userText)
        {
            if (string.IsNullOrEmpty(userText))
            {
                chyba += nowveta.Length;
                return;
            }
            int min = Math.Min(nowveta.Length, userText.Length);
            for (int i = 0; i < min; i++)
            {
                if (nowveta[i] != userText[i])
                    chyba++;
            }
            if (userText.Length < nowveta.Length)
            {
                chyba += nowveta.Length - userText.Length;
            }
            else if (userText.Length > nowveta.Length)
            {
                chyba += userText.Length - nowveta.Length;
            }
        }
    }
}