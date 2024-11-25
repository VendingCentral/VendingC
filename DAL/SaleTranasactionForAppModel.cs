using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingC.DAL
{
    public class SaleTranasactionForAppModel
    {
        public int MachineId { get; set; } = 0;
        public string MachineCode { get; set; }
        public DateTime SaleDate { get; set; } = new DateTime(1990, 1, 1);
        public string SaleNo { get; set; } = "";
        public int PurchaseAmount { get; set; } = 0;
        public int PaidAmount { get; set; } = 0;
        public int BalanceAmount { get; set; } = 0;
        public bool Gender { get; set; } = false;
        public int Age { get; set; } = 0;
        public List<PaymentTranasactionModel> PaymentsTransactions { get; set; }
        public List<ProductTranasactionModel> ProductTranasactions { get; set; }
    }
    public class PaymentTranasactionModel
    {
        public string SaleNo { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public int TransactionAmount { get; set; } = 0;
        public string PaymentDirection { get; set; } = "";
        public string PaymentInfo { get; set; } = "";
        public DateTime ReadDateTime { get; set; } = new DateTime(1990, 1, 1);
    }
    public class ProductTranasactionModel
    {
        public string SaleNo { get; set; } = "";
        public int SlotId { get; set; } = 0;
        public string ProductCode { get; set; } = "";
        public int ProductPrice { get; set; } = 0;
        public string DispenseStatus { get; set; }
        public DateTime TransactionDateTime { get; set; } = new DateTime(1990, 1, 1);
    }
}
