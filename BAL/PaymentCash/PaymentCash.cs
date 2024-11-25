using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace VendingC.BAL
{
    class PaymentCash
    {

        //bool Running = false; // Indicates the status of the main poll loop
        int pollTimer = 250; // Timer in ms between polls
        int reconnectionAttempts = 20, reconnectionInterval = 3; // Connection info to deal with retrying connection to validator
        volatile bool Connected = false, ConnectionFail = false; // Threading bools to indicate status of connection with validator
        CValidator Validator; // The main validator class - used to send commands to the unit
        bool FormSetup = false; // Boolean so the form will only be setup once
        // System.Windows.Forms.Timer reconnectionTimer = new System.Windows.Forms.Timer(); // Timer used to give a delay between reconnect attempts
        Thread ConnectionThread; // Thread used to connect to the validator
      


        private byte FindMaxProtocolVersion()
        {
            // not dealing with protocol under level 6
            // attempt to set in validator
            byte b = 0x06;
            while (true)
            {
                Validator.SetProtocolVersion(b);
                if (Validator.CommandStructure.ResponseData[0] == CCommands.SSP_RESPONSE_FAIL)
                    return --b;
                b++;
                if (b > 20)
                    return 0x06; // return default if protocol 'runs away'
            }
        }


        private bool IsUnitTypeSupported(char type)
        {
            if (type == (char)0x00)
                return true;
            return false;
        }


        private bool ConnectToValidator()
        {
            // setup the timer
            // reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

            // run for number of attempts specified
            for (int i = 0; i < reconnectionAttempts; i++)
            {
                // reset timer
                // reconnectionTimer.Enabled = true;

                // close com port in case it was open
            //   Validator = new CValidator();
              Validator.SSPComms.CloseComPort();

                 //turn encryption off for first stage
                  Validator.CommandStructure.EncryptionStatus = false;

                // open com port and negotiate keys
                if (Validator.OpenComPort() && Validator.NegotiateKeys())
                {
                    Validator.CommandStructure.EncryptionStatus = true; // now encrypting
                    // find the max protocol version this validator supports
                    byte maxPVersion = FindMaxProtocolVersion();
                    if (maxPVersion > 6)
                    {
                        Validator.SetProtocolVersion(maxPVersion);
                    }
                    else
                    {
                        Global.log.Trace("ERROR: This program does not support units under protocol version 6, update firmware.");
                        // MessageBox.Show("This program does not support units under protocol version 6, update firmware.", "ERROR");
                        return false;
                    }
                    // get info from the validator and store useful vars
                    Validator.ValidatorSetupRequest();
                    // Get Serial number
                    Validator.GetSerialNumber();
                    // check this unit is supported by this program
                    if (!IsUnitTypeSupported(Validator.UnitType))
                    {

                        Global.log.Trace("Unsupported unit type, this SDK supports the BV series and the NV series (excluding the NV11)");
                        // MessageBox.Show("Unsupported unit type, this SDK supports the BV series and the NV series (excluding the NV11)");
                        //Application.Exit();
                        return false;
                    }
                    // inhibits, this sets which channels can receive notes
                    Validator.SetInhibits();
                    // enable, this allows the validator to receive and act on commands
                    Validator.EnableValidator();

                    return true;
                }
                //while (reconnectionTimer.Enabled) Application.DoEvents(); // wait for reconnectionTimer to tick
            }
            return false;
        }


        // This is the same as the above function but set up differently for threading.
        private void ConnectToValidatorThreaded()
        {
            // setup the timer
            //reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

            // run for number of attempts specified
            for (int i = 0; i < reconnectionAttempts; i++)
            {
                // reset timer
                //reconnectionTimer.Enabled = true;

                // close com port in case it was open
                Validator.SSPComms.CloseComPort();

                // turn encryption off for first stage
                Validator.CommandStructure.EncryptionStatus = false;

                // open com port and negotiate keys
                if (Validator.OpenComPort() && Validator.NegotiateKeys())
                {
                    Validator.CommandStructure.EncryptionStatus = true; // now encrypting
                    // find the max protocol version this validator supports
                    byte maxPVersion = FindMaxProtocolVersion();
                    if (maxPVersion > 6)
                    {
                        Validator.SetProtocolVersion(maxPVersion);
                    }
                    else
                    {
                        Global.log.Trace("ERROR: This program does not support units under protocol version 6, update firmware.");
                        //MessageBox.Show("This program does not support units under protocol version 6, update firmware.", "ERROR");
                        Connected = false;
                        return;
                    }
                    // get info from the validator and store useful vars
                    Validator.ValidatorSetupRequest();
                    // inhibits, this sets which channels can receive notes
                    Validator.SetInhibits();
                    // enable, this allows the validator to operate
                    Validator.EnableValidator();

                    Connected = true;
                    return;
                }
                //while (reconnectionTimer.Enabled) Application.DoEvents(); // wait for reconnectionTimer to tick
            }
            Connected = false;
            ConnectionFail = true;
        }

       public async Task<bool> PayCash(int amount)
        {
            Global.General.CashCreditAmount = 0;
            Global.General.CashInAmount = 0;
            var testtime = DateTime.Now;
            //btnRun.Enabled = false;
            Validator = new CValidator();
            Validator.CommandStructure.ComPort = Global.CashMachineComPort;
            Validator.CommandStructure.SSPAddress = Global.SSPAddress;
            Validator.CommandStructure.Timeout = 8000;
            Validator.CommandStructure.BaudRate = 9600; 

            // connect to the validator
            if (ConnectToValidator())
            {
                Global.NoteValidator.Running = true;
                Global.log.Trace("\r\nPoll Loop\r\n*********************************\r\n");
                //textBox1.AppendText("\r\nPoll Loop\r\n*********************************\r\n");
                //btnHalt.Enabled = true;
            }

            while (Global.NoteValidator.Running)
            {
                //Global.log.Trace("Seconds"+(DateTime.Now - testtime).TotalSeconds);
                if (((DateTime.Now - testtime).TotalSeconds >= 30))
                {
                    Global.log.Trace("Breaking the loop");
                    break;
                }
                // if the poll fails, try to reconnect
                if (!Validator.DoPoll(amount))
                {
                    //textBox1.AppendText("Poll failed, attempting to reconnect...\r\n");
                    Global.log.Trace("Poll failed, attempting to reconnect...");
                    Connected = false;
                    ConnectionThread = new Thread(ConnectToValidatorThreaded);
                    ConnectionThread.Start();

                  
                    while (!Connected)
                    {
                        if (ConnectionFail)
                        {
                            Global.log.Trace("Failed to reconnect to validator\r\n");
                            //textBox1.AppendText("Failed to reconnect to validator\r\n");
                            return false;
                        }
                        //Application.DoEvents();
                    }
                    
                    //textBox1.AppendText("Reconnected successfully\r\n");
                }

                //timer1.Enabled = true;
                // update form
                //UpdateUI();
                // setup dynamic elements of win form once
                //if (!FormSetup)
                //{
                //    SetupFormLayout();
                //    FormSetup = true;
                //}
                //while (timer1.Enabled)
                //{
                //    Application.DoEvents();
                //    Thread.Sleep(1); // Yield to free up CPU
                //}
            }

            //close com port and threads
            Validator.SSPComms.CloseComPort();
            if (Global.General.CashInAmount == amount)
            {
                return true;
            }
            else if (Global.General.CashInAmount > amount)
            {
                Global.General.TotalNetAmount = Global.General.CashInAmount;
                Global.General.CashCreditAmount = Global.General.CashInAmount - amount;
                return true;
            }
            else if (Global.General.CashInAmount < amount)
            {
                Global.General.TotalNetAmount = Global.General.CashInAmount;
                Global.General.CashCreditAmount = Global.General.CashInAmount;
                return false;
            }

            else
            {
                return false;
            }

            //btnRun.Enabled = true;
            //btnHalt.Enabled = false;
        }

        
    }
}
