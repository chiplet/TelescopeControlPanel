﻿//TODO:
//  - reset screen when disconnecting from joystick

using System;
using System.Collections.Generic;
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
using SharpDX.DirectInput;
using System.Threading;

namespace JoystrickControlDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const int XAxisMax = 65535;
        private const int YAxisMax = 65535;
        private const int ZAxisMax = 65535;

        string DefaultJoystickNameTextBoxMessage = "Logitech Extreme 3D";

        JoystickMonitor joystickMonitor;
        CancellationTokenSource joystickMonitorCancellationSource;

        public MainWindow()
        {
            InitializeComponent();
            JoystickNameTextBox.Text = DefaultJoystickNameTextBoxMessage;

            // set cursor preview initially to center
            CanvasCursor.SetValue(Canvas.LeftProperty, JoystickCanvasPreview.ActualWidth / 2);
            CanvasCursor.SetValue(Canvas.TopProperty, JoystickCanvasPreview.ActualHeight / 2);

        }

        private void PreviewGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PreviewGridBorder.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        private void PreviewGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PreviewGridBorder.BorderBrush = new SolidColorBrush(Colors.Black);
        }

        private void JoystickNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (JoystickNameTextBox.Text == DefaultJoystickNameTextBoxMessage)
            {
                JoystickNameTextBox.Text = "";
            }
        }

        private void JoystickNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (JoystickNameTextBox.Text == "")
            JoystickNameTextBox.Text = DefaultJoystickNameTextBoxMessage;
        }

        private async void JoystickConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // initialize progress bar colours
                XProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                YProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                ZProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(255, 29, 101, 255));

                JoystickConnectButton.IsEnabled = false;
                JoystickDisconnectButton.IsEnabled = true;
                JoystickNameTextBox.IsEnabled = false;
                joystickMonitor = new JoystickMonitor(JoystickNameTextBox.Text.Trim());

                joystickMonitorCancellationSource = new CancellationTokenSource();
                var progress = new Progress<JoystickUpdate>(s => ProcessJoystickUpdate(s));
                MainStatusBarMessage.Text = String.Format("Successfully connected to joystick: {0}", JoystickNameTextBox.Text.Trim());

                await Task.Run(() => joystickMonitor.PollJoystick(progress, joystickMonitorCancellationSource.Token), joystickMonitorCancellationSource.Token);

            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Error! :( | "+ex.Message);

                // clean up
                JoystickConnectButton.IsEnabled = true;
                JoystickDisconnectButton.IsEnabled = false;
                JoystickNameTextBox.IsEnabled = true;
                MainStatusBarMessage.Text = String.Format("Could not connect to joystick: {0}. Reason: {1}", JoystickNameTextBox.Text.Trim(), ex.Message);

                dullifyProgressBards();
            }
        }

        private void ProcessJoystickUpdate(JoystickUpdate state)
        {
            if (state.Offset == JoystickOffset.X)
            {
                XProgressBar.Value = calculateXAxisPercentage(state.Value);
                CanvasCursor.SetValue(Canvas.LeftProperty, calculateCanvasCursorXPosition(state.Value));
                Console.WriteLine(calculateCanvasCursorXPosition(state.Value));
            }

            if (state.Offset == JoystickOffset.Y)
            {
                YProgressBar.Value = calculateYAxisPercentage(state.Value);
                CanvasCursor.SetValue(Canvas.TopProperty, calculateCanvasCursorYPosition(state.Value));

            }

            if (state.Offset == JoystickOffset.RotationZ)
            {
                ZProgressBar.Value = calculateZAxisPercentage(state.Value);
            }
            
        }


        private void JoystickDisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            joystickMonitorCancellationSource.Cancel();
            JoystickConnectButton.IsEnabled = true;
            JoystickDisconnectButton.IsEnabled = false;

            dullifyProgressBards();

            // Output StatusBar message
            MainStatusBarMessage.Text = String.Format("Successfully disconnected from joystick: {0}", JoystickNameTextBox.Text.Trim());

        }

        private int calculateXAxisPercentage(int num)
        {
            return (int)((double)num / XAxisMax * 100);
        }

        private int calculateYAxisPercentage(int num)
        {
            return (int)((double)num / YAxisMax * 100);
        }

        private int calculateZAxisPercentage(int num)
        {
            return (int)((double)num / ZAxisMax * 100);
        }

        private double calculateCanvasCursorXPosition(int num)
        {
            return (calculateXAxisPercentage(num) * JoystickCanvasPreview.ActualWidth / 100);
        }
        private double calculateCanvasCursorYPosition(int num)
        {
            return (calculateYAxisPercentage(num) * JoystickCanvasPreview.ActualHeight / 100);
        }

        private void dullifyProgressBards()
        {
            // dullify joystick progress bars
            XProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            YProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            ZProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }
    }    
}
