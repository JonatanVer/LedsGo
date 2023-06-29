using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LedsGo.Instruments;
using LedsGo.Utilities;
using OpenTap;

namespace LedsGo.Steps
{
    [Display("DAQLJStep", Groups: new[] { "LedsGo" }, Description: "Configure a selected DAQ-device")]
    [AllowChildrenOfType(typeof(DAQChildStep))]
    public class LabJackStep : TestStep
    {

        [Display("Device:", Description: "Select the device.", Order:1)]
        public LabJackInstrument Device { get; set; }

        [Display("Signal:", Description: "Select the signal.", Order: 2)]
        public SIGNAL Signal { get; set; }

        [Display("In/Out:", Description: "Select the IO.", Order: 3)]
        public DIRECTION Direction { get; set; }

        [Display("Channel:", Description: "Select the digital input", Order: 4)]
        [EnabledIf("Signal", SIGNAL.Digital, HideIfDisabled = true)]
        public DIO DIN_Channel { get; set; }

        [Display("Readtype:", Description: "Select how to measure the analog pin", Order: 5)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        public READTYPE Readtype { get; set; }

        [Display("Unit:", Description: "Select if you want to measure current or voltage", Order: 6)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        [EnabledIf("Readtype", READTYPE.Singe_Shot, HideIfDisabled = true)]
        public UNIT Unit { get; set; }

        [Display("Channel:", Description: "Select the digital input", Order: 7)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        public AIN AIN_Channel { get; set; }

        [Display("Sample rate:", Description: "Samples per second", Order: 8)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        [EnabledIf("Readtype", READTYPE.Continious, HideIfDisabled = true)]
        public int sampleRate { get; set; }

        [Display("Sample Duration:", Description: "The duration of the measurement in sec", Order: 9)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        [EnabledIf("Readtype", READTYPE.Continious, HideIfDisabled = true)]
        public double sampleDuration { get; set; }

        [Display("Channel:", Description: "Select the digital input", Order: 10)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Output, HideIfDisabled = true)]
        public DAC DAC_Channel { get; set; }


        [Display("Output state:", Description: "Sets the output from the selected channel to the selected state.", Order: 11)]
        [EnabledIf("Signal", SIGNAL.Digital, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Output, HideIfDisabled = true)]
        public STATE DOutputState { get; set; }

        [Display("Expected input state:", Description: "Expected input state of the selected channel.", Order: 12)]
        [EnabledIf("Signal", SIGNAL.Digital, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        public STATE DExpectedInputState { get;  set; }

        [Display("Voltage:", Description: "sets teh voltage of the selected channel.", Order: 13)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Output, HideIfDisabled = true)]
        public double AnalogVoltageOut { get; set; }

        [Display("Voltage:", Description: "gets the voltage of the selected channel.", Order: 14)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        public double AnalogVoltageIn { get; private set; }

        [Display("Single shot type: ", Description: "Maximum measurent.", Order: 15)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        [EnabledIf("Readtype", READTYPE.Singe_Shot, HideIfDisabled = true)]
        public SINGLESHOTTYPE Singleshottype { get; set; }

        [Display("Minimum value: ", Description: "Maximum measurent.", Order: 16)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        [EnabledIf("Readtype", READTYPE.Singe_Shot, HideIfDisabled = true)]
        public double Min { get; set; }

        [Display("Maximum value: ", Description: "Maximum measurent.", Order:17)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        [EnabledIf("Readtype", READTYPE.Singe_Shot, HideIfDisabled = true)]
        public double Max { get; set; }

        [Display("Factor: ", Description: "this factor is multiplied with the measured voltage to display the actual voltage.", Order: 18)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        [EnabledIf("Readtype", READTYPE.Singe_Shot, HideIfDisabled = true)]
        public double Factor { get; set; }

        [Display("Sample Interval:", Description: "Select the digital input", Order: 19)]
        [EnabledIf("Signal", SIGNAL.Analog, HideIfDisabled = true)]
        [EnabledIf("Direction", DIRECTION.Input, HideIfDisabled = true)]
        [EnabledIf("Readtype", READTYPE.Continious, HideIfDisabled = true)]
        public double[] SampledInterval { get; private set; }

        Verdict result = Verdict.Pass;

        public override void PrePlanRun()
        {
            Device.Open();
        }
        public override void Run()
        {
            if (Signal == SIGNAL.Digital)
            {
                if (Direction == DIRECTION.Input)
                {
                    result = ReadDigital();
                }
                else
                {
                    if (Direction == DIRECTION.Output)
                    {
                        result = WriteDigital();
                    }
                }
            }
            else
            {
                if (Signal == SIGNAL.Analog)
                {
                    if (Direction == DIRECTION.Input)
                    {
                        if(Readtype == READTYPE.Singe_Shot)
                        {
                            if(Singleshottype == SINGLESHOTTYPE.MeasOne)
                            {
                                result = ReadAnalog(SINGLESHOTTYPE.MeasOne);
                            }
                            else if(Singleshottype == SINGLESHOTTYPE.MeasEnv)
                            {
                                result = ReadAnalog(SINGLESHOTTYPE.MeasEnv);
                            }
                            else if(Singleshottype == SINGLESHOTTYPE.MeasMean)
                            {
                                result = ReadAnalog(SINGLESHOTTYPE.MeasMean);
                            }
                            
                        }
                        else if(Readtype == READTYPE.Continious)
                        {
                            ReadAnalogContinious();
                        }
                        
                    }
                    else
                    {
                        if (Direction == DIRECTION.Output)
                        {
                            result = WriteAnalog();
                        }
                    }
                }
            }

            if (result != Verdict.Pass)
            {
                Device.DigitalWrite(DIO.FIO2, STATE.Low);
            }

            UpgradeVerdict(result);

        }

        public Verdict ReadDigital()
        {
            var verdict = Verdict.Fail;

            var InputRead = Device.DigitalRead(DIN_Channel);

            if (DExpectedInputState == InputRead)
            {
                verdict = Verdict.Pass;
            }

            Log.Debug(DIN_Channel.ToString() + "read: " + InputRead.ToString());

            return verdict;
        }
        public Verdict WriteDigital()
        {
            var verdict = Verdict.Pass;

            Device.DigitalWrite(DIN_Channel, DOutputState);

            Log.Debug(DIN_Channel.ToString() + "set to: " + DOutputState.ToString());

            return verdict;
        }
        public Verdict ReadAnalog(SINGLESHOTTYPE type)
        {
            var verdict = Verdict.Fail;
            if(type == SINGLESHOTTYPE.MeasOne)
            {
                AnalogVoltageIn = Device.AnalogRead(AIN_Channel) * Factor;
            }
            else if (type == SINGLESHOTTYPE.MeasEnv)
            {
                AnalogVoltageIn = Device.AnalogReadEnv(AIN_Channel) * Factor;
            }
            else if (type == SINGLESHOTTYPE.MeasMean)
            {
                AnalogVoltageIn = Device.AnalogReadMean(AIN_Channel) * Factor;
            }

            if (Unit == UNIT.Current)
            {
                AnalogVoltageIn *= 197.5;
            }

            if (AnalogVoltageIn <= 0)
                AnalogVoltageIn = 0;

            Log.Debug(AIN_Channel.ToString() + " measured: " + AnalogVoltageIn.ToString());

            if (Min <= AnalogVoltageIn)
                if (Max >= AnalogVoltageIn)
                    verdict = Verdict.Pass;

            return verdict;
        }
        public void ReadAnalogContinious()
        {
            SampledInterval = Device.AnalogRead(AIN_Channel, sampleRate, sampleDuration);

            for (int i = 0; i < SampledInterval.Length; i++)
            { 

                if (SampledInterval[i] <= 0)
                    SampledInterval[i] = 0;
            }

            RunChildSteps(); // hier de sample controleren
        }
        public Verdict WriteAnalog()
        {
            var verdict = Verdict.Pass;

            if (AnalogVoltageOut < 5.01)
            {

                Device.AnalogWrite(DAC_Channel, AnalogVoltageOut);

                var ValueRead = Device.AnalogRead(DAC_Channel);

                if (Math.Round(ValueRead, 4) == Math.Round(AnalogVoltageOut, 4))
                {
                    verdict = Verdict.Pass;
                    Log.Debug(AIN_Channel.ToString() + "set to voltage: " + result.ToString());
                }
            }
            else
                Log.Error("Voltage value must not be greather than 5.00V");

            return verdict;
        }
        //public Verdict ReadAnalog()
        //{
        //    var verdict = Verdict.Fail;

        //    AnalogVoltageIn = Device.AnalogRead(AIN_Channel) * Factor;

        //    if(Unit == UNIT.Current)
        //    {
        //        AnalogVoltageIn *= 200;
        //    }

        //    if (AnalogVoltageIn <= 0)
        //        AnalogVoltageIn = 0;

        //    Log.Debug(AIN_Channel.ToString() + " measured: " + AnalogVoltageIn.ToString());

        //    if (Min <= AnalogVoltageIn)
        //        if (Max >= AnalogVoltageIn)
        //            verdict = Verdict.Pass;

        //    return verdict;
        //}
        //public Verdict ReadAnalogEnv()
        //{
        //    var verdict = Verdict.Fail;

        //    AnalogVoltageIn = Device.AnalogReadEnv(AIN_Channel) * Factor;

        //    if (Unit == UNIT.Current)
        //    {
        //        AnalogVoltageIn *= 200;
        //    }

        //    if (AnalogVoltageIn <= 0)
        //        AnalogVoltageIn = 0;

        //    Log.Debug(AIN_Channel.ToString() + " measured: " + AnalogVoltageIn.ToString());

        //    if (Min <= AnalogVoltageIn)
        //        if (Max >= AnalogVoltageIn)
        //            verdict = Verdict.Pass;

        //    return verdict;
        //}
        //public Verdict ReadAnalogMean()
        //{
        //    var verdict = Verdict.Fail;

        //    AnalogVoltageIn = Device.AnalogReadMean(AIN_Channel) * Factor;

        //    if (Unit == UNIT.Current)
        //    {
        //        AnalogVoltageIn *= 200;
        //    }

        //    if (AnalogVoltageIn <= 0)
        //        AnalogVoltageIn = 0;

        //    Log.Debug(AIN_Channel.ToString() + " measured: " + AnalogVoltageIn.ToString());

        //    if (Min <= AnalogVoltageIn)
        //        if (Max >= AnalogVoltageIn)
        //            verdict = Verdict.Pass;

        //    return verdict;
        //}

        public override void PostPlanRun()
        {
            //Device.DigitalWrite(DIO.FIO2, STATE.Low);
            Device.Close();
        }
    }
}
