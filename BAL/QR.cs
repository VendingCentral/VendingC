using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static VendingC.Global;
using System.Configuration;
using System.Text;
using VendingC.Models;
using System.Net.Http;
using System.Diagnostics;

namespace VendingC.Utilities
{
    public class QR
    {
        public class PaymentRecieved
        {
            public Guid Id { get; set; }
            public decimal NetAmount { get; set; } = 0;
            public DateTime DateTime { get; set; } = DateTime.Now;
            public string Status { get; set; } = "Succes";

        }
        public static async Task<string> GetMasterQR(string saleNo, string MachineID, decimal QRamount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Setting Base address.  
                    client.BaseAddress = new Uri("https://apibaflnotification.vendingc.com/api/Notification/GenerateQR");
                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();
                    // HTTP GET
                    string urlParameters = "?saleNo=" + saleNo + "&MachineID=" + MachineID + "&amount=" + QRamount;
                    response = await client.GetAsync(urlParameters).ConfigureAwait(false);
                    _ResponseData _responseData = new _ResponseData();
                    Global.log.Trace("Response API .......!\r\n");
                    Global.log.Trace(response + "\r\n");
                    // Verification  
                    if (response.IsSuccessStatusCode)
                    {
                        // Reading Response.  
                        string result = response.Content.ReadAsStringAsync().Result;
                        result = result.TrimStart('\"');
                        result = result.TrimEnd('\"');
                        result = result.Replace("\\", "");
                        var documentResponse = await response.Content.ReadAsStringAsync();
                        _responseData = JsonConvert.DeserializeObject<_ResponseData>(documentResponse);
                        Global.log.Trace(_responseData.ToString());
                        Global.log.Trace("URL: {0}", _responseData.URL);
                        Global.log.Trace("ResponseCode: {0}", _responseData.ResponseCode);
                        Global.log.Trace("Response Description: {0}", _responseData.ResponseDesc);
                        Global.log.Trace("QR-Code Image string: {0}", _responseData.QRCode);
                        //byte[] arr = Encoding.ASCII.GetBytes(_responseData.QRCode);
                        if (_responseData.QRCode != null)
                        {

                            return _responseData.QRCode;
                        }
                        // List<myDataObject> list = new List<myDataObject>();
                        //  decimal PaidAmount = obj.transactionAmount;

                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Global.log.Trace ( ex.ToString ( ) );
                return null;
            }
        }

        public static async Task<decimal> ReadQRAmountAPIAsync(string MachineID, string SaleID, decimal QRamount)
        {
            //Date = "2022-07-05 11:20:09.0001077";
            MachineID = "50";
            QRamount = 40;
            SaleID = "50-1731275658";
            try
            {
                using (var client = new HttpClient())
                {
                    // Setting Base address.  
                    client.BaseAddress = new Uri("https://apibaflnotification.vendingc.com/api/Notification/Find_Notification");
                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();
                    // HTTP GET
                    string urlParameters = "?MachineID=" + MachineID + " &SaleID=" + SaleID + "&amount=" + QRamount;
                    response = await client.GetAsync(urlParameters).ConfigureAwait(false);
                    // Verification  
                    if (response.IsSuccessStatusCode)
                    {
                        // Reading Response.  
                        string result = response.Content.ReadAsStringAsync().Result;
                        Global.ResponsePOS = result;

                        result = result.TrimStart('\"');
                        result = result.TrimEnd('\"');
                        result = result.Replace("\\", "");

                        var obj = JsonConvert.DeserializeObject<PaymentRecieved>(result);
                        // List<myDataObject> list = new List<myDataObject>();
                        //  decimal PaidAmount = obj.transactionAmount;
                        if (obj.NetAmount > 0)
                        {
                            // amount -= Global.General.ChargesIfAny;
                            Global.General.QRread = true;
                           //TODO CashHandler.SubmitCashTransaction("Notification", "QR", 0, Convert.ToInt32(obj.NetAmount));
                            return obj.NetAmount;
                        }
                        else
                            return 0;
                    }
                    else
                        return 0;
                }
            }
            catch (Exception ex)
            {
                Global.log.Trace ( ex.ToString ( ) );
                return 0;
            }
        }

        // QR CODE FOR VENDING C
        public static async Task<byte[]> FunGenerateQR(string saleNo, string MachineID, decimal QRamount)
        {
            var apiresponse = new byte[0];

            //if (Global.General.Total_Due_Amount <= Global.General.CreditAmount) return 0;

            //int @Amount = Convert.ToInt32(Global.General.Total_Due_Amount - Global.General.CreditAmount);

            // @Amount += Convert.ToInt32(Global.General.ChargesIfAny);

            // for verifying the notification
            //QRamount = Amount;
            RequestQRModel request = new RequestQRModel();
            request.Amount = Convert.ToInt32(QRamount);
            request.AssignedID = MachineID;
            request.verifyTokken = Global.General.SALE_NO;
            apiresponse = await FunctionGenAsync(request);

            return apiresponse;

        }

        public static async Task<byte[]> FunctionGenAsync(RequestQRModel request)
        {
            var apiresponse = new byte[0];

            try
            {
                using (var client = new HttpClient())
                {
                    // Setting Base address.  
                    client.BaseAddress = new Uri("https://apimobileapp.vendingc.com/GenerateQRString");
                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();
                    // HTTP POST
                    response = await client.PostAsJsonAsync(client.BaseAddress, request);
                    StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    //  string urlParameters = "?AssignedID=" + request.AssignedID + "&Amount=" + request.Amount.ToString() + "&verifyTokken=" + request.verifyTokken + "&requestedDateTime=" + request.requestedDateTime;
                    // response = await client.GetAsync(urlParameters);
                    // Verification  
                    if (response.IsSuccessStatusCode)
                    {

                        var documentResponse = await response.Content.ReadAsStringAsync();
                        apiresponse = JsonConvert.DeserializeObject<byte[]>(documentResponse);
                        Global.log.Trace(apiresponse.ToString());
                        
                    }

                }
            }
            catch (Exception ex)
            {
                Global.log.Trace ( ex.ToString ( ) );
                }

            return apiresponse;
        }


        public static async Task<decimal> ReadVendingPayQRAmountAPIAsync(string MachineID, string SaleID, decimal QRamount)
        {
            //Date = "2022-07-05 11:20:09.0001077";
            //QRamount = 5;
            //aleID = "Test-1675162410";
            try
            {
                using (var client = new HttpClient())
                {
                    // Setting Base address.  
                    client.BaseAddress = new Uri("https://apimobileapp.vendingc.com/findQrPayment");
                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();
                    // HTTP GET
                    string urlParameters = "?machineID=" + MachineID + "&amount=" + QRamount + " &saleId=" + SaleID;
                    response = await client.GetAsync(urlParameters).ConfigureAwait(false);
                    // Verification  
                    if (response.IsSuccessStatusCode)
                    {
                        // Reading Response.  
                        string result = response.Content.ReadAsStringAsync().Result;
                        Global.ResponsePOS = result;
                        result = result.TrimStart('\"');
                        result = result.TrimEnd('\"');
                        result = result.Replace("\\", "");

                        var obj = JsonConvert.DeserializeObject<VendingPyPaymentRecieved>(result);
                        // List<myDataObject> list = new List<myDataObject>();
                        //  decimal PaidAmount = obj.transactionAmount;
                        if (obj.NetAmount > 0)
                        {
                            Global.General.QRread = true;
                           // CashHandler.SubmitCashTransaction("Notification", "vendingPay", 0, Convert.ToInt32(obj.NetAmount));
                            return obj.NetAmount;
                        }
                        else
                            return 0;
                    }
                    else
                        return 0;
                }
            }
            catch (Exception ex)
            {
                Global.log.Trace ( ex.ToString ( ) );
                return 0;
            }
        }


    }
}
