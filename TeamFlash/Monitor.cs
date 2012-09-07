﻿using System;
using System.Text;
using System.Threading;

namespace TeamFlash
{
    class Monitor
    {
        public void SetLed(byte led, bool turnItOn, bool flashIt)
        {
            SetLed(led, turnItOn, flashIt, null, false);
        }

        public void SetLed(byte led, bool turnItOn, bool flashIt, int? flashDurationInSeconds)
        {
            SetLed(led, turnItOn, flashIt, flashDurationInSeconds, false);
        }

        public void SetLed(byte led, bool turnItOn, bool flashIt, int? flashDurationInSeconds, bool turnOffAfterFlashing)
        {
            var hUsb = GetDelcomDeviceHandle(); // open the device
            if (hUsb == 0) return;
            if (turnItOn)
            {
                if (flashIt)
                {
                    DelcomUsbLight.DelcomLEDControl(hUsb, led, DelcomUsbLight.LEDFLASH);
                    if (flashDurationInSeconds.HasValue)
                    {
                        Thread.Sleep(flashDurationInSeconds.Value * 1000);
                        var ledStatus = turnOffAfterFlashing ? DelcomUsbLight.LEDOFF : DelcomUsbLight.LEDON;
                        DelcomUsbLight.DelcomLEDControl(hUsb, led, ledStatus);
                    }
                }
                else
                {
                    DelcomUsbLight.DelcomLEDControl(hUsb, led, DelcomUsbLight.LEDON);
                }
            }
            else
            {
                DelcomUsbLight.DelcomLEDControl(hUsb, led, DelcomUsbLight.LEDOFF);
            }
            DelcomUsbLight.DelcomCloseDevice(hUsb);
        }

        readonly StringBuilder deviceName = new StringBuilder(DelcomUsbLight.MAXDEVICENAMELEN);

        uint GetDelcomDeviceHandle()
        {
            if (string.IsNullOrEmpty(deviceName.ToString()))
            {
                // Search for the first match USB device, For USB IO Chips use USBIODS
                var devicesFound = DelcomUsbLight.DelcomGetNthDevice(0, 0, deviceName);

                if (devicesFound == 0)
                    Console.WriteLine("Can't find build light device, or it's in use by another program");
            }

            var hUsb = DelcomUsbLight.DelcomOpenDevice(deviceName, 0); // open the device
            return hUsb;
        }
    }
}
