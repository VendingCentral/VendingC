using MetroLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;

namespace VendingC
{
    public class Global
    {
        public static bool Failed = false;
        public static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<FrontPage>();
        public static bool Reset_Clear_Cart_Timer = false;

        public static bool UNDER_TEST_MODE = false;
        //public static string ComPort = ConfigurationManager.AppSettings["NOTE_PORT"].ToString();
        public static string MotorComPort = "";
        public static string CashMachineComPort = "";
        public static string POSMachineComPort = "";
        public static byte SSPAddress = 0;

        //public static int MachineID = Convert.ToInt32(ConfigurationManager.AppSettings["MACHINE_ID"]);
        //public static int CompanyID = Convert.ToInt32(ConfigurationManager.AppSettings["COMPANY_ID"]);
        public static int MachineID;
        public static string MachineCode;
        public static int CompanyID;
        public static int ProductLimit;
        public static string Live_ConnectionString = "";// ConfigurationManager.AppSettings["ServerCon"].ToString();
        public static string Local_ConnectionString = "";// ConfigurationManager.AppSettings["LocalCon"].ToString();

        public static ShoppingCart newCartList = new ShoppingCart();

    
        public enum ActionList
        {
            None,
            GivingChange,
            RefundPayCard,
            PaymentRequestPayCard,
            ExcuseAndGoHome,
            GoHome,
            CollectMoney,
            CollectingMoney,
            StartDispensing,
            CheckDispenseStatus,
            AnotherTransaction,
            ReturnRefundChange,
            Dispensing,
            Processing,
            CancelPayCardRecharge
        }

  
        public class VMController
        {
            //VM Controller Global
           // public static string ControllerBoardComPort = ConfigurationManager.AppSettings["MOTORS_PORT"].ToString();
            public static int ControllerBoardBaudRate = 9600;
            public static string MOTOR_ID = "";
            public static bool MOTOR_COMMAND_STATUS = false;
            public static bool CONTROLLER_WORKING_STATUS = false;       //This variable can bs used as Heart Signal for Vending Machine Motor/Sensor Controller Board

            public static bool IS_CONTROLLER_READY = false;

            public static Queue<ShoppingCart> PENDING_RUNS = new Queue<ShoppingCart>();

            public static bool PROCESSING = false;

            public static bool HEART_BEAT = false;
        }

        public class NoteValidator
        {
            public static bool NV_OUT_OF_ORDER = false;
            //public static string NV11ComPort = ConfigurationManager.AppSettings["NOTE_PORT"].ToString();
            public static int NV11BaudRate = 9600;
            public static byte SSPAddress = 0;

            public static char user_input_char = ' ';
            public static string CurrentNoteValue = "";
            public static string CreditedNoteValue = "0.00";
            public static Int32 TotalAmount = 0;
            public static string CurrentNoteCurrency = "";
            public static bool StorageStatus = false;   //Initialized as empty
            public static int PayoutNoteCount = 0;


            public static bool SSP_VALIDATOR_ENABLED_STATUS = false;
            public static bool SSP_VALIDATOR_DIASBLED_STATUS = false;
            public static bool SSP_PAYOUT_ENABLED_STATUS = false;
            public static bool SSP_PAYOUT_DISABLED_STATUS = false;

            public static bool SSP_EMPTY_PAYOUT_DEVICE_COMM_STATUS = false;
            public static bool SSP_PAYOUT_NEXT_NOTE_COMM_STATUS = false;
            public static bool SSP_RETURN_NOTE_COMM_STATUS = false;

            // CComands used in CheckGenericResponseCon()
            public static bool SSP_RESPONSE_OK_STATUS = false;        //already used not required to call again
            public static bool SSP_RESPONSE_COMMAND_NOT_KNOWN_STATUS = false;                           //Command response is UNKNOWN
            public static bool SSP_RESPONSE_WRONG_NO_PARAMETERS_STATUS = false;                         //Command response is WRONG PARAMETERS
            public static bool SSP_RESPONSE_PARAMETER_OUT_OF_RANGE_STATUS = false;                      //Command response is PARAM OUT OF RANGE
            public static bool SSP_RESPONSE_COMMAND_CANNOT_BE_PROCESSED_0x03_STATUS = false;            //Validator has responded with \"Busy\", command cannot be processed at this time\r\n  OR 
            public static bool SSP_RESPONSE_COMMAND_CANNOT_BE_PROCESSED_STATUS = false;                  //Global.log.Trace("Command response is CANNOT PROCESS COMMAND, error code - 0x" + BitConverter.ToString(m_cmd.ResponseData, 1, 1) + "\r\n");
            public static bool SSP_RESPONSE_SOFTWARE_ERROR_STATUS = false;                               //Command response is SOFTWARE ERROR
            public static bool SSP_RESPONSE_FAIL_STATUS = false;                                         //Command response is FAIL\r\n
            public static bool SSP_RESPONSE_KEY_NOT_SET_STATUS = false;                                  //Command response is KEY NOT SET, renegotiate keys

            //CCommands used in DoPollCon()
            public static bool SSP_POLL_TEBS_CASHBOX_OUT_OF_SERVICE_STATUS = false;
            public static bool SSP_POLL_TEBS_CASHBOX_TAMPER_STATUS = false;
            public static bool SSP_POLL_TEBS_CASHBOX_IN_SERVICE_STATUS = false;
            public static bool SSP_POLL_TEBS_CASHBOX_UNLOCK_ENABLED_STATUS = false;
            public static bool SSP_POLL_JAM_RECOVERY_STATUS = false;
            public static bool SSP_POLL_ERROR_DURING_PAYOUT_STATUS = false;
            public static bool SSP_POLL_SMART_EMPTYING_STATUS = false;
            public static bool SSP_POLL_SMART_EMPTIED_STATUS = false;
            public static bool SSP_POLL_CHANNEL_DISABLE_STATUS = false;
            public static bool SSP_POLL_INITIALISING_STATUS = false;
            public static bool SSP_POLL_COIN_MECH_ERROR_STATUS = false;
            public static bool SSP_POLL_EMPTYING_STATUS = false;
            public static bool SSP_POLL_EMPTIED_STATUS = false;                                 // This single poll response indicates that the payout device has finished emptying.                                                                        
            public static bool SSP_POLL_COIN_MECH_JAMMED_STATUS = false;
            public static bool SSP_POLL_COIN_MECH_RETURN_PRESSED_STATUS = false;
            public static bool SSP_POLL_PAYOUT_OUT_OF_SERVICE_STATUS = false;
            public static bool SSP_POLL_NOTE_FLOAT_REMOVED_STATUS = false;
            public static bool SSP_POLL_NOTE_FLOAT_ATTACHED_STATUS = false;
            public static bool SSP_POLL_NOTE_TRANSFERED_TO_STACKER_STATUS = false;             // A note has been transferred from the payout storage to the cashbox         
            public static bool SSP_POLL_NOTE_PAID_INTO_STACKER_AT_POWER_UP_STATUS = false;
            public static bool SSP_POLL_NOTE_PAID_INTO_STORE_AT_POWER_UP_STATUS = false;
            public static bool SSP_POLL_NOTE_STACKING_STATUS = false;                      // A note is in transit to the cashbox.
            public static bool SSP_POLL_NOTE_DISPENSED_AT_POWER_UP_STATUS = false;
            public static bool SSP_POLL_NOTE_HELD_IN_BEZEL_STATUS = false;                 // This response indicates a note is being dispensed and is resting in the bezel waiting to be removed before the validator can continue
            public static bool SSP_POLL_BAR_CODE_TICKET_ACKNOWLEDGE_STATUS = false;
            public static bool SSP_POLL_DISPENSED_STATUS = true;                          // The note has been dispensed and removed from the bezel by the user.
            public static bool SSP_POLL_JAMMED_STATUS = false;
            public static bool SSP_POLL_HALTED_STATUS = false;
            public static bool SSP_POLL_FLOATING_STATUS = false;
            public static bool SSP_POLL_FLOATED_STATUS = false;
            public static bool SSP_POLL_TIME_OUT_STATUS = false;
            public static bool SSP_POLL_DISPENSING_STATUS = false;                         // The validator is in the process of paying out a note, this will continue to poll until the note has been fully dispensed and removed from the front of the validator by the user.                                                                                                
            public static bool SSP_POLL_NOTE_STORED_IN_PAYOUT_STATUS = false;              // A note has been stored in the payout device to be paid out instead of going into the cashbox.
            public static bool SSP_POLL_INCOMPLETE_PAYOUT_STATUS = false;
            public static bool SSP_POLL_INCOMPLETE_FLOAT_STATUS = false;
            public static bool SSP_POLL_CASHBOX_PAID_STATUS = false;
            public static bool SSP_POLL_COIN_CREDIT_STATUS = false;
            public static bool SSP_POLL_NOTE_PATH_OPEN_STATUS = false;
            public static bool SSP_POLL_NOTE_CLEARED_FROM_FRONT_STATUS = false;           // A note was detected somewhere inside the validator on startup and was rejected from the front of the unit.                                                      
            public static bool SSP_POLL_NOTE_CLEARED_TO_CASHBOX_STATUS = false;           // A note was detected somewhere inside the validator on startup and was cleared into the cashbox.
            public static bool SSP_POLL_CASHBOX_REMOVED_STATUS = false;                   // The cashbox has been removed from the unit. This will continue to poll until the cashbox is replaced.
            public static bool SSP_POLL_CASHBOX_REPLACED_STATUS = false;                   // The cashbox has been replaced, this will only display on a poll once.
            public static bool SSP_POLL_BAR_CODE_TICKET_VALIDATED_STATUS = false;
            public static bool SSP_POLL_FRAUD_ATTEMPT_STATUS = false;                     // A fraud attempt has been detected. The second byte indicates the channel of the note that a fraud has been attempted on.
            public static bool SSP_POLL_STACKER_FULL_STATUS = false;                      //The stacker (cashbox) is full.                                                                                                                  
            public static bool SSP_POLL_DISABLED_STATUS = false;                           // The validator is disabled, it will not execute any commands or do any actions until enabled.
            public static bool SSP_POLL_UNSAFE_NOTE_JAM_STATUS = false;                    // An unsafe jam has been detected. This is where a user has inserted a note and the note is jammed somewhere that the user can potentially recover the note from.                                                               
            public static bool SSP_POLL_SAFE_NOTE_JAM_STATUS = false;                      // A safe jam has been detected. This is where the user has inserted a note and the note is jammed somewhere that the user cannot reach.
            public static bool SSP_POLL_NOTE_STACKED_STATUS = false;                       // A note has reached the cashbox.
            public static bool SSP_POLL_NOTE_REJECTED_STATUS = false;                      // A note as been rejected from the validator
            public static bool SSP_POLL_NOTE_REJECTING_STATUS = false;                     // A note is being rejected from the validator. This will carry on polling while the note is in transit.
            public static bool SSP_POLL_CREDIT_NOTE_STATUS = false;                        // A credit event has been detected, this is when the validator has accepted a note as legal currency.
            public static bool SSP_POLL_READ_NOTE_STATUS = false;                         //Global.log.Trace("Note in escrow, amount: " + CHelpers.FormatToCurrency(noteVal) + " " + GetChannelCurrency(m_CurrentPollResponse[i + 1]) + "\r\n");
            public static bool SSP_POLL_SLAVE_RESET_STATUS = false;                            //Unit reset

            public static bool SSP_POLL_NV_WORKING_STATUS = false;                            //This variable can bs used as Heart Signal for Note Validator

            // QUERY REJECTION VARIABLES
            public static bool SSP_QUERY_REJECTION_NOTE_ACCEPTED = false;
            public static bool SSP_QUERY_REJECTION_NOTE_LENGHT_INCORRECT = false;
            public static bool SSP_QUERY_REJECTION_INVALID_NOTE = false;
            public static bool SSP_QUERY_REJECTION_INVALID_NOTE_0x2 = false;
            public static bool SSP_QUERY_REJECTION_INVALID_NOTE_0x3 = false;
            public static bool SSP_QUERY_REJECTION_INVALID_NOTE_0x4 = false;
            public static bool SSP_QUERY_REJECTION_INVALID_NOTE_0x5 = false;
            public static bool SSP_QUERY_REJECTION_SECOND_NOTE_INSERTED_DURING_READ = false;
            public static bool SSP_QUERY_REJECTION_HOST_REJECTED_NOTE = false;
            public static bool SSP_QUERY_REJECTION_INVALID_NOTE_0x9 = false;
            public static bool SSP_QUERY_REJECTION_INVALID_NOTE_READ_0x0A = false;
            public static bool SSP_QUERY_REJECTION_NOTE_TOO_LONG = false;
            public static bool SSP_QUERY_REJECTION_VALIDATOR_DSIABLED = false;
            public static bool SSP_QUERY_REJECTION_MECHNISM_SLOW_STALLED = false;
            public static bool SSP_QUERY_REJECTION_STRIM_ATTEMPT = false;
            public static bool SSP_QUERY_REJECTION_FRAUD_CHANNEL_REJECT = false;
            public static bool SSP_QUERY_REJECTION_NO_NOTES_INSERTED = false;
            public static bool SSP_QUERY_REJECTION_PEAK_DETECT_FAIL = false;
            public static bool SSP_QUERY_REJECTION_TWISTED_NOTE_DETECTED = false;
            public static bool SSP_QUERY_REJECTION_ESCROW_TIME_OUT = false;
            public static bool SSP_QUERY_REJECTION_REAR_SENSOR_FAIL = false;
            public static bool SSP_QUERY_REJECTION_SLOT_FAIL_1 = false;
            public static bool SSP_QUERY_REJECTION_SLOT_FAIL_2 = false;
            public static bool SSP_QUERY_REJECTION_LENS_OVER_SAMPLE = false;
            public static bool SSP_QUERY_REJECTION_INCORRECT_NOTE_WIDTH = false;
            public static bool SSP_QUERY_REJECTION_NOTE_TOO_SHORT = false;

            public static bool PROCESSING = true;

            public static bool LOW_NOTE_CHANGE = false;

            public static bool Running = false;

            public static bool NOTE_ROUTING_DONE = false;
        }

        public static bool MACHINE_HEAR_BEAT = false;

        public static ActionList NextAction = ActionList.None;
        public static string ResponseMessagePOS= "";
        internal static string ResponsePOS="";

        public static string UserMessage { get; set; } = "";

        public class VMPosMachine
        {
            internal static bool Timeout;
        }
        public class General
        {
            public enum CashType
            {
                Cash,
                PayCard,
                EasyPaisa,
                JazzCash
            }


            public static string DISPENSED_All = "DA"; // Complete Dispensed
            public static string DISPENSED_PARTIALY = "DP"; // Complete Dispensed
            public static string DISPENSED_FAIL = "DF"; // Complete Dispensed

            public static string DISPENSE_STATUS = ""; //This is for to check what has happened to dispense item... is it DA, DP or DF

            public const string CASH_IN = "IN";
            public const string CASH_OUT = "OUT";


            public const string PRODUCT_TRANSACTION_ERROR = "Fail";
            public const string PRODUCT_TRANSACTION_SUCCESS = "Success";

            public static string SALE_NO = "";//GetNewSaleNo();

            public static string SALE_NO_NOTE_DISPENSED = "";

            public static bool ENOUGH_AMOUNT_ENTERED = false;

            public static int OLD_BALANCE = 0;

            public static bool DOWNLOADER_IS_READY = true;

            public static bool GOTO_HOME = false;

            public static bool PROCESSING = false;
            public static bool INPROGRESS = false;


            public static int CASH_RETURNED_AMOUNT = 0;
            public static int FAILED_TO_RETURN_AMOUNT = 0;
            public static int STACKED_NOTE_VALUE = 50;

            public static bool IS_COINS_RETURNED = true;
            public static bool IS_NOTE_RETURNED = true;

            public static bool I_AM_HOME = true;

            public static bool I_WANT_HOME = false;

            public static int SCREEN_TIMEOUT = 3;

            public static string PS_CLOSE_REASON = "";

            public static bool ASKING_FOR_MORE_TIME = false;

            public static string DISPENSE = "DONE";

            public static int PENDING_PAYOUT_NOTE = 0;

            public static int CashInAmount = 0;

            public static string CashIn = "0|Cash";

            public static int CreditAmount = 0;
            public static int CashCreditAmount = 0;
            public static int TotalCostAmount = 0;
            public static int TotalNetAmount = 0;
            public static string PaymentOption = "";
            public static int TOTAL_PAID_AMOUNT = 0;
            public static int TOTAL_PURCHASED_AMOUNT = 0;
            public static int TOTAL_BALANCE_AMOUNT = 0;



            public static int CashInserted = 0;

            public static int CreditAlreadyInserted = 0;
            public static bool QRread = false;
            public static double Total_Due_Amount = 0;

            public static double ChargesIfAny = 0;
            public static bool WantAnotherTransaction = true;

            public static bool EndTransaction = false;

            public static int TransactionReturnAmount = 0;


            public static bool showSorryPopupOnce = true;

            public static int CartIndex = 0;

            public static bool SyncComplete = false;
            public static bool CashBackClicked = false;
            public static bool BuyMoreItemsClicked = false;

            public static string MachinePassword = "";

            public static string SaleNo_Head = "";
            public static string ServerUrl = "";
            public static bool NoteValidator = false;
            public static bool QR_PaymentOption = false;
            public static bool CardPaymentOption = false;

            public static bool ProductTransactionIsLive = false;
            public static bool IsMotorConfiguration = false;
        }

        public class PaymentType
        {
            public static bool CashPay = false;
            public static bool VisaQRPay = false;
            public static bool MasterQRPay = false;
            public static bool VendiAppPay = false;
            public static bool PosPay = false;

        }

        public class _ResponseData
        {
            public string QRCode { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseDesc { get; set; }
            public object SessionID { get; set; }
            public object URL { get; set; }
        }

    }
}
