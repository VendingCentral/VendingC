using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingC.Utilities
{
    public class CheckInternet
    {
        public bool CheckForInternet()
        {
            bool IsInternetAvailable = false;
            //if internet is available
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                IsInternetAvailable = true;
            else
                IsInternetAvailable = false;
            return IsInternetAvailable;
        }

    }
}
