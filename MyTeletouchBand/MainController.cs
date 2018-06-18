using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;

namespace MyTeletouchBand
{
    public class MainController
    {
        private PairedDeviceInfo device;
        private int lastCommandId;
        private int lastProcessedCommandId;
        private ConcurrentQueue<CommandData> commandQueue;
        public event EventHandler<bool> ConnectedChanged;
        public CoreDispatcher dispatcher;

        public MainController()
        {
            commandQueue = new ConcurrentQueue<CommandData>();
        }

        public void SetDispatcher(CoreDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }


        public bool UseGyrometerForMouse
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values.ContainsKey("UseGyrometerForMouse") &&
                    (bool)ApplicationData.Current.LocalSettings.Values["UseGyrometerForMouse"];
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values["UseGyrometerForMouse"] = value;
            }
        }

        public bool UseAccelerometerForJoystick
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values.ContainsKey("UseAccelerometerForJoystick") &&
                    (bool)ApplicationData.Current.LocalSettings.Values["UseAccelerometerForJoystick"];
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values["UseAccelerometerForJoystick"] = value;
            }
        }

        public bool ConnectAutomatically
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values.ContainsKey("ConnectAutomatically") &&
                    (bool)ApplicationData.Current.LocalSettings.Values["ConnectAutomatically"];
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values["ConnectAutomatically"] = value;
            }
        }

        internal void SendKeyboadData(byte[] pressedKeys)
        {
            if (!this.IsConnected)
                return;

            byte[] data = new byte[4];
            data[0] = 1;
            data[1] = pressedKeys[0];
            data[2] = pressedKeys[1];
            data[3] = pressedKeys[2];

            commandQueue.Enqueue(new CommandData(data, data[3] == 0));
        }

        internal void SendMouseData(Point offsetXY, bool leftButtonDown, bool middleButtonDown, bool rightButtonDown, bool ensureSuccess)
        {
            if (!this.IsConnected)
                return;

            byte[] data = new byte[4];
            data[0] = 2;
            data[1] = getMouseButtons(leftButtonDown, middleButtonDown, rightButtonDown);
            data[2] = (byte)offsetXY.X;//X
            data[3] = (byte)offsetXY.Y;//Y

            if (ensureSuccess || commandQueue.Count < 3)
                commandQueue.Enqueue(new CommandData(data, ensureSuccess));
        }

        internal void SendJoystickData(Point offsetXY, bool button1Down, bool button2Down, bool button3Down, bool button4Down, bool button5Down, bool ensureSuccess)
        {
            if (!this.IsConnected)
                return;

            byte[] data = new byte[4];
            data[0] = 3;
            data[1] = (byte)(127 + (int)offsetXY.X);
            data[2] = (byte)(127 + (int)offsetXY.Y);
            data[3] = getJoystickButtons(button1Down, button2Down, button3Down, button4Down, button5Down);

            if (ensureSuccess || commandQueue.Count < 3)
                commandQueue.Enqueue(new CommandData(data, ensureSuccess));
        }

        private async void ProcessCommandStack()
        {
            while (IsConnected)
            {
                if (this.commandQueue.Count == 0)
                {
                    await Task.Delay(100);
                    continue;
                }

                CommandData commandData;
                if (this.commandQueue.TryDequeue(out commandData))
                {
                    await sendCommandData(commandData.Data, commandData.EnsureSuccess);
                }
            }
        }

        private async Task sendCommandData(byte[] commandData, bool ensureSuccess)
        {
            if (this.IsConnected)
            {
                try
                {
                    lastCommandId++;
                    if (lastCommandId > 255)
                        lastCommandId = 0;

                    StringBuilder sbCmd = new StringBuilder();
                    sbCmd.AppendFormat("{0}", (int)commandData[0]);
                    sbCmd.AppendFormat("|{0}", lastCommandId);

                    for (int i = 1; i < commandData.Length; i++)
                    {
                        bool isZero = commandData[i] == 0;
                        sbCmd.AppendFormat("|{0}", isZero ? "0" : commandData[i].ToString("X2"));
                    }

                    sbCmd.AppendFormat("|{0}]", lastCommandId);

                    for (int retry = 0; retry < 2 && lastProcessedCommandId != lastCommandId; retry++)
                    {
                        if (!await sendCommandAsync(sbCmd, ensureSuccess))
                        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            {
                                commandQueue = new ConcurrentQueue<CommandData>();
                                ConnectedChanged(this, false);
                            });

                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private async Task<bool> sendCommandAsync(StringBuilder sbCmd, bool ensureSuccess)
        {
            device.LastCommandId = lastCommandId;
            bool result = await this.device.SendBLEData(sbCmd);

            if (ensureSuccess)
            {
                await Task.Delay(Constants.ResponseDeleyMs);
                for (int waitResponse = 0; waitResponse < 4 && device.LastProcessedCommandId != lastCommandId; waitResponse++)
                    await Task.Delay(Constants.ResponseDeleyMs);
                lastProcessedCommandId = this.device.LastProcessedCommandId;
            }
            else
                lastProcessedCommandId = lastCommandId;

            return result;
        }

        internal async Task SetDevice(PairedDeviceInfo device)
        {
            try
            {
                this.device = device;
                if (!await this.device.LoadGattServices())
                    this.device = null;
            }
            catch
            {
                this.device = null;
            }

            if (this.IsConnected)
            {
                if (ConnectedChanged != null)
                    ConnectedChanged(this, true);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run((Action)ProcessCommandStack);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                if (ConnectedChanged != null)
                    ConnectedChanged(this, false);
            }
        }

        public bool IsConnected
        {
            get { return this.device != null; }
        }

        private byte getMouseButtons(bool buttonLeft, bool buttonCenter, bool buttonRight)
        {
            byte res = 0x00000000;

            if (buttonLeft)
                res |= 0x01;                                            /* If pressed, mask bit to indicate button press */

            if (buttonRight)
                res |= 0x02;											/* If pressed, mask bit to indicate button press */

            if (buttonCenter)
                res |= 0x04;                                            /* If pressed, mask bit to indicate button press */

            return res;
        }

        private byte getJoystickButtons(bool button1, bool button2, bool button3, bool button4, bool button5)
        {
            byte res = 0x00;

            if (button1)
                res |= 0x01;                                            /* If pressed, mask bit to indicate button press */

            if (button2)
                res |= 0x02;											/* If pressed, mask bit to indicate button press */

            if (button3)
                res |= 0x04;											/* If pressed, mask bit to indicate button press */

            if (button4)
                res |= 0x08;											/* If pressed, mask bit to indicate button press */

            if (button5)
                res |= 0x10;											/* If pressed, mask bit to indicate button press */

            return res;
        }
    }
}
