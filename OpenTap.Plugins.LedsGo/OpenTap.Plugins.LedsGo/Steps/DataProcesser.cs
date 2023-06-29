//Copyright 2012-2019 Keysight Technologies
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
using OpenTap;
using LedsGo.Instruments;
using LedsGo.Utilities;
using System.IO;

// This example shows how a child can get a parent with of a certain type, and then
// recover some properties from that parent.

namespace LedsGo.Steps
{
    public enum PCB : int
    {
        Undefined = 0,
        DALI = 1,
        KATE = 2,
    }

    public enum DALIType : int
    {
        Undefined = 0,
        Normal = 1,
        Switched = 2
    }

    public enum KATEType : int
    {
        Undefined = 0,
        NO_DIM = 1,
        DIM = 2,
    }
    public enum KATENO_DIMType: int
    {
        Undefined = 0,
        K56mA = 1,
        Other =2
    }

    [Display("ChildDAQ", Groups: new[] { "LedsGo" }, Description: "Analizes DALI curve")]
    [AllowAsChildIn(typeof(LabJackStep))]
    // Excluding the AllowAsChildIn attribute would mean this Child TestStep could be used with any parent.
    public class DAQChildStep : TestStep
    {
        [Display("Type PCB:", Order: 40, Description: "Choose the type of PCB")]
        public PCB PCBType { get; set; }

        [Display("Type PCB DALI:", Order: 41, Description: "Choose the type of DALI to process")]
        [EnabledIf("PCBType", PCB.DALI, HideIfDisabled = true)]
        public DALIType Dalitype { get; set; }

        [Display("Type PCB KATE:", Order: 42, Description: "Choose the type of DALI to process")]
        [EnabledIf("PCBType", PCB.KATE, HideIfDisabled = true)]
        public KATEType Katetype { get; set; }

        [Display("Min Point:", Order: 43, Description: "Min in mA")]
        [EnabledIf("PCBType", PCB.DALI, HideIfDisabled = true)]
        public double PointMin { get; set; }

        [Display("Error on Min:", Order: 44, Description: "Interval (pos and neg) that the measured points can deviate to")]
        [EnabledIf("PCBType", PCB.DALI, HideIfDisabled = true)]
        public double ErrOnMin { get; set; }

        [Display("Max Point:", Order: 45, Description: "Max in mA")]
        [EnabledIf("PCBType", PCB.DALI, HideIfDisabled = true)]
        public double PointMax { get; set; }

        [Display("Error on Max:", Order: 46, Description: "Interval (pos and neg) that the measured points can deviate to")]
        [EnabledIf("PCBType", PCB.DALI, HideIfDisabled = true)]
        public double ErrOnMax{ get; set; }

        [Display("Period of curve:", Order: 47, Description: "Period of curve in sec")]
        [EnabledIf("PCBType", PCB.DALI, HideIfDisabled = true)]
        public double period { get; set; }

        [Display("Threshold Min:", Order: 48, Description: "Min in mA")]
        [EnabledIf("PCBType", PCB.KATE, HideIfDisabled = true)]
        [EnabledIf("Katetype", KATEType.NO_DIM, HideIfDisabled = true)]
        public double ThMin { get; set; }

        [Display("Error on Threshold Min:", Order: 49, Description: "Interval (pos and neg) that the measured points can deviate to")]
        [EnabledIf("PCBType", PCB.KATE, HideIfDisabled = true)]
        [EnabledIf("Katetype", KATEType.NO_DIM, HideIfDisabled = true)]
        public double ErrOnThMin { get; set; }

        [Display("Threshold Max:", Order: 50, Description: "Max in mA")]
        [EnabledIf("PCBType", PCB.KATE, HideIfDisabled = true)]
        public double ThMax { get; set; }

        [Display("Error on Threshold Max:", Order: 51, Description: "Interval (pos and neg) that the measured points can deviate to")]
        [EnabledIf("PCBType", PCB.KATE, HideIfDisabled = true)]
        public double ErrOnThMax { get; set; }

        [Display("Interval until max:", Order: 52, Description: "Interval until current is max (in sec)")]
        [EnabledIf("PCBType", PCB.KATE, HideIfDisabled = true)]
        [EnabledIf("Katetype", KATEType.NO_DIM, HideIfDisabled = true)]
        public double interval { get; set; }

        [Display("Type NO_DIM KATE", Order: 53, Description: "Choose what type of KATE PCB it is")]
        [EnabledIf("PCBType", PCB.KATE, HideIfDisabled = true)]
        [EnabledIf("Katetype", KATEType.NO_DIM, HideIfDisabled = true)]
        public KATENO_DIMType Kateno_dimtype { get; set; }


        private double[] IainData;
        private double[] DataInCurrent;
        private double[] EnveloppedData;
        private double[] MeanedData;
        private double[] VerdictData;
        //alle atributen naar hier

        public override void PrePlanRun()
        {
            base.PrePlanRun();
        }

        public override void Run()
        {
            IainData = GetParent<LabJackStep>().SampledInterval;
            int SampleRate = GetParent<LabJackStep>().sampleRate;
            LabJackInstrument Device = GetParent<LabJackStep>().Device;
            Verdict verdict = Verdict.Fail;
            

            if (PCBType == PCB.DALI)
            {
                if(Dalitype == DALIType.Normal)
                {
                    verdict = CheckDALINormal(SampleRate);
                }
                else if(Dalitype == DALIType.Switched)
                {
                    verdict = CheckDALISwitched(SampleRate);
                }
            }
            else if (PCBType == PCB.KATE)
            {
                if(Katetype == KATEType.NO_DIM)
                {
                    if(Kateno_dimtype == KATENO_DIMType.Other)
                    {
                        verdict = CheckKATENO_DIM(SampleRate);
                    }
                    else if(Kateno_dimtype == KATENO_DIMType.K56mA)
                    {
                        verdict = CheckKATE56mA(SampleRate);
                    }                    
                }
                else if (Katetype == KATEType.DIM)
                {
                    verdict = CheckKATEDIM(SampleRate);
                }
            }

            if (verdict != Verdict.Pass)
            {
                Device.DigitalWrite(DIO.FIO2, STATE.Low);
            }
            UpgradeVerdict(verdict);
        }
        public override void PostPlanRun()
        {
            base.PostPlanRun();
        }
        public Verdict CheckDALINormal(int SampleRate)
        {
            double PeriodInSamples = period * SampleRate;

            DataInCurrent = new double[IainData.Length];
            ConvertDataToCurrent();

            VerdictData = DataInCurrent;

            return CheckDALIGeneral(PeriodInSamples);
        }
        public Verdict CheckDALISwitched(int SampleRate)
        {
            double PeriodInSamples = period * SampleRate/50;

            DataInCurrent = new double[IainData.Length];
            ConvertDataToCurrent();

            MeanData(SampleRate);

            VerdictData = MeanedData;

            return CheckDALIGeneral(PeriodInSamples);
        }
        public Verdict CheckDALIGeneral(double PeriodInSamples)
        {
            var result = Verdict.Fail;

            int DiscardedZeroSam = FindZero(0); // stpoint  = 0
            int FrsMaxSam = FindMax(DiscardedZeroSam);
            if (FrsMaxSam != -1)
            {
                int FrsZeroSam = FindZero(FrsMaxSam);
                if (FrsZeroSam != -1)
                {
                    int SecMaxSam = FindMax(FrsZeroSam);
                    if (SecMaxSam != -1)
                    {
                        int SecZeroSam = FindZero(SecMaxSam);
                        if (SecZeroSam != -1)
                        {
                            int PeriodMax = SecMaxSam - FrsMaxSam;
                            int PeriodMin = SecZeroSam - FrsZeroSam;

                            if (1.1 * PeriodInSamples >= PeriodMin && 0.9 * PeriodInSamples <= PeriodMin)
                            {
                                if (1.1 * PeriodInSamples >= PeriodMax && 0.9 * PeriodInSamples <= PeriodMax)
                                {
                                    result = Verdict.Pass;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
        public Verdict CheckKATENO_DIM(int SampleRate)
        {
            double PeriodInSamples = interval * SampleRate / 25;

            DataInCurrent = new double[IainData.Length];
            ConvertDataToCurrent();

            EnveloppeDataKate(SampleRate);

            int ThMinSam = FindThMin(0); // stpoint  = 0

            if (ThMinSam != -1)
            {
                int ThMaxSam = FindThMax(ThMinSam);
                if (ThMaxSam != -1)
                {
                    int PeriodMeas = ThMaxSam - ThMinSam;
                    if (1.2 * PeriodInSamples >= PeriodMeas && 0.8 * PeriodInSamples <= PeriodMeas)
                    {
                        return Verdict.Pass;
                    }
                    else { return Verdict.Fail; }
                }
                else { return Verdict.Fail; }
            }
            else { return Verdict.Fail; }
        }
        public Verdict CheckKATEDIM(int SampleRate)
        {
            DataInCurrent = new double[IainData.Length];
            ConvertDataToCurrent();

            //EnveloppeDataKate(SampleRate);

            double delta = -1;
            for (int i = 10; i < DataInCurrent.Length; i++)
            {
                if ((DataInCurrent[i] - DataInCurrent[i - 10]) > delta)
                {
                    delta = DataInCurrent[i] - DataInCurrent[i - 10];
                }
            }

            // delta moet even groot zijn als de max stroom +/- 10%
            if (delta <= (ThMax + ErrOnThMax) && delta >= (ThMax - ErrOnThMax))
            {
                return Verdict.Pass;
            }
            else
            {
                return Verdict.Fail;
            }
        }
        public Verdict CheckKATE56mA(int SampleRate)
        {
            double PeriodInSamples = interval * SampleRate / 25;

            DataInCurrent = new double[IainData.Length];
            ConvertDataToCurrent();

            EnveloppeDataKate56mA(SampleRate);

            int ThMinSam = FindThMin(0); // stpoint  = 0

            if (ThMinSam != -1)
            {
                int ThMaxSam = FindThMax(ThMinSam);
                if (ThMaxSam != -1)
                {
                    int PeriodMeas = ThMaxSam - ThMinSam;
                    if (1.2 * PeriodInSamples >= PeriodMeas && 0.8 * PeriodInSamples <= PeriodMeas)
                    {
                        return Verdict.Pass;
                    }
                    else { return Verdict.Fail; }
                }
                else { return Verdict.Fail; }
            }
            else { return Verdict.Fail; }
        }
        public int FindZero(int startpoint)
        {
            if (startpoint == -1)
            {
                return -1;
            }
            for (int i = startpoint; i < VerdictData.Length; i++)
            {
                if(VerdictData[i] <= (PointMin + ErrOnMin) && VerdictData[i] >= (PointMin - ErrOnMin))
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindMax(int startpoint)
        {
            if(startpoint == -1)
            {
                return -1;
            }
            for (int i = startpoint; i < VerdictData.Length; i++)
            {
                if (VerdictData[i] <= (PointMax + ErrOnMax) && VerdictData[i] >= (PointMax - ErrOnMax))
                {
                    return i;
                }
            }
            return -1;
        }
        public void ConvertDataToCurrent()
        {
            for(int i = 0; i< IainData.Length; i++)
            {
                DataInCurrent[i] = IainData[i]*200;
            }
        }
        private void MeanData(int samplerate)
        {
            int interval = samplerate / 40;
            MeanedData = new double[DataInCurrent.Length / interval];

            double sum = 0;
            for (int i = 0; i < DataInCurrent.Length; i = i + interval)
            {
                for (int j = 0; j < interval; j++)
                {
                    sum = sum + DataInCurrent[i + j];
                }
                MeanedData[i / interval] = sum/50;
                sum = 0;

            }
        }

        public int FindThMin(int startpoint)
        {
            if (startpoint == -1)
            {
                return -1;
            }
            for (int i = startpoint; i < EnveloppedData.Length; i++)
            {
                if (EnveloppedData[i] <= (ThMin + ErrOnThMin) && EnveloppedData[i] >= (ThMin - ErrOnThMin))
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindThMax(int startpoint)
        {
            if (startpoint == -1)
            {
                return -1;
            }
            for (int i = startpoint; i < EnveloppedData.Length; i++)
            {
                if (EnveloppedData[i] <= (ThMax + ErrOnThMax) && EnveloppedData[i] >= (ThMax - ErrOnThMax))
                {
                    return i;
                }
            }
            return -1;
        }
        private void EnveloppeDataKate(int samplerate)
        {
            int interval = samplerate / 400;
            EnveloppedData = new double[DataInCurrent.Length / interval];

            double sum = -1;
            for (int i = 0; i < DataInCurrent.Length; i = i + interval)
            {
                for (int j = 0; j < interval; j++)
                {
                    if (sum < DataInCurrent[i + j])
                    {
                        sum = DataInCurrent[i + j];
                    }
                }
                EnveloppedData[i / interval] = sum;
                sum = -1;

            }
        }
        private void EnveloppeDataKate56mA(int samplerate)
        {
            int interval = samplerate / 400;
            EnveloppedData = new double[DataInCurrent.Length / interval];

            double sum = -1;
            for (int i = 0; i < DataInCurrent.Length; i = i + interval)
            {
                for (int j = 0; j < interval; j++)
                {
                    sum += DataInCurrent[i + j];
                }
                EnveloppedData[i / interval] = sum/interval;
                sum = -1;

            }
        }
    }
}
