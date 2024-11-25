//using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VendingC.DAL;
using Windows.Storage;
using HttpClient = System.Net.Http.HttpClient;

namespace VendingC.Data
    {
    class VendingC
        {
        public VendingC ()
            {
            InitializeDB ( );
            }

        public async static void InitializeDB ()
            {
            await ApplicationData.Current.LocalFolder.CreateFileAsync ( "VendingC.db", CreationCollisionOption.OpenIfExists );
            string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );

            using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                {
                con.Open ( );
                string initCMD = "CREATE TABLE IF NOT EXISTS " +
                                "TBLSLOT (" +
                                "SLOTID INT PRIMARY KEY," +
                                "SLOTNO INT UNIQUE," +
                                "PRODUCTID INT NULL," +
                                "PRODUCTCODE NVARCHAR(500) NULL," +
                                "PRODUCTNAME NVARCHAR(500) NULL," +
                                "DESCRIPTION NVARCHAR(500) NULL," +
                                "IMAGEURL NVARCHAR(100) NULL," +
                                "CAPACITY INT NULL," +
                                "PRICE INT NULL," +
                                "CURRENTCOUNT INT NULL," +
                                "REFILLCOUNT INT NULL," +
                                "ISACTIVE BOOL NULL," +
                                "ISDELETED BOOL NULL," +
                                "DELETEDDATE DATETEIME NULL," +
                                "MODIFIEDDATE DATETEIME NULL," +
                                "SYNCDATETIME DATETEIME NULL)";

                SqliteCommand CMDcreateTable = new SqliteCommand ( initCMD, con );
                CMDcreateTable.ExecuteReader ( );

                //initCMD = "CREATE UNIQUE INDEX idx_tblslot_slotno ON TBLSLOT(SLOTNO);";
                //CMDcreateTable = new SqliteCommand(initCMD, con);
                //CMDcreateTable.ExecuteReader();

                initCMD = "CREATE TABLE IF NOT EXISTS " +
                               "TBLMACHINE (" +
                               "MACHINEID INT PRIMARY KEY," +
                               "COMPANYID INT NULL," +
                               "MACHINECODE NVARCHAR(500) NULL," +
                               "MACHINENAME NVARCHAR(500) NULL," +
                               "TOTALSLOTS INT NULL," +
                               "PRODUCTLIMITPERTXN INT NULL," +
                               "HELPLINENUMBER NVARCHAR(500) NULL," +
                               "LOCATION NVARCHAR(500) NULL," +
                               "MACHINEPASSWORD NVARCHAR(500) NULL," +
                               "ISCASH BOOL NULL," +
                               "ISVENDINGPAY BOOL NULL," +
                               "VENDINGPAY_PAN NVARCHAR(500) NULL," +
                               "VENDINGPAY_CHARGESPERCENTAGE decimal(3,2) NULL," +
                               "ISVISAQR BOOL NULL," +
                               "VISAQR_PAN NVARCHAR(500) NULL," +
                               "VISAQR_CHARGESPERCENTAGE decimal(3,2) NULL," +
                               "ISMASTERQR BOOL NULL," +
                               "MASTERQR_PAN NVARCHAR(500) NULL," +
                               "MASTERQR_CHARGESPERCENTAGE decimal(3,2) NULL," +
                               "ISPOS_ECR BOOL NULL," +
                               "POS_ECR_ID NVARCHAR(500) NULL," +
                               "POS_CHARGESPERCENTAGE decimal(3,2) NULL," +
                               "INSTALLEDBY NVARCHAR(500) NULL," +
                               "INSTALLEDDATE DATETEIME NULL)";
                CMDcreateTable = new SqliteCommand ( initCMD, con );
                CMDcreateTable.ExecuteReader ( );

                initCMD = "CREATE TABLE IF NOT EXISTS " +
                 "TBLSALETRANS(" +
                 "SALETRANSID INTEGER PRIMARY KEY AUTOINCREMENT," +
                 "MACHINEID INT NULL," +
                 "COMPANYID INT NULL," +
                 "MACHINECODE NVARCHAR(500) NULL," +
                 "SALEDATETIME DATETEIME NULL," +
                 "SALENO NVARCHAR(500) NOT NULL," +
                 "PURCHASEAMOUNT INT NULL," +
                 "PAIDAMOUNT INT NULL," +
                 "BALANCEAMOUNT INT NULL," +
                 "GENDER BOOL NULL," +
                 "AGE INT NULL," +
                 "ISDATASENT BOOL NULL)";

                CMDcreateTable = new SqliteCommand ( initCMD, con );
                CMDcreateTable.ExecuteReader ( );

                initCMD = "CREATE TABLE IF NOT EXISTS " +
                    "TBLPAYMENTTRANS(" +
                    "PAYMENTTRANSID INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "SALENO NVARCHAR(500) NOT NULL," +
                    "PAYMENTMETHOD NVARCHAR(50) NULL," +
                    "TRANSAMOUNT INT NULL," +
                    "PAYMENTDIRECTION NVARCHAR(50) NULL," +
                    "PAYMENTINFO NVARCHAR(50) NULL," +
                    "READDATETIME DATETEIME NULL)";

                CMDcreateTable = new SqliteCommand ( initCMD, con );
                CMDcreateTable.ExecuteReader ( );


                initCMD = "CREATE TABLE IF NOT EXISTS " +
                       "TBLPRODUCTTRANS(" +
                       "PRODTRANSID INTEGER PRIMARY KEY AUTOINCREMENT," +
                       "SALENO NVARCHAR(500) NOT NULL," +
                       "SLOTID INT NULL," +
                       "PRODUCTCODE NVARCHAR(500) NULL," +
                       "PRODUCTAMOUNT INT NULL," +
                       "DISPENSESTATUS NVARCHAR(50) NULL," +
                       "FAILREASON NVARCHAR(50) NULL," +
                       "DISPENSEDATETIME DATETEIME NULL)";

                CMDcreateTable = new SqliteCommand ( initCMD, con );
                CMDcreateTable.ExecuteReader ( );

                con.Close ( );
                }
            }

        public static async Task<bool> addSlot ( int SLOTID, int SLOTNO, int PRODUCTID, String PRODUCTCODE, String PRODUCTNAME, String DESCRIPTION, String IMAGEURL, int CAPACITY, int PRICE, int CURRENTCOUNT,int REFILLCOUNT, bool ISACTIVE, bool ISDELETED, DateTime DELETEDDATE, DateTime MODIFIEDDATE, DateTime SYNCDATETIME )
            {
            if ( !SLOTID.Equals ( "" ) && !PRODUCTCODE.Equals ( "" ) )
                {
                string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );

                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );
                    SqliteCommand CMD_Insert = new SqliteCommand ( );
                    CMD_Insert.Connection = con;

                    //CMD_Insert.CommandText = "REPLACE INTO TBLSLOT(SLOTID, PRODUCTCODE, PRODUCTID, PRICE, DESCRIPTION, CAPACITY, CURRENTCOUNT, MACHINEID, ISACTIVE, ISDELETED, COMPANYID, SLOTNO, LIVE_SLOTID, DELETEDDATE, MODIFIEDDATE, SYNCDATETIME) VALUES(@SLOTID, @PRODUCTCODE, @PRODUCTID, @PRICE, @DESCRIPTION, @CAPACITY, @CURRENTCOUNT, @MACHINEID, @ISACTIVE, @ISDELETED, @COMPANYID, @SLOTNO, @LIVE_SLOTID, @DELETEDDATE, @MODIFIEDDATE, @SYNCDATETIME);";
                    CMD_Insert.CommandText = "REPLACE INTO TBLSLOT VALUES(@SLOTID,@SLOTNO,@PRODUCTID,@PRODUCTCODE,@PRODUCTNAME,@DESCRIPTION,@IMAGEURL,@CAPACITY ,@PRICE,@CURRENTCOUNT,@REFILLCOUNT,@ISACTIVE,@ISDELETED,@DELETEDDATE,@MODIFIEDDATE,@SYNCDATETIME);";
                    CMD_Insert.Parameters.AddWithValue ( "@SLOTID", SLOTID );
                    CMD_Insert.Parameters.AddWithValue ( "@SLOTNO", SLOTNO );
                    CMD_Insert.Parameters.AddWithValue ( "@PRODUCTID", PRODUCTID );
                    CMD_Insert.Parameters.AddWithValue ( "@PRODUCTCODE", PRODUCTCODE );
                    CMD_Insert.Parameters.AddWithValue ( "@PRODUCTNAME", PRODUCTNAME );
                    CMD_Insert.Parameters.AddWithValue ( "@DESCRIPTION", DESCRIPTION );
                    CMD_Insert.Parameters.AddWithValue ( "@IMAGEURL", IMAGEURL );
                    CMD_Insert.Parameters.AddWithValue ( "@CAPACITY", CAPACITY );
                    CMD_Insert.Parameters.AddWithValue ( "@PRICE", PRICE );
                    CMD_Insert.Parameters.AddWithValue ( "@CURRENTCOUNT", CURRENTCOUNT );
                    CMD_Insert.Parameters.AddWithValue ( "@REFILLCOUNT", REFILLCOUNT);
                    CMD_Insert.Parameters.AddWithValue ( "@ISACTIVE", ISACTIVE );
                    CMD_Insert.Parameters.AddWithValue ( "@ISDELETED", ISDELETED );
                    CMD_Insert.Parameters.AddWithValue ( "@DELETEDDATE", DELETEDDATE );
                    CMD_Insert.Parameters.AddWithValue ( "@MODIFIEDDATE", MODIFIEDDATE );
                    CMD_Insert.Parameters.AddWithValue ( "@SYNCDATETIME", SYNCDATETIME );

                    CMD_Insert.ExecuteReader ( );

                    con.Close ( );
                    }
                }
            return true;
            }

        public static async Task<bool> addMachine ( int MACHINEID, int COMPANYID, String MACHINECODE, String MACHINENAME, int TOTALSLOTS,int PRODUCTLIMITPERTXN, String HELPLINENUMBER, String LOCATION, String MACHINEPASSWORD, bool ISCASH, bool ISVENDINGPAY, String VENDINGPAY_PAN, decimal VENDINGPAY_CHARGESPERCENTAGE, bool ISVISAQR, String VISAQR_PAN, decimal VISAQR_CHARGESPERCENTAGE, bool ISMASTERQR, String MASTERQR_PAN, decimal MASTERQR_CHARGESPERCENTAGE, bool ISPOS_ECR, String POS_ECR_ID, decimal POS_CHARGESPERCENTAGE, String INSTALLEDBY, String INSTALLEDDATE )
            {
            if ( !MACHINEID.Equals ( "" ) && !MACHINECODE.Equals ( "" ) )
                {
                string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );

                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );
                    SqliteCommand CMD_Insert = new SqliteCommand ( );
                    CMD_Insert.Connection = con;

                    CMD_Insert.CommandText = "REPLACE INTO TBLMACHINE VALUES(@MACHINEID,@COMPANYID,@MACHINECODE,@MACHINENAME,@TOTALSLOTS,@PRODUCTLIMITPERTXN,@HELPLINENUMBER,@LOCATION,@MACHINEPASSWORD,@ISCASH, @ISVENDINGPAY,@VENDINGPAY_PAN,@VENDINGPAY_CHARGESPERCENTAGE, @ISVISAQR, @VISAQR_PAN,@VISAQR_CHARGESPERCENTAGE,@ISMASTERQR,@MASTERQR_PAN,@MASTERQR_CHARGESPERCENTAGE,@ISPOS_ECR,@POS_ECR_ID,@POS_CHARGESPERCENTAGE,@INSTALLEDBY,@INSTALLEDDATE);";

                    CMD_Insert.Parameters.AddWithValue ( "@MACHINEID", MACHINEID );
                    CMD_Insert.Parameters.AddWithValue ( "@COMPANYID", COMPANYID );
                    CMD_Insert.Parameters.AddWithValue ( "@MACHINECODE", MACHINECODE );
                    CMD_Insert.Parameters.AddWithValue ( "@MACHINENAME", MACHINENAME );
                    CMD_Insert.Parameters.AddWithValue ( "@TOTALSLOTS", TOTALSLOTS );
                    CMD_Insert.Parameters.AddWithValue ("@PRODUCTLIMITPERTXN", PRODUCTLIMITPERTXN);
                    CMD_Insert.Parameters.AddWithValue ( "@HELPLINENUMBER", HELPLINENUMBER );
                    CMD_Insert.Parameters.AddWithValue ( "@LOCATION", LOCATION );
                    CMD_Insert.Parameters.AddWithValue ( "@MACHINEPASSWORD", MACHINEPASSWORD );
                    CMD_Insert.Parameters.AddWithValue ( "@ISCASH", ISCASH );
                    CMD_Insert.Parameters.AddWithValue ( "@ISVENDINGPAY", ISVENDINGPAY );
                    CMD_Insert.Parameters.AddWithValue ( "@VENDINGPAY_PAN", VENDINGPAY_PAN );
                    CMD_Insert.Parameters.AddWithValue ( "@VENDINGPAY_CHARGESPERCENTAGE", VENDINGPAY_CHARGESPERCENTAGE );
                    CMD_Insert.Parameters.AddWithValue ( "@ISVISAQR", ISVISAQR );
                    CMD_Insert.Parameters.AddWithValue ( "@VISAQR_PAN", VISAQR_PAN );
                    CMD_Insert.Parameters.AddWithValue ( "@VISAQR_CHARGESPERCENTAGE", VISAQR_CHARGESPERCENTAGE );
                    CMD_Insert.Parameters.AddWithValue ( "@ISMASTERQR", ISMASTERQR );
                    CMD_Insert.Parameters.AddWithValue ( "@MASTERQR_PAN", MASTERQR_PAN );
                    CMD_Insert.Parameters.AddWithValue ( "@MASTERQR_CHARGESPERCENTAGE", MASTERQR_CHARGESPERCENTAGE );
                    CMD_Insert.Parameters.AddWithValue ( "@ISPOS_ECR", ISPOS_ECR );
                    CMD_Insert.Parameters.AddWithValue ( "@POS_ECR_ID", POS_ECR_ID );
                    CMD_Insert.Parameters.AddWithValue ( "@POS_CHARGESPERCENTAGE", POS_CHARGESPERCENTAGE );
                    CMD_Insert.Parameters.AddWithValue ( "@INSTALLEDBY", INSTALLEDBY );
                    CMD_Insert.Parameters.AddWithValue ( "@INSTALLEDDATE", INSTALLEDDATE );


                    CMD_Insert.ExecuteReader ( );

                    con.Close ( );
                    }
                }
            return true;
            }

        public class productDetails
            {
            public int productId { get; set; }
            public String productName { get; set; }
            public String description { get; set; }
            public String productCode { get; set; }
            public int costPrice { get; set; }
            public String imageUrl { get; set; }
            public String syncDateTime { get; set; }
            public int saleQuantity { get; set; }

            public productDetails ( int productId, String productName, String description, String productCode, int costPrice, String imageUrl, int saleQuantity, String syncDateTime )
                {
                this.productId = productId;
                this.productName = productName;
                this.description = description;
                this.productCode = productCode;
                this.costPrice = costPrice;
                this.imageUrl = imageUrl;
                this.saleQuantity = saleQuantity;
                this.syncDateTime = syncDateTime;

                }
            }

        public static async Task<bool> SendDataToServerAsync ()
            {
            var model = new SaleTranasactionForAppModel ( );
            var responsee = new HttpResponseMessage ( );
            //Dictionary<string, object> data = new Dictionary<string, object>();
            string baseURL = "https://webapis.vendingc.com/api/SalesTransaction/";
            string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );
            using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                {
                con.Open ( );
                String selectCmd1 = "SELECT * FROM TBLSALETRANS WHERE ISDATASENT = 0 LIMIT 5;";
                SqliteCommand cmd_getRec1 = new SqliteCommand ( selectCmd1, con );
                SqliteDataReader reader1 = cmd_getRec1.ExecuteReader ( );
                while ( reader1.Read ( ) )
                    {
                    model.MachineId = Global.MachineID;
                    model.MachineCode = Global.MachineCode;
                    model.SaleDate = Convert.ToDateTime ( reader1 [ "SALEDATETIME" ] );
                    model.SaleNo = reader1 [ "SALENO" ].ToString ( );
                    model.PurchaseAmount = Convert.ToInt32 ( reader1 [ "PURCHASEAMOUNT" ] );
                    model.PaidAmount = Convert.ToInt32 ( reader1 [ "PAIDAMOUNT" ] );
                    model.BalanceAmount = Convert.ToInt32 ( reader1 [ "BALANCEAMOUNT" ] );
                    model.Gender = Convert.ToBoolean ( reader1 [ "GENDER" ] );
                    model.Age = Convert.ToInt32 ( reader1 [ "AGE" ] );

                    List<PaymentTranasactionModel> PaymentTransaction = new List<PaymentTranasactionModel> ( );

                    String selectCmd2 = "SELECT * FROM TBLPAYMENTTRANS WHERE SALENO='" + reader1 [ "SALENO" ].ToString ( ) + "';";
                    SqliteCommand cmd_getRec2 = new SqliteCommand ( selectCmd2, con );
                    SqliteDataReader reader2 = cmd_getRec2.ExecuteReader ( );
                    while ( reader2.Read ( ) )
                        {
                        PaymentTransaction.Add ( new PaymentTranasactionModel
                            {
                            SaleNo = reader2 [ "SALENO" ].ToString ( ),
                            PaymentMethod = reader2 [ "PAYMENTMETHOD" ].ToString ( ),
                            TransactionAmount = Convert.ToInt32 ( reader2 [ "TRANSAMOUNT" ] ),
                            PaymentDirection = reader2 [ "PAYMENTDIRECTION" ].ToString ( ),
                            PaymentInfo = reader2 [ "PAYMENTINFO" ].ToString ( ),
                            ReadDateTime = Convert.ToDateTime ( reader2 [ "READDATETIME" ] ),
                            } );
                        }
                    model.PaymentsTransactions = PaymentTransaction;

                    List<ProductTranasactionModel> ProductTranasaction = new List<ProductTranasactionModel> ( );
                    selectCmd2 = "SELECT * FROM TBLPRODUCTTRANS WHERE SALENO='" + reader1 [ "SALENO" ].ToString ( ) + "';";
                    cmd_getRec2 = new SqliteCommand ( selectCmd2, con );
                    reader2 = cmd_getRec2.ExecuteReader ( );
                    while ( reader2.Read ( ) )
                        {
                        ProductTranasaction.Add ( new ProductTranasactionModel
                            {
                            SaleNo = reader2 [ "SALENO" ].ToString ( ),
                            SlotId = Convert.ToInt32 ( reader2 [ "SLOTID" ] ),
                            ProductCode = reader2 [ "PRODUCTCODE" ].ToString ( ),
                            ProductPrice = Convert.ToInt32 ( reader2 [ "PRODUCTAMOUNT" ] ),
                            DispenseStatus = reader2 [ "DISPENSESTATUS" ].ToString ( ),
                            TransactionDateTime = Convert.ToDateTime ( reader2 [ "DISPENSEDATETIME" ] )
                            } );
                        }
                    model.ProductTranasactions = ProductTranasaction;

                    List<SaleTranasactionForAppModel> requestObj = new List<SaleTranasactionForAppModel> ( );

                    requestObj.Add ( model );

                    string json = JsonConvert.SerializeObject ( requestObj );

                    var stringContent = new StringContent ( json, UnicodeEncoding.UTF8, "application/json" );

                    using ( var httpClient = new HttpClient ( ) )
                        {

                        responsee = Task.Run ( () => httpClient.PostAsync ( baseURL, stringContent ) ).Result;
                        }

                    if ( responsee.IsSuccessStatusCode )
                        {
                        try
                            {
                            SqliteCommand CMD_Insert = new SqliteCommand ( );
                            CMD_Insert.Connection = con;
                            CMD_Insert.CommandText = "UPDATE TBLSALETRANS SET ISDATASENT = 1 WHERE SALENO = @SALENO AND SALETRANSID = @SALETRANSID;";
                            CMD_Insert.Parameters.AddWithValue ( "@SALENO", reader1 [ "SALENO" ].ToString ( ) );
                            CMD_Insert.Parameters.AddWithValue ( "@SALETRANSID", reader1 [ "SALETRANSID" ].ToString ( ) );
                            CMD_Insert.ExecuteReader ( );
                            }
                        catch ( Exception ex )
                            {
                            Global.log.Trace ( "Exception sending data to server: " + ex.ToString ( ) );
                            }
                        }
                    else
                        {
                        Global.log.Trace ( "Response from sending data to server: " + responsee );

                        }

                    }

                con.Close ( );

                }
            return true;
            }

        public static async Task<bool> DeactivateSlotOnServer ( int slotId )
            {
            string baseURL = $"https://webapis.vendingc.com/api/ProductSlot/ActivateSlot?SlotId={slotId}&status=false";
            var requestBody = new
                {
                SlotId = slotId,
                status = false
                };
            var content = new StringContent ( Newtonsoft.Json.JsonConvert.SerializeObject ( requestBody ), UnicodeEncoding.UTF8, "application/json" );

            using ( var httpClient = new HttpClient ( ) )
                {

                var response = Task.Run ( () => httpClient.PutAsync ( baseURL, content ) ).Result;
                if ( response.IsSuccessStatusCode )
                    {
                    Global.log.Trace ( "Slot" + slotId + "deactivated" );
                    return true;
                    }
                else
                    {
                    return false;
                    }
                }
            }

        public static async Task<bool> CheckServerConnection ()
            {
            int machineID = Global.MachineID;
            string baseURL = $"https://webapis.vendingc.com/api/ProductSlot/GetMachineSlots/{machineID}/";
            try
                {
                using ( var httpClient = new HttpClient ( ) )
                    {
                    var response = Task.Run ( () => httpClient.GetAsync ( baseURL ) ).Result;
                    if ( response.IsSuccessStatusCode )
                        {
                        return true;
                        }
                    }
                }

            catch
                {

                }
            return false;
            }

        public static async Task<bool> GetMachineConfigServer ( string machineID )
            {
            string baseURL = $"https://webapis.vendingc.com/api/Machine/{machineID}/";

            string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );

            using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                {
                con.Open ( );
                SqliteCommand cmd_getRec = new SqliteCommand ( "DELETE FROM TBLMACHINE;VACUUM;", con );
                cmd_getRec.ExecuteNonQuery ( );
                con.Close ( );
                }

            using ( var httpClient = new HttpClient ( ) )
                {
                var response = Task.Run ( () => httpClient.GetAsync ( baseURL ) ).Result;
                if ( response.IsSuccessStatusCode )
                    {
                    var reponseData = response.Content.ReadAsStringAsync ( ).Result;
                    dynamic json = JsonConvert.DeserializeObject ( reponseData );
                    try
                        {
                        addMachine (
                        Convert.ToInt32 ( json [ "id" ] ),
                        Convert.ToInt32 ( json [ "companyId" ] ),
                        Convert.ToString ( json [ "machineCode" ] ),
                        Convert.ToString ( json [ "machineName" ] ),
                        Convert.ToInt32 ( json [ "totalSlots" ] ),
                        Convert.ToInt32 ( json["productLimitPerTxn"]),
                        Convert.ToString ( json [ "helpLineNumber" ] ),
                        Convert.ToString ( json.machineConfiguration.location ),
                        Convert.ToString ( json.machineConfiguration.machinePassword ),
                        Convert.ToBoolean ( json.machineConfiguration.isCash ),
                        Convert.ToBoolean ( json.machineConfiguration.isVendingPay ),
                        Convert.ToString ( json.machineConfiguration.vendingPay_PAN ),
                        Convert.ToDecimal ( json.machineConfiguration.vendingPay_ChargesPercentage ),
                        Convert.ToBoolean ( json.machineConfiguration.isVisaQR ),
                        Convert.ToString ( json.machineConfiguration.visaQR_PAN ),
                        Convert.ToDecimal ( json.machineConfiguration.visaQR_ChargesPercentage ),
                        Convert.ToBoolean ( json.machineConfiguration.isMasterQR ),
                        Convert.ToString ( json.machineConfiguration.masterQR_PAN ),
                        Convert.ToDecimal ( json.machineConfiguration.masterQR_ChargesPercentage ),
                        Convert.ToBoolean ( json.machineConfiguration.isPOS_ECR ),
                        Convert.ToString ( json.machineConfiguration.poS_ECR_Id ),
                        Convert.ToDecimal ( json.machineConfiguration.poS_ChargesPercentage ),
                         Convert.ToString ( json.machineConfiguration.installedBy ),
                        Convert.ToString ( json.machineConfiguration.installedDate )
                        );
                        }
                    catch ( Exception ex )
                        {
                        Global.log.Trace ( "Exception getting machine configurations from server: " + ex.ToString ( ) );

                        }
                    Global.MachineID = Convert.ToInt32 ( json [ "id" ] );
                    Global.MachineCode = Convert.ToString ( json [ "machineCode" ] );
                    Global.CompanyID = Convert.ToInt32 ( json [ "companyId" ] );
                    Global.PaymentType.CashPay = Convert.ToBoolean ( json.machineConfiguration.isCash );
                    Global.PaymentType.PosPay = Convert.ToBoolean ( json.machineConfiguration.isPOS_ECR );
                    Global.PaymentType.VisaQRPay = Convert.ToBoolean ( json.machineConfiguration.isVisaQR );
                    Global.PaymentType.MasterQRPay = Convert.ToBoolean ( json.machineConfiguration.isMasterQR );
                    Global.PaymentType.VendiAppPay = Convert.ToBoolean ( json.machineConfiguration.isVendingPay );
                    }
                }
            Global.log.Trace ( "Machine Configurations fetched from server. MachineID = " + Global.MachineID + "MachineCode=" + Global.MachineCode + "  CompanyID=" + Global.CompanyID );
            return true;
            }

        public static bool GetMachineConfigLocal ()
            {
            try
                {
                string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );
                SqliteDataReader reader;
                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );

                    String selectCmd = "SELECT * FROM TBLMACHINE";
                    SqliteCommand cmd_getRec = new SqliteCommand ( selectCmd, con );

                    reader = cmd_getRec.ExecuteReader ( );

                    while ( reader.Read ( ) )
                        {
                        Global.MachineID = Convert.ToInt32 ( reader [ "MACHINEID" ] );
                        Global.MachineCode = Convert.ToString ( reader [ "MACHINECODE" ] );
                        Global.CompanyID = Convert.ToInt32(reader["COMPANYID"]);
                        Global.ProductLimit = Convert.ToInt32(reader["PRODUCTLIMITPERTXN"]);
                        Global.PaymentType.CashPay = Convert.ToBoolean ( reader [ "ISCASH" ] );
                        Global.PaymentType.VendiAppPay = Convert.ToBoolean ( reader [ "ISVENDINGPAY" ] );
                        Global.PaymentType.VisaQRPay = Convert.ToBoolean ( reader [ "ISVISAQR" ] );
                        Global.PaymentType.MasterQRPay = Convert.ToBoolean ( reader [ "ISMASTERQR" ] );
                        Global.PaymentType.PosPay = Convert.ToBoolean ( reader ["ISPOS_ECR"] );
                        }
                    con.Close ( );

                    if ( Global.MachineID != 0 )
                        {
                        return true;
                        }
                    }
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( "Exception getting local machine config: " + ex.ToString ( ) );
                }
            return false;

            }




        //public static List<Product> GetProductsFromServer()
        public static async Task<bool> GetProductsFromServer ()
            {
            var x = 0;
            try
                {
                string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );
                SqliteDataReader reader;

                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );

                    //String selectCmd = "SELECT * FROM tblProduct";
                    SqliteCommand cmd_getRec = new SqliteCommand ( "SELECT COUNT(*) AS COUNT FROM TBLSALETRANS WHERE ISDATASENT = 0;", con );

                    reader = cmd_getRec.ExecuteReader ( );

                    while ( reader.Read ( ) )
                        {
                        x = Convert.ToInt32 ( reader [ "COUNT" ].ToString ( ) );
                        }
                    con.Close ( );
                    }
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( "Exception getting products from server: " + ex.ToString ( ) );

                }
            if ( x == 0 && Global.General.INPROGRESS == false )
                {
                int machineID = Global.MachineID;
                string baseURL = $"https://webapis.vendingc.com/api/ProductSlot/GetMachineSlots/{machineID}/";
                List<Product> prodList = new List<Product> ( );

                StorageFolder localAppData = ApplicationData.Current.LocalFolder;


                using ( var httpClient = new HttpClient ( ) )
                    {
                    var response = Task.Run ( () => httpClient.GetAsync ( baseURL ) ).Result;
                    if ( response.IsSuccessStatusCode )
                        {
                        var reponseData = response.Content.ReadAsStringAsync ( ).Result;
                        dynamic json = JsonConvert.DeserializeObject ( reponseData );


                        string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, "VendingC.db");

                        using (SqliteConnection con = new SqliteConnection($"Filename={pathToDB}"))
                        {
                            con.Open();
                            SqliteCommand cmd_getRec = new SqliteCommand("DELETE FROM TBLSLOT;VACUUM;", con);
                            cmd_getRec.ExecuteNonQuery();
                            con.Close();
                        }

                        foreach ( var data in json )
                            {

                            try
                                {
                                if ( Convert.ToString ( data [ "productImage" ] ) != "" )
                                    {

                                    string fileName = Path.GetFileName ( Convert.ToString ( data [ "productImage" ] ) );
                                    string filePath = Path.Combine ( localAppData.Path, "ProductImages", fileName );
                                    using ( WebClient client = new WebClient ( ) )
                                        {
                                        try
                                            {
                                            client.DownloadFile ( "https://webapis.vendingc.com/api" + Convert.ToString ( data [ "productImage" ] ), filePath );
                                            }
                                        catch ( Exception ex )
                                            {
                                            Global.log.Trace ( "Exception downloading pictures: " + ex.ToString ( ) );

                                            }
                                        }



                                    }
                                }

                            catch ( Exception ex )
                                {
                                Global.log.Trace ( "Exception downloading pictures: " + ex.ToString ( ) );

                                }
                            try
                                {

                                await addSlot (
                                   Convert.ToInt32 ( data [ "slotId" ] ),
                                   Convert.ToInt32 ( data [ "slotNo" ] ),
                                   Convert.ToInt32 ( data [ "productId" ] ),
                                   Convert.ToString ( data [ "productCode" ] ),
                                   Convert.ToString ( data [ "productName" ] ),
                                   Convert.ToString ( data [ "descriptipn" ] ),
                                   Path.Combine ( localAppData.Path, "ProductImages", Path.GetFileName ( Convert.ToString ( data [ "productImage" ] ) ) ),
                                   Convert.ToInt32 ( data [ "capacity" ] ),
                                   Convert.ToInt32 ( data [ "salePrice" ] ),
                                   ( Convert.ToInt32 ( data [ "totalRefillCount" ] ) - Convert.ToInt32 ( data [ "totalProducttransactionCount" ] ) ),// saleQuantity
                                   ( Convert.ToInt32 ( data [ "totalRefillCount" ] ) - Convert.ToInt32 ( data [ "totalProducttransactionCount" ] ) ),// saleQuantity
                                   Convert.ToBoolean ( data [ "isActive" ] ),
                                   false,
                                   DateTime.Now,
                                   DateTime.Now,
                                    DateTime.Now
                                    );
                                }
                            catch ( Exception ex )
                                {
                                Global.log.Trace ( "Exception Adding Slot in Local DB: " + ex.ToString ( ) );

                                }

                            }


                        }
                    }
                }

            return true;
            // return prodList;

            }
        public static List<Product> GetProductsFromLocalDB ()
            {
            List<Product> prodList = new List<Product> ( );
            string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );

            using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                {
                con.Open ( );

                String selectCmd = "SELECT PRODUCTID,PRODUCTNAME,DESCRIPTION,PRODUCTCODE,MAX(PRICE) AS PRICE,IMAGEURL,SUM(CURRENTCOUNT) AS CURRENTCOUNT,SYNCDATETIME FROM TBLSLOT WHERE ISACTIVE = 1 AND CURRENTCOUNT >0 GROUP BY PRODUCTCODE;";
                // String selectCmd = "SELECT PRODUCTID,PRODUCTNAME,DESCRIPTION,PRODUCTCODE,COSTPRICE,IMAGEURL,COMPANYID,SUM(SALEQUANTITY) AS SALEQUANTITY,SYNCDATETIME FROM TBLPRODUCT WHERE PRODUCTCODE IN (SELECT PRODUCTCODE FROM TBLSLOT WHERE ISACTIVE = 1) GROUP BY PRODUCTCODE;";
                // String selectCmd = "SELECT * FROM TBLPRODUCT WHERE PRODUCTCODE IN (SELECT PRODUCTCODE FROM TBLSLOT WHERE ISACTIVE = 1 AND CURRENTCOUNT >0);";
                SqliteCommand cmd_getRec = new SqliteCommand ( selectCmd, con );

                SqliteDataReader reader = cmd_getRec.ExecuteReader ( );

                while ( reader.Read ( ) )
                    {
                    prodList.Add ( new Product (
                        Convert.ToInt32 ( reader [ "PRODUCTID" ] ),
                        reader [ "PRODUCTNAME" ].ToString ( ),
                        reader [ "DESCRIPTION" ].ToString ( ),
                        reader [ "PRODUCTCODE" ].ToString ( ),
                        Convert.ToInt32 ( reader [ "PRICE" ] ),
                        reader [ "IMAGEURL" ].ToString ( ),
                        Convert.ToInt32 ( reader [ "CURRENTCOUNT" ] ),
                        reader [ "SYNCDATETIME" ].ToString ( )
                        ) );
                    }

                con.Close ( );
                }


            return prodList;


            }

        public static async Task<List<String>> GetSlotNo(string ProductCode)
        {
            try
            {
                List<String> items = new List<String>();

                String slotNo = await ExecuteString("SELECT SlotNo as x FROM tblSlot WHERE ProductCode = '" + ProductCode + "' AND ISACTIVE = true AND CurrentCount > 0 ORDER BY SlotNo ASC LIMIT 1");//.ToTwoCharacter();
                String slotId = await ExecuteString("SELECT slotId as x FROM tblSlot WHERE ProductCode = '" + ProductCode + "' AND ISACTIVE = true AND CurrentCount > 0 ORDER BY SlotNo ASC LIMIT 1");//.ToTwoCharacter();
                items.Add(slotNo);
                items.Add(slotId);
                if (slotNo == "")
                {
                    string queryLocal = "update tblSlot set IsDeleted = 1, IsActive = 0, DeletedDate = DATE('now') where ProductCode = '" + ProductCode + "';";
                    await ExecuteNonQuery(queryLocal);

                    try
                    {
                        slotNo = await ExecuteString("SELECT SlotId as SlotId FROM tblSlot WHERE ProductCode = '" + ProductCode + "' AND ISACTIVE = true ORDER BY SlotNo ASC LIMIT 1");//.ToTwoCharacter();
                        await DeactivateSlotOnServer(Convert.ToInt32(slotNo));
                    }
                    catch (Exception ex)
                    {
                        Global.log.Trace("Exception unable to deactivate slotId " + ex.ToString());

                    }

                    slotNo = "";
                }
                return items;
            }
            catch (Exception ex)
            {
                Global.log.Trace(ex.ToString());
                return null;
            }
        }

        public static async Task<String> ExecuteString ( string qry )
            {
            try
                {
                string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );
                SqliteDataReader reader;
                var x = "";
                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );

                    //String selectCmd = "SELECT * FROM tblProduct";
                    SqliteCommand cmd_getRec = new SqliteCommand ( qry, con );

                    reader = cmd_getRec.ExecuteReader ( );

                    while ( reader.Read ( ) )
                        {
                        x = reader ["x"].ToString ( );
                        }
                    con.Close ( );
                    }
                return x;
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( "Exception ExecuteString: " + ex.ToString ( ) );
                return "";
                }
            }

        public static async Task<bool> ExecuteNonQuery ( string qry )
            {
            try
                {
                string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );
                SqliteDataReader reader;
                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );
                    SqliteCommand cmd_getRec = new SqliteCommand ( qry, con );
                    cmd_getRec.ExecuteNonQuery ( );

                    }
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( "Exception ExecuteNonQuery: " + ex.ToString ( ) );
                }
            return true;
            }

        public static async Task<bool> UpdateTblSlotDataInLocalDB ( ShoppingCartEntry _currentDispensing )
            {
            string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );

            int Count = 0;

            try
                {
                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );

                    //"SELECT * FROM TBLPAYMENTTRANS WHERE SALENO='" + reader1["SALENO"].ToString() + "';";
                    String selectCmd = "SELECT COUNT(*) AS COUNT FROM TBLPRODUCTTRANS WHERE PRODUCTCODE ='" + _currentDispensing.Product.productCode + "' AND SLOTID ='" + _currentDispensing.slotId + "'AND DISPENSEDATETIME > (SELECT SYNCDATETIME FROM TBLSLOT WHERE SLOTID='" + _currentDispensing.slotId + "');";
                    SqliteCommand cmd_getRec = new SqliteCommand ( selectCmd, con );
                    SqliteDataReader reader = cmd_getRec.ExecuteReader ( );

                    if ( reader.HasRows )
                        {
                        reader.Read ( );
                        Count = Convert.ToInt32 ( reader [ "COUNT" ].ToString ( ) );

                        }
                    con.Close ( );

                    }
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( "Exception Updating Local Slot data: " + ex.ToString ( ) );
                }



            try
                {
                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );
                    SqliteCommand CMD_Insert = new SqliteCommand ( );

                    CMD_Insert.Connection = con;

                    CMD_Insert.CommandText = "UPDATE TBLSLOT SET CURRENTCOUNT = REFILLCOUNT - @COUNT, MODIFIEDDATE =  DateTime('now') WHERE ISDELETED IS '0' AND SLOTID = @SLOTID AND PRODUCTCODE = @PRODUCTCODE;";
                    //CMD_Insert.CommandText = "UPDATE TBLSLOT SET CURRENTCOUNT = CURRENTCOUNT - 1, MODIFIEDDATE = DATE('now') WHERE SLOTNO = @SLOTNO AND PRODUCTCODE = @PRODUCTCODE;";
                    CMD_Insert.Parameters.AddWithValue ( "@PRODUCTCODE", _currentDispensing.Product.productCode );
                    CMD_Insert.Parameters.AddWithValue ( "@SLOTID", _currentDispensing.slotId);
                    CMD_Insert.Parameters.AddWithValue("@COUNT", Count);
                    //CMD_Insert.ExecuteReader();

                    using ( SqliteDataReader reader = CMD_Insert.ExecuteReader ( ) )
                        {
                        while ( reader.Read ( ) )
                            {
                            Global.log.Trace ( String.Format ( "{0}", reader [ 0 ] ) );
                            }
                        }

                    con.Close ( );
                    }
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( "Exception Updating Local Slot data: " + ex.ToString ( ) );

                }
            return true;
            }

        public static async Task<bool> SubmitSaleTransaction ()
            {
            string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );

            try
                {
                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );
                    SqliteCommand CMD_Insert = new SqliteCommand ( );

                    CMD_Insert.Connection = con;

                    CMD_Insert.CommandText = "INSERT INTO TBLSALETRANS(MACHINEID, COMPANYID, MACHINECODE, SALEDATETIME, SALENO, PURCHASEAMOUNT, PAIDAMOUNT, BALANCEAMOUNT, GENDER, AGE, ISDATASENT) VALUES(@MACHINEID, @COMPANYID, @MACHINECODE, @SALEDATETIME, @SALENO, @PURCHASEAMOUNT, @PAIDAMOUNT, @BALANCEAMOUNT, @GENDER, @AGE, @ISDATASENT);";

                    CMD_Insert.Parameters.AddWithValue ( "@MACHINEID", Global.MachineID );
                    CMD_Insert.Parameters.AddWithValue ( "@COMPANYID", Global.CompanyID );
                    CMD_Insert.Parameters.AddWithValue ( "@MACHINECODE", Global.MachineCode );
                    CMD_Insert.Parameters.AddWithValue ( "@SALEDATETIME", DateTime.Now );
                    CMD_Insert.Parameters.AddWithValue ( "@SALENO", Global.General.SALE_NO );
                    CMD_Insert.Parameters.AddWithValue ( "@PURCHASEAMOUNT", Global.General.TOTAL_PURCHASED_AMOUNT.ToString ( ) );
                    CMD_Insert.Parameters.AddWithValue ( "@PAIDAMOUNT", Global.General.TOTAL_PAID_AMOUNT.ToString ( ) );
                    CMD_Insert.Parameters.AddWithValue ( "@BALANCEAMOUNT", Global.General.CreditAmount.ToString ( ) );
                    CMD_Insert.Parameters.AddWithValue ( "@GENDER", 0 );
                    CMD_Insert.Parameters.AddWithValue ( "@AGE", 0 );
                    CMD_Insert.Parameters.AddWithValue ( "@ISDATASENT", 0 );
                    CMD_Insert.ExecuteReader ( );

                    con.Close ( );

                    }
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( "Exception Logging Sales Transaction: " + ex.ToString ( ) );
                }
            return true;
            }
        public static async Task<bool> SubmitProductTransaction ( ShoppingCartEntry _currentDispensing )
            {
            string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );

            try
                {
                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );
                    SqliteCommand CMD_Insert = new SqliteCommand ( );

                    CMD_Insert.Connection = con;

                    CMD_Insert.CommandText = "INSERT INTO TBLPRODUCTTRANS(SALENO, SLOTID, PRODUCTCODE, PRODUCTAMOUNT, DISPENSESTATUS, FAILREASON, DISPENSEDATETIME) VALUES(@SALENO, @SLOTID, @PRODUCTCODE, @PRODUCTAMOUNT, @DISPENSESTATUS, @FAILREASON, @DISPENSEDATETIME);";

                    CMD_Insert.Parameters.AddWithValue ( "@SALENO", Global.General.SALE_NO );
                    CMD_Insert.Parameters.AddWithValue ( "@SLOTID", _currentDispensing.slotId);
                    CMD_Insert.Parameters.AddWithValue ( "@PRODUCTCODE", _currentDispensing.Product.productCode );
                    CMD_Insert.Parameters.AddWithValue ( "@PRODUCTAMOUNT", _currentDispensing.Product.cost.ToString ( ) );
                    CMD_Insert.Parameters.AddWithValue ( "@DISPENSESTATUS", _currentDispensing.dispensed == true ? Global.General.PRODUCT_TRANSACTION_SUCCESS : Global.General.PRODUCT_TRANSACTION_ERROR );
                    CMD_Insert.Parameters.AddWithValue ( "@FAILREASON", _currentDispensing.failReason == null ? "" : _currentDispensing.failReason );
                    CMD_Insert.Parameters.AddWithValue ( "@DISPENSEDATETIME", DateTime.Now );

                    CMD_Insert.ExecuteReader ( );

                    con.Close ( );

                    }
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( "Exception Logging Product Transaction: " + ex.ToString ( ) );
                }
            return true;
            }
        public static async Task<bool> SubmitPaymentTransaction ()
            {
            string pathToDB = Path.Combine ( ApplicationData.Current.LocalFolder.Path, "VendingC.db" );

            try
                {
                using ( SqliteConnection con = new SqliteConnection ( $"Filename={pathToDB}" ) )
                    {
                    con.Open ( );
                    SqliteCommand CMD_Insert = new SqliteCommand ( );

                    CMD_Insert.Connection = con;

                    CMD_Insert.CommandText = "INSERT INTO TBLPAYMENTTRANS(SALENO, PAYMENTMETHOD, TRANSAMOUNT, PAYMENTDIRECTION, PAYMENTINFO, READDATETIME) VALUES(@SALENO, @PAYMENTMETHOD, @TRANSAMOUNT, @PAYMENTDIRECTION, @PAYMENTINFO, @READDATETIME);";

                    CMD_Insert.Parameters.AddWithValue ( "@SALENO", Global.General.SALE_NO );
                    CMD_Insert.Parameters.AddWithValue ( "@PAYMENTMETHOD", Global.General.PaymentOption );
                    CMD_Insert.Parameters.AddWithValue ( "@TRANSAMOUNT", Global.General.TotalNetAmount.ToString ( ) );
                    CMD_Insert.Parameters.AddWithValue ( "@PAYMENTDIRECTION", "IN" );
                    CMD_Insert.Parameters.AddWithValue ( "@PAYMENTINFO", Global.ResponsePOS );
                    CMD_Insert.Parameters.AddWithValue ( "@READDATETIME", DateTime.Now );

                    CMD_Insert.ExecuteReader ( );

                    con.Close ( );

                    }
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( "Exception Logging Payment Transaction: " + ex.ToString ( ) );
                }
            return true;
            }

        }

    }