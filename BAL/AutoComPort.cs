using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace VendingC.Utilities
{
    public class AutoComPort
    {
        private List<string> portNamesList = new List<string>();
        bool motorPortFound = false;
        bool ecrPortFound = false;
        public async Task DetectComPortsAsync()
        {
         //   await ProcessTransaction("COM7");
            LoadAvailablePorts();

            foreach (var portName in portNamesList)
            {
                //  Check if this port is for the Motor Controller
                if (!motorPortFound && await GetMotorComportAsync(portName))
                {
                    Global.MotorComPort = portName;
                    Global.log.Trace($"Motor Controller found on port: {portName}");
                    motorPortFound = true; // Mark that the motor port has been found
                    continue;
                }
                // Check if this port is for the ECR
                else if (Global.PaymentType.PosPay && !ecrPortFound && await ProcessTransaction(portName))
                {
                    Global.POSMachineComPort = portName;
                    Global.log.Trace($"ECR found on port: {portName}");
                    ecrPortFound = true; // Mark that the ECR has been found
                    continue;
                }

                // Break if both ports have been found
                if (!string.IsNullOrEmpty(Global.MotorComPort) &&(!Global.PaymentType.PosPay || !string.IsNullOrEmpty(Global.POSMachineComPort)))
                {
                    break;
                }
            }
        }

        private void LoadAvailablePorts()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM"))
                {
                    if (key != null)
                    {
                        foreach (string portName in key.GetValueNames())
                        {
                            string portValue = (string)key.GetValue(portName);
                            if (portValue != Global.MotorComPort && portValue != Global.POSMachineComPort)
                            {
                                portNamesList.Add(portValue);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Global.log.Trace(e.ToString());
            }
        }
        public async Task<bool> GetMotorComportAsync(string portName)
        {
            int hb = 0;
            try
            {
                SerialPort _serialPort1 = new SerialPort();
                _serialPort1.BaudRate = Global.VMController.ControllerBoardBaudRate;
                _serialPort1.ReadTimeout = 8000;

                _serialPort1.PortName = portName;
                if (!_serialPort1.IsOpen)
                    _serialPort1.Open();
                //Clear Serial Ports
                _serialPort1.Write("");
                //sending hb to get the Boolean response 
                // _serialPort1.Write("hb");
                _serialPort1.Write("hb, none");
                while (true)
                {
                    try
                    {
                        string message1 = _serialPort1.ReadLine();
                        Global.log.Trace("PCB message Read: " + message1);
                        if (message1.Contains("true"))
                        {
                            Global.log.Trace("Motor Controller Port Found");
                            //  MessageBox.Show("Port Found");
                            Global.MotorComPort = portName;
                            hb = 1;
                            break;

                        }
                    }
                    catch (Exception ex)
                    {
                        Global.log.Trace("Motor Controller Port Set up Failed");
                        Global.log.Trace(ex.ToString());
                        _serialPort1.Close();
                        break;
                    }
                }

                _serialPort1.Close();
            }
            catch (Exception ex)
            {
                Global.log.Trace(ex.ToString());
            }
            if (hb == 0) return false;
            else return true;
        }
//private static SemaphoreSlim semaphore = new SemaphoreSlim(2); 
        async Task<bool> ProcessTransaction(string portName)
        {
            DateTime startTime = DateTime.Now;
            TimeSpan timeoutDuration = TimeSpan.FromSeconds(8);
            bool isFound = false;
            string message = "";
          //  await semaphore.WaitAsync();
            using (SerialPort _serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One))
            {
               
                try
                {
                    _serialPort.Open();
                    Global.log.Trace("Port opened successfully.");

                    // Send signal to POS machine
                    _serialPort.Write("0500");
                    Global.log.Trace("Writing to POS machine.");

                    while (true)
                    {
                      
                        try
                        {
                            // Check for timeout
                            if (DateTime.Now - startTime > timeoutDuration)
                            {
                                Global.VMPosMachine.Timeout = true;
                                Global.log.Trace("Transaction timeout.");
                                break;
                            }

                            message += _serialPort.ReadExisting();
                            Global.log.Trace("ECR read: " + message);

                            // Check for settlement or cancellation
                            if (message.ToLower().Contains("batch no.") || message.ToLower().Contains("response") ||message.ToLower().Contains("settlement") || message.ToLower().Contains("cancelled"))
                            {
                                isFound = true;
                                Global.log.Trace("Settlement completed! ");
                                break;
                            }
                        }
                        catch (TimeoutException)
                        {
                            Global.log.Trace("Read operation timed out.");
                            Global.VMPosMachine.Timeout = true;
                            break;
                        }
                        catch (IOException ex)
                        {
                            Global.log.Trace($"IOException: {ex.Message}");
                            Global.VMPosMachine.Timeout = false;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Global.log.Trace($"Exception: {ex.Message}");
                            Global.VMPosMachine.Timeout = false;
                            break;
                        }
                    }
                    Console.WriteLine("Processing transaction...");
                    // Simulate work
                  //  await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Global.log.Trace($"Error opening port: {ex.Message}");
                    return false; // Failed to open port
                }
               
            } // SerialPort is automatically closed and disposed here

            return isFound;
        }


        private void HandleResponseCodes(string message)
        {
            if (message.Contains("Response Code=51"))
            {
                Global.ResponseMessagePOS = "Insufficient funds. Please try another card!";
            }
            else if (message.Contains("Response Code=55"))
            {
                Global.ResponseMessagePOS = "Incorrect PIN. Please try again!";
            }
            else if (message.Contains("Response Code=35"))
            {
                Global.ResponseMessagePOS = "Transaction limit for today has exceeded. Try another card!";
            }
        }
    

    private async Task<bool> CheckPortAsync(string portName)
        {

            SerialPort _serialPort = new SerialPort();
            // CardPaidAmount = Convert.ToInt32(amount);
            _serialPort.PortName = portName;
            //_serialPort.BaudRate = Global.BAUD_RATE;
            _serialPort.BaudRate = 115200;
            _serialPort.ReadTimeout = 8000;
          //  _serialPort.ReadTimeout = 3000;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            string message = "";
            string message1 = "";
            string message2 = "";
            string ItemResult = "";
            if (!_serialPort.IsOpen)
            {
                try
                {

                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    Global.log.Trace(ex.ToString());
                    // MessageBox.Show(ex.Message);
                }
            }
            else
            {
                _serialPort.Close();
                _serialPort.Open();
            }
            //Clear POS Serial Port
            // _serialPort.Write("");
            //Sending signal settlement to POS machine signal
            _serialPort.Write("0500");
            Global.log.Trace("writing ");
            // Thread.Sleep(3000);
            while (true)
            {
                try
                {
                    while (!(message.ToLower().Contains("settlement") || message.ToLower().Contains("cancelled")))
                    {
                        message += _serialPort.ReadExisting();
                        Global.log.Trace("ECR read: " + message);
                        // tryCount--;
                    }
                    if (message.ToLower().Contains("date"))
                    {
                        Global.VMPosMachine.Timeout = false;
                        if (message.Contains("Response Code=00") && message.Contains("Total Amount="))
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
                        break;
                    }

                    break;
                }
              
                catch (Exception ex)
                {
                    Global.log.Trace(ex.ToString());
                    Global.VMPosMachine.Timeout = false;
                    ItemResult = "Timeout";
                    break;
                }
            }
            //     _serialPort.Write("");
            _serialPort.Close();


            return true;

            //return message;
            //  Thread.Sleep(2000);
        }

        private async Task<string> ReadResponseAsync(SerialPort serialPort)
        {
            var responseBuilder = new System.Text.StringBuilder();
            bool isComplete = false;

            return await Task.Run(() =>
            {
                while (!isComplete)
                {
                    try
                    {
                        string line = serialPort.ReadLine();
                        // responseBuilder.Append(line + "\n"); // Append line with newline for clarity
                        if (line.Contains("true")) isComplete = true; // Set complete after first read, adjust as needed

                    }
                    catch (TimeoutException)
                    {
                        isComplete = true;
                    }
                    catch (Exception ex)
                    {
                        Global.log.Trace("Error reading from port: " + ex.ToString());
                        isComplete = true;
                    }
                }

                return responseBuilder.ToString();
            });
        }
    }

    //public class AutoComPort
    // {
    //    // static MCart _currentDispensing;
    //     static bool reading = false;
    //     static int cartIndex = 0;
    //     SerialPort _serialPort = new SerialPort();
    //     List<string> portNamesList = new List<string>();
    //     string m_ComPorts;
    //     int com_len;

    //     public async Task<int> GetMotorComportAsync()
    //     {
    //         int hb = 0;
    //         try
    //         {
    //             SerialPort _serialPort1 = new SerialPort();
    //             _serialPort1.BaudRate = Global.VMController.ControllerBoardBaudRate;
    //             _serialPort1.ReadTimeout = 8000;

    //             try
    //             {
    //                 RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
    //                 string[] portNames = key.GetValueNames();

    //                 foreach (string portName in portNames)
    //                 {
    //                     string portValue = (string)key.GetValue(portName);
    //                     if (portValue != Global.MotorComPort && portValue != Global.POSMachineComPort)
    //                     {
    //                         portNamesList.Add(portValue);
    //                     }
    //                 }
    //                 com_len = portNamesList.Count;

    //                 //string aqs = SerialDevice.GetDeviceSelector();
    //                 //var deviceCollection = await DeviceInformation.FindAllAsync(aqs);
    //                 //foreach (var item in deviceCollection)
    //                 //{
    //                 //    if (item.Name.Contains("COM"))
    //                 //    {
    //                 //        int length = item.Name.IndexOf("COM");
    //                 //        portNamesList.Add(item.Name.Substring(length,4));

    //                 //    }
    //                 //}
    //                 //com_len = portNamesList.Count;
    //             }
    //             catch (Exception e)
    //             { Global.log.Trace(e.ToString()); }



    //             for (int i = 0; i < com_len; i++)
    //             {
    //                 _serialPort1.PortName = portNamesList[i].ToString();
    //                 if (!_serialPort1.IsOpen)
    //                     _serialPort1.Open();
    //                 //Clear Serial Ports
    //                 _serialPort1.Write("");
    //                 //sending hb to get the Boolean response 
    //                 // _serialPort1.Write("hb");
    //                 _serialPort1.Write("hb, none");
    //                 while (true)
    //                 {
    //                     try
    //                     {
    //                         string message1 = _serialPort1.ReadLine();
    //                         Global.log.Trace("PCB message Read: " + message1);
    //                         if (message1.Contains("true"))
    //                         {
    //                             Global.log.Trace("Motor Controller Port Found");
    //                           //  MessageBox.Show("Port Found");
    //                             Global.MotorComPort = portNamesList[i].ToString();
    //                             hb = 1;
    //                             break;

    //                         }
    //                     }
    //                     catch (Exception ex)
    //                     {
    //                         Global.log.Trace ( "Motor Controller Port Set up Failed" );
    //                         Global.log.Trace ( ex.ToString ( ) );
    //                         _serialPort1.Close();
    //                         break;
    //                     }
    //                 }
    //                 if (hb == 1)
    //                 {
    //                     break;
    //                 }
    //             }
    //             _serialPort1.Close();
    //         }
    //         catch (Exception ex)
    //         {
    //             Global.log.Trace ( ex.ToString ( ) );
    //             }
    //         return hb;
    //     }

    // } 
}
