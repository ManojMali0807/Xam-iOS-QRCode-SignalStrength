
using UIKit;
using ZXing;
using ZXing.Mobile;

using System;
using System.Threading.Tasks;
using Foundation;

namespace QRCodeScanner
{
    public partial class ViewController : UIViewController
    {
        UIViewController appController;

        IScannerViewController viewController;
        //CustomScannerViewController viewController;

        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        void HandleScanResult(Result result)
        {
            string msg = "";

            if (result != null && !string.IsNullOrEmpty(result.Text))
                msg = "Found QRCode: " + result.Text;
            else
                msg = "Scanning Canceled!";

            this.InvokeOnMainThread(() =>
            {
                using (var av = new UIAlertView("QRCode Result", msg, null, "OK", null))
                {
                    av.Show();
                }
            });
        }

        partial void UIButton197_TouchUpInside(UIButton sender)
        {
            try
            {
                // CODE to get current view controller
                foreach (var window in UIApplication.SharedApplication.Windows)
                {
                    if (window.RootViewController != null)
                    {
                        appController = window.RootViewController;
                        break;
                    }
                }

                viewController = new CustomScannerViewController(new MobileBarcodeScanningOptions(), new MobileBarcodeScanner());
                appController.PresentViewController((UIViewController)viewController, true, null);

                viewController.OnScannedResult += barcodeResult =>
                {

                    ((UIViewController)viewController).InvokeOnMainThread(() =>
                    {

                        viewController.Cancel();

                        // Handle error situation that occurs when user manually closes scanner in the same moment that a QR code is detected
                        try
                        {
                            ((UIViewController)viewController).DismissViewController(true, () =>
                            {
                                //HandleScanResult(barcodeResult);
                            });
                        }
                        catch (ObjectDisposedException)
                        {
                            // In all likelihood, iOS has decided to close the scanner at this point. But just in case it executes the
                            // post-scan code instead, set the result so we will not get a NullReferenceException.
                            //HandleScanResult(barcodeResult);
                        }
                    });

                    HandleScanResult(barcodeResult);
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION- " + ex.Message);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            GetSignalStrength();
        }

        private void GetSignalStrength()
        {
            var application = UIApplication.SharedApplication;
            var statusBarView = application.ValueForKey(new NSString("statusBar")) as UIView;
            var foregroundView = statusBarView.ValueForKey(new NSString("foregroundView")) as UIView;

            UIView dataNetworkItemView = null;
            foreach (UIView subview in foregroundView.Subviews)
            {
                if ("UIStatusBarSignalStrengthItemView" == subview.Class.Name)
                {
                    dataNetworkItemView = subview;
                    break;
                }
            }
            if (null == dataNetworkItemView)
            {
                lblRange.Text = "NO SERVICE";
                return;
            }
            var abc = dataNetworkItemView.GetEnumerator();
            int bars = ((NSNumber)dataNetworkItemView.ValueForKey(new NSString("signalStrengthBars"))).Int32Value;

            lblRange.Text = string.Format("Signal bars are: {0}", bars);
        }
    }
}
