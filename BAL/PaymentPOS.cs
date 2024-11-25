using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace VendingC.Utilities
{

    internal class PaymentPOS
    {
        public int CardPaidAmount = 0;
        public bool CardPayConfirmed = false;

        SerialPort _serialPort = new SerialPort();
        public string ItemResult = "";
        private bool IsPaymentCancelled;
        public string message = "";
        public string message1 = "";
        public string message2 = "";
        private int tryCount;
       

        public async Task<bool> TransactionSales(string amount)
        {

            CardPaidAmount = Convert.ToInt32(amount);
            _serialPort = new SerialPort();
            _serialPort.PortName = Global.POSMachineComPort;
            //_serialPort.BaudRate = Global.BAUD_RATE;
            _serialPort.BaudRate = 115200;
            //_serialPort.ReadTimeout = 65000;
            _serialPort.ReadTimeout = 3000;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;

            tryCount = 200;



            //convert amount into the 12 digit amount 
            amount = "0200" + new string('0', 10 - amount.Length) + amount + "00";
            Global.log.Trace(amount);
            if (!_serialPort.IsOpen)
            {
                try
                {

                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    Global.log.Trace ( ex.ToString ( ) );
                    // MessageBox.Show(ex.Message);
                    }
            }
            else
            {
                _serialPort.Write("");
                _serialPort.Close();
                _serialPort.Open();
            }
            //Clear POS Serial Port
            // _serialPort.Write("");
            //Sending amount to POS machine signal
            _serialPort.Write(amount);
           // _serialPort.Write("0500");
            Global.log.Trace("writing ");
            // Thread.Sleep(3000);
            while (true)
            {
                try
                {
                    while (!(message.ToLower().Contains("response") || message.ToLower().Contains("cancelled") || tryCount == 0) )
                    {
                        message += _serialPort.ReadExisting();
                        Global.log.Trace("read: " + message);
                       // tryCount--;
                    }
                    if (message.ToLower().Contains("date"))
                    {
                        Global.VMPosMachine.Timeout = false;
                        if (message.Contains("Response Code = 00") && message.Contains("Total Amount = " + CardPaidAmount + ".00") || message.Contains("Response Code=00") && message.Contains("Total Amount=" + CardPaidAmount + ".00"))
                        {
                            message2 = "Date=";
                            message1 = message.Substring(message.IndexOf(message2) + message2.Length, message.IndexOf('\n'));
                            message2 = "Time=";
                            message1 = message.Substring(message.IndexOf(message2) + message2.Length, message.IndexOf('\n'));
                            message2 = "TID=";
                            message1 = message.Substring(message.IndexOf(message2) + message2.Length, message.IndexOf('\n'));
                            message2 = "MID=";
                            message1 = message.Substring(message.IndexOf(message2) + message2.Length, message.IndexOf('\n'));
                            message2 = "Total Amount=";
                            message1 = message.Substring(message.IndexOf(message2) + message2.Length, message.IndexOf('\n'));
                            Global.ResponsePOS = message;
                            //message2 ="Response=";
                            //message1 = message.Substring(message.IndexOf(message2) + message2.Length, message.IndexOf('\n')-1);
                            //Global.log.Trace("Response:" + message1);
                            //TODO ReadWrite.PaidAmount = CardPaidAmount;
                          //TODO  ReadWrite.Write(ReadWrite.PaidAmount.ToString(), Global.Actions.AddToAmount.ToString());
                            CardPayConfirmed = true;
                            break;
                        }
                        if (message.Contains("Response Code=51"))
                        {
                            Global.ResponseMessagePOS = "Insufficient funds. Please try another card!";
                            _serialPort.Close();
                            break;
                        }
                        if (message.Contains("Response Code=55"))
                        {
                            Global.ResponseMessagePOS = "Incorrect PIN. Please try again!";
                            _serialPort.Close();
                            break;
                        }
                        if (message.Contains("Response Code=35"))
                        {
                            Global.ResponseMessagePOS = "Transaction limit for today has exceeded. Try another card!";
                            _serialPort.Close();
                            break;
                        }
                    }
                    if (message.ToLower().Contains("cancelled"))
                    {
                        Global.VMPosMachine.Timeout = true;
                        ItemResult = "Timeout";
                        IsPaymentCancelled = true;
                        break;
                    }

                    break;
                }
                catch (Exception ex)
                {
                    Global.log.Trace ( ex.ToString ( ) );
                    Global.VMPosMachine.Timeout = false;
                    ItemResult = "Timeout";
                    break;
                }
            }
       //     _serialPort.Write("");
            _serialPort.Close();

            if(CardPayConfirmed)
            {
                return true;
            }
            else
            {
                return false;
            }
            //return message;
            //  Thread.Sleep(2000);
        }

    }
}
