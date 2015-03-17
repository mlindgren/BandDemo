using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public class LockedFile
    {
        public StorageFile File
        {
            get; set;
        }

        public ReaderWriterLockSlim Lock
        {
            get; set;
        }

        public DateTime LastWritten
        {
            get; set;
        }

        public async Task Initialize(string path)
        {
            File = await ApplicationData.Current.LocalFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);
            Lock = new ReaderWriterLockSlim();
            LastWritten = DateTime.Now;
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private LockedFile AccelerometerData
        {
            get; set;
        }

        private LockedFile GyroscopeData
        {
            get; set;
        }

        private LockedFile HeartRateData
        {
            get; set;
        }

        private LockedFile SkinTempData
        {
            get; set;
        }

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            AccelerometerData = new LockedFile();
            GyroscopeData = new LockedFile();
            HeartRateData = new LockedFile();
            SkinTempData = new LockedFile();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private async Task UpdateAccelerometerData(BandSensorReadingEventArgs<IBandAccelerometerReading> args)
        {
            AccelerometerXValue.Text = args.SensorReading.AccelerationX.ToString("###00.00");
            AccelerometerYValue.Text = args.SensorReading.AccelerationY.ToString("###00.00");
            AccelerometerZValue.Text = args.SensorReading.AccelerationZ.ToString("###00.00");

            TimeSpan timeSinceSaved = DateTime.Now - AccelerometerData.LastWritten;

            if(timeSinceSaved.TotalSeconds < 1)
            {
                return;
            }

            string csvLine = args.SensorReading.Timestamp.ToString() + "," +
                             AccelerometerXValue.Text + "," +
                             AccelerometerYValue.Text + "," +
                             AccelerometerZValue.Text + "\n";

            AccelerometerData.Lock.EnterWriteLock();

            await FileIO.AppendTextAsync(AccelerometerData.File, csvLine);

            AccelerometerData.Lock.ExitWriteLock();

            AccelerometerData.LastWritten = DateTime.Now;
        }

        private async Task UpdateGyroscopeData(BandSensorReadingEventArgs<IBandGyroscopeReading> args)
        {
            GyroscopeXValue.Text = args.SensorReading.AngularVelocityX.ToString("###00.00");
            GyroscopeYValue.Text = args.SensorReading.AngularVelocityY.ToString("###00.00");
            GyroscopeZValue.Text = args.SensorReading.AngularVelocityZ.ToString("###00.00");

            TimeSpan timeSinceSaved = DateTime.Now - GyroscopeData.LastWritten;

            if (timeSinceSaved.TotalSeconds < 1)
            {
                return;
            }

            string csvLine = args.SensorReading.Timestamp.ToString() + "," +
                             GyroscopeXValue.Text + "," +
                             GyroscopeYValue.Text + "," +
                             GyroscopeZValue.Text + "\n";

            GyroscopeData.Lock.EnterWriteLock();

            await FileIO.AppendTextAsync(GyroscopeData.File, csvLine);

            GyroscopeData.Lock.ExitWriteLock();

            GyroscopeData.LastWritten = DateTime.Now;
        }

        private async Task UpdateHeartRateData(BandSensorReadingEventArgs<IBandHeartRateReading> args)
        {
            HeartRateValue.Text = args.SensorReading.HeartRate.ToString();

            string csvLine = args.SensorReading.Timestamp.ToString() + "," +
                             HeartRateValue.Text + "," +
                             args.SensorReading.Quality.ToString() + "\n";

            HeartRateData.Lock.EnterWriteLock();

            await FileIO.AppendTextAsync(HeartRateData.File, csvLine);

            HeartRateData.Lock.ExitWriteLock();

            System.Diagnostics.Debug.WriteLine("Wrote heart rate data");
        }

        private async Task UpdateSkinTempData(BandSensorReadingEventArgs<IBandSkinTemperatureReading> args)
        {
            string csvLine = args.SensorReading.Timestamp.ToString() + "," +
                             args.SensorReading.Temperature.ToString() + "\n";

            SkinTempData.Lock.EnterWriteLock();

            await FileIO.AppendTextAsync(SkinTempData.File, csvLine);

            SkinTempData.Lock.ExitWriteLock();
        }

        private async void StartRecording()
        {
            System.Diagnostics.Debug.WriteLine("Starting!");

            if(App.BandClient != null)
            {
                return;
            }

            StatusLabel.Text = "Connecting...";

            IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();

            try
            {
                App.BandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);

                // do work after successful connect
                StatusLabel.Text = "Connected!";

                IBandSensorManager manager = App.BandClient.SensorManager;

                manager.Accelerometer.ReadingChanged += async (sender, args) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await UpdateAccelerometerData(args);
                    });
                };

               manager.Gyroscope.ReadingChanged += async (sender, args) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await UpdateGyroscopeData(args);
                    });
                };

                manager.HeartRate.ReadingChanged += async (sender, args) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await UpdateHeartRateData(args);
                    });
                };

                manager.SkinTemperature.ReadingChanged += async (sender, args) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await UpdateSkinTempData(args);
                    });
                };

                await AccelerometerData.Initialize("accelerometer.csv");
                await GyroscopeData.Initialize("gyroscope.csv");
                await HeartRateData.Initialize("heartrate.csv");
                await SkinTempData.Initialize("skintemp.csv");

                manager.Accelerometer.ReportingInterval = manager.Accelerometer.SupportedReportingIntervals.Last();
                manager.Gyroscope.ReportingInterval = manager.Gyroscope.SupportedReportingIntervals.Last();
                manager.HeartRate.ReportingInterval = manager.HeartRate.SupportedReportingIntervals.Last();
                manager.SkinTemperature.ReportingInterval = manager.SkinTemperature.SupportedReportingIntervals.Last();

                LogLabel.Text += "Trying to start reading accelerometer...";
                await App.BandClient.SensorManager.Accelerometer.StartReadingsAsync();
                LogLabel.Text += " ...done!\n";

                LogLabel.Text += "Trying to start reading gyroscope...";
                await App.BandClient.SensorManager.Gyroscope.StartReadingsAsync();
                LogLabel.Text += " ...done!\n";

                LogLabel.Text += "Trying to start reading heart rate...";
                await App.BandClient.SensorManager.HeartRate.StartReadingsAsync();
                LogLabel.Text += " ...done!\n";

                LogLabel.Text += "Trying to start reading skin temp...";
                await App.BandClient.SensorManager.SkinTemperature.StartReadingsAsync();
                LogLabel.Text += "...done!\n";
               
            }
            catch (Exception)
            {
                StatusLabel.Text = "Could not connect.";
            }
        }

        private async void StopRecording()
        {

            if(App.BandClient == null)
            {
                return;
            }

            StatusLabel.Text = "Disconnecting...";

            await App.BandClient.SensorManager.Accelerometer.StopReadingsAsync();
            await App.BandClient.SensorManager.Gyroscope.StopReadingsAsync();
            await App.BandClient.SensorManager.HeartRate.StopReadingsAsync();
            await App.BandClient.SensorManager.SkinTemperature.StopReadingsAsync();

            App.BandClient = null;

            StatusLabel.Text = "Disconnected.";
        }

        private void RecordButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StartRecording();
        }

        private void StopButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StopRecording();
        }
    }


}
