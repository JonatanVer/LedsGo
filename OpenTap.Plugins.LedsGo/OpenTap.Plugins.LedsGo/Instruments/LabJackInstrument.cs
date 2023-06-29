using LedsGo.Utilities;
using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

//Note this template assumes that you have a SCPI based instrument, and accordingly
//extends the ScpiInstrument base class.

//If you do NOT have a SCPI based instrument, you should modify this instance to extend
//the (less powerful) Instrument base class.

namespace LedsGo.Instruments
{
    [Display("LabJack", Group: "LedsGo", Description: "Insert a labjack device.")]
    public class LabJackInstrument : Instrument
    {
        private LabJackClass labJackDevice = new LabJackClass();

        private CONNECTION connectionType;

        private string deviceSerial;

        private string ipAdress;

        private string identifier;

        [XmlIgnore]
        [Browsable(false)]
        public bool isUSB { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public bool isEthernet { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public bool isNone { get; set; }

        [Browsable(false)]
        public List<string> LabJackList { get; set; }

        [Browsable(false)]
        private void GetLabjacksUSB()
        {
            TapThread.Start(() =>
            {
                LabJackList = LabJackClass.GetDeviceList(DEVICE.ANY, CONNECTION.USB);
                DeviceSerial = LabJackList.FirstOrDefault();
                OnPropertyChanged(DeviceSerial);
            }
            );
        }

        [Display(Name: "Connection type", Description: "conection typ with labjack.", Order: 1.0)]
        public CONNECTION ConnectionType
        {
            get
            {
                return connectionType;
            }
            set
            {
                connectionType = value;

                isUSB = false; isEthernet = false; isNone = false;

                switch (connectionType)
                {
                    case CONNECTION.USB: isUSB = true; GetLabjacksUSB(); break;
                    case CONNECTION.Ethernet: isEthernet = true; break;
                    default: break;
                }
            }
        }

        [Display("USB devices:", Description: "Selecteer het serienummer van de labjack die je wilt instellen.", Order: 1.1)]
        [AvailableValues("LabJackList")]
        [EnabledIf(nameof(isUSB), true, HideIfDisabled = true)]
        public string DeviceSerial
        {
            get
            {
                return identifier;
            }
            set
            {
                deviceSerial = value;
                identifier = deviceSerial;
            }
        }

        [Display("IP labjack adress:", Description: "ip adres van labjack", Order: 1.2)]
        [EnabledIf(nameof(isUSB), true, HideIfDisabled = true)]
        public string IpUsb
        {
            get
            {
                return ipAdress;
            }
            set
            {
                ipAdress = value;
            }
        }
        [Display("IP labjack adress:", Description: "ip adres van labjack", Order: 1.3)]
        [EnabledIf(nameof(isEthernet), true, HideIfDisabled = true)]
        public string IpEthernet 
        { 
            get
            {
                return ipAdress;
            }
            set
            {
                ipAdress = value;
                identifier = ipAdress;
            }
        }

        [Display("Subnet", Description: "Set subnet", Order: 1.4)]
        [EnabledIf(nameof(isUSB), true, HideIfDisabled = true)]
        public string Subnet { get; set; }

        [Display("Gateway", Description: "Set gateway", Order: 1.5)]
        [EnabledIf(nameof(isUSB), true, HideIfDisabled = true)]
        public string Gateway { get; set; }


        [Display("Set IP.", Description: "Set static ip adress", Order: 1.6)]
        [Browsable(true)]
        [EnabledIf(nameof(isUSB), true, HideIfDisabled = true)]
        public void SetStaticNetworkSettings()
        {
            labJackDevice.Open(deviceSerial);
            labJackDevice.SetNetworkSettings(IpUsb, Subnet, Gateway);
            labJackDevice.Close();
        }

        [Display("Get IP from labjack.", Description: "Get IP from labjack.", Order: 1.7)]
        [Browsable(true)]
        [EnabledIf(nameof(isUSB), true, HideIfDisabled = true)]
        public void getNetworkSettingsUSB()
        {
            TapThread.Start(() =>
                {
                    labJackDevice.Open(deviceSerial);
                    var NetworkSettings = labJackDevice.GetNetworkSettings();
                    labJackDevice.Close();

                    IpUsb = "";
                    Subnet = "";
                    Gateway = "";

                    IpUsb = NetworkSettings["ETHERNET_IP"];
                    Subnet = NetworkSettings["ETHERNET_SUBNET"];
                    Gateway = NetworkSettings["ETHERNET_GATEWAY"];

                    OnPropertyChanged(IpUsb);
                    OnPropertyChanged(Subnet);
                    OnPropertyChanged(Gateway);
                }
            );

        }

        [Display("Set DHCP.", Description: "Set dhcp", Order: 1.8)]
        [Browsable(true)]
        [EnabledIf(nameof(isUSB), true, HideIfDisabled = true)]
        public void SetDHCP()
        {
            labJackDevice.Open(deviceSerial);
            labJackDevice.SetDHCP();
            labJackDevice.Close();
        }

        public LabJackInstrument()
        {
            Name = "Labjack";
        }

        public override void Open()
        {
            try
            {
                var result = labJackDevice.Open(identifier);

                if (result == ERROR.NO_ERROR)
                   base.Open();
                else
                {
                    Log.Error("Failed to open device.");
                    throw new Exception(result.ToString());
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to open device.");
                throw new Exception(e.Message);
            }

        }

        public override void Close()
        {
            if(IsConnected)
            {
                labJackDevice.Close();
                base.Close();
            }
        }

        public void DigitalWrite(DIO channel, STATE value)
        {
            labJackDevice.DigitalWrite(channel, value);
        }

        public STATE DigitalRead(DIO channel)
        {
            return labJackDevice.DigitalRead(channel);
        }

        public void AnalogWrite(DAC channel, double value)
        {
            labJackDevice.AnalogWrite(channel, value);
        }

        public double AnalogRead(AIN channel)
        {
            return labJackDevice.AnalogRead(channel);
        }
        public double AnalogReadEnv(AIN channel)
        {
            return labJackDevice.AnalogReadEnv(channel);
        }
        public double AnalogReadMean(AIN channel)
        {
            return labJackDevice.AnalogReadMean(channel);
        }
        public double[] AnalogRead(AIN channel, int sampleRate, double sampleDuration)
        {
            return labJackDevice.AnalogRead(channel, sampleRate, sampleDuration);
        }

        public double AnalogRead(DAC channel)
        {
            return labJackDevice.AnalogRead(channel);
        }
    }
}
