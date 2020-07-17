/*
    \file CodeDevice.cs
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
using compiler2.Generate;

namespace compiler2.Code
{
    public class CodeDevice : CodeBase
    {
        static private int m_NoEntries = 0;
        static private List<CodeDevice> m_Entries = new List<CodeDevice>();

        public static int NoDeviceEntries
        {
            get { return CodeDevice.m_NoEntries; }
        }

        public static CodeDevice GetEntry(int index)
        {
            return m_Entries[index];
        }

        private readonly deviceType_t m_DeviceType;
        private readonly CodeRoom m_CodeRoom;
        private readonly char m_HouseCode;
        private readonly int m_DeviceCode;
        private readonly string m_MacAddress;
        private readonly CodeProcedure m_OffProcedure;
        private readonly CodeProcedure m_OnProcedure;

        public CodeDevice(int declarationLineNumber, int pass, deviceType_t deviceType, CodeRoom codeRoom, string identifier, char houseCode, int deviceCode, CodeProcedure /*ActionCode*/ offProcedure, CodeProcedure /*ActionCode*/ onProcedure)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdDevice, TypeEnum.DeviceType)
        {
            Debug.Assert(identifier.Length <= 30);
            Debug.Assert(houseCode >= 'A' && houseCode <= 'P');
            Debug.Assert(deviceCode >= 1 && deviceCode <= 16);

            m_DeviceType = deviceType;
            m_CodeRoom = codeRoom;
            m_HouseCode = houseCode;
            m_DeviceCode = deviceCode;
            m_OffProcedure = offProcedure;
            m_OnProcedure = onProcedure;

            m_Entries.Add(this);
            m_NoEntries++;

            if (m_DeviceType == deviceType_t.devicePIRSensor)
            {
                this.NoteUsage();
            }
        }

        public CodeDevice(int declarationLineNumber, int pass, deviceType_t deviceType, CodeRoom codeRoom, string identifier, char houseCode, string macAddress, CodeProcedure /*ActionCode*/ offProcedure, CodeProcedure /*ActionCode*/ onProcedure)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdDevice, TypeEnum.DeviceType)
        {
            Debug.Assert(identifier.Length <= 30);
            Debug.Assert(macAddress.Length == 26);

            m_DeviceType = deviceType;
            m_CodeRoom = codeRoom;
            m_HouseCode = houseCode;
            m_MacAddress = macAddress;
            m_OffProcedure = offProcedure;
            m_OnProcedure = onProcedure;

            m_Entries.Add(this);
            m_NoEntries++;

        }

        public deviceType_t DeviceType
        {
            get { return m_DeviceType; }
        }


        public CodeRoom CodeRoomValue
        {
            get { return m_CodeRoom; }
        }

        public char HouseCode
        {
            get
            {
                return m_HouseCode;
            }
        }

        public int DeviceCode
        {
            get
            {
                Debug.Assert(m_DeviceType != deviceType_t.deviceHueLamp);
                return m_DeviceCode;
            }
        }

        public CodeProcedure /*ActionCode*/ OffProcedure
        {
            get { return m_OffProcedure; }
        }


        public CodeProcedure /*ActionCode*/ OnProcedure
        {
            get { return m_OnProcedure; }
        }

        public string MacAddress
        {
            get
            {
                Debug.Assert(m_DeviceType == deviceType_t.deviceHueLamp);
                return m_MacAddress;
            }
        }
    
    }
}
