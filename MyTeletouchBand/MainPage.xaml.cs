using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace MyTeletouchBand
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ActivePageType activePageType;

        public MainPage()
        {
            this.InitializeComponent();

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (App.Current as App).MainController.SetDispatcher(Dispatcher);
            (App.Current as App).MainController.ConnectedChanged += MainController_ConnectedChanged;
        }

        void MainController_ConnectedChanged(object sender, bool connected)
        {
            if (connected)
                ChangeActivePage(ActivePageType.Mouse);
            else
                ChangeActivePage(ActivePageType.Connect);
        }

        private void ChangeActivePage(ActivePageType newPageType)
        {
            this.activePageType = newPageType;

            this.ConnectControl.Visibility = (newPageType == ActivePageType.Connect ||
                newPageType == ActivePageType.Connecting ||
                newPageType == ActivePageType.ConnectNoBluetooth) ? Visibility.Visible : Visibility.Collapsed;
            this.BandControl.Visibility = (newPageType == ActivePageType.Mouse) ? Visibility.Visible : Visibility.Collapsed;
            //this.MouseControl.Visibility = (newPageType == ActivePageType.Mouse) ? Visibility.Visible : Visibility.Collapsed;
            //this.KeyboardControl.Visibility = (newPageType == ActivePageType.Keyboard) ? Visibility.Visible : Visibility.Collapsed;
            //this.JoystickControl.Visibility = (newPageType == ActivePageType.Joystick) ? Visibility.Visible : Visibility.Collapsed;
            //this.NavBar.Visibility = (newPageType == ActivePageType.Connect ||
            //    newPageType == ActivePageType.Connecting ||
            //    newPageType == ActivePageType.ConnectNoBluetooth) ? Visibility.Collapsed : Visibility.Visible;

            //this.ButtonMouse.Background = (newPageType == ActivePageType.Mouse) ? new SolidColorBrush() { Color = Colors.White, Opacity = 0.75 } : new SolidColorBrush() { Color = Colors.Transparent };
            //this.ButtonKeyboard.Background = (newPageType == ActivePageType.Keyboard) ? new SolidColorBrush() { Color = Colors.White, Opacity = 0.75 } : new SolidColorBrush() { Color = Colors.Transparent };
            //this.ButtonJoystick.Background = (newPageType == ActivePageType.Joystick) ? new SolidColorBrush() { Color = Colors.White, Opacity = 0.75 } : new SolidColorBrush() { Color = Colors.Transparent };

            switch (newPageType)
            {
                case ActivePageType.Connect:
                case ActivePageType.ConnectNoBluetooth:
                case ActivePageType.Connecting:
                    KeepDisplayVisible.StopDisplayRequest();
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                    this.ConnectControl.Initialize(newPageType);
                    break;
                case ActivePageType.Mouse:
                    KeepDisplayVisible.StartDisplayRequest();
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                    this.BandControl.Initialize();
                    break;
                //case ActivePageType.Keyboard:
                //    KeepDisplayVisible.StartDisplayRequest();
                //    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                //    this.KeyboardControl.Initialize();
                //    break;
                //case ActivePageType.Joystick:
                //    KeepDisplayVisible.StartDisplayRequest();
                //    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                //    //this.JoystickControl.Initialize(newPageType);
                //    break;
                default:
                    break;
            }
        }
    }
}
