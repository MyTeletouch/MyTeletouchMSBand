using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Display;

namespace MyTeletouchBand
{
    public static class KeepDisplayVisible
    {
        private static DisplayRequest displayRequest;
        private static int drCount = 0;

        public static void StartDisplayRequest()
        {
            try
            {
                if (displayRequest == null)
                {
                    // This call creates an instance of the displayRequest object 
                    displayRequest = new DisplayRequest();
                }
            }
            catch /*(Exception ex)*/
            {
                //rootPage.NotifyUser("Error Creating Display Request: " + ex.Message, NotifyType.ErrorMessage);
            }

            if (displayRequest != null)
            {
                try
                {
                    // This call activates a display-required request. If successful,  
                    // the screen is guaranteed not to turn off automatically due to user inactivity. 
                    displayRequest.RequestActive();
                    drCount += 1;
                    //rootPage.NotifyUser("Display request activated (" + drCount + ")", NotifyType.StatusMessage);
                }
                catch /*(Exception ex)*/
                {
                    //rootPage.NotifyUser("Error: " + ex.Message, NotifyType.ErrorMessage);
                }
            }
        }

        public static void StopDisplayRequest()
        {
            if (displayRequest != null)
            {
                try
                {
                    // This call de-activates the display-required request. If successful, the screen 
                    // might be turned off automatically due to a user inactivity, depending on the 
                    // power policy settings of the system. The requestRelease method throws an exception  
                    // if it is called before a successful requestActive call on this object. 
                    displayRequest.RequestRelease();
                    drCount -= 1;
                    //rootPage.NotifyUser("Display request released (" + drCount + ")", NotifyType.StatusMessage);
                }
                catch /*(Exception ex)*/
                {
                    //rootPage.NotifyUser("Error: " + ex.Message, NotifyType.ErrorMessage);
                }
            }
        }
    }
}
