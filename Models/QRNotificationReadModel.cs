using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingC
{
    public class QRNotificationReadModel
    {
        public string id;
        public string merchantId;
        public string stan;
        public string p2M_ID;
        public string merchantPAN;
        public string ica;
        public string dataHash;
        public decimal transactionAmount;
        public string postingDateTime;
    }
    public class ServerCheckClass
    {
        public bool responseCode { get; set; }
        public string responseDescription { get; set; }

    }
}
