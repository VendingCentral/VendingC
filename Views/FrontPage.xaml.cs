//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using VendingC.Utilities;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Page = Windows.UI.Xaml.Controls.Page;
using MetroLog;
using MetroLog.Targets;
using System.Threading.Tasks;
using Windows.UI.Core;
using System.IO.Ports;
using Windows.Storage;
using Windows.Storage.Streams;
using VendingC.BAL;
using System.Threading;
using System.Security.Policy;
using System.Timers;
using OpenCvSharp;
using LogLevel = MetroLog.LogLevel;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.ApplicationModel;

namespace VendingC
{
    sealed partial class FrontPage : Page
    {
        public string[] fileArray;
        private static int temp = 0;

        private DispatcherTimer timer;
        private int basetime;

        private DispatcherTimer devTimer;
        private int devBaseTime;
        private int devModeCount;

        private DispatcherTimer syncTimer;
        private int syncBaseTime = 50;

        private DispatcherTimer CreditTimer;
        private int CreditTimerbasetime;

        Bitmap bm = null;
        // static readonly string Machine_Prefix = Convert.ToString(ConfigurationManager.AppSettings["MachinePrefix"]);
        //static string Machine_Prefix = "786110";
        //static string Machine_Prefix = "1";
        double cashAmount = 0;
        bool isPaymentSuccessful = false;
        private float Lat;
        private float Lon;
        private uint? _desireAccuracyInMetersValue;
        private static string Test_prefix = "";
        static SerialPort _serialPort;

        private StorageFile storeFile;
        private IRandomAccessStream stream;
        PaymentCash cashPay = new PaymentCash();
        CValidator Validator = new CValidator();
        AutoComPort autoComPort = new AutoComPort();
        Heartbeat hb = new Heartbeat();
        CancellationTokenSource source = new CancellationTokenSource();
        public string packageVersion;

        //private static string paymentOption = "";

        // public ILogger log;

        internal FrontPageViewModel ViewModel
        {
            private set;
            get;
        }
        public ShoppingCartPageViewModel ViewModel2;

        public FrontPage()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            packageVersion = "Version " + string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget());
            //log = LogManagerFactory.DefaultLogManager.GetLogger<FrontPage>();

            ApplicationData.Current.LocalFolder.CreateFolderAsync("ProductImages");
            ApplicationData.Current.LocalFolder.CreateFolderAsync("Potraits");

            this.ViewModel = new FrontPageViewModel();
            this.InitializeComponent();
            this.ViewModel2 = new ShoppingCartPageViewModel();

            Global.log.Trace("Getting Videos");
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

            mediaPlayer.MediaPlayer.IsMuted = true;
            mediaPlayer.MediaPlayer.MediaEnded += OnMediaEnded;
            mediaPlayer.MediaPlayer.Play();
            fileArray = Directory.GetFiles(Convert.ToString(new Uri("ms-appx://../Assets/Videos/")));
            Global.log.Trace("Getting Videos done");

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += timer_Tick;

            devTimer = new DispatcherTimer();
            devTimer.Interval = new TimeSpan(0, 0, 1);
            devTimer.Tick += devTimer_Tick;

            syncTimer = new DispatcherTimer();
            syncTimer.Interval = new TimeSpan(0, 0, 1);
            syncTimer.Tick += syncTimer_Tick;
            // syncBaseTime = 600;
            syncTimer.Start();

            CreditTimer = new DispatcherTimer();
            CreditTimer.Interval = new TimeSpan(0, 0, 1);
            CreditTimer.Tick += CreditTimer_Tick;


            ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
            formattableTitleBar.BackgroundColor = Colors.Transparent;
            formattableTitleBar.ButtonHoverBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            // Global.MachineID = Convert.ToInt32(userName.Split('-')[0]);
            // Global.MachineCode = userName.Split('-')[1];

            if (!Data.VendingC.GetMachineConfigLocal())
            {
                configtext.Visibility = Visibility.Visible;
                configbut.Visibility = Visibility.Visible;
                updatebut.Visibility = Visibility.Visible;
            }
            else
            {

            }

            // Heartbeat.hb();
            comPortSetUp();

            //Thread thread = new Thread(hb.HeartBeat());
            // thread.Start();

            Task.Factory.StartNew(() => hb.HeartBeat());

            //await hb.HeartBeat();
            //string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //if (!userName.Contains("kiosk"))
            //{
            //    configtext.Visibility = Visibility.Visible;
            //    configbut.Visibility = Visibility.Visible;
            //}

            //Global.log.Trace("Getting location");
            //GetLocation();
            //Global.log.Trace("Location fetched");
            //  ViewModel.ProductList = ProductDatabase.ProductList.Select((product) => new ProductViewModel(product)).ToList();

            // Windows.Storage.StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;

        }

        //public async void GetLocation()
        //{
        //    var access = await Geolocator.RequestAccessAsync();

        //    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = _desireAccuracyInMetersValue };
        //    Geoposition pos = await geolocator.GetGeopositionAsync();

        //    //Define an object named Lat to access Latitude.  
        //    Lat = (float)pos.Coordinate.Point.Position.Latitude;

        //    //Define an object named Lon to access Longitude  
        //    Lon = (float)pos.Coordinate.Point.Position.Longitude;

        //}

        public async Task comPortSetUp()
        {

            //await System.Threading.Tasks.Task.Run ( () => autoComPort.GetMotorComportAsync ( ) );
            await autoComPort.DetectComPortsAsync();
            if (Global.MotorComPort != "")
            {
                Global.log.Trace("Motor reset on Port" + Global.MotorComPort);
            }

            // await System.Threading.Tasks.Task.Run ( () => Validator.AutoComPort ( ) );
            await Validator.AutoComPort();
            Global.log.Trace("Cash Machine reset on Port" + Global.CashMachineComPort);
        }

        private void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel2.OnLoaded();
        }

        private void OnUnloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel2.OnUnloaded();
        }

        //METHOD TO LOOP VIDEOS IN A FOLDER
        private async void OnMediaEnded(MediaPlayer sender, object args)
        {
            temp++;
            if (temp >= (fileArray.Length))
            {
                temp = 0;
            }
            sender.IsMuted = true;
            // sender.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/" + Path.GetFileName(fileArray[temp])));
            sender.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/" + Path.GetFileName(fileArray[temp])));
            sender.Play();
        }

        public void setMachConfig(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (configtext.Text != "")
            {
                try
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                      () =>
                      {
                          Data.VendingC.GetMachineConfigServer(configtext.Text);
                      }
                    );

                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                      () =>
                      {
                          SendUpdateData();
                      }
                    );
                    //    var response = Task.Run(() => Data.VendingC.GetMachineConfigServer(configtext.Text)).Result;
                    //   response = Task.Run(() => SendUpdateData()).Result;
                    configtext.Visibility = Visibility.Collapsed;
                    configbut.Visibility = Visibility.Collapsed;
                    updatebut.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    Global.log.Trace(ex.ToString());
                }
                Global.General.CreditAmount = 0;
                devBaseTime = 3;
                ResetProperties();
                configtext.Visibility = Visibility.Collapsed;
                configbut.Visibility = Visibility.Collapsed;
                updatebut.Visibility = Visibility.Collapsed;
            }



        }
        public async void updateData(object sender, Windows.UI.Xaml.RoutedEventArgs e)

        {

            if (configtext.Text != "")
            {
                try
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                      () =>
                      {
                          Data.VendingC.GetMachineConfigServer(configtext.Text);
                      }
                    );
                    //    var response = Task.Run(() => Data.VendingC.GetMachineConfigServer(configtext.Text)).Result;
                    //   response = Task.Run(() => SendUpdateData()).Result;
                    configtext.Visibility = Visibility.Collapsed;
                    configbut.Visibility = Visibility.Collapsed;
                    updatebut.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    Global.log.Trace(ex.ToString());
                }
                try
                {
                    await Data.VendingC.SendDataToServerAsync();
                    await Data.VendingC.GetProductsFromServer();
                    UpdateProductList();


                    configtext.Visibility = Visibility.Collapsed;
                    configbut.Visibility = Visibility.Collapsed;
                    updatebut.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    Global.log.Trace(ex.ToString());
                }
                Global.General.CreditAmount = 0;
                ResetProperties();
            }
            configtext.Visibility = Visibility.Collapsed;
            configbut.Visibility = Visibility.Collapsed;
            updatebut.Visibility = Visibility.Collapsed;
            devBaseTime = 3;



        }

        public static string GetNewSaleNo()
        {

            string SaleNoS = DateTime.Now.ToString("yyMMddHHmm");
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            SaleNoS = Global.MachineCode + "-" + Test_prefix + unixTimestamp.ToString();
            //while (!int.TryParse(SaleNoS, out SaleNo))
            //{
            //    SaleNoS = DateTime.Now.ToString("yyyyMMddHHmm");
            //}
            return SaleNoS;
        }

        public bool UpdateProductList()
        {
            try
            {
                ViewModel.ProductList = ProductDatabase.getProductList().Select((product) => new ProductViewModel(product)).ToList();
                MainGrid.ItemsSource = ViewModel.ProductList;

            }
            catch (Exception ex)
            {
                Global.log.Trace("Exception updating productlist " + ex.ToString());

            }
            return true;
        }

        public async Task<bool> SendUpdateData()
        {

            CheckInternet internet = new CheckInternet();
            bool isInternetActive = internet.CheckForInternet();

            if (isInternetActive && await Data.VendingC.CheckServerConnection() && Global.General.CreditAmount == 0)
            {
                try
                {
                    //await System.Threading.Tasks.Task.Run ( () => Data.VendingC.GetMachineConfigServer ( Global.MachineID.ToString ( )) );
                    // await System.Threading.Tasks.Task.Run ( () => Data.VendingC.SendDataToServerAsync ( ) );
                    //await System.Threading.Tasks.Task.Run ( () => Data.VendingC.GetProductsFromServer ( ) );

                    //await Task.Factory.StartNew ( () => Data.VendingC.GetMachineConfigServer ( Global.MachineID.ToString ( ) ) );
                    //await Task.Factory.StartNew ( () => Data.VendingC.SendDataToServerAsync ( ) );
                    //await Task.Factory.StartNew ( () => Data.VendingC.GetProductsFromServer ( ) );

                    await Data.VendingC.GetMachineConfigServer(Global.MachineID.ToString());
                    await Data.VendingC.SendDataToServerAsync();
                    //await Data.VendingC.GetProductsFromServer ( );


                }
                catch (Exception ex)
                {
                    Global.log.Trace("Exception syncing data to/from server " + ex.ToString());

                }
            }
            syncBaseTime = 50;

            return true;
        }

        private void ProductListItemClicked(object sender, ItemClickEventArgs e)
        {

            Global.General.INPROGRESS = true;
            var productViewModel = (ProductViewModel)e.ClickedItem;
            this.ViewModel.OnBuyClick(productViewModel);

            checkoutbut.IsEnabled = true; //SETTING CHECKOUT BUTTON ENABLED 
            emptyCart.Visibility = Visibility.Collapsed;

            cartsum.Visibility = Visibility.Visible;

            if (this.ViewModel.ShoppingCart.Entries.Count >= Global.ProductLimit)
            {
                MainGrid.IsEnabled = false;
                maxprodtext.Visibility = Visibility.Visible;
                maxprodtext.Text = "Max " + Global.ProductLimit.ToString() + " product(s) allowed.";

            }
            basetime = 20;
            disptimer.Text = basetime.ToString();
            timer.Start();
        }


        private async void Agecheck(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Global.General.INPROGRESS = true;
            qrimgText.Text = "Is your age 18 or above?";
            qrimgText.Visibility = Visibility.Visible;
            AgeCheckButtons.Visibility = Visibility.Visible;

            if (!StandardPopup.IsOpen)
            {
                StandardPopup.IsOpen = true;
            }
            basetime = 10;

        }

        //---------------------------- CHECKOUT BUTTON CLICKED FUNCTION  -----------------------------------------------------------
        private async void OnWindowsCheckoutClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Global.PaymentType.CashPay = true;
            //Global.MotorComPort = "12";

            CashButton.Visibility = Global.PaymentType.CashPay ? Visibility.Visible : Visibility.Collapsed;
            QRVcButton.Visibility = Global.PaymentType.VendiAppPay ? Visibility.Visible : Visibility.Collapsed;
            QRMasterButton.Visibility = Global.PaymentType.MasterQRPay ? Visibility.Visible : Visibility.Collapsed;
            POSButton.Visibility = Global.PaymentType.PosPay ? Visibility.Visible : Visibility.Collapsed;

            MainGrid.IsEnabled = false;
            CartGrid.IsEnabled = false;
            AgeCheckButtons.Visibility = Visibility.Collapsed;
            qrimgText.Visibility = Visibility.Collapsed;

            Global.General.INPROGRESS = true;


            //  AutoComPort autoComPort = new AutoComPort();
            //var i = await autoComPort.GetMotorComportAsync();

            Global.General.TotalCostAmount = ViewModel2.ShoppingCart.CostsSummary.Total;
            Global.General.TotalNetAmount = Math.Abs(Global.General.CreditAmount - ViewModel2.ShoppingCart.CostsSummary.Total);
            Global.log.Trace("Total Amount: " + Global.General.TotalCostAmount);
            Global.log.Trace("Total Credit Amount: " + Global.General.CreditAmount);
            Global.log.Trace("Total Net Amount: " + Global.General.TotalNetAmount);

            //if (Global.PaymentType.CashPay && isInternetActive)
            //{
            if (Global.MotorComPort != "")
            {
                if (Global.General.TotalCostAmount > Global.General.CreditAmount && Global.General.TotalNetAmount != 0)
                {
                    if (Global.General.CreditAmount == 0)
                    {
                        Global.General.SALE_NO = GetNewSaleNo();
                        Global.log.Trace("SaleNo generated: " + Global.General.SALE_NO);
                    }
                    checkoutbut.IsEnabled = false; //SETTING CHECKOUT BUTTON DISABLED 
                                                   // MainGrid.IsEnabled = false;
                                                   // CartGrid.IsEnabled = false;
                    paymentbuttons.Visibility = Visibility.Visible;
                    if (!StandardPopup.IsOpen)
                    {
                        StandardPopup.IsOpen = true;
                    }

                    basetime = 30;
                    disptimer.Text = basetime.ToString();
                    timer.Start();
                }
                else
                {
                    if (!StandardPopup.IsOpen)
                    {
                        StandardPopup.IsOpen = true;
                    }

                    basetime = 30;
                    disptimer.Text = basetime.ToString();
                    timer.Start();
                    qrimgText.Text = "Dispensing Products! \n Please Wait! ";
                    qrimgText.Visibility = Visibility.Visible;

                    loadingImg.Visibility = Visibility.Visible;

                    Global.log.Trace("Credit amount cancels with Transaction Amount for " + Global.General.SALE_NO);
                    timer.Stop();
                    cancelButton.IsEnabled = false;
                    await System.Threading.Tasks.Task.Run(() => this.ViewModel2.OnWindowsCheckoutClicked());
                    // await this.ViewModel2.OnWindowsCheckoutClicked ( ) ;


                    Global.General.TOTAL_PAID_AMOUNT = Global.General.TOTAL_PAID_AMOUNT + Global.General.CashInAmount;
                    //Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Math.Abs(Global.General.TotalCostAmount - Global.General.CreditAmount);
                    Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Global.General.TotalCostAmount;
                    Global.General.TOTAL_BALANCE_AMOUNT = Global.General.TOTAL_BALANCE_AMOUNT + Global.General.CreditAmount;

                    if (Global.General.CreditAmount != 0)
                    {
                        Global.log.Trace("Credit Amount: " + Global.General.CreditAmount);
                        dispmess.Text = "Payment Sccessful: Product/s Dispense failed!";
                        creditAmount.Text = Global.General.CreditAmount.ToString();
                        creditAmount.Visibility = Visibility.Visible;
                        creditText.Visibility = Visibility.Visible;
                        credittime.Visibility = Visibility.Visible;
                        CreditTimerbasetime = 60;
                        CreditTimer.Start();

                    }
                    else
                    {
                        Global.log.Trace("Product dispened successfully against SaleNo:" + Global.General.SALE_NO);
                        creditAmount.Text = "0";
                        Data.VendingC.SubmitSaleTransaction();
                        Global.General.TOTAL_PAID_AMOUNT = 0;
                        Global.General.TOTAL_PURCHASED_AMOUNT = 0;
                        Global.General.TOTAL_BALANCE_AMOUNT = 0;
                        creditAmount.Visibility = Visibility.Collapsed;
                        creditText.Visibility = Visibility.Collapsed;
                        credittime.Visibility = Visibility.Collapsed;
                        CreditTimer.Stop();

                    }

                    isPaymentSuccessful = true;
                    basetime = 0;
                    cashAmount = 0;

                    Global.log.Trace("Transaction Completed for saleNo: " + Global.General.SALE_NO + " Resetting System");
                    ResetProperties();
                    timer.Stop();
                    UpdateProductList();
                    ClosePopupClicked();
                }
            }
            else
            {
                Global.log.Trace("Auto COM port set up failed");
                paymentbuttons.Visibility = Visibility.Collapsed;
                qrimgText.Visibility = Visibility.Visible;
                qrimgText.Text = "Facing Technical Issues due to ComPort. Please try again later.";
                if (!StandardPopup.IsOpen)
                {
                    StandardPopup.IsOpen = true;
                }

                basetime = 5;
                disptimer.Text = basetime.ToString();
                timer.Start();

            }
            //}
            //else
            //{
            //    Global.log.Trace("Internet Inactive");
            //    paymentbuttons.Visibility = Visibility.Collapsed;
            //    qrimgText.Visibility = Visibility.Visible;
            //    qrimgText.Text = "Facing Internet Issues. Please try again later.";
            //    if (!StandardPopup.IsOpen)
            //    {
            //        StandardPopup.IsOpen = true;
            //    }

            //    basetime = 5;
            //    disptimer.Text = basetime.ToString();
            //    timer.Start();

            //}

            try
            {
                BAL.ClickPicture cp = new BAL.ClickPicture();
                await cp.CaptureCameraAsync();
            }

            catch (Exception ex)
            {
                Global.log.Trace("Unable to capture picture" + ex.ToString());
            }

            //if (paymentOption == "QRalfalahPAY")
            //{
            //    QRalfalahPAY(sender, e);
            //}
            //else if (paymentOption == "VendingCPay")
            //{
            //    QRvendingcPAY(sender, e);
            //}
            //else if (paymentOption == "")
            //{
            //    //POPUP

            //}
            // if the Popup is open, then close it 
            if (LoadingPopup.IsOpen)
            {
                LoadingPopup.IsOpen = false;
            }

        }


        //---------------------------- REMOVING PRODUCT  -----------------------------------------------------------
        private void OnRemoveClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var button = (HyperlinkButton)sender;
            var entryViewModel = (ShoppingCartEntryViewModel)button.Tag;

            this.ViewModel2.OnEntryRemoveClick(entryViewModel);

            if (this.ViewModel2.Entries.Count <= 1)
            {
                checkoutbut.IsEnabled = false;
                cartsum.Visibility = Visibility.Collapsed;
                MainGrid.IsEnabled = true;
                emptyCart.Visibility = Visibility.Visible;
            }
            if (this.ViewModel2.Entries.Count <= 3)
            {
                maxprodtext.Visibility = Visibility.Collapsed;
                MainGrid.IsEnabled = true;

            }

        }
        //---------------------------- QR ALFALAH PAYMENT OPTION START  -----------------------------------------------------------
        private async void QRalfalahPAY(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

            CheckInternet internet = new CheckInternet();
            bool isInternetActive = internet.CheckForInternet();

            paymentbuttons.Visibility = Visibility.Collapsed;

            if (isInternetActive)
            {



                Global.log.Trace("Starting Alfalah transaction for saleNo: " + Global.General.SALE_NO);
                Global.General.PaymentOption = "QRalfalahPAY";
                timer.Stop();
                syncTimer.Stop();
                loadingImg.Visibility = Visibility.Visible;
                qrimgText.Visibility = Visibility.Visible;
                qrimgText.Text = "Please wait. Getting things ready!";
                cashAmount = 0;
                isPaymentSuccessful = false;
                tots.Text = "Rs." + Global.General.TotalNetAmount.ToString();
                //  this.ViewModel2.OnWindowsCheckoutClicked();
                string Qrimage = "";
                try
                {

                    Qrimage = await QR.GetMasterQR(Global.General.SALE_NO, Convert.ToString(Global.MachineID), Global.General.TotalNetAmount);

                    byte[] bytes = Convert.FromBase64String(Qrimage);
                    var bitmap = new BitmapImage();

                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        await qri.SetSourceAsync(ms.AsRandomAccessStream());
                        qrimg.Source = qri;
                    }
                    Global.log.Trace("Alfalah QR fetching successful against SaleNo: " + Global.General.SALE_NO + " with amount: " + Global.General.TotalNetAmount);

                }
                catch (Exception ex)
                {
                    Global.log.Trace(ex.ToString());
                    Global.log.Trace("Alfalah QR fetching failed against SaleNo: " + Global.General.SALE_NO + " with amount: " + Global.General.TotalNetAmount);
                }

                try
                {
                    if (Qrimage != null && Qrimage.Length != 0)
                    {
                        qrimgText.Text = "Please pay through scanning the QR on your Mobile App!";
                        loadingImg.Visibility = Visibility.Collapsed;
                        qrimg.Visibility = Visibility.Visible;
                        qrimgText.Visibility = Visibility.Visible;
                        tots.Visibility = Visibility.Visible;
                        infotext.Visibility = Visibility.Visible;
                        disptimer.Visibility = Visibility.Visible;

                        infotext.Text = "Availale through \n Bank Alfalah, JazzCash \n and Easy Paisa.";
                        infotext.Visibility = Visibility.Visible;

                        //TIMER
                        basetime = 60;
                        disptimer.Text = basetime.ToString();
                        timer.Start();

                        while (basetime != 0 || cashAmount != 0)
                        {
                            if ((cashAmount = (double)await QR.ReadQRAmountAPIAsync(Convert.ToString(Global.MachineID), Global.General.SALE_NO, Global.General.TotalNetAmount)) != 0)
                            {
                                //cancelButton.Visibility = Visibility.Collapsed;
                                cancelButton.IsEnabled = false;

                                Global.General.CashInAmount = Global.General.TotalNetAmount;
                                Global.log.Trace("Payment notification found against SaleNo:" + Global.General.SALE_NO);
                                dispmess.Text = "Payment of amount: " + Global.General.TotalNetAmount + " recieved! \n Dispensing Products! \n Please Wait! ";
                                disptimer.Visibility = Visibility.Collapsed;
                                tots.Visibility = Visibility.Collapsed;
                                qrimg.Visibility = Visibility.Collapsed;
                                qrimgText.Visibility = Visibility.Collapsed;
                                dispmess.Visibility = Visibility.Visible;

                                //qrimgText.Text = "Payment of amount PKR" + Global.General.TotalNetAmount + " recieved! \n Dispensing Products! \n Please Wait! ";
                                //qrimgText.Visibility = Visibility.Visible;
                                //loadingImg.Visibility = Visibility.Visible;

                                Global.log.Trace("Iniating dispense sequence against saleNo:" + Global.General.SALE_NO);
                                //await Task.Factory.StartNew(() => this.ViewModel2.OnWindowsCheckoutClicked());
                                // await System.Threading.Tasks.Task.Run ( () => this.ViewModel2.OnWindowsCheckoutClicked ( ) );
                                // await this.ViewModel2.OnWindowsCheckoutClicked ( );
                                timer.Stop();
                                await System.Threading.Tasks.Task.Run(() => this.ViewModel2.OnWindowsCheckoutClicked());

                                Global.General.TOTAL_PAID_AMOUNT = Global.General.TOTAL_PAID_AMOUNT + Global.General.CashInAmount;
                                //Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Math.Abs( Global.General.TotalCostAmount - Global.General.CreditAmount);
                                Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Global.General.TotalCostAmount;
                                Global.General.TOTAL_BALANCE_AMOUNT = Global.General.TOTAL_BALANCE_AMOUNT + Global.General.CreditAmount;

                                Data.VendingC.SubmitPaymentTransaction();

                                //  dispmess.Text = "Payment Sccessful: Product/s Dispense Successful!";

                                if (Global.General.CreditAmount != 0)
                                {
                                    dispmess.Text = "Payment Sccessful: Product/s Dispense Failed.\n Please Use your credit amount \n or contact our admistrator!";
                                    Global.log.Trace("Credit amount : " + Global.General.CreditAmount + "against SaleNo:" + Global.General.SALE_NO);
                                    creditAmount.Text = Global.General.CreditAmount.ToString();
                                    creditAmount.Visibility = Visibility.Visible;
                                    creditText.Visibility = Visibility.Visible;
                                    credittime.Visibility = Visibility.Visible;
                                    CreditTimerbasetime = 60;
                                    CreditTimer.Start();

                                }
                                else
                                {
                                    dispmess.Text = "Payment Sccessful: Product/s Dispensed!";
                                    Data.VendingC.SubmitSaleTransaction();
                                    Global.General.TOTAL_PAID_AMOUNT = 0;
                                    Global.General.TOTAL_PURCHASED_AMOUNT = 0;
                                    Global.General.TOTAL_BALANCE_AMOUNT = 0;
                                    creditAmount.Visibility = Visibility.Collapsed;
                                    creditText.Visibility = Visibility.Collapsed;
                                    credittime.Visibility = Visibility.Collapsed;
                                    CreditTimer.Stop();

                                }

                                isPaymentSuccessful = true;
                                basetime = 0;
                                cashAmount = 0;

                            }
                        }

                        if (!isPaymentSuccessful)
                        {
                            Global.log.Trace("Payment unsuccessful against SaleNo: " + Global.General.SALE_NO);
                            dispmess.Visibility = Visibility.Visible;
                            dispmess.Text = "Product Dispensed Failed: Payment Failed";

                        }
                    }
                    else
                    {
                        Global.log.Trace("QR not recieved from bank against SaleNo: " + Global.General.SALE_NO);
                        loadingImg.Visibility = Visibility.Collapsed;
                        dispmess.Text = "Bank service down! \r\n QR code not received. Please try again later!";
                        dispmess.Visibility = Visibility.Visible;

                    }
                    try
                    {
                        await Task.Delay(5000, source.Token);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                catch (Exception ex)
                {
                    Global.log.Trace(ex.ToString());
                }
                Global.log.Trace("Transaction Completed for saleNo: " + Global.General.SALE_NO + ". Resetting System");
                Global.log.Trace("==================================================================================");

                ResetProperties();
                timer.Stop();
                // await UpdateProductList();
                UpdateProductList();
                ClosePopupClicked();
                await Task.Factory.StartNew(() => SendUpdateData());
            }
            else
            {
                timer.Stop();
                basetime = 20;
                disptimer.Text = basetime.ToString();
                timer.Start();
                Global.log.Trace("can not proceed with VISA QR option against SaleNo: " + Global.General.SALE_NO + " due to internet");
                loadingImg.Visibility = Visibility.Collapsed;
                dispmess.Text = "Unable to proceed due to a internet connectivity issue. Please try again later...";
                dispmess.Visibility = Visibility.Visible;

            }
        }
        //---------------------------- QR ALFALAH PAYMENT OPTION END  -----------------------------------------------------------

        //---------------------------- QR VENDING APP PAYMENT OPTION START  -----------------------------------------------------------
        private async void QRvendingcPAY(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CheckInternet internet = new CheckInternet();
            bool isInternetActive = internet.CheckForInternet();

            paymentbuttons.Visibility = Visibility.Collapsed;

            if (isInternetActive)
            {
                Global.log.Trace("Starting VendingPayApp transaction for saleNo: " + Global.General.SALE_NO);
                Global.General.PaymentOption = "VendingCPay";
                timer.Stop();
                syncTimer.Stop();
                loadingImg.Visibility = Visibility.Visible;
                qrimgText.Visibility = Visibility.Visible;
                qrimgText.Text = "Please wait. Getting things ready!";

                cashAmount = 0;
                isPaymentSuccessful = false;
                tots.Text = "Rs." + Global.General.TotalNetAmount.ToString();

                var apiresponse = new byte[0];
                try
                {
                    apiresponse = await QR.FunGenerateQR(Global.General.SALE_NO, Convert.ToString(Global.MachineID), Global.General.TotalNetAmount);

                    Bitmap bmp;

                    using (MemoryStream ms = new MemoryStream(apiresponse))
                    {
                        await qri.SetSourceAsync(ms.AsRandomAccessStream());
                        qrimg.Source = qri;

                    }
                    Global.log.Trace("QR received against saleNo: " + Global.General.SALE_NO);

                }
                catch (Exception ex)
                {
                    Global.log.Trace(ex.ToString());
                    Global.log.Trace("QR not received from VendingC App agaisnt SaleNo:" + Global.General.SALE_NO + " with amount:" + Global.General.TotalNetAmount);
                }
                if (apiresponse.Count() != 0)
                {
                    qrimgText.Text = "Please pay through scanning the QR on your Mobile App!";
                    infotext.Text = "Available through Vending Central Mobile App.";
                    loadingImg.Visibility = Visibility.Collapsed;
                    qrimg.Visibility = Visibility.Visible;
                    qrimgText.Visibility = Visibility.Visible;
                    tots.Visibility = Visibility.Visible;
                    disptimer.Visibility = Visibility.Visible;
                    infotext.Visibility = Visibility.Visible;

                    //TIMER
                    basetime = 60;
                    disptimer.Text = basetime.ToString();
                    timer.Start();

                    while (basetime != 0 || cashAmount != 0)
                    {

                        if ((cashAmount = (double)await QR.ReadVendingPayQRAmountAPIAsync(Convert.ToString(Global.MachineID), Global.General.SALE_NO, Global.General.TotalNetAmount)) != 0)
                        {
                            Global.General.CashInAmount = Global.General.TotalNetAmount;
                            //cancelButton.Visibility = Visibility.Collapsed;
                            cancelButton.IsEnabled = false;
                            Global.log.Trace("Payment notification Successfully found against saleNo: " + Global.General.SALE_NO + " with amount:" + Global.General.TotalNetAmount);
                            dispmess.Text = "Payment of amount" + Global.General.TotalNetAmount + "recieved! \n Dispensing Products! \n Please Wait! ";
                            dispmess.Visibility = Visibility.Visible;
                            disptimer.Visibility = Visibility.Collapsed;
                            tots.Visibility = Visibility.Collapsed;
                            infotext.Visibility = Visibility.Collapsed;
                            qrimg.Visibility = Visibility.Collapsed;
                            qrimgText.Visibility = Visibility.Collapsed;
                            dispmess.Visibility = Visibility.Visible;

                            //qrimgText.Text = "Payment of amount PKR" + Global.General.TotalNetAmount + " recieved! \n Dispensing Products! \n Please Wait! ";
                            //qrimgText.Visibility = Visibility.Visible;
                            //loadingImg.Visibility = Visibility.Visible;

                            Global.log.Trace("Iniating dispense sequence against saleNo:" + Global.General.SALE_NO);
                            //await Task.Factory.StartNew(() => this.ViewModel2.OnWindowsCheckoutClicked());
                            //await System.Threading.Tasks.Task.Run ( () => this.ViewModel2.OnWindowsCheckoutClicked ( ) );
                            // await this.ViewModel2.OnWindowsCheckoutClicked ( ) ;
                            timer.Stop();
                            await System.Threading.Tasks.Task.Run(() => this.ViewModel2.OnWindowsCheckoutClicked());

                            Global.General.TOTAL_PAID_AMOUNT = Global.General.TOTAL_PAID_AMOUNT + Global.General.CashInAmount;
                            //Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Math.Abs( Global.General.TotalCostAmount - Global.General.CreditAmount);
                            Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Global.General.TotalCostAmount;
                            Global.General.TOTAL_BALANCE_AMOUNT = Global.General.TOTAL_BALANCE_AMOUNT + Global.General.CreditAmount;

                            Data.VendingC.SubmitPaymentTransaction();

                            if (Global.General.CreditAmount != 0)
                            {
                                dispmess.Text = "Payment Sccessful: Product/s Dispense Failed.\n Please Use your credit amount \n or contact our admistrator!";
                                Global.log.Trace("Credit amount : " + Global.General.CreditAmount + "against SaleNo:" + Global.General.SALE_NO);
                                creditAmount.Text = Global.General.CreditAmount.ToString();
                                creditAmount.Visibility = Visibility.Visible;
                                creditText.Visibility = Visibility.Visible;
                                credittime.Visibility = Visibility.Visible;
                                CreditTimerbasetime = 60;
                                CreditTimer.Start();

                            }
                            else
                            {
                                dispmess.Text = "Payment Sccessful: Product/s Dispensed!";
                                Data.VendingC.SubmitSaleTransaction();
                                Global.General.TOTAL_PAID_AMOUNT = 0;
                                Global.General.TOTAL_PURCHASED_AMOUNT = 0;
                                Global.General.TOTAL_BALANCE_AMOUNT = 0;
                                creditAmount.Visibility = Visibility.Collapsed;
                                creditText.Visibility = Visibility.Collapsed;
                                credittime.Visibility = Visibility.Collapsed;
                                CreditTimer.Stop();

                            }

                            isPaymentSuccessful = true;
                            basetime = 0;
                            cashAmount = 0;

                        }
                    }

                    if (!isPaymentSuccessful)
                    {
                        dispmess.Visibility = Visibility.Visible;
                        dispmess.Text = "Product Dispensed Failed: Payment Failed";
                        Global.log.Trace("Payment not successful against saleNo: " + Global.General.SALE_NO);

                    }
                }
                else
                {
                    Global.log.Trace("QR not received from Vending C app agaisnt saleNo: " + Global.General.SALE_NO);
                    loadingImg.Visibility = Visibility.Collapsed;
                    dispmess.Text = "Payment Failed. Please try again later!";
                    dispmess.Visibility = Visibility.Visible;

                }
                try
                {
                    await Task.Delay(5000, source.Token);
                }
                catch (Exception ex)
                {

                }
                Global.log.Trace("Transaction Completed for saleNo: " + Global.General.SALE_NO + ". Resetting System");
                Global.log.Trace("==================================================================================");

                ResetProperties();
                timer.Stop();
                UpdateProductList();
                ClosePopupClicked();

                await Task.Factory.StartNew(() => SendUpdateData());
            }
            else
            {
                //TIMER
                basetime = 20;
                disptimer.Text = basetime.ToString();
                timer.Start();
                Global.log.Trace("can not proceed with VISA QR option against SaleNo: " + Global.General.SALE_NO + " due to internet");
                loadingImg.Visibility = Visibility.Collapsed;
                dispmess.Text = "Unable to proceed due to a internet connectivity issue. Please try again later...";
                dispmess.Visibility = Visibility.Visible;

            }
        }
        //---------------------------- QR VENDING APP PAYMENT OPTION END  -----------------------------------------------------------

        private async void PosPAY(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Global.POSMachineComPort = "COM6";


            Global.log.Trace("Starting POS Machine transaction for saleNo: " + Global.General.SALE_NO);
            Global.General.PaymentOption = "POSMachine";
            timer.Stop();
            syncTimer.Stop();
            paymentbuttons.Visibility = Visibility.Collapsed;
            loadingImg.Visibility = Visibility.Visible;
            qrimgText.Visibility = Visibility.Visible;
            qrimgText.Text = "Please wait. Getting things ready!";

            cashAmount = 0;
            isPaymentSuccessful = false;
            tots.Text = "Rs." + Global.General.TotalNetAmount.ToString();


            PaymentPOS paymentPos = new PaymentPOS();
            try
            {
                if (!(string.IsNullOrEmpty(Global.POSMachineComPort)))
                {
                    qrimgText.Text = "Please tap or insert the card on the POS machine!";
                    loadingImg.Visibility = Visibility.Collapsed;
                    qrimgText.Visibility = Visibility.Visible;
                    tots.Visibility = Visibility.Visible;
                    infotext.Visibility = Visibility.Visible;
                    disptimer.Visibility = Visibility.Visible;

                    infotext.Text = "Availale through VISA or Master cards";
                    infotext.Visibility = Visibility.Visible;

                    //TIMER
                    basetime = 60;
                    disptimer.Text = basetime.ToString();
                    timer.Start();
                    await Task.Delay(1000);
                    if (await paymentPos.TransactionSales(Global.General.TotalNetAmount.ToString()))
                    {
                        Global.General.CashInAmount = Global.General.TotalNetAmount;
                        //cancelButton.Visibility = Visibility.Collapsed;
                        cancelButton.IsEnabled = false;
                        dispmess.Text = "Payment of amount" + Global.General.TotalNetAmount + "recieved! \n Dispensing Products! \n Please Wait! ";
                        // qrimgText.Text = "Payment of amount PKR" + Global.General.TotalNetAmount + " recieved! \n Dispensing Products! \n Please Wait! ";
                        Global.log.Trace("Payment successful against SaleNo:" + Global.General.SALE_NO);
                        disptimer.Visibility = Visibility.Collapsed;
                        tots.Visibility = Visibility.Collapsed;
                        dispmess.Visibility = Visibility.Visible;
                        // qrimgText.Visibility = Visibility.Visible;
                        loadingImg.Visibility = Visibility.Visible;

                        Global.log.Trace("Iniating dispense sequence against saleNo:" + Global.General.SALE_NO);
                        // await Task.Factory.StartNew(() => this.ViewModel2.OnWindowsCheckoutClicked());
                        //await System.Threading.Tasks.Task.Run ( () => this.ViewModel2.OnWindowsCheckoutClicked ( ) );

                        //await this.ViewModel2.OnWindowsCheckoutClicked ( ) ;

                        timer.Stop();
                        await System.Threading.Tasks.Task.Run(() => this.ViewModel2.OnWindowsCheckoutClicked());

                        Global.General.TOTAL_PAID_AMOUNT = Global.General.TOTAL_PAID_AMOUNT + Global.General.CashInAmount;
                        //Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Math.Abs( Global.General.TotalCostAmount - Global.General.CreditAmount);
                        Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Global.General.TotalCostAmount;
                        Global.General.TOTAL_BALANCE_AMOUNT = Global.General.TOTAL_BALANCE_AMOUNT + Global.General.CreditAmount;

                        Data.VendingC.SubmitPaymentTransaction();

                        dispmess.Text = "Payment Sccessful: Product/s Dispense Successful!";

                        if (Global.General.CreditAmount != 0)
                        {
                            Global.log.Trace("Credit amount : " + Global.General.CreditAmount + "against SaleNo:" + Global.General.SALE_NO);
                            dispmess.Text = "Payment Sccessful: Product/s Dispensed!";
                            creditAmount.Text = Global.General.CreditAmount.ToString();
                            creditAmount.Visibility = Visibility.Visible;
                            creditText.Visibility = Visibility.Visible;
                            credittime.Visibility = Visibility.Visible;
                            CreditTimerbasetime = 60;
                            CreditTimer.Start();

                        }
                        else
                        {
                            dispmess.Text = "Payment Sccessful: Product/s Dispensed!";
                            Data.VendingC.SubmitSaleTransaction();
                            Global.General.TOTAL_PAID_AMOUNT = 0;
                            Global.General.TOTAL_PURCHASED_AMOUNT = 0;
                            Global.General.TOTAL_BALANCE_AMOUNT = 0;
                            creditAmount.Visibility = Visibility.Collapsed;
                            creditText.Visibility = Visibility.Collapsed;
                            credittime.Visibility = Visibility.Collapsed;

                            CreditTimer.Stop();

                        }

                        isPaymentSuccessful = true;
                        basetime = 0;
                        cashAmount = 0;

                    }
                    if (!isPaymentSuccessful)
                    {
                        Global.log.Trace("Payment unsuccessful against SaleNo: " + Global.General.SALE_NO);
                        dispmess.Visibility = Visibility.Visible;
                        dispmess.Text = "Product Dispensed Failed: Payment Failed";

                    }
                }
                else
                {
                    Global.log.Trace("POS machine isnot connected against SaleNo: " + Global.General.SALE_NO);
                    loadingImg.Visibility = Visibility.Collapsed;
                    dispmess.Text = "Bank service down! \r\n Currently the POS Machine is not working. Please try again later!";
                    dispmess.Visibility = Visibility.Visible;

                }

                try
                {
                    await Task.Delay(5000, source.Token);
                }
                catch (Exception ex)
                {

                }

            }
            catch (Exception ex)
            {
                Global.log.Trace(ex.ToString());
            }
            Global.log.Trace("Transaction Completed for saleNo: " + Global.General.SALE_NO + ". Resetting System");
            Global.log.Trace("==================================================================================");

            ResetProperties();
            timer.Stop();
            // await UpdateProductList();
            UpdateProductList();
            ClosePopupClicked();
            await Task.Factory.StartNew(() => SendUpdateData());

        }

        private async void CashPAY(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Global.CashMachineComPort = "COM7";

            //bool isCashmachineAvailable = await cashPay.AutoComPort();
            if (Global.CashMachineComPort != "")
            {

                Global.log.Trace("Cash Machine Detected on Port" + Global.CashMachineComPort);

                Global.log.Trace("Starting Cash Machine transaction for saleNo: " + Global.General.SALE_NO);
                Global.General.PaymentOption = "CashMachine";
                timer.Stop();
                syncTimer.Stop();
                paymentbuttons.Visibility = Visibility.Collapsed;
                loadingImg.Visibility = Visibility.Visible;
                qrimgText.Visibility = Visibility.Visible;
                qrimgText.Text = "Please insert exact cash amount into the machine. \nExcess Cash would not be returned.";

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(this.BaseUri, "/Assets/Payment/cash.jpg");
                qrimg.Source = bitmapImage;
                qrimg.Visibility = Visibility.Visible;

                cashAmount = 0;
                isPaymentSuccessful = false;
                tots.Text = "Rs." + Global.General.TotalNetAmount.ToString();


                try
                {
                    // qrimgText.Text = "Please insert cash in the machine!";
                    loadingImg.Visibility = Visibility.Collapsed;
                    // qrimgText.Visibility = Visibility.Visible;
                    tots.Visibility = Visibility.Visible;
                    disptimer.Visibility = Visibility.Visible;

                    cashinserted.Visibility = Visibility.Visible;

                    //TIMER
                    basetime = 30;
                    disptimer.Text = "Cart available for: " + basetime.ToString() + " seconds.";
                    timer.Start();
                    //paymentbuttons.InvalidateArrange ( );
                    //Action teest = delegate () { };

                    //teest ( );
                    //  while (basetime != 0 || cashAmount != 0)
                    //   {
                    //if ( await System.Threading.Tasks.Task.Run ( () => cashPay.PayCash ( Global.General.TotalNetAmount ) ) )
                    ///if ( await Task.Run ( () => cashPay.PayCash ( Global.General.TotalNetAmount ) ) )
                    // await Task.Delay ( 100 );

                    if (await System.Threading.Tasks.Task.Run(() => cashPay.PayCash(Global.General.TotalNetAmount)))
                    {
                        cancelButton.Visibility = Visibility.Collapsed;
                        cashinserted.Visibility = Visibility.Collapsed;
                        dispmess.Text = "Payment of amount " + Global.General.TotalNetAmount + " recieved! \nDispensing Products! \nPlease Wait! ";
                        // qrimgText.Text = "Payment of amount PKR" + Global.General.TotalNetAmount + " recieved! \n Dispensing Products! \n Please Wait! ";
                        Global.log.Trace("Payment successful against SaleNo:" + Global.General.SALE_NO);
                        disptimer.Visibility = Visibility.Collapsed;
                        tots.Visibility = Visibility.Collapsed;
                        dispmess.Visibility = Visibility.Visible;
                        qrimgText.Visibility = Visibility.Collapsed;
                        qrimg.Visibility = Visibility.Collapsed;
                        loadingImg.Visibility = Visibility.Visible;
                        infotext.Visibility = Visibility.Collapsed;

                        //this.Frame.Navigate ( typeof ( MainPage1 ) );

                        Global.log.Trace("Iniating dispense sequence against saleNo:" + Global.General.SALE_NO);


                        // while ( !x )
                        //   {
                        //  await Task.Factory.StartNew ( () => x = this.ViewModel2.OnWindowsCheckoutClicked ( ) );
                        //  }

                        timer.Stop();







                        await System.Threading.Tasks.Task.Run(() => this.ViewModel2.OnWindowsCheckoutClicked());
                        //await System.Threading.Tasks.Task.Run ( () => this.ViewModel2.OnWindowsCheckoutClicked ( ) );
                        //await this.ViewModel2.OnWindowsCheckoutClicked ( ) ;
                        //await Task.Delay ( 5000 );
                        cancelButton.IsEnabled = false;
                        Global.General.TOTAL_PAID_AMOUNT = Global.General.TOTAL_PAID_AMOUNT + Global.General.CashInAmount;
                        //Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Math.Abs( Global.General.TotalCostAmount - Global.General.CreditAmount);
                        Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + Global.General.TotalCostAmount;
                        Global.General.TOTAL_BALANCE_AMOUNT = Global.General.TOTAL_BALANCE_AMOUNT + Global.General.CreditAmount;

                        Global.General.CreditAmount = Global.General.CashCreditAmount + Global.General.CreditAmount;
                        Data.VendingC.SubmitPaymentTransaction();


                        if (Global.General.CreditAmount != 0)
                        {
                            dispmess.Text = "Payment Sccessful: Product/s Dispense Failed.\n Please Use your credit amount \n or contact our admistrator!";
                            Global.log.Trace("Credit amount : " + Global.General.CreditAmount + " calculated against SaleNo:" + Global.General.SALE_NO);
                            creditAmount.Text = Global.General.CreditAmount.ToString();
                            creditAmount.Visibility = Visibility.Visible;
                            creditText.Visibility = Visibility.Visible;
                            credittime.Visibility = Visibility.Visible;
                            CreditTimerbasetime = 60;
                            CreditTimer.Start();

                        }
                        else
                        {
                            dispmess.Text = "Payment Sccessful: Product/s Dispense Successful!";
                            Data.VendingC.SubmitSaleTransaction();
                            Global.General.TOTAL_PAID_AMOUNT = 0;
                            Global.General.TOTAL_PURCHASED_AMOUNT = 0;
                            Global.General.TOTAL_BALANCE_AMOUNT = 0;
                            creditAmount.Visibility = Visibility.Collapsed;
                            creditText.Visibility = Visibility.Collapsed;
                            credittime.Visibility = Visibility.Collapsed;
                            CreditTimer.Stop();

                        }


                        isPaymentSuccessful = true;
                        basetime = 0;
                        cashAmount = 0;

                    }

                    else
                    {
                        Global.log.Trace("Payment incomplate against SaleNo: " + Global.General.SALE_NO);

                        if (Global.General.CashCreditAmount != 0)
                        {
                            // Global.General.CreditAmount = Global.General.CashCreditAmount + Global.General.CreditAmount;
                            Global.General.TOTAL_PURCHASED_AMOUNT = Global.General.TOTAL_PURCHASED_AMOUNT + 0;
                            Global.General.TOTAL_PAID_AMOUNT = Global.General.TOTAL_PAID_AMOUNT + Global.General.CashInAmount;
                            Global.General.CreditAmount = Math.Abs(Global.General.TOTAL_PAID_AMOUNT - Global.General.TOTAL_PURCHASED_AMOUNT);

                            Global.log.Trace("Credit amount : " + Global.General.CreditAmount + "against SaleNo:" + Global.General.SALE_NO);
                            creditAmount.Text = Global.General.CreditAmount.ToString();
                            creditAmount.Visibility = Visibility.Visible;
                            creditText.Visibility = Visibility.Visible;
                            credittime.Visibility = Visibility.Visible;
                            CreditTimerbasetime = 60;
                            CreditTimer.Start();

                        }

                    }
                    // }

                    if (!isPaymentSuccessful)
                    {
                        Global.log.Trace("Payment unsuccessful against SaleNo: " + Global.General.SALE_NO);
                        dispmess.Visibility = Visibility.Visible;
                        dispmess.Text = "Product Dispensed Failed: Payment Failed";

                    }
                }
                catch (Exception ex)
                {
                    Global.log.Trace(ex.ToString());
                }
                //Thread.Sleep ();
                try
                {
                    await Task.Delay(5000, source.Token);
                }
                catch (Exception ex)
                {

                }

            }
            else
            {
                Global.log.Trace("Cash Machine Not detected");
            }
            Global.log.Trace("Transaction Completed for saleNo: " + Global.General.SALE_NO + ". Resetting System");
            Global.log.Trace("==================================================================================");

            ResetProperties();
            UpdateProductList();
            ClosePopupClicked();
            await Task.Factory.StartNew(() => SendUpdateData());
            // cashPay.Reset();

        }

        //---------------------------- CANCEL BUTTON ON POPUP -----------------------------------------------------------
        public async void CancelButton(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            cancelButton.IsEnabled = false;
            Global.log.Trace("Transaction Canceled for saleNo: " + Global.General.SALE_NO + ". Resetting System");
            timer.Stop();
            source.Cancel();
            //await Task.Delay ( 5000 );
            Global.NoteValidator.Running = false;
            ResetProperties();
            ClosePopupClicked();

        }
        //---------------------------- RESETTING PROPERTIES -----------------------------------------------------------
        private async void ResetProperties()
        {
            //RESETTING ITEM PROPERTIES
            ViewModel2.ShoppingCart.Clear();
            UpdateProductList();

            MainGrid.IsEnabled = true;
            CartGrid.IsEnabled = true;
            checkoutbut.IsEnabled = false;
            isPaymentSuccessful = false;
            Global.General.INPROGRESS = false;

            cartsum.Visibility = Visibility.Collapsed;
            maxprodtext.Visibility = Visibility.Collapsed;
            dispmess.Visibility = Visibility.Collapsed;
            tots.Visibility = Visibility.Collapsed;
            qrimg.Visibility = Visibility.Collapsed;
            qrimgText.Visibility = Visibility.Collapsed;
            tots.Visibility = Visibility.Collapsed;
            infotext.Visibility = Visibility.Collapsed;
            disptimer.Visibility = Visibility.Collapsed;
            paymentbuttons.Visibility = Visibility.Visible;
            loadingImg.Visibility = Visibility.Collapsed;
            loadingImg2.Visibility = Visibility.Collapsed;
            emptyCart.Visibility = Visibility.Visible;
            paymentbuttons.Visibility = Visibility.Collapsed;
            cashinserted.Visibility = Visibility.Collapsed;


            qrimgText.Text = "Please pay through scanning the QR on your Mobile App!";

            Global.ResponsePOS = "";

            //Global.General.SALE_NO = "";

            if (Global.General.CreditAmount == 0)
            {
                creditAmount.Visibility = Visibility.Collapsed;
                creditText.Visibility = Visibility.Collapsed;
                credittime.Visibility = Visibility.Collapsed;
                Global.General.TOTAL_PAID_AMOUNT = 0;
                Global.General.TOTAL_PURCHASED_AMOUNT = 0;
                Global.General.TOTAL_BALANCE_AMOUNT = 0;
            }
            cashAmount = 0;
            syncTimer.Start();
            Global.General.CashInAmount = 0;
            cancelButton.IsEnabled = true;
            cancelButton.Visibility = Visibility.Visible;



        }

        private void ClosePopupClicked()
        {

            // if the Popup is open, then close it 
            if (StandardPopup.IsOpen)
            {
                StandardPopup.IsOpen = false;
            }
        }

        // Handles the Click event on the Button on the page and opens the Popup. 
        private void ShowPopupOffsetClicked(object sender, RoutedEventArgs e)
        {
            // open the Popup if it isn't open already 
            if (!StandardPopup.IsOpen)
            {
                StandardPopup.IsOpen = true;
            }
        }
        //---------------------------- GENERAL TRANSACTION TIMER-----------------------------------------------------------
        void timer_Tick(object sender, object e)
        {
            basetime = basetime - 1;
            disptimer.Text = "Cart available for: " + basetime.ToString() + " seconds.";
            cashinserted.Text = "Rs. " + Global.General.CashInAmount + " recieved.";
            if (Global.General.CashInAmount > 0)
            {
                cancelButton.IsEnabled = false;
            }
            if (basetime == 40)
            {
                qrimg.Visibility = Visibility.Collapsed;
                loadingImg2.Visibility = Visibility.Visible;
                qrimgText.Text = "Verifying Payment. Please Wait.";

            }
            if (basetime == 0)
            {
                Global.log.Trace("No activity detected for saleNo: " + Global.General.SALE_NO + ". Resetting System");
                ClosePopupClicked();
                ResetProperties();

            }

        }
        //---------------------------- DEV MODE TIMER-----------------------------------------------------------
        public void DevMode(object sender, object e)
        {
            devBaseTime = 4;
            devTimer.Start();

            if (devModeCount == 7)
            {
                devBaseTime = 25;
                devTimer.Start();
                configtext.Text = "";
                Test_prefix = "TEST";
                devMode.Visibility = Visibility.Collapsed;
                configtext.Visibility = Visibility.Visible;
                configbut.Visibility = Visibility.Visible;
                updatebut.Visibility = Visibility.Visible;
                Global.log.Trace("Dev Mode activated");

            }
            else
            {
                devModeCount++;
            }

        }

        public void devTimer_Tick(object sender, object e)
        {
            devBaseTime = devBaseTime - 1;

            if (devBaseTime == 2)
            {
                devModeCount = 0;

            }

            if (devBaseTime == 0)
            {
                Test_prefix = "";
                devMode.Visibility = Visibility.Visible;
                configtext.Visibility = Visibility.Collapsed;
                configbut.Visibility = Visibility.Collapsed;
                updatebut.Visibility = Visibility.Collapsed;
                Global.log.Trace("Dev Mode deactivated");

            }

        }
        //---------------------------- PRODUCT SYNCING TIMER-----------------------------------------------------------
        public async void syncTimer_Tick(object sender, object e)
        {
            syncBaseTime = syncBaseTime - 1;

            if (syncBaseTime == 0 && Global.General.INPROGRESS == false)
            {
                MainGrid.IsEnabled = false;
                CartGrid.IsEnabled = false;

                if (!LoadingPopup.IsOpen)
                {
                    LoadingPopup.IsOpen = true;
                }
                ResetProperties();
                await Task.Factory.StartNew(() => SendUpdateData());
                // await hb.HeartBeat();
                // if the Popup is open, then close it 
                if (LoadingPopup.IsOpen)
                {
                    LoadingPopup.IsOpen = false;
                }
                MainGrid.IsEnabled = true;
                CartGrid.IsEnabled = true;

                UpdateProductList();

            }
            else if (syncBaseTime == 0 && Global.General.INPROGRESS == true)
            {
                syncBaseTime = 50;

            }
            else
            {

            }
        }

        //---------------------------- Credit Amount TIMER-----------------------------------------------------------
        void CreditTimer_Tick(object sender, object e)
        {
            CreditTimerbasetime = CreditTimerbasetime - 1;
            credittime.Text = "Availble for:  " + CreditTimerbasetime.ToString() + " s";


            if (CreditTimerbasetime <= 0 && Global.General.INPROGRESS == false)
            {
                Data.VendingC.SubmitSaleTransaction();
                Global.log.Trace("Credit amount reset");
                Global.General.CreditAmount = 0;
                credittime.Visibility = Visibility.Collapsed;
                creditAmount.Visibility = Visibility.Collapsed;
                creditText.Visibility = Visibility.Collapsed;
                credittime.Visibility = Visibility.Collapsed;
                CreditTimer.Stop();
                ResetProperties();


            }
            else if (CreditTimerbasetime <= 0 && Global.General.INPROGRESS == true)
            {
                CreditTimerbasetime = 60;

            }
            else
            {

            }

        }

    }
}