using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using LedsGo.Instruments;
using LedsGo.Utilities;
using OpenTap;

namespace LedsGo.Plugin.Knop
{
    public static class EventsTapPlan
    {
        public static string sFilePath;
        public static bool PlanRun;
        public static bool InitDone;

        public static event EventHandler StartHandler;
        public static event EventHandler StopHandler;
        public static event EventHandler LoadHandeler;

        public static LabJackClass InstrumentButton { get; set; }

        public static void Listen()
        {
            //Search for changes in external buttons.
            //Than call CallStartEvent();CallStopEvent();CallLoadEvent();
            TapThread.Start(() => run());
            //Thread test = new Thread(() => run());
            //test.Start();
            //Task.Run(() => run());
            //run();
        }

        private static void run()
        {
            //AQlabjackInstrument Instrument = DAQLJStep.InstrumentDockable;
            InstrumentButton = new LabJackClass();
            
            //InstrumentButton.InitButton();
            //DinValue = true;
            STATE resultflag = STATE.High;


            int Available = 0;
            do
            {
                Available = InstrumentButton.GetConnectedDevices();
            } while (Available == 0);

                while (true)
            {
                if(PlanRun == false)
                {
                    if (InitDone == false)
                    {
                        //int DeviceCon = InstrumentButton.GetConnectedDevices();
                        //if(DeviceCon > 0)
                        //{
                        InstrumentButton.Open("192.168.1.189");
                        InitDone = true;
                        //}
                    }
                    if (InitDone == true)
                    {
                        //int DeviceCon = InstrumentButton.GetConnectedDevices();
                        //if( DeviceCon > 0)
                        //{
                        resultflag = InstrumentButton.DigitalRead(Utilities.DIO.CIO0); //CIO0  
                        //InstrumentButton.Close();                                                                                           
                    }
                }

               
                //InstrumentButton.Close();

                if (resultflag == STATE.Low && PlanRun == false)
                {
                    PlanRun = true;
                    resultflag = STATE.High;
                    InstrumentButton.Close();
                   
                    //Log.Info(null,"Knop gedrukt");
                    CallStartEvent();
                    //Thread.Sleep(2000);

                }

               // Thread.Sleep(25); // person needs to hold longer then 25ms

            }
            
        }

        public static void CallStartEvent()
        {
            StartHandler(null, EventArgs.Empty);
        }

        public static void CallStopEvent()
        {
            StopHandler(null, EventArgs.Empty);
        }

        public static void CallLoadEvent()
        {
            LoadHandeler(null, EventArgs.Empty);
        }
    }
}
