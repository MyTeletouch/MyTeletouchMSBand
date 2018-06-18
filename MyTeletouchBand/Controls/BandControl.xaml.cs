using Microsoft.Band;
using Microsoft.Band.Personalization;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MyTeletouchBand.Controls
{
    public sealed partial class BandControl : UserControl
    {
        private IBandClient bandClient;
        private static readonly Guid myTileId = new Guid("94D7F0B7-0874-43E3-A570-3A9884C32DB1");
        private static readonly Guid pageId = new Guid("9689E8B5-BC4F-4725-BE5E-C9F0646E5927");
        private static readonly SolidColorBrush brushSelected = new SolidColorBrush() { Color = Colors.White, Opacity = 0.75 };
        private static readonly SolidColorBrush brushNotSelected = new SolidColorBrush() { Color = Colors.Transparent };

        private bool isMouse = true;
        private bool isKeepDown = false;
        private bool isAutoOnOff = false;
        private bool isButton1Down = true;
        private bool isButton2Down = true;
        private bool isButton3Down = true;
        private Point offsetXY = new Point();
        private DateTime lastMove;
        private int deleyMoveMs = 50;

        public BandControl()
        {
            this.InitializeComponent();
            this.ButtonMouse.Background = this.isMouse ? brushSelected : brushNotSelected;
            this.ButtonJoystick.Background = !this.isMouse ? brushSelected : brushNotSelected;
        }

        internal async void Initialize()
        {
            var bandManager = BandClientManager.Instance;
            var pairedBands = await bandManager.GetBandsAsync();

            try
            {
                if (pairedBands.Length < 1)
                {
                    info.Text = "Could not connect";
                    return;
                }

                bandClient = await bandManager.ConnectAsync(pairedBands[0]);
                info.Text = "Connected to Band";
                lastMove = DateTime.Now;

                // Create a Tile with a TextButton on it.
                BandTile myTile = new BandTile(myTileId)
                {
                    Name = "My Teletouch",
                    TileIcon = await LoadIcon("ms-appx:///Assets/BandTileIconLarge.png"),
                    SmallIcon = await LoadIcon("ms-appx:///Assets/BandTileIconSmall.png")
                };
                myTile.AdditionalIcons.Add(await LoadIcon("ms-appx:///Assets/left.png"));
                myTile.AdditionalIcons.Add(await LoadIcon("ms-appx:///Assets/center.png"));
                myTile.AdditionalIcons.Add(await LoadIcon("ms-appx:///Assets/right.png"));
                myTile.AdditionalIcons.Add(await LoadIcon("ms-appx:///Assets/switch.png"));
                myTile.AdditionalIcons.Add(await LoadIcon("ms-appx:///Assets/switch1.png"));
                myTile.AdditionalIcons.Add(await LoadIcon("ms-appx:///Assets/switch2.png"));

                // get the current theme from the Band     
                BandTheme theme = await bandClient.PersonalizationManager.GetThemeAsync();
                BandColor colorButton = theme.Base;
                BandColor colorButtonPressed = new BandColor(0xFF, 0xFF, 0xFF);
                PageRect rect1 = new PageRect(5, 0, 60, 110);
                PageRect rect2 = new PageRect(70, 0, 60, 110);
                PageRect rect3 = new PageRect(135, 0, 60, 110);
                PageRect rect4 = new PageRect(200, 0, 40, 35);
                PageRect rect5 = new PageRect(200, 37, 40, 35);
                PageRect rect6 = new PageRect(200, 75, 40, 35);

                FilledButton buttonLeft = new FilledButton() { ElementId = 1, Rect = rect1, BackgroundColor = colorButton };
                FilledButton buttonCenter = new FilledButton() { ElementId = 2, Rect = rect2, BackgroundColor = colorButton };
                FilledButton buttonRight = new FilledButton() { ElementId = 3, Rect = rect3, BackgroundColor = colorButton };
                FilledButton buttonSwitch = new FilledButton() { ElementId = 4, Rect = rect4, BackgroundColor = colorButton };
                FilledButton buttonLock = new FilledButton() { ElementId = 5, Rect = rect5, BackgroundColor = colorButton };
                FilledButton buttonOnOff = new FilledButton() { ElementId = 6, Rect = rect6, BackgroundColor = colorButton };
                Icon iconLeft = new Icon() { ElementId = 7, Rect = rect1, Color = colorButtonPressed, HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center, VerticalAlignment = Microsoft.Band.Tiles.Pages.VerticalAlignment.Center };
                Icon iconCenter = new Icon() { ElementId = 8, Rect = rect2, Color = colorButtonPressed, HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center, VerticalAlignment = Microsoft.Band.Tiles.Pages.VerticalAlignment.Center };
                Icon iconRight = new Icon() { ElementId = 9, Rect = rect3, Color = colorButtonPressed, HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center, VerticalAlignment = Microsoft.Band.Tiles.Pages.VerticalAlignment.Center };
                Icon iconSwitch = new Icon() { ElementId = 10, Rect = rect4, Color = colorButtonPressed, HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center, VerticalAlignment = Microsoft.Band.Tiles.Pages.VerticalAlignment.Center };
                Icon iconLock = new Icon() { ElementId = 11, Rect = rect5, Color = colorButtonPressed, HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center, VerticalAlignment = Microsoft.Band.Tiles.Pages.VerticalAlignment.Center };
                Icon iconOnOff = new Icon() { ElementId = 12, Rect = rect6, Color = colorButtonPressed, HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center, VerticalAlignment = Microsoft.Band.Tiles.Pages.VerticalAlignment.Center };

                FilledPanel panel = new FilledPanel(iconLeft, iconCenter, iconRight, iconSwitch, iconLock, iconOnOff, 
                    buttonLeft, buttonCenter, buttonRight, buttonSwitch, buttonLock, buttonOnOff)
                {
                    Rect = new PageRect(0, 0, 240, 110),
                    HorizontalAlignment = Microsoft.Band.Tiles.Pages.HorizontalAlignment.Center
                    
                };
                myTile.PageLayouts.Add(new PageLayout(panel));

                // Remove the Tile from the Band, if present. An application won't need to do this everytime it runs. 
                // But in case you modify this sample code and run it again, let's make sure to start fresh.
                //await bandClient.TileManager.RemoveTileAsync(myTileId);

                // Create the Tile on the Band.
                await bandClient.TileManager.AddTileAsync(myTile);
                await bandClient.TileManager.SetPagesAsync(myTileId, new PageData(pageId, 0, 
                    new FilledButtonData(1, colorButtonPressed),
                    new FilledButtonData(2, colorButtonPressed),
                    new FilledButtonData(3, colorButtonPressed),
                    new FilledButtonData(4, colorButtonPressed),
                    new FilledButtonData(5, colorButtonPressed),
                    new FilledButtonData(6, colorButtonPressed),
                    new IconData(7, 2),
                    new IconData(8, 3),
                    new IconData(9, 4),
                    new IconData(10, 5),
                    new IconData(11, 6),
                    new IconData(12, 7)));

                // Subscribe to events 
                bandClient.TileManager.TileOpened += EventHandler_TileOpened;
                bandClient.TileManager.TileClosed += EventHandler_TileClosed;
                bandClient.TileManager.TileButtonPressed += EventHandler_TileButtonPressed;
                bandClient.SensorManager.Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;

               // Start listening for events 
               await bandClient.TileManager.StartReadingsAsync(); 
            }
            catch (BandException bandException)
            {
                Debug.WriteLine(bandException.Message);
                info.Text = "Could not connect";
            }
        }

        private async Task<BandIcon> LoadIcon(string uri)
        {
            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }

        private async void EventHandler_TileOpened(object sender, BandTileEventArgs<IBandTileOpenedEvent> e)
        {    // This method is called when the user taps on our Band tile    
             //    
             // e.TileEvent.TileId is the tile’s Guid    
             // e.TileEvent.Timestamp is the DateTimeOffset of the event    
             //    
             // handle the event 

            await bandClient.SensorManager.Gyroscope.StartReadingsAsync();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                info1.Text = "Tile Opened";
            });
        }

        private async void EventHandler_TileClosed(object sender, BandTileEventArgs<IBandTileClosedEvent> e) 
        {    // This method is called when the user exits our Band tile    
             //    
             // e.TileEvent.TileId is the tile’s Guid    
             // e.TileEvent.Timestamp is the DateTimeOffset of the event 

            //    
            // handle the event 
            if (isAutoOnOff)
                await bandClient.SensorManager.Gyroscope.StopReadingsAsync();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                info1.Text = "Tile Closed";
            });
        }

        private async void EventHandler_TileButtonPressed(object sender, BandTileEventArgs<IBandTileButtonPressedEvent> e)
        {
            // This method is called when the user presses the    
            // button in our tile’s layout    
            //    
            // e.TileEvent.TileId is the tile’s Guid    
            // e.TileEvent.Timestamp is the DateTimeOffset of the event    
            // e.TileEvent.PageId is the Guid of our page with the button    
            // e.TileEvent.ElementId is the value assigned to the button    
            //                       in our layout, i.e.    
            //                       TilePageElementId.Button_PushMe    
            //    
            // handle the event 
            if (e.TileEvent.PageId == pageId)
            {
                switch (e.TileEvent.ElementId)
                {
                    case 1:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {

                            info2.Text = "Left pressed";
                            isButton1Down = !isButton1Down;

                            if (isMouse)
                                (App.Current as App).MainController.SendMouseData(offsetXY, isButton1Down, isButton2Down, isButton3Down, true);
                            else
                                (App.Current as App).MainController.SendJoystickData(offsetXY, isButton1Down, isButton2Down, isButton3Down, false, false, true);

                            if (!isKeepDown)
                            {
                                isButton1Down = false;
                                if (isMouse)
                                    (App.Current as App).MainController.SendMouseData(offsetXY, isButton1Down, isButton2Down, isButton3Down, true);
                                else
                                    (App.Current as App).MainController.SendJoystickData(offsetXY, isButton1Down, isButton2Down, isButton3Down, false, false, true);
                            }
                        });
                        break;
                    case 2:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            info2.Text = "Center pressed";
                            isButton2Down = !isButton2Down;

                            if (isMouse)
                                (App.Current as App).MainController.SendMouseData(offsetXY, isButton1Down, isButton2Down, isButton3Down, true);
                            else
                                (App.Current as App).MainController.SendJoystickData(offsetXY, isButton1Down, isButton2Down, isButton3Down, false, false, true);

                            if (!isKeepDown)
                            {
                                isButton2Down = false;
                                if (isMouse)
                                    (App.Current as App).MainController.SendMouseData(offsetXY, isButton1Down, isButton2Down, isButton3Down, true);
                                else
                                    (App.Current as App).MainController.SendJoystickData(offsetXY, isButton1Down, isButton2Down, isButton3Down, false, false, true);
                            }
                        });
                        break;
                    case 3:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            info2.Text = "Right pressed";
                            isButton3Down = !isButton3Down;

                            if (isMouse)
                                (App.Current as App).MainController.SendMouseData(offsetXY, isButton1Down, isButton2Down, isButton3Down, true);
                            else
                                (App.Current as App).MainController.SendJoystickData(offsetXY, isButton1Down, isButton2Down, isButton3Down, false, false, true);

                            if (!isKeepDown)
                            {
                                isButton3Down = false;
                                if (isMouse)
                                    (App.Current as App).MainController.SendMouseData(offsetXY, isButton1Down, isButton2Down, isButton3Down, true);
                                else
                                    (App.Current as App).MainController.SendJoystickData(offsetXY, isButton1Down, isButton2Down, isButton3Down, false, false, true);
                            }
                        });
                        break;
                    case 4:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            info2.Text = "Switch pressed";
                            isMouse = !isMouse;
                            this.ButtonMouse.Background = this.isMouse ? brushSelected : brushNotSelected;
                            this.ButtonJoystick.Background = !this.isMouse ? brushSelected : brushNotSelected;
                        });
                        break;
                    case 5:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            info2.Text = "Lock pressed";
                            toggleSwitchLock.IsOn = !toggleSwitchLock.IsOn;
                            isKeepDown = toggleSwitchLock.IsOn;
                        });
                        break;
                    case 6:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            info2.Text = "On/Off pressed";
                            toggleSwitchAutoOnOff.IsOn = !toggleSwitchAutoOnOff.IsOn;
                            isAutoOnOff = toggleSwitchAutoOnOff.IsOn;
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        private async void Gyroscope_ReadingChanged(object sender, Microsoft.Band.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Sensors.IBandGyroscopeReading> e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                info3.Text = string.Format("X:{0:0.00}", e.SensorReading.AccelerationX);
                info4.Text = string.Format("Y:{0:0.00}", e.SensorReading.AccelerationY);
                info5.Text = string.Format("Z:{0:0.00}", e.SensorReading.AccelerationZ);

                if (isMouse)
                {
                    if (Math.Abs(e.SensorReading.AccelerationX) > 0.5)
                        offsetXY.Y = 30 * e.SensorReading.AccelerationX;
                    else if (Math.Abs(e.SensorReading.AccelerationX) > 0.35)
                        offsetXY.Y = 10 * e.SensorReading.AccelerationX;
                    else
                        offsetXY.Y = 0;

                    if (Math.Abs(e.SensorReading.AccelerationY) > 0.5)
                        offsetXY.X = -30 * e.SensorReading.AccelerationY;
                    else if (Math.Abs(e.SensorReading.AccelerationY) > 0.35)
                        offsetXY.X = -10 * e.SensorReading.AccelerationY;
                    else
                        offsetXY.X = 0;
                    TimeSpan ts = DateTime.Now - lastMove;
                    if (ts.TotalMilliseconds > deleyMoveMs)
                    {
                        lastMove = DateTime.Now;
                        (App.Current as App).MainController.SendMouseData(offsetXY, isButton1Down, isButton2Down, isButton3Down, false);
                    }
                }
                else
                {
                    if (Math.Abs(e.SensorReading.AccelerationX) > 0.25)
                        offsetXY.Y = 127 * e.SensorReading.AccelerationX;
                    else
                        offsetXY.Y = 0;

                    if (Math.Abs(e.SensorReading.AccelerationY) > 0.25)
                        offsetXY.X = -127 * e.SensorReading.AccelerationY;
                    else
                        offsetXY.X = 0;

                    TimeSpan ts = DateTime.Now - lastMove;
                    if (ts.TotalMilliseconds > deleyMoveMs)
                    {
                        lastMove = DateTime.Now;
                        (App.Current as App).MainController.SendJoystickData(offsetXY, isButton1Down, isButton2Down, isButton3Down, false, false, false);
                    }
                }
            });
        }

        private void ButtonMouse_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.isMouse = true;
            this.ButtonMouse.Background = this.isMouse ? brushSelected : brushNotSelected;
            this.ButtonJoystick.Background = !this.isMouse ? brushSelected : brushNotSelected;
        }

        private void ButtonJoystick_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.isMouse = false;
            this.ButtonMouse.Background = this.isMouse ? brushSelected : brushNotSelected;
            this.ButtonJoystick.Background = !this.isMouse ? brushSelected : brushNotSelected;
        }

        private void toggleSwitchLock_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.isKeepDown = toggleSwitchLock.IsOn;
        }

        private void toggleSwitchAutoOnOff_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.isAutoOnOff = toggleSwitchAutoOnOff.IsOn;
        }
    }
}
