using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Band;
using Microsoft.Band.Sensors;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace BandDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Starting!");

            IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();

            try
            {
                App.BandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);
                
                // do work after successful connect
                StatusLabel.Text = "Connected!";

                App.BandClient.SensorManager.Accelerometer.ReadingChanged += async (sender, args) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        UpdateAccelerometerData(args);
                    });
                };

                App.BandClient.SensorManager.Gyroscope.ReadingChanged += async (sender, args) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        UpdateGyroscopeData(args);
                    });
                };

                App.BandClient.SensorManager.HeartRate.ReadingChanged += async (sender, args) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        UpdateHeartRateData(args);
                    });
                };

                LogLabel.Text += "Trying to start reading accelerometer...";
                await App.BandClient.SensorManager.Accelerometer.StartReadingsAsync();
                LogLabel.Text += " ...done!\n";

                LogLabel.Text += "Trying to start reading gyroscope...";
                await App.BandClient.SensorManager.Gyroscope.StartReadingsAsync();
                LogLabel.Text += " ...done!\n";

                LogLabel.Text += "Trying to start reading heart rate...";
                await App.BandClient.SensorManager.HeartRate.StartReadingsAsync();
                LogLabel.Text += " ...done!\n";
            }
            catch(Exception)
            {
                StatusLabel.Text = "Could not connect.";
            }
        }
        private void UpdateAccelerometerData(BandSensorReadingEventArgs<IBandAccelerometerReading> args)
        {
            AccelerometerXValue.Text = args.SensorReading.AccelerationX.ToString("###00.00");
            AccelerometerYValue.Text = args.SensorReading.AccelerationY.ToString("###00.00");
            AccelerometerZValue.Text = args.SensorReading.AccelerationZ.ToString("###00.00");

            System.Diagnostics.Debug.WriteLine("Got accelerometer event!");
        }

        private void UpdateGyroscopeData(BandSensorReadingEventArgs<IBandGyroscopeReading> args)
        {
            GyroscopeXValue.Text = args.SensorReading.AngularVelocityX.ToString("###00.00");
            GyroscopeYValue.Text = args.SensorReading.AngularVelocityY.ToString("###00.00");
            GyroscopeZValue.Text = args.SensorReading.AngularVelocityZ.ToString("###00.00");

            System.Diagnostics.Debug.WriteLine("Got gyroscope event!");
        }

        private void UpdateHeartRateData(BandSensorReadingEventArgs<IBandHeartRateReading> args)
        {
            HeartRateValue.Text = args.SensorReading.HeartRate.ToString();

            System.Diagnostics.Debug.WriteLine("Got heart rate event!");
        }
    }


}
