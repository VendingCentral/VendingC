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
using System.Data;
using System.Linq;

namespace VendingC
{
    public delegate void ShoppingCartEntriesChangedEventHandler(ShoppingCart sender, ShoppingCartEntriesChangedEventArgs args);
    public delegate void ShoppingCartCostsSummaryChangedEventHandler(ShoppingCart sender);

    /// <summary>
    /// An entry in a shopping cart.
    /// </summary>
    public struct ShoppingCartEntry
    {
        public Product Product;
        public int Quantity;
        public bool motorRan { get; internal set; }
        public bool dispensed { get; internal set; }
        public bool processed { get; internal set; }
        public int slotId { get; internal set; }
        public int slotNo { get; internal set; }
        public DateTime dispenseTime { get; internal set; }
        
        public string failReason { get; internal set; }
    }

    /// <summary>
    /// The costs summary (e.g. sub-total, tax, etc.) of a shopping cart.
    /// </summary>
    public struct ShoppingCartCostsSummary
    {
        public int ItemsSubtotal;
        public int Total;
        public decimal CreditAmount;
    }

    /// <summary>
    /// Manages all the business logic of a shopping cart.
    /// </summary>
    public class ShoppingCart
    {
        private List<ShoppingCartEntry> _shoppingCartEntries = null;
        public ShoppingCartPageViewModel ViewModel2;



        public ShoppingCart ()
        {
        _shoppingCartEntries = new List<ShoppingCartEntry>();
            UpdateCostsSummary();
           
    }

        /// <summary>
        /// The list of entries in the shopping cart.
        /// </summary>
        public List<ShoppingCartEntry> Entries
        {
            get
            {
                return _shoppingCartEntries;
            }
        }

        /// <summary>
        /// Raised when the 'Entries' list is changed or when one of its items is updated.
        /// </summary>
        public event ShoppingCartEntriesChangedEventHandler EntriesChanged;

        /// <summary>
        /// Raised when the 'CostsSummary' property is changed.
        /// </summary>
        public event ShoppingCartCostsSummaryChangedEventHandler CostsSummaryChanged;


        /// <summary>
        /// The costs summary (e.g. sub-total, tax, etc.) of the shopping cart
        /// </summary>
        public ShoppingCartCostsSummary CostsSummary
        {
            private set;
            get;
        }

     

        private void AddOrUpdateEntry(Product product, Func<ShoppingCartEntry, ShoppingCartEntry> updateFunc)
        {
            this.ViewModel2 = new ShoppingCartPageViewModel ( );

            //int productIndex = _shoppingCartEntries.IndexOf((entryy) => (entryy.Product == product));

            // if (productIndex < 4)
       
                if (Entries.Count < 3)
                {
                    // Create new entry.
                    ShoppingCartEntry entry = new ShoppingCartEntry()
                    {
                        Product = product,
                        // Quantity = 0,
                    };

                    // Allow update.
                    entry = updateFunc(entry);

                    // Add to list.
                    _shoppingCartEntries.Add(entry);

                    // Raise changed event.
                    RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType.EntryAdded, _shoppingCartEntries.Count - 1);
                }
          
            //if ( Entries.Count == 0 )
           //     {
          //      ViewModel2.CheckoutButtonEnabled = true;
           //     }
            // else
            //  {
            //   ShoppingCartEntry entry = _shoppingCartEntries[productIndex];

            // Apply update
            //   entry = updateFunc(entry);
            //   _shoppingCartEntries[productIndex] = entry;

            // Raise changed event.
            //    RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType.EntryUpdated, productIndex);
            //}
            }

        /// <summary>
        /// Adds the specified quantity of the product in the cart. If the product doesn't currently exist
        /// it will be added.
        /// </summary>
        public void Add(Product product, int quantity)
        {
            if (Counts(product) < product.saleQuantity)
            {
                AddOrUpdateEntry(
                product,
                (entry) =>
            {
                int newQuantity = entry.Quantity + quantity;

                if (newQuantity < 0)
                {
                    throw new Exception("Quantity can't go below 0.");
                }

                entry.Quantity = newQuantity;
                return entry;
            });
            }
        }
        private void RemoveIf(Func<ShoppingCartEntry, bool> pred)
        {
            int lastRemovedIndex = -1;
            int removedCount = 0;

            for (int i = _shoppingCartEntries.Count - 1; i >= 0; --i)
            {
                if (pred(_shoppingCartEntries[i]))
                {
                    _shoppingCartEntries.RemoveAt(i);
                    lastRemovedIndex = i;
                    removedCount++;
                    break;
                }
            }

            // Raise changed event
            if (removedCount == 1)
            {
                RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType.EntryRemoved, lastRemovedIndex);
            }
            else if (removedCount > 1)
            {
                // Too many items were changed.
                // So just report a reset.
                RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType.EntriesReset);
            }
        }

        /// <summary>
        /// Removes the specified product from the shopping cart.
        /// </summary>
        /// <param name="product">The product to be removed.</param>
        public void Remove(Product product)
        {
            RemoveIf((entry) => entry.Product == product);
        }

        /// <summary>
        /// Removes all the items from the Shopping Cart.
        /// </summary>
        public void Clear()
        {
            for(int i = Entries.Count; i >=0; i--)
            {
                RemoveIf((entry) => true);

            }
        }

        
        /// <summary>
        /// Checks whether or not the shopping cart contains the specified 'Product'.
        /// </summary>
        /// <param name="product">The product to check for.</param>
        /// <returns>Whether or not the product is in the shopping cart.</returns>
        public bool Contains(Product product)
        {
            return _shoppingCartEntries.Any((entry) => (entry.Product == product));
        }
        public int Counts(Product product)
        {
            return _shoppingCartEntries.Count((entry) => (entry.Product == product));
        }

        /// <summary>
        /// Called when the 'Entries' list has changed, so that listeners can be notified.
        /// </summary>
        private void RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType type, int itemIndex = -1)
        {
            // Update 'CostsSummary' property.
            UpdateCostsSummary();

            // Invoke public event.
            var args = new ShoppingCartEntriesChangedEventArgs(type, itemIndex);
            this.EntriesChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Updates the 'CostsSummary' property.
        /// </summary>
        private void UpdateCostsSummary()
        {
            this.CostsSummary = CalculateCostsSummary(this.Entries);
            this.CostsSummaryChanged?.Invoke(this);
        }

        /// <summary>
        /// Calculate the summary of costs (e.g. sub-total, tax, etc.) for the given list of shopping cart entries.
        /// </summary>
        private static ShoppingCartCostsSummary CalculateCostsSummary(IReadOnlyList<ShoppingCartEntry> entries)
        {
            ShoppingCartCostsSummary costsSummary = new ShoppingCartCostsSummary();

            foreach (ShoppingCartEntry entry in entries)
            {
                costsSummary.ItemsSubtotal += entry.Quantity * entry.Product.cost;
                
            }
            costsSummary.Total = costsSummary.ItemsSubtotal;

            return costsSummary;
        }

    }
}
