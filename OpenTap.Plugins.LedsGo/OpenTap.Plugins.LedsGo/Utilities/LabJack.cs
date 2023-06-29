using LabJack;
using OpenTap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedsGo.Utilities
{
    public enum DEVICE : int
    {
        Undefined = 0,
        ANY = 1,              //Open Any supported Labjack device
        T4 = 2,              //Open a T4 LabJack
        T7 = 3              //Open a T7 LabJack
    }
    public enum ACTION : int
    {
        READ = 0,
        WRITE = 1
    }
    public enum AIN : int
    {
        Undefined = 0,
        AIN0 = 1,
        AIN1 = 2,
        AIN2 = 3,
        AIN3 = 4,
        AIN4 = 5,
        AIN5 = 6,
        AIN6 = 7,
        AIN7 = 8,
        AIN8 = 9,
        AIN9 = 10,
        AIN10 = 11,
        AIN11 = 12,
        AIN12 = 13,
        AIN13 = 14,
        AIN14 = 15,
        ALL = 16
    }

    public enum DAC : int
    {
        Undefined = 0,
        DAC0 = 1,
        DAC1 = 2
    }
    public enum AIN_MODE : int
    {
        undefined = 0,
        DIFF = 3,               //Differential Mode
        SINGLE_ENDED = 199      //Single ended Mode
    }

    public enum AIN_RANGE : int
    {
        undefined = 0,
        RANGE_10V = 1,
        RANGE_1V = 2,
        RANGE_0V1 = 3,
        RANGE_0V01 = 4
    }

    public enum DIO : int
    {
        undefined = 0,
        FIO0 = 1,
        FIO1 = 2,
        FIO2 = 3,
        FIO3 = 4,
        FIO4 = 5,
        FIO5 = 6,
        FIO6 = 7,
        FIO7 = 8,
        EIO0 = 9,
        EIO1 = 10,
        EIO2 = 11,
        EIO3 = 12,
        EIO4 = 13,
        EIO5 = 14,
        EIO6 = 15,
        EIO7 = 16,
        CIO0 = 17,
        CIO1 = 18,
        CIO2 = 19,
        CIO3 = 20,
        MIO0 = 21,
        MIO1 = 22,
        MIO2 = 23
    }
    public enum CONNECTION : int
    {
        Undefined = 0,
        USB = 1,
        Ethernet = 3
    }

    public enum SIGNAL : int
    {
        Undefined = 0,
        Analog = 1,
        Digital = 2
    }

    public enum DIRECTION : int
    {
        Undefined = 0,
        Input = 1,
        Output = 2
    }
    public enum READTYPE : int
    {
        Undefined = 0,
        Singe_Shot = 1,
        Continious = 2
    }

    public enum STATE : int
    {
        Undefined = 0,
        High = 1,
        Low = 2
    }

    public enum SINGLESHOTTYPE : int
    {
        Undefined = 0,
        MeasOne = 1,
        MeasEnv = 2,
        MeasMean = 3
    }

    public enum UNIT : int
    {
        Undefined = 0,
        Voltage = 1,
        Current = 2
    }

    public enum ERROR : int
    {
        ERROR = -1,
        NO_ERROR = 0
    }

    //public enum READTYPE
    //{
    //    [Display("Single Shot")]
    //    SINGLE_SHOT,
    //    [Display("Continuous")]
    //    CONTINUOUS
    //}

    public enum RANGE : int
    {
        RANGE_10V = 0,
        RANGE_1V = 1,
        RANGE_0V1 = 2,
        RANGE_0V01 = 3
    }

    public class LabJackClass
    {
        int handle = 0;
        int devType = 0;
        int conType = 0;
        int serNum = 0;
        int connectedSerNum = 0;
        int ipAddr = 0;
        int port = 0;
        int maxBytesPerMB = 0;
        string ipAddrStr = "";

        public static List<string> GetDeviceList(DEVICE Device, CONNECTION Connection)
        {
            var devices = new List<string>();

            Dictionary<int, string> DEVICE_NAMES = new Dictionary<int, string> {
                {LJM.CONSTANTS.dtT7, "T7"},
                {LJM.CONSTANTS.dtT4, "T4"}
            };

            Dictionary<int, string> CONN_NAMES = new Dictionary<int, string> {
                {LJM.CONSTANTS.ctUSB, "USB"},
                {LJM.CONSTANTS.ctETHERNET, "Ethernet"}
            };

            const int MAX_SIZE = LJM.CONSTANTS.LIST_ALL_SIZE;

            int numFound = 0;

            int[] aDeviceTypes = new int[MAX_SIZE];
            int[] aConnectionTypes = new int[MAX_SIZE];
            int[] aSerialNumbers = new int[MAX_SIZE];
            int[] aIPAddresses = new int[MAX_SIZE];

            int handle = 0;

            try
            {
                //Find LabJack devices with listAllS.
                LJM.ListAllS(Device.ToString(), Connection.ToString(), ref numFound, aDeviceTypes, aConnectionTypes, aSerialNumbers, aIPAddresses);

                for (int i = 0; i < numFound; i++)
                {

                    string dev;
                    if (!DEVICE_NAMES.TryGetValue(aDeviceTypes[i], out dev))
                        dev = aDeviceTypes[i].ToString();
                    string con;
                    if (!CONN_NAMES.TryGetValue(aConnectionTypes[i], out con))
                        con = aConnectionTypes[i].ToString();

                    var result = LJM.OpenS(dev, con, aSerialNumbers[i].ToString(), ref handle);
                    string valueName = "SERIAL_NUMBER";
                    double value = 0;
                    LJM.eReadName(handle, valueName, ref value);

                    devices.Add(value.ToString());
                }
            }
            catch (LJM.LJMException e)
            {
                throw new Exception(e.ToString());
            }

            LJM.CloseAll();

            return devices;
        }
        public ERROR Open(string Identifier)
        {
            var resultOpen = LJM.LJMERROR.NOERROR;
            var result = ERROR.NO_ERROR;

            try
            {
                resultOpen = LJM.OpenS("T7", "ANY", Identifier, ref handle);
                LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
                LJM.NumberToIP(ipAddr, ref ipAddrStr);

                DigitalWrite(DIO.FIO2, STATE.Low);
            }
            catch (LJM.LJMException e)
            {
                throw new Exception(e.ToString());
            }

            if (resultOpen == LJM.LJMERROR.NOERROR)
            {
                result = ERROR.NO_ERROR;
            }
            else
            {
                result = ERROR.ERROR;
            }

            return result;
        }
        public LJM.LJMERROR Close()
        {
            return LJM.Close(handle);
        }
        public LJM.LJMERROR Reset()
        {
            return LJM.eWriteName(handle, "SYSTEM_REBOOT", 0x4C4A0000);            //Anders worden de nieuwe settings niet van kracht.
        }
        public LJM.LJMERROR SetDeviceName(string name)
        {
            return LJM.eWriteNameString(handle, "DEVICE_NAME_DEFAULT", name);
        }
        public string GetSerialNumber()
        {
            //Setup and call eReadName to read a value.
            string name = "SERIAL_NUMBER";
            double value = 0;
            LJM.eReadName(handle, name, ref value);

            return value.ToString();
        }
        public void SetDHCP()
        {
            LJM.eWriteName(handle, "ETHERNET_DHCP_ENABLE_DEFAULT", 1);

            Reset();
        }
        public void SetNetworkSettings(string IP, string Subnet, string Gateway)
        {
            //Setup and call eWriteNames to set the ethernet configuration.
            string[] aNames = new string[] { "ETHERNET_IP_DEFAULT", "ETHERNET_SUBNET_DEFAULT", "ETHERNET_GATEWAY_DEFAULT", "ETHERNET_DHCP_ENABLE_DEFAULT" };

            int ip = 0;
            int subnet = 0;
            int gateway = 0;

            int dhcpEnable = 0;  //1 = Enable, 0 = Disable

            LJM.IPToNumber(IP, ref ip);
            LJM.IPToNumber(Subnet, ref subnet);
            LJM.IPToNumber(Gateway, ref gateway);

            double[] aValues = new double[] { (uint)ip, (uint)subnet, (uint)gateway, dhcpEnable };
            int numFrames = aNames.Length;
            int errAddr = -1;

            LJM.eWriteNames(handle, numFrames, aNames, aValues, ref errAddr);

            Reset();
        }
        public Dictionary<string, string> GetNetworkSettings()
        {
            var result = new Dictionary<string, string>();
            //Setup and call eReadNames to read ethernet configuration.
            string[] aNames = new string[] { "ETHERNET_IP", "ETHERNET_SUBNET", "ETHERNET_GATEWAY" };

            double[] aValues = new double[aNames.Length];

            int numFrames = aNames.Length;

            int errAddr = -1;

            string str = "";

            LJM.eReadNames(handle, numFrames, aNames, aValues, ref errAddr);

            for (int i = 0; i < numFrames; i++)
            {
                LJM.NumberToIP((int)Convert.ToUInt32(aValues[i]), ref str);
                result.Add(aNames[i], str);
            }

            return result;
        }
        public int GetConnectedDevices()
        {
            int numDevices = 0;           

            int[] DeviceTypes = new int[LJM.CONSTANTS.LIST_ALL_SIZE];           
            int[] ConnectionTypes = new int[LJM.CONSTANTS.LIST_ALL_SIZE];
            int[] SerialNumbers = new int[LJM.CONSTANTS.LIST_ALL_SIZE];
            int[] IPadds = new int[LJM.CONSTANTS.LIST_ALL_SIZE];

            LJM.ListAllS("ANY", "ETHERNET", ref numDevices, DeviceTypes, ConnectionTypes, SerialNumbers, IPadds);
            return numDevices;
        }
        public STATE DigitalRead(DIO channel)
        {
            double dState = 0.0;

            try
            {
                LJM.eReadName(handle, channel.ToString(), ref dState);              
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return dState == 1.0 ? STATE.High : STATE.Low;
        }
        public void DigitalWrite(DIO channel, STATE value)
        {
            double dState = 0.0;

            dState = (value == STATE.High ? 1.0 : 0.0);

            try
            {
                LJM.eWriteName(handle, channel.ToString(), dState);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public  double AnalogRead(AIN channel)
        {
            double[] scanData = new double[100];
            double scanrate = 10000;
            double temp = 0;
            try
            {
                int address = 0, type = 0;

                LJM.NameToAddress(channel.ToString(), ref address, ref type) ;

                scanData[0] = 0.0;

                LJM.StreamBurst(handle, 1, new int[] { address }, ref scanrate, 100, scanData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            for (int i = 0; i < 100; i++)
            {
                temp += scanData[i];
            }

            return temp / 100;

        }

        public double AnalogReadEnv(AIN channel)
        {
            double[] scanData = new double[500];
            double[] envData = new double[25];
            double scanrate = 2000;
            double temp = 0;
            try
            {
                int address = 0, type = 0;

                LJM.NameToAddress(channel.ToString(), ref address, ref type) ;

                scanData[0] = 0.0;

                LJM.StreamBurst(handle, 1, new int[] { address }, ref scanrate, 500, scanData);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            double sum = -1;
            for (int i = 0; i < scanData.Length; i = i + 20)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (sum < scanData[i + j])
                    {
                        sum = scanData[i + j];
                    }
                }
                envData[i / 20] = sum;
                sum = -1;

            }

            for (int i = 0; i < 25; i++)
            {
                temp += envData[i];
            }
            return temp / 25;
        }

        public double AnalogReadMean(AIN channel)
        {
            double[] scanData = new double[500];
            double scanrate = 2000;
            double temp = 0;
            try
            {
                int address = 0, type = 0;

                LJM.NameToAddress(channel.ToString(), ref address, ref type);

                scanData[0] = 0.0;

                LJM.StreamBurst(handle, 1, new int[] { address }, ref scanrate, 500, scanData);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            for (int i = 0; i < 500; i++)
            {
                temp += scanData[i];
            }
            return temp / 500;
        }

        public double[] AnalogRead(AIN channel, int sampleRate, double sampleDuration)
        {

            double numSamples = sampleRate * sampleDuration;
            double scanRate = (double)sampleRate;
            int int_numSamples = (int) Math.Ceiling(numSamples);
            double[] scanData = new double[int_numSamples];

            try
            {
                int address = 0, type = 0;

                LJM.NameToAddress(channel.ToString(), ref address, ref type);

                scanData[0] = 0.0;

                LJM.StreamBurst(handle, 1, new int[] { address }, ref scanRate, int_numSamples, scanData);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return scanData;
        }

        public double AnalogRead(DAC channel)
        {
            double value = 0.00;

            try
            {
                LJM.eReadName(this.handle, channel.ToString(), ref value);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return value;
        }

        public void AnalogWrite(DAC channel, double value)
        {
            try
            {
                LJM.eWriteName(this.handle, channel.ToString(), value);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
