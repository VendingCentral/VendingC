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
using System.Threading;

namespace VendingC
{
    /// <summary>
    /// Contains the details for a product that is available for sale.
    /// </summary>
   public class Product
    {
     
        public Product(int productId, String productName, String description, String productCode, int cost, String imageUrl, int saleQuantity, String syncDateTime)
        {
            this.productId = productId;
            this.productName = productName;
            this.description = description;
            this.productCode = productCode;
            this.cost = cost;
            this.imageUrl = imageUrl;
            this.saleQuantity = saleQuantity;
            this.syncDateTime = syncDateTime;
        }

        public int productId { get; private set; }

        /// <summary>
        /// The image of the product.
        /// </summary>
        public String imageUrl { get; private set; }

        /// <summary>
        /// The name of the product.
        /// </summary>
        public String productName { get; private set; }

        /// <summary>
        /// The detailed description of the product.
        /// </summary>
        public String description { get; private set; }

        /// <summary>
        /// The amount the product cost.
        /// Note: Using the 'int' type significantly reduces the likelihood of rouding errors compared to other
        /// types such as 'double'.
        /// </summary>
        public int cost { get; private set; }
        public String productCode { get; private set; }
        public String syncDateTime { get; private set; }
        public int saleQuantity { get; private set; }

    }
}
