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
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using VendingC.Data;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace VendingC
{
    public class ShoppingCartPageViewModel :
        NotificationBase
    {
        private ObservableCollection<ShoppingCartEntryViewModel> _entries = null;
        private CoreDispatcher _uiDispatcher = null;
        private bool _isLoaded = false;
        private Visibility _buttonVisibility;


        public ShoppingCartPageViewModel ()
        {
            this.ShoppingCart = AppState.ShoppingCart;

            _uiDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        public Visibility ButtonVisibility
            {
            get; 
            set;
              
            }

        /// <summary>
        /// The shopping cart we are using.
        /// </summary>
        public ShoppingCart ShoppingCart
        {
            private set;
            get;
        }

        /// <summary>
        /// The list of view models for the shopping cart entries.
        /// </summary>
        public ReadOnlyObservableCollection<ShoppingCartEntryViewModel> Entries
        {
            private set;
            get;
        }

        /// <summary>
        /// The cost of all the items combined, as a properly formatted string.
        /// </summary>
        public string SubtotalString
        {
            private set;
            get;
        }

        /// <summary>
        /// The total cost of the order, as a properly formatted string.
        /// </summary>
        public string TotalCostString
        {
            private set;
            get;
        }

        /// <summary>
        /// Whether or not the 'Cash' button is enabled.
        /// </summary>
        public bool CashButtonEnabled
        {
            private set;
            get;
        }

        /// <summary>
        /// Whether or not the 'POS' button is enabled.
        /// </summary>
        public bool POSButtonEnabled
        {
            private set;
            get;
        }

        /// <summary>
        /// Whether or not the 'VendingQR' button is enabled.
        /// </summary>
        public bool QRVcButtonEnabled
        {
            private set;
            get;
        }

        /// <summary>
        /// Whether or not the 'MASTER QR' button is enabled.
        /// </summary>
        public bool QRMasterButtonEnabled
        {
            private set;
            get;
        }

        /// <summary>
        /// Whether or not the 'Checkout' button is enabled.
        /// </summary>
        public bool CheckoutButtonEnabled
        {
            private set;
            get;
        }

        /// <summary>
        /// Called when the page is loaded in XAML.
        /// </summary>
        public void OnLoaded()
        {

            // Register the shopping cart change notifications.
            this.ShoppingCart.EntriesChanged += OnShoppingCartEntriesChangedCallback;
            this.ShoppingCart.CostsSummaryChanged += OnShoppingCartCostsSummaryChangedCallback;

            // Ensure our generated values are up to date.
            ResetEntries();
            UpdateCostsSummaryStrings();

            _isLoaded = true;
        }

        /// <summary>
        /// Called when the page is unloaded in XAML.
        /// </summary>
        public void OnUnloaded()
        {
            _isLoaded = false;

            // Unregister the shopping cart change notifications.
            // Since the shopping cart's lifetime will outlive this page, we don't want the shopping cart
            // holding a reference to this class as this will prevent the memory from being freed.
            this.ShoppingCart.CostsSummaryChanged -= OnShoppingCartCostsSummaryChangedCallback;
            this.ShoppingCart.EntriesChanged -= OnShoppingCartEntriesChangedCallback;

            _entries.Clear();
        }

        /// <summary>
        /// Called when one of the entry's 'remove' button is clicked.
        /// </summary>
        public void OnEntryRemoveClick(ShoppingCartEntryViewModel entryViewModel)
        {
            this.ShoppingCart.Remove(entryViewModel.ProductViewModel.Product);
        }

        /// <summary>
        /// Called when the 'Windows Checkout' button is clicked.
        /// </summary>
        public async Task <bool> OnWindowsCheckoutClicked ()
        {
            Global.newCartList = this.ShoppingCart;

            //STARTING MOTOR CONTROLLER
            MotorController mc = new MotorController();
            await mc.StartTransaction(this.ShoppingCart);
           // await Data.VendingC.
           // ();

            //LOGGING TRANSATION DATA IN DATABASE
            // UserDataLib.SubmitSaleTransactionAsync(this.ShoppingCart);
           // ShoppingCart.Clear();
           // await WindowsPaymentOperation.CheckoutAsync(this.ShoppingCart);

            return true;
        }

        /// <summary>
        /// Called when the 'Entries' list in the shopping cart changes.
        /// </summary>
        private void OnShoppingCartEntriesChangedCallback(ShoppingCart sender, ShoppingCartEntriesChangedEventArgs args)
        {
            _uiDispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
            {
                OnShoppingCartEntriesChanged(sender, args);
            });
        }

        /// <summary>
        /// Called when the 'Entries' list in the shopping cart changes.
        /// </summary>
        private void OnShoppingCartEntriesChanged(ShoppingCart sender, ShoppingCartEntriesChangedEventArgs args)
        {
            if (!_isLoaded)
            {
                return;
            }

            switch (args.Type)
            {
            case ShoppingCartEntriesChangedType.EntryAdded:
                var entryViewModel = new ShoppingCartEntryViewModel(this.ShoppingCart, this.ShoppingCart.Entries[args.Index]);
                _entries.Insert(args.Index, entryViewModel);
                break;

            case ShoppingCartEntriesChangedType.EntryRemoved:
                _entries.RemoveAt(args.Index);
                break;

            case ShoppingCartEntriesChangedType.EntriesReset:
            default:
                ResetEntries();
                break;
            }
        }

        /// <summary>
        /// Completely re-creates the list of shopping cart entry view models.
        /// </summary>
        private void ResetEntries()
        {
            var newEntryViewModels =
                from entry in this.ShoppingCart.Entries
                select new ShoppingCartEntryViewModel(this.ShoppingCart, entry);

            _entries = new ObservableCollection<ShoppingCartEntryViewModel>(newEntryViewModels);
            this.Entries = new ReadOnlyObservableCollection<ShoppingCartEntryViewModel>(_entries);

            this.RaisePropertyChanged(nameof(this.Entries));
        }

        /// <summary>
        /// Called when the shopping cart's 'CostsSummary' property changes.
        /// </summary>
        /// <param name="sender"></param>
        private void OnShoppingCartCostsSummaryChangedCallback(ShoppingCart sender)
        {
            _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, UpdateCostsSummaryStrings);
        }

        /// <summary>
        /// Updates the strings that display the cost summary (e.g. sub-total, tax, etc.) of the shopping cart.
        /// </summary>
        private void UpdateCostsSummaryStrings()
        {
            ShoppingCartCostsSummary costsSummary = this.ShoppingCart.CostsSummary;

            // Update values.
            this.SubtotalString = "Rs. " + costsSummary.ItemsSubtotal;
            this.TotalCostString = "Rs. " + costsSummary.Total;

            // Raise all the 'PropertyChanged' events.
            this.RaisePropertyChanged(nameof(this.SubtotalString));
            this.RaisePropertyChanged(nameof(this.TotalCostString));

        }

    }
}
