using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using VendingC.Data;
using System.Management;
using System;
using VendingC.Utilities;
using System.Diagnostics;
using System.Collections.Generic;

namespace VendingC
{
    public class MotorController
    {
        static ShoppingCartEntry _currentDispensing;
        static ShoppingCart _shoppingCart;
        static bool reading = false;
        static int cartIndex = 0;
        SerialPort _serialPort = new SerialPort();
        public string motorNo = "";

        public MotorController()
        {
            _serialPort.Close();
            _serialPort.BaudRate = 9600;
            _serialPort.ReadTimeout = 6000;
            // AutoComPort autoComPort = new AutoComPort();
            // autoComPort.GetMotorComportAsync();
        }

        public async Task<bool> StartTransaction(ShoppingCart ShoppingCart)
        {
            // AutoComPort autoComPort = new AutoComPort();
            //  autoComPort.GetMotorComportAsync();

            _shoppingCart = ShoppingCart;
            try
            {
                Global.log.Trace("Cart Count: " + _shoppingCart.Entries.Count);
                for (int i = 0; i <= _shoppingCart.Entries.Count - 1; i++)
                {
                    //lblWait.Text = "Dispensing : " + shoppingCart.Entries[i].Product.productName;
                    //lblWait.Refresh();
                    Global.log.Trace("Dispensing : " + _shoppingCart.Entries.Count);
                    _currentDispensing = _shoppingCart.Entries[i];
                    //var p = _shoppingCart.Entries[i];
                    // p.motorRan = await StartMotor(_currentDispensing, i);
                    _currentDispensing.motorRan = await StartMotor(_currentDispensing, i);
                    _shoppingCart.Entries[i] = _currentDispensing;
                    //  _shoppingCart.Entries[i] = p;

                }
                await DispenseHandler.GetTrasactionStatus(_shoppingCart);
                if (Global.General.DISPENSE_STATUS == "DA")
                {
                    //await DispenseHandler.ClearpayementRegister();
                    // puNewThankYou pn = new puNewThankYou(5);
                    //pn.TopMost = true;
                    //pn.ShowDialog();
                }
                else if (Global.General.DISPENSE_STATUS == "DP" || Global.General.DISPENSE_STATUS == "DF")
                {
                    //Global.General.CashInAmount = 0;
                    //puNewSorryScreen ps = new puNewSorryScreen(Global.UserMessage, 5);
                    //ps.TopMost = true;
                    //ps.ShowDialog();

                    if (Global.General.DISPENSE_STATUS == "DP")
                    {

                        //ReadWrite.Write(Global.General.CreditAmount.ToString(), Global.Actions.AddToAmount.ToString());
                    }
                    else
                    {
                        //ReadWrite.Write(Global.General.CreditAmount.ToString(), Global.Actions.AddToAmount.ToString());

                    }
                }
                if (Global.General.CreditAmount > 0)
                {
                    //CashHandler.SubmitCashTransaction("IN", "Another", 0, Global.General.CreditAmount);
                }
                //disable Note Validator
                //ReadWrite.Write("Stop", Global.Actions.Enabled.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _currentDispensing.failReason = "Port Issue";
                await Data.VendingC.SubmitProductTransaction(_currentDispensing);
                await DispenseHandler.GetTrasactionStatus(_shoppingCart);
                Global.log.Trace(ex.Message);
                return false;
            }
        }
        public async Task<bool> StartMotor(ShoppingCartEntry cartItem, int CartIndex)
        {
            cartIndex = CartIndex;
            bool ReadFromMotor = false;
            List<String> items = new List<String>();


            items = await Data.VendingC.GetSlotNo(_currentDispensing.Product.productCode.ToString());
            motorNo = items[0];
            _currentDispensing.slotId = Convert.ToInt32(items[1]);
            motorNo=motorNo.ToString().PadLeft(2, '0');
            _currentDispensing.slotNo = Convert.ToInt32(motorNo);
            if (motorNo == "" || motorNo == "00" || motorNo == "invalid" || motorNo == "0")
            {
                if (motorNo == "00")
                {
                    motorNo = await Data.VendingC.ExecuteString("SELECT SlotNo as slotNo FROM tblSlot WHERE ProductCode = " + _currentDispensing.Product.productCode.ToString() + " ORDER BY SlotNo DESC LIMIT 1"); //.ToTwoCharacter();
                    _currentDispensing.slotNo = Convert.ToInt32(motorNo);
                }
            }
            else
            {
                //_currentDispensing = cartItem;
                ReadFromMotor = await ReadFromMotorAsync();
            }
            return ReadFromMotor;
        }
        public async Task<bool> ReadFromMotorAsync ()
        {
            _serialPort = new SerialPort();
            // set prt no automaticalls
            //_serialPort.PortName = Global.ComPort; //deive manager
                                                   //_serialPort.BaudRate = Global.VMController.ControllerBoardBaudRate;
          //  if (Global.ComPort == "")
           // {
           //     _serialPort.PortName = "COM3";
          //  }
            //else
           // {
                _serialPort.PortName = Global.MotorComPort;
            //}
            _serialPort.BaudRate = 9600;
            _serialPort.ReadTimeout = 8000;

            bool motorRan = false;
            bool ItemResult = false;

            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    Global.log.Trace(ex.Message);
                }
            }
            //Clear Motor Serial Port
            _serialPort.Write("");
            //Sending motor number signal
            _serialPort.Write(motorNo);
            while (true)
            {
                try
                {
                    motorRan = true;
                    string message = "";
                    message = _serialPort.ReadLine();
                    Global.log.Trace("Motor read: " + message);
                    if (message.ToLower().Contains("product dispatched"))
                    {
                        //  if (Global.newCartList.Entries.Count > 0)
                        // {
                        //  var cart = Global.newCartList.Entries[0];

                        //   var p = Global.newCartList.Entries[cartIndex         ];
                        //   p.dispensed = true;
                        //  Global.newCartList.Entries[cartIndex] = p;

                        // Global.newCartList.Entries[cartIndex].dispensed = true;
                        _currentDispensing.dispensed = true;
                        // _currentDispensing.motorRan = true;
                        _currentDispensing.dispenseTime = DateTime.Now;
                        await DispenseHandler.SetDispenseSuccess(_currentDispensing);

                        
                        // UserDataLib.SubmitSaleTransactionAsync(this.ShoppingCart);
                        break;
                        // }
                    }
                    else if (message.ToLower().Contains("product failed"))
                    {
                        //  if (_currentDispensing != null)
                        // {
                        Global.log.Trace("Setting Dispensing Fail: " + _currentDispensing.Product.productCode);
                        // }

                        _currentDispensing.dispensed = false;
                        // _currentDispensing.motorRan = true;
                       // _currentDispensing.dispenseTime = DateTime.Now;
                        await DispenseHandler.SetDispenseFail(_currentDispensing);
                        _currentDispensing.failReason = "Slot Issue";
                        //LOGGING TRANSATION DATA IN DATABASE
                       await Data.VendingC.SubmitProductTransaction(_currentDispensing);

                        Global.VMController.PROCESSING = false;
                        ItemResult = true;
                        break;
                    }
                    else if (message.ToLower().Contains("running"))
                    {
                        // var p = Global.newCartList.Entries[cartIndex];
                        //p.processed = true;
                        //Global.newCartList.Entries[cartIndex] = p;

                        //Global.newCartList.Entries[cartIndex].processed = true;
                        _currentDispensing.processed = true;

                        ItemResult = true;
                    }
                    else if (message.ToLower().Contains("working"))
                    {
                        Global.VMController.HEART_BEAT = true;
                    }
                    else if (message.ToLower().Contains("stopped"))
                    {
                        _serialPort.ReadTimeout = 8000;
                    }
                    else if (string.IsNullOrWhiteSpace(message))
                    {
                        Global.VMController.HEART_BEAT = false;
                    }
                }
                catch (TimeoutException ex)
                {
                    // if (_currentDispensing != null)
                    // {
                    Global.log.Trace("Setting Dispensing Fail: " + _currentDispensing.Product.productCode);
                    // }
                    await DispenseHandler.SetDispenseFail(_currentDispensing);

                    _currentDispensing.dispensed = false;
                    //_currentDispensing.motorRan = true;
                    _currentDispensing.dispenseTime = DateTime.Now;
                    _currentDispensing.failReason = "Slot Issue";
                    //LOGGING TRANSATION DATA IN DATABASE
                   await Data.VendingC.SubmitProductTransaction(_currentDispensing);
                    //await DispenseHandler.SetDispenseSuccess(_currentDispensing);

                    Global.VMController.PROCESSING = false;
                    ItemResult = true;
                    break;
                }
                catch (Exception ex)
                {
                    //if (_currentDispensing != null)
                    // {
                    Global.log.Trace("Setting Dispensing Fail: " + _currentDispensing.Product.productCode);
                    // }
                    await DispenseHandler.SetDispenseFail(_currentDispensing);
                    //LOGGING TRANSATION DATA IN DATABASE
                    await Data.VendingC.SubmitProductTransaction(_currentDispensing);
                    Global.VMController.PROCESSING = false;
                    ItemResult = true;
                    break;
                }
            }
            _serialPort.Close();
            //Thread.Sleep(2000);
            return ItemResult;
        }

        public async Task<int> GetHeartBeatFromPCB()
        {
            int hb = 0;
            try
            {
                SerialPort _serialPort1 = new SerialPort();
                //_serialPort1.PortName = Global.VMController.ControllerBoardComPort; TO DO
                _serialPort1.BaudRate = Global.VMController.ControllerBoardBaudRate;
                _serialPort1.ReadTimeout = 3000;
                //to get the battery Health of the TABS.
                //string result = await BatteryHealth();

                Global.log.Trace("Checking PCB Heartbeat");
                if (!_serialPort1.IsOpen)
                    _serialPort1.Open();
                //Clear Serial Ports
                _serialPort1.Write("");
                //sending hb to get the Boolean response 
                // _serialPort1.Write("hb");
                //_serialPort1.Write("hb," + result);
                while (true)
                {
                    try
                    {
                        string message = _serialPort1.ReadLine();
                        Global.log.Trace("PCB message Read: " + message);
                        if (message.Contains("true"))
                        {
                            hb = 1;
                            Global.log.Trace("PCB HeartBeat Read: " + hb.ToString());
                            break;
                        }
                        else if (message.Contains("false"))
                        {

                            await Shutdown();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Global.log.Trace("PCB HeartBeat Read: " + ex.Message);
                    }
                }
                _serialPort1.Close();
            }
            catch (Exception ex)
            {
                Global.log.Trace("PCB HeartBeat Read: " + ex.Message);
                return hb;
            }
            return hb;
        }

        // public async Task<string> BatteryHealth()
        // {
        //     PowerStatus power;
        //     Type t = typeof(System.Windows.Forms.PowerStatus);
        //     PropertyInfo[] pi = t.GetProperties();
        //      PropertyInfo propLife = null, propCharge = null;
        //     for (int i = 0; i < pi.Length; i++)
        //     {
        //      if (pi[i].Name == "BatteryLifePercent")
        //        {
        //        propLife = pi[i];
        //break;
        //           }
        //         else if (pi[i].Name == "PowerLineStatus")
        //       {
        //         propCharge = pi[i];
        //      }
        // }
        // object propLifeval = propLife.GetValue(SystemInformation.PowerStatus, null);
        //object propChargeval = propCharge.GetValue(SystemInformation.PowerStatus, null);

        //    if (Convert.ToDouble(propLifeval) <= 0.30 && propChargeval.ToString() == "Offline")
        //        return "Charge";

        //    if (Convert.ToDouble(propLifeval) >= 0.90 && (propChargeval.ToString() == "Online"))
        //        return "DisCharge";
        //     else
        //          return "Nothing";
        //   }
        public async Task<bool> Shutdown()
        {
            ManagementBaseObject mboShutdown = null;
            ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams =
              mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system. Use "2" to reboot.
            mboShutdownParams["Flags"] = "1";
            mboShutdownParams["Reserved"] = "0";
            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                mboShutdown = manObj.InvokeMethod("Win32Shutdown",
                  mboShutdownParams, null);
            }
            return true;
        }

    }
}