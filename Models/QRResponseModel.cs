using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingC.Models
{
    public class QRResponseModel
    {
        public ResponseModel response { get; set; }
        public string signature { get; set; }

    }
    public class ResponseModel
    {
        public string qrCode { get; set; }
        public string responseCode { get; set; }
        public string responseDesc { get; set; }
    }
    public class PaymentRecieved
    {
        public Guid Id { get; set; }
        public decimal NetAmount { get; set; } = 0;
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Succes";

    }
    public class VendingPyPaymentRecieved
    {
        public int Id { get; set; }
        public decimal NetAmount { get; set; } = 0;
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Transaction not found";
    }
}
