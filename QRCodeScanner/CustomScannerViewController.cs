
/*
 *  @Author                 : Manojkumar Mali
 *  @Description        : Cutomised scanner view controller
 *  @FileName           : CustomScannerViewController.cs
 *  @Date                   : 21 Jan 2019
 *  @LastModified       : 21 Jan 2019 [File created]
*/

using UIKit;
using System;
using CoreGraphics;
using ZXing.Mobile;
using System.Threading.Tasks;

namespace QRCodeScanner
{
    public class CustomScannerViewController : UIViewController, IScannerViewController
    {
        #region VARIABLEs AND PROPERTIes
        private UIView loadingBg;
        private UIButton btnDone;
        private UIView parentView;
        private UIView contactView;
        private UITextField txtContactNumber;
        private ZXingScannerView scannerView;
        private UIActivityIndicatorView loadingView;
        private UIStatusBarStyle originalStatusBarStyle = UIStatusBarStyle.Default;

        public bool ContinuousScanning { get; set; }
        public UIView CustomLoadingView { get; set; }
        public MobileBarcodeScanner Scanner { get; set; }
        public MobileBarcodeScanningOptions ScanningOptions { get; set; }

        public event Action<ZXing.Result> OnScannedResult;
        #endregion

        #region CONSTRUCTOR
        public CustomScannerViewController(MobileBarcodeScanningOptions options, MobileBarcodeScanner scanner)
        {
            options.PossibleFormats = new System.Collections.Generic.List<ZXing.BarcodeFormat>
            {
            ZXing.BarcodeFormat.QR_CODE, ZXing.BarcodeFormat.EAN_13,ZXing.BarcodeFormat.EAN_8
            };

            this.ScanningOptions = options;
            this.Scanner = scanner;

            CGRect appFrame = UIScreen.MainScreen.Bounds;

            this.View.Frame = new CGRect(0, 0, appFrame.Width, appFrame.Height);
            this.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
        }
        #endregion

        #region METHODs
        /// <summary>
        /// Method to handle DONE button click event
        /// </summary>
        private void BtnDone_TouchUpInside(object sender, EventArgs e)
        {
            if (txtContactNumber != null)
            {
                scannerView.StopScanning();

                if (txtContactNumber != null)
                    this.OnScannedResult?.Invoke(new ZXing.Result(txtContactNumber.Text.Trim(), null, null, ZXing.BarcodeFormat.QR_CODE));
                else
                    this.OnScannedResult?.Invoke(new ZXing.Result(string.Empty, null, null, ZXing.BarcodeFormat.QR_CODE));
            }
        }

        /// <summary>
        /// Method to handle design changes on scanner setup completion process.
        /// Hides loading view from screen
        /// </summary>
        private void HandleOnScannerSetupComplete()
        {
            BeginInvokeOnMainThread(() =>
            {
                if (loadingView != null && loadingBg != null && loadingView.IsAnimating)
                {
                    loadingView.StopAnimating();

                    UIView.BeginAnimations("zoomout");

                    UIView.SetAnimationDuration(2.0f);
                    UIView.SetAnimationCurve(UIViewAnimationCurve.EaseOut);

                    loadingBg.Transform = CGAffineTransform.MakeScale(2.0f, 2.0f);
                    loadingBg.Alpha = 0.0f;

                    UIView.CommitAnimations();

                    loadingBg.RemoveFromSuperview();
                }
            });
        }
        #endregion

        #region OVERRIDEN METHODs
        public UIViewController AsViewController()
        {
            return this;
        }

        /// <summary>
        /// Cancel this instance of view controller.
        /// </summary>
        public void Cancel()
        {
            InvokeOnMainThread(scannerView.StopScanning);
            InvokeOnMainThread(() =>
            {
                // Calling with animated:true here will result in a blank screen when the scanner is closed on iOS 7.
                DismissViewController(false, null);
            });
        }

        public void Torch(bool on)
        {
            if (scannerView != null)
                scannerView.Torch(on);
        }

        public void ToggleTorch()
        {
            if (scannerView != null)
                scannerView.ToggleTorch();
        }

        public void PauseAnalysis()
        {
            scannerView.PauseAnalysis();
        }

        public void ResumeAnalysis()
        {
            scannerView.ResumeAnalysis();
        }

        public bool IsTorchOn
        {
            get { return scannerView.IsTorchOn; }
        }

        /// <summary>
        /// Views the did load.
        /// Contains code to create view on controller
        /// </summary>
        public override void ViewDidLoad()
        {
            // Loading view design
            loadingBg = new UIView(this.View.Frame) { BackgroundColor = UIColor.Black, AutoresizingMask = UIViewAutoresizing.FlexibleDimensions };

            // Activity indicator view design
            loadingView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleMargins
            };
            loadingView.Frame = new CGRect((this.View.Frame.Width - loadingView.Frame.Width) / 2,
                (this.View.Frame.Height - loadingView.Frame.Height) / 2,
                loadingView.Frame.Width,
                loadingView.Frame.Height);

            loadingBg.AddSubview(loadingView);
            View.AddSubview(loadingBg);
            loadingView.StartAnimating();

            // Parent view design which contains 2 subviews;
            // 1- Contact and button view
            // 2- Scanner view
            parentView = new UIView(new CGRect(0, 0, View.Frame.Width, View.Frame.Height));

            // Contact view design
            contactView = new UIView(new CGRect(0, 0, View.Frame.Width, View.Frame.Height / 4))
            {
                BackgroundColor = UIColor.White
            };

            txtContactNumber = new UITextField
            {
                Frame = new CGRect(contactView.Frame.X + 20, (contactView.Frame.Height / 2) - 20, (contactView.Frame.Width - contactView.Frame.Width / 3), 40),
                BackgroundColor = UIColor.White,
                AutocorrectionType = UITextAutocorrectionType.No,
                SpellCheckingType = UITextSpellCheckingType.No,
                Placeholder = "Contact number",
                BorderStyle = UITextBorderStyle.RoundedRect
            };
            txtContactNumber.Layer.BorderWidth = 0.5f;
            txtContactNumber.Layer.CornerRadius = 5;

            btnDone = new UIButton
            {
                Frame = new CGRect(txtContactNumber.Frame.Width + 40, (contactView.Frame.Height / 2) - 20, 70, 40),
                BackgroundColor = UIColor.Blue
            };
            btnDone.SetTitle("Done", UIControlState.Normal);
            btnDone.Layer.BorderWidth = 0.0f;
            btnDone.Layer.CornerRadius = 5;
            btnDone.TouchUpInside += BtnDone_TouchUpInside;

            contactView.AddSubview(txtContactNumber);
            contactView.AddSubview(btnDone);

            // Scanner view design
            scannerView = new ZXingScannerView(new CGRect(0, contactView.Frame.Height, View.Frame.Width, View.Frame.Height - contactView.Frame.Height))
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
                UseCustomOverlayView = this.Scanner.UseCustomOverlay,
                CustomOverlayView = this.Scanner.CustomOverlay,
                TopText = this.Scanner.TopText,
                BottomText = this.Scanner.BottomText,
                CancelButtonText = this.Scanner.CancelButtonText,
                FlashButtonText = this.Scanner.FlashButtonText
            };

            scannerView.OnCancelButtonPressed += Cancel;

            parentView.AddSubview(contactView);
            parentView.AddSubview(scannerView);

            //this.View.AddSubview(scannerView);
            this.View.InsertSubviewBelow(parentView, loadingView);

            this.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
        }

        public override void ViewDidAppear(bool animated)
        {
            scannerView.OnScannerSetupComplete += HandleOnScannerSetupComplete;

            originalStatusBarStyle = UIApplication.SharedApplication.StatusBarStyle;

            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
                SetNeedsStatusBarAppearanceUpdate();
            }
            else
                UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);

            // Start scanning
            Task.Factory.StartNew(() =>
           {
               BeginInvokeOnMainThread(() => scannerView.StartScanning(result =>
               {
                   // If scanner stop the scanning
                   if (!ContinuousScanning)
                   {
                       scannerView.StopScanning();
                   }

                   this.OnScannedResult?.Invoke(result);

               }, this.ScanningOptions));
           });
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (scannerView != null)
                scannerView.StopScanning();

            scannerView.OnScannerSetupComplete -= HandleOnScannerSetupComplete;
        }

        public override void ViewWillDisappear(bool animated)
        {
            UIApplication.SharedApplication.SetStatusBarStyle(originalStatusBarStyle, false);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            if (scannerView != null)
                scannerView.DidRotate(this.InterfaceOrientation);
        }

        public override bool ShouldAutorotate()
        {
            if (ScanningOptions.AutoRotate != null)
            {
                return (bool)ScanningOptions.AutoRotate;
            }
            return false;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.All;
        }

        [Obsolete("Deprecated in iOS6. Replace it with both GetSupportedInterfaceOrientations and PreferredInterfaceOrientationForPresentation")]
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            if (ScanningOptions.AutoRotate != null)
            {
                return (bool)ScanningOptions.AutoRotate;
            }
            return false;
        }
        #endregion
    }
}
