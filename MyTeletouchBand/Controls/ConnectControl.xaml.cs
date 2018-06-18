using System;
using System.Collections.ObjectModel;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MyTeletouchBand.Controls
{
    public sealed partial class ConnectControl : UserControl
    {
        private ObservableCollection<PairedDeviceInfo> _pairedDevices;  // a local copy of paired device information
        //private StreamSocket _socket; // socket object used to communicate with the device

        public ConnectControl()
        {
            this.InitializeComponent();
            _pairedDevices = new ObservableCollection<PairedDeviceInfo>();
            PairedDevicesList.ItemsSource = _pairedDevices;
            this.Loaded += ConnectControl_Loaded;
        }

        void ConnectControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ConnectControl_Loaded;
            RefreshPairedDevicesList();
        }

        internal void Initialize(ActivePageType newPageType)
        {
            switch (newPageType)
            {
                case ActivePageType.Connect:
                    break;
                case ActivePageType.ConnectNoBluetooth:
                    break;
                case ActivePageType.Connecting:
                    break;
            }
        }

        private void FindPairedDevices_Click(object sender, RoutedEventArgs e)
        {
            RefreshPairedDevicesList();
        }

        /// <summary>
        /// Asynchronous call to re-populate the ListBox of paired devices.
        /// </summary>
        private async void RefreshPairedDevicesList()
        {
            FindPairedDevices.IsEnabled = false;
            ConnectingIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            try
            {
                // Search for all paired devices
                string genericUUID = GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.GenericAccess);
                var peers = await DeviceInformation.FindAllAsync(genericUUID);

                // By clearing the backing data, we are effectively clearing the ListBox
                _pairedDevices.Clear();

                if (peers.Count == 0)
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    // Found paired devices.
                    foreach (var peer in peers)
                    {
                        BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromIdAsync(peer.Id);
                        if (bleDevice.Name == "MyTeletouch")
                            _pairedDevices.Add(new PairedDeviceInfo(bleDevice));
                    }
                }
            }
            catch /*(Exception ex)*/
            {
            }
            finally
            {
                FindPairedDevices.IsEnabled = true;
                ConnectingIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                EmptyIndicator.Visibility = _pairedDevices.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async void ConnectToDevice(PairedDeviceInfo deviceInfo)
        {
            FindPairedDevices.IsEnabled = false;
            ConnectingIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;

            await (App.Current as App).MainController.SetDevice(deviceInfo);

            if (!(App.Current as App).MainController.IsConnected)
                await new MessageDialog("Connection Failed. Please make sure that your bluetooth device is plugged in properly.").ShowAsync();

            PairedDevicesList.SelectedItem = null;
            FindPairedDevices.IsEnabled = true;
            ConnectingIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void PairedDevicesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check whether the user has selected a device
            if (PairedDevicesList.SelectedItem != null)
            {
                PairedDeviceInfo pdi = PairedDevicesList.SelectedItem as PairedDeviceInfo;
                ConnectToDevice(pdi);
            }
        }

        private async void FindInStore_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.myteletouch.com"));
        }
    }
}
