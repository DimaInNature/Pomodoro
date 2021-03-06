using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace Pomodoro_Timer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Background градиент

        private readonly LinearGradientBrush workingGradient = new LinearGradientBrush(
            Color.FromRgb(56, 54, 96),
            Color.FromRgb(240, 131, 218),
            new Point(0, 0),
            new Point(0, 1));

        private readonly LinearGradientBrush breakGradient = new LinearGradientBrush(
            Color.FromArgb(255, 23, 232, 217),
            Color.FromArgb(255, 94, 124, 234),
            new Point(0, 0),
            new Point(0.74, 0.73));

        #endregion

        #region Сохранённые настройки пользователя

        public static int pomodoroDuration = Pomodoro.Properties.Settings.Default.pomodoroDuration * 60; //25 * 60; 480 max 
        public static int pomodoroBreak = Pomodoro.Properties.Settings.Default.pomodoroBreak * 60; //5 * 60; 480 max
        public static int pomodoroLongBreak = Pomodoro.Properties.Settings.Default.pomodoroLongBreak * 60; //15 * 60; 480 max
        public static int pomodoroLongBreakOccurance = Pomodoro.Properties.Settings.Default.pomodoroLongBreakOccurance; // 100 max
        public static string workingSounds = Environment.CurrentDirectory + @"\Assets\Sounds\workingSounds\bgm_" + Pomodoro.Properties.Settings.Default.workingSounds + ".mp3";
        public static string alarmSounds = Environment.CurrentDirectory + @"\Assets\Sounds\alarmSounds\alm_" + Pomodoro.Properties.Settings.Default.alarmSounds + ".mp3";

        #endregion

        public static MP3Player alarmSoundsOGG;
        public static MP3Player workingSoundsOGG;
        private int pomodoroCount;
        private startStopRestartEnum startStopBool = startStopRestartEnum.start;
        private int time;
        private readonly DispatcherTimer Timer;

        private enum startStopRestartEnum
        {
            start,
            stop,
            restart,
            resume,
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Background = workingGradient;
            time = pomodoroDuration;
            countdownTimer.Content = FormatTimer(time);

            workingSoundsOGG = new MP3Player(workingSounds, "workingSounds");
            alarmSoundsOGG = new MP3Player(alarmSounds, "alarmSounds");
            workingSoundsOGG.Volume("workingSounds", 1000);
            alarmSoundsOGG.Volume("alarmSounds", 1000);

            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Tick += TimerTick;
        }

        void TimerTick(object sender, EventArgs e)
        {
            if (time > 0)
            {
                time--;
                countdownTimer.Content = FormatTimer(time);
            }
            else
            {
                workingSoundsOGG.Stop("workingSounds");
                alarmSoundsOGG.Play("alarmSounds");

                pomodoroCount++;
                Timer.Stop();
                StartPauseButton.Content = "OK";
                startStopBool = startStopRestartEnum.restart;
                RestartButton.Visibility = Visibility.Collapsed;
            }
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if(startStopBool == startStopRestartEnum.start)
            {
                if(pomodoroCount < pomodoroLongBreakOccurance * 2 + 1)
                {
                    if(pomodoroCount % 2 == 0)
                    {
                        this.Background = workingGradient;
                        StartPauseButton.Visibility = Visibility.Visible;
                        RestartButton.Visibility = Visibility.Visible;
                        DoneBreakButton.Visibility = Visibility.Collapsed;
                        time = pomodoroDuration;
                    }
                    else
                    {
                        this.Background = breakGradient;
                        StartPauseButton.Visibility = Visibility.Collapsed;
                        RestartButton.Visibility = Visibility.Collapsed;
                        DoneBreakButton.Visibility = Visibility.Visible;
                        time = pomodoroBreak;
                    }
                }
                else
                {
                    this.Background = breakGradient;
                    StartPauseButton.Visibility = Visibility.Collapsed;
                    RestartButton.Visibility = Visibility.Collapsed;
                    DoneBreakButton.Visibility = Visibility.Visible;
                    time = pomodoroLongBreak;
                    pomodoroCount = -1;
                }

                workingSoundsOGG.Play("workingSounds");

                Timer.Start();
                StartPauseButton.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/Buttons/timer-pause-big.png")));
                startStopBool = startStopRestartEnum.stop;
            }
            else if (startStopBool == startStopRestartEnum.restart)
            {
                alarmSoundsOGG.Stop("alarmSounds");
                StartPauseButton.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/Buttons/timer-start-big.png")));
                startStopBool = startStopRestartEnum.start;
                RestartButton.Visibility = Visibility.Collapsed;
            }
            else if (startStopBool==startStopRestartEnum.stop)
            {
                workingSoundsOGG.Stop("workingSounds");
                alarmSoundsOGG.Stop("alarmSounds");

                Timer.Stop();
                StartPauseButton.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/Buttons/timer-start-big.png")));
                startStopBool = startStopRestartEnum.resume;
                RestartButton.Visibility = Visibility.Collapsed;
            }
            else if(startStopBool == startStopRestartEnum.resume)
            {
                workingSoundsOGG.Play("workingSounds");
                Timer.Start();
                StartPauseButton.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/Buttons/timer-pause-big.png")));
                startStopBool = startStopRestartEnum.stop;
                RestartButton.Visibility = Visibility.Visible;
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if(startStopBool == startStopRestartEnum.stop)
            {
                alarmSoundsOGG.Stop("alarmSounds");
                workingSoundsOGG.Stop("workingSounds");

                pomodoroCount = 0;
                Timer.Stop();
                countdownTimer.Content = FormatTimer(pomodoroDuration);

                startStopBool = startStopRestartEnum.start;
                StartPauseButton.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/Buttons/timer-start-big.png")));
                RestartButton.Visibility = Visibility.Collapsed;
            }
        }

        private void DoneBreakButton_Click(object sender, RoutedEventArgs e)
        {
            alarmSoundsOGG.Stop("alarmSounds");
            workingSoundsOGG.Stop("workingSounds");
            Timer.Stop();
            countdownTimer.Content = FormatTimer(pomodoroDuration);

            startStopBool = startStopRestartEnum.start;
            StartPauseButton.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/Buttons/timer-start-big.png")));
            DoneBreakButton.Visibility = Visibility.Collapsed;
            StartPauseButton.Visibility = Visibility.Visible;

            if (time > 0)
            {
                pomodoroCount++;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Window settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private static string FormatTimer(int time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        bool conditionTray = true; //полный размер

        

        private void Tray_Click(object sender, RoutedEventArgs e)
        {
            if (conditionTray)
            {
                ExitBut.Height = 40;
                ExitBut.Width = 40;
                Pomodoro_Timer.Height = 150;
                Pomodoro_Timer.Width = 300;
                countdownTimer.Height = 50;
                countdownTimer.Width = 130;
                countdownTimer.FontSize = 30;
                SettingsButton.Visibility = Visibility.Hidden;
                StartPauseButton.Height = 40;
                StartPauseButton.Width = 40;
                DoneBreakButton.Width = 40;
                DoneBreakButton.Height = 40;
                RestartButton.Height = 40;
                RestartButton.Width = 40;
                conditionTray = false;
            }
            else
            {
                Pomodoro_Timer.Height = 450;
                Pomodoro_Timer.Width = 800;
                countdownTimer.Height = 150; 
                countdownTimer.Width = 400; 
                countdownTimer.FontSize = 100;
                SettingsButton.Visibility = Visibility.Visible;
                StartPauseButton.Height = 60;
                StartPauseButton.Width = 60;
                DoneBreakButton.Width = 60;
                DoneBreakButton.Height = 60;
                RestartButton.Height = 60;
                RestartButton.Width = 60;
                conditionTray = true;
            }
        }
    }
}
