using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.UI.Popups;
using Windows.ApplicationModel.Core;
using System.Collections.ObjectModel;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VisionEdgeML
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static DispatcherTimer dTimer;
        static RegistryManager registryManager;
        public static string connectionString = "HostName=mdsedgehub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=xLTxQkkmMA1zfazMHU101qJ88YdkZm/CTIkkW/LFk84=";
        ObservableCollection<SnackData> dataList = new ObservableCollection<SnackData>();
        int totalsum = 0;
        //private static Twin deviceTwin;
        string temps = "";
        public MainPage()
        {
            this.InitializeComponent();
            ChoGrid.Visibility = Visibility.Collapsed;
            KongGrid.Visibility = Visibility.Collapsed;
            GoGrid.Visibility = Visibility.Collapsed;
            KanGrid.Visibility = Visibility.Collapsed;
            KanmilGrid.Visibility = Visibility.Collapsed;

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            //GetTwin();
            dTimer = new DispatcherTimer();
            dTimer.Tick += dispatcherTimer_Tick;
            dTimer.Interval = new TimeSpan(0, 0, 1);
            dTimer.Start();
        }
        //public async Task GetTwin()
        //{
        //    Twin deviceTwin = await registryManager.GetTwinAsync("raspberryedge");
        //}
        private void dispatcherTimer_Tick(object sender, object e)
        {
            AddTwin();
        }
        public async void AddTwin()
        {
            var deviceTwin = await registryManager.GetTwinAsync("visionedgehub");
            bool chocoflag, gongflag, goflag, kanflag, kanmilflag;
            chocoflag = deviceTwin.Tags["choco"];
            gongflag = deviceTwin.Tags["gong"];
            goflag = deviceTwin.Tags["go"];
            kanflag = deviceTwin.Tags["kan"];
            kanmilflag = deviceTwin.Tags["kanmil"];

            if (chocoflag | gongflag | goflag | kanflag | kanmilflag)
            {
                dTimer.Stop();
                
                if (deviceTwin.Tags["choco"] == 1)
                    temps = "chocosongi";
                else if (deviceTwin.Tags["gong"] == 1)
                    temps = "gongryongbaksa";
                else if (deviceTwin.Tags["go"] == 1)
                    temps = "goraebab";
                else if (deviceTwin.Tags["kan"] == 1)
                    temps = "kancho";
                else if (deviceTwin.Tags["kanmil"] == 1)
                    temps = "kanchosweet";

                
                await ShowConfirmationDialog(temps);
                UpdateNullTwinAsync();
                //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                //    async () =>
                //    {
                //        await ShowConfirmationDialog(temps);
                //    });
            }
        }
        public async Task UpdateNullTwinAsync()
        {
            try
            {
                var deviceTwin = await registryManager.GetTwinAsync("visionedgehub");
                temps = null;
                //deviceTwin = await registryManager.GetTwinAsync("raspberryedge");
                //Twin deviceTwin = await registryManager.GetTwinAsync("raspberryedge");
                //deviceTwin.ETag = "AAAAAAAAAOs=";
                var patch = @"{tags:{choco: 0, gong: 0, go: 0, kan: 0, kanmil: 0}}";
                registryManager.UpdateTwinAsync(deviceTwin.DeviceId, patch, deviceTwin.ETag).Wait();
            }
            catch (Exception)
            {
                var result = new
                {
                    StatusCode = 500,
                    Message = "Error during set temperature"
                };
            }
            
        }

        public async Task ShowConfirmationDialog(string result)
        {
            
            // Create the message dialog and set its content.
            var msg = new MessageDialog("Are you sure " + result + " ?", "Confirmation");

            // Add commands and set their callbacks as inline event handlers.
            // The callback methods run in the UI thread.
            msg.Commands.Add(new UICommand("Yes", async command =>
            {
                SetData(result);
                await SetPicAsync(result);
                dTimer.Start();
            }));

            msg.Commands.Add(new UICommand("No", async command =>
            {
            }));
            // Set the command that will be invoked by default i.e. when the user presses Enter.
            msg.DefaultCommandIndex = 0;

            // Set the command to be invoked when Esc is pressed.
            msg.CancelCommandIndex = 1;

            // Show the message dialog
            await msg.ShowAsync();
        }
        private async Task SetPicAsync(string name)
        {
            DisabledFeedGrid.Visibility = Visibility.Collapsed;
            if (name == "chocosongi")
            {
                ChoGrid.Visibility = Visibility.Visible;
                await Task.Delay(TimeSpan.FromSeconds(2));
                ChoGrid.Visibility = Visibility.Collapsed;
            }
            else if (name == "gongryongbaksa")
            {
                KongGrid.Visibility = Visibility.Visible;
                await Task.Delay(TimeSpan.FromSeconds(2));
                KongGrid.Visibility = Visibility.Collapsed;
            }
            else if (name == "goraebab")
            {
                GoGrid.Visibility = Visibility.Visible;
                await Task.Delay(TimeSpan.FromSeconds(2));
                GoGrid.Visibility = Visibility.Collapsed;
            }
            else if (name == "kancho")
            {
                KanGrid.Visibility = Visibility.Visible;
                await Task.Delay(TimeSpan.FromSeconds(2));
                KanGrid.Visibility = Visibility.Collapsed;
            }
            else if (name == "kanchosweet")
            {
                KanmilGrid.Visibility = Visibility.Visible;
                await Task.Delay(TimeSpan.FromSeconds(2));
                KanmilGrid.Visibility = Visibility.Collapsed;
            }
            DisabledFeedGrid.Visibility = Visibility.Visible;
        }
        private async void pay_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog msg = new MessageDialog("Payment Complete.", "Confirmation");
            await msg.ShowAsync();
            dataList.Clear();
            totalsum = 0;
            total.Text = "0";

        }
        private void SetData(string name)
        {
            if (listView.Items.Count == 0)
            {
                if (name == "chocosongi")
                {
                    SnackData item1 = new SnackData() { Name = "초코송이", Tag = "chocosongi", Qty = 1, Value = 800 };
                    dataList.Add(item1);
                    totalsum += 800;
                    listView.ItemsSource = dataList;
                }
                else if (name == "gongryongbaksa")
                {
                    SnackData item2 = new SnackData() { Name = "공룡박사", Tag = "gongryongbaksa", Qty = 1, Value = 1200 };
                    dataList.Add(item2);
                    totalsum += 1200;
                    listView.ItemsSource = dataList;
                }
                else if (name == "goraebab")
                {
                    SnackData item3 = new SnackData() { Name = "고래밥", Tag = "goraebab", Qty = 1, Value = 900 };
                    dataList.Add(item3);
                    totalsum += 900;
                    listView.ItemsSource = dataList;
                }
                else if (name == "kancho")
                {
                    SnackData item4 = new SnackData() { Name = "칸쵸", Tag = "kancho", Qty = 1, Value = 1000 };
                    dataList.Add(item4);
                    totalsum += 1000;
                    listView.ItemsSource = dataList;
                }
                else if (name == "kanchosweet")
                {
                    SnackData item5 = new SnackData() { Name = "칸쵸스윗밀크", Tag = "kanchosweet", Qty = 1, Value = 1100 };
                    dataList.Add(item5);
                    totalsum += 1100;
                    listView.ItemsSource = dataList;
                }
                total.Text = totalsum.ToString();
            }
            else
            {
                int cho = 0;
                int gong = 0;
                int go = 0;
                int kan = 0;
                int kanmil = 0;
                int j = dataList.Count;
                for (int i = 0; i < j; i++)
                {
                    if (dataList[i].Tag == "chocosongi")
                    {
                        cho++;
                    }
                    else if (dataList[i].Tag == "gongryongbaksa")
                    {
                        gong++;
                    }
                    else if (dataList[i].Tag == "goraebab")
                    {
                        go++;
                    }
                    else if (dataList[i].Tag == "kancho")
                    {
                        kan++;
                    }
                    else if (dataList[i].Tag == "kanchosweet")
                    {
                        kanmil++;
                    }
                }

                int k = dataList.Count;
                for (int i = 0; i < k; i++)
                {
                    if (dataList[i].Tag == name)
                    {
                        dataList[i].Qty++;
                        if (name == "chocosongi")
                        {
                            dataList[i].Value += 800;
                            totalsum += 800;
                        }
                        else if (name == "gongryongbaksa")
                        {
                            dataList[i].Value += 1200;
                            totalsum += 1200;
                        }
                        else if (name == "goraebab")
                        {
                            dataList[i].Value += 900;
                            totalsum += 900;
                        }
                        else if (name == "kancho")
                        {
                            dataList[i].Value += 1000;
                            totalsum += 1000;
                        }
                        else if (name == "kanchosweet")
                        {
                            dataList[i].Value += 1100;
                            totalsum += 1100;
                        }
                        listView.ItemsSource = null;
                        total.Text = totalsum.ToString();
                        listView.ItemsSource = dataList;
                        break;
                    }

                }

                if (cho == 0 && name == "chocosongi")
                {
                    SnackData item1 = new SnackData() { Name = "초코송이", Tag = "chocosongi", Qty = 1, Value = 800 };
                    dataList.Add(item1);
                    totalsum += 800;
                    listView.ItemsSource = dataList;
                }
                else if (gong == 0 && name == "gongryongbaksa")
                {
                    SnackData item2 = new SnackData() { Name = "공룡박사", Tag = "gongryongbaksa", Qty = 1, Value = 1200 };
                    dataList.Add(item2);
                    totalsum += 1200;
                    listView.ItemsSource = dataList;
                }
                else if (go == 0 && name == "goraebab")
                {
                    SnackData item3 = new SnackData() { Name = "고래밥", Tag = "goraebab", Qty = 1, Value = 900 };
                    dataList.Add(item3);
                    totalsum += 900;
                    listView.ItemsSource = dataList;
                }
                else if (kan == 0 && name == "kancho")
                {
                    SnackData item4 = new SnackData() { Name = "칸쵸", Tag = "kancho", Qty = 1, Value = 1000 };
                    dataList.Add(item4);
                    totalsum += 1000;
                    listView.ItemsSource = dataList;
                }
                else if (kanmil == 0 && name == "kanchosweet")
                {
                    SnackData item5 = new SnackData() { Name = "칸쵸스윗밀크", Tag = "kanchosweet", Qty = 1, Value = 1100 };
                    dataList.Add(item5);
                    totalsum += 1100;
                    listView.ItemsSource = dataList;
                }
                total.Text = totalsum.ToString();
            }
        }
    }
}
