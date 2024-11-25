using System;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using VendingC.Data;
using System.Collections.Generic;

namespace VendingC
    {
    public static class DispenseHandler
        {
        public static int tick = 0;

        public static async Task<bool> SetDispenseSuccess ( ShoppingCartEntry _currentDispensing )
            {
            try
                {
                //LOGGING TRANSATION DATA IN DATABASE
               await Data.VendingC.SubmitProductTransaction ( _currentDispensing );
                }
            catch(Exception ex)
                {
                Global.log.Trace ( ex.ToString ( ) );
                }

            try
                {
                await Data.VendingC.UpdateTblSlotDataInLocalDB ( _currentDispensing );
                }
            catch(Exception ex)
                {
                Global.log.Trace ( ex.ToString ( ) );
                }

            return true;
            }

        

        public static async Task<bool> SetDispenseFail ( ShoppingCartEntry _currentDispensing )
            {
            try
                {
                if ( !_currentDispensing.dispensed )
                    {
                    string queryLocal = "update tblSlot set IsDeleted = 1, IsActive = 0, DeletedDate = DATE('now') where SlotId = '" + _currentDispensing.slotId + "';";
                    await Data.VendingC.ExecuteNonQuery ( queryLocal );

                    try
                        {
                        await Data.VendingC.DeactivateSlotOnServer ( _currentDispensing.slotId);
                        }
                    catch ( Exception ex )
                        {
                        Global.log.Trace ( "Exception unable to deactivate slotNo " + ex.ToString ( ) );

                        }
                    }

                return true;
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( ex.ToString ( ) );
                return false;
                }


            }

        //public static async Task ClearpayementRegister()
        //{
        //    if (Global.General.WantAnotherTransaction)
        //    {
        //        Global.General.CashInAmount = 0;
        //        ReadWrite.Write(Global.General.CreditAmount.ToString(), Global.Actions.AddToAmount.ToString());
        //    }
        //    else
        //    {
        //        if (Global.General.CreditAmount > 0)
        //        {
        //            ReadWrite.Write((Global.General.CreditAmount / 10).ToString(), Global.Actions.NotesToPay.ToString());
        //        }
        //        Global.General.CashInAmount = 0;
        //        Global.General.CreditAmount = 0;
        //        ReadWrite.Write("0", Global.Actions.ClearAmount.ToString());
        //        ReadWrite.Write("0", Global.Actions.NoteCount.ToString());
        //    }
        //    ReadWrite.Write("Stop", Global.Actions.Enabled.ToString());
        //}

        public static async Task<string> GetTrasactionStatus ( ShoppingCart ShoppingCart )
            {
            string res = "";
            try
                {
                int failedCount = ShoppingCart.Entries.Where ( x => x.dispensed == false ).Count ( );
                //Global.General.CreditAmount -= Convert.ToInt32(Global.General.ChargesIfAny);
                Global.General.TransactionReturnAmount = ShoppingCart.Entries.Where ( x => x.dispensed == true ).Select ( c => c.Product.cost ).Sum ( );
                // Global.General.CreditAmount -= Global.General.TransactionReturnAmount;
                int TempCreditAmount = ShoppingCart.Entries.Where ( x => x.dispensed == false ).Select ( c => c.Product.cost ).Sum ( );
                Global.General.TotalCostAmount = Global.General.TotalCostAmount- TempCreditAmount;
                Global.log.Trace ( "Credit amount calulated because of dispense failed:" + TempCreditAmount );


                if ( TempCreditAmount > 0 )
                    {
                    if ( Global.General.CreditAmount == 0 )
                        {
                        Global.General.CreditAmount = TempCreditAmount;
                        }
                    else if ( Global.General.CashInAmount != 0 )
                        {
                        Global.General.CreditAmount = Global.General.CreditAmount + Math.Abs( Global.General.CreditAmount - TempCreditAmount);
                        }
                    else
                        {
                        Global.General.CreditAmount = Global.General.CreditAmount - Global.General.TotalCostAmount;
                        }

                    }
                else
                    {
                    if ( Global.General.TotalCostAmount < Global.General.CreditAmount )
                        {
                        Global.General.CreditAmount = Global.General.CreditAmount - Global.General.TotalCostAmount;
                        }
                    else
                        {
                        Global.General.CreditAmount = 0;

                        }
                    }


                if ( failedCount == ShoppingCart.Entries.Count )
                    {
                    //DispenseStatus DF
                    Global.General.DISPENSE_STATUS = "DF";
                    Global.UserMessage = "No item was dispensed: Apologies for inconvenience please collect your change. Contact on \r\n"
                                            + " to seek help.";
                    //                          + ConfigurationManager.AppSettings["Admin_Help_Number"].ToString() + " to seek help.";
                    }
                else if ( failedCount == 0 )
                    {
                    //Dispens All
                    Global.General.DISPENSE_STATUS = "DA";
                    Global.UserMessage = "🎉🎉🎉 Hurrah! 🎉🎉🎉\r\nThank you  Please visit again.";
                    }
                else if ( failedCount < ShoppingCart.Entries.Count )
                    {

                    //Dispense Partial
                    Global.General.DISPENSE_STATUS = "DP";
                    //Global.UserMessage = "🎉Thank you!🎉 and apologies for missing items. please collect your change. Contact on\r\n "
                    //   + ConfigurationManager.AppSettings["Admin_Help_Number"].ToString() + " to seek help.";
                    }


                //await new TransactionHandler().SubmitProductTransaction(paymentType);
                // await new TransactionHandler().SubmitSaleData(paymentType);
                res = "success";
                }
            catch ( Exception ex )
                {
                Global.log.Trace ( ex.ToString ( ) );
                res = "error:" + ex.Message;
                }
            return res;
            }

        }
    }
