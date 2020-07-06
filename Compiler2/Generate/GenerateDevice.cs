/*
    \file GenerateDevice.cs
    Copyright Notice\n
    Copyright (C) 1995-2020 Richard Croxall - developer and architect\n
    Copyright (C) 1995-2020 Dr Sylvia Croxall - code reviewer and tester\n

    This file is part of Compiler2.

    Compiler2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Compiler2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Compiler2.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using compiler2.Compile;

namespace compiler2.Generate
{
    public enum device_state_t// used for X10 device state
    {
	     stateOff,
	     stateOn,
	     stateDim1,
	     stateDim2,
	     stateDim3,
	     stateDim4,
	     stateDim5,
	     stateDim6,
	     stateDim7,
	     stateDim8,
	     stateDim9,
	     stateDim10,
	     stateDim11,
	     stateDim12,
	     stateDim13,
	     stateDim14,
	     stateDim15,
	     stateDim16,
	     stateDim17,
	     stateUnknown
    }

    public enum device_code_t :short  // used to address an X-10 device within a house code
    {
	     UNIT1 = 0x0C,
	     UNIT2 = 0x1C,
	     UNIT3 = 0x04,
	     UNIT4 = 0x14,
	     UNIT5 = 0x02,
	     UNIT6 = 0x12,
	     UNIT7 = 0x0A,
	     UNIT8 = 0x1A,
	     UNIT9 = 0x0E,
	     UNIT10 = 0x1E,
	     UNIT11 = 0x06,
	     UNIT12 = 0x16,
	     UNIT13 = 0x00,
	     UNIT14 = 0x10,
	     UNIT15 = 0x08,
	     UNIT16 = 0x18
    }

    public enum house_code_t : short// used to address particular house codes
    {
	     HouseA = 0x06,
	     HouseB = 0x0E,
	     HouseC = 0x02,
	     HouseD = 0x0A,
	     HouseE = 0x01,
	     HouseF = 0x09,
	     HouseG = 0x05,
	     HouseH = 0x0D,
	     HouseI = 0x07,
	     HouseJ = 0x0F,
	     HouseK = 0x03,
	     HouseL = 0x0B,
	     HouseM = 0x00,
	     HouseN = 0x08,
	     HouseO = 0x04,
	     HouseP = 0x0C,
    } 

    [Flags]
    public enum dayEnum : short
    {
        DAY_SUN = (1<<0),
        DAY_MON = (1<<1),
        DAY_TUE = (1<<2),
        DAY_WED = (1<<3),
        DAY_THU = (1<<4),
        DAY_FRI = (1<<5),
        DAY_SAT = (1<<6),
        DAY_WORK = (1<<7),
        DAY_NONWORK = (1<<8),
        DAY_FIRSTWORK = (1<<9),
        DAY_NONFIRSTWORK = (1 << 12), //TODO add New feature to runtime

        // BST & GMT are mutually exclusive
        DAY_BST = (1<<10),
        DAY_GMT = (1<<11),
        DAY_SCHOOL_NIGHT = (1 << 12),
    }

    public enum deviceType_t : ushort
    {
	    deviceAppliance, //only off and on
        deviceLamp, //with 17 levels of brightness for incandescant lamps
        deviceApplianceLamp, //Lamp attached to Appliance module.
        deviceHueLamp, //Philips Hue Lamp connected via Philips hub.
        devicePIRSensor, //Passive Infra Red sensor which has two codes. One for darkness indicator, one for movement of a warm body
        deviceIRRemote, //device which generates infra red remote control messages identical to the normal handheld remote.
    };


    static class GenerateDevice
    {
        static Dictionary<TokenEnum, device_state_t> m_TokenDeviceStateDictionary = new Dictionary<TokenEnum,device_state_t>();
        static device_code_t[] m_DeviceCodeMap;
        static house_code_t[] m_HouseMap;
        static Dictionary<TokenEnum, deviceType_t> m_DeviceTypeDictionary = new Dictionary<TokenEnum, deviceType_t>();

        static GenerateDevice()
        {
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_on, device_state_t.stateOn);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_off, device_state_t.stateOff);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim1, device_state_t.stateDim1);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim2, device_state_t.stateDim2);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim3, device_state_t.stateDim3);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim4, device_state_t.stateDim4);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim5, device_state_t.stateDim5);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim6, device_state_t.stateDim6);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim7, device_state_t.stateDim7);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim8, device_state_t.stateDim8);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim9, device_state_t.stateDim9);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim10, device_state_t.stateDim10);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim11, device_state_t.stateDim11);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim12, device_state_t.stateDim12);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim13, device_state_t.stateDim13);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim14, device_state_t.stateDim14);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim15, device_state_t.stateDim15);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim16, device_state_t.stateDim16);
            m_TokenDeviceStateDictionary.Add(TokenEnum.token_dim17, device_state_t.stateDim17);

            m_HouseMap = new house_code_t[16]
            {
                house_code_t.HouseA, house_code_t.HouseB, house_code_t.HouseC, house_code_t.HouseD,
                house_code_t.HouseE, house_code_t.HouseF, house_code_t.HouseG, house_code_t.HouseH,
                house_code_t.HouseI, house_code_t.HouseJ, house_code_t.HouseK, house_code_t.HouseL,
                house_code_t.HouseM, house_code_t.HouseN, house_code_t.HouseO, house_code_t.HouseP,
            };

            m_DeviceCodeMap = new device_code_t[16]
            {
                device_code_t.UNIT1, device_code_t.UNIT2, device_code_t.UNIT3, device_code_t.UNIT4,
                device_code_t.UNIT5, device_code_t.UNIT6, device_code_t.UNIT7, device_code_t.UNIT8,
                device_code_t.UNIT9, device_code_t.UNIT10, device_code_t.UNIT11, device_code_t.UNIT12,
                device_code_t.UNIT13, device_code_t.UNIT14, device_code_t.UNIT15, device_code_t.UNIT16
            };

            m_DeviceTypeDictionary.Add(TokenEnum.token_appliance, deviceType_t.deviceAppliance);
            m_DeviceTypeDictionary.Add(TokenEnum.token_lamp, deviceType_t.deviceLamp);
            m_DeviceTypeDictionary.Add(TokenEnum.token_applianceLamp, deviceType_t.deviceApplianceLamp);
            m_DeviceTypeDictionary.Add(TokenEnum.token_hueLamp, deviceType_t.deviceHueLamp);
            m_DeviceTypeDictionary.Add(TokenEnum.token_sensor, deviceType_t.devicePIRSensor);
            m_DeviceTypeDictionary.Add(TokenEnum.token_remote, deviceType_t.deviceIRRemote);
        }

        public static house_code_t MapHouseCode(char houseCode)
        {
            Debug.Assert(houseCode >= 'A' && houseCode <= 'P');
            return m_HouseMap[(int)(houseCode - 'A')];
        }

        public static device_code_t MapDeviceCode(int deviceCode)
        {
            Debug.Assert(deviceCode >= 1 && deviceCode <=16);
            return m_DeviceCodeMap[deviceCode - 1];
        }
        public static deviceType_t MapDevice(TokenEnum tokenEnum)
        {
            return m_DeviceTypeDictionary[tokenEnum];
        }

        public static bool MapDeviceStateContainsKey(TokenEnum tokenEnum)
        {
            return m_TokenDeviceStateDictionary.ContainsKey(tokenEnum);
        }

        public static device_state_t MapDeviceState(TokenEnum tokenEnum)
        {
            return m_TokenDeviceStateDictionary[tokenEnum];
        }
    }
}
