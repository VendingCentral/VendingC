using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingC.Models
{
    public class RequestQRModel
    {
        public string AssignedID { get; set; }
        public double Amount { get; set; }
        public string verifyTokken { get; set; }
        public DateTime requestedDateTime { get; set; }
    }
}
