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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace VendingC
{
    /// <summary>
    /// The view model for the 'FrontPage' page.
    /// </summary>
    class FrontPageViewModel: INotifyPropertyChanged
    {
        public FrontPageViewModel()
        {
            this.ShoppingCart = AppState.ShoppingCart;
            try
                {
                this.ProductList = ProductDatabase.getProductList().Select ( ( product ) => new ProductViewModel ( product ) ).ToList ( );
                }
            catch ( Exception ex ) { Global.log.Trace ( ex.ToString ( ) ); }
            }




        /// <summary>
        /// The list of products (as view models) to display
        /// </summary>
        public IReadOnlyList<ProductViewModel> ProductList
        {
            set;
            get;
        }

        /// <summary>
        /// The shopping cart currently in use.
        /// </summary>
        public ShoppingCart ShoppingCart
        {
            private set;
            get;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// </summary>
        public void OnBuyClick(ProductViewModel productViewModel)
        {
            this.ShoppingCart.Add(productViewModel.Product, quantity: 1);
        }
    }
}
