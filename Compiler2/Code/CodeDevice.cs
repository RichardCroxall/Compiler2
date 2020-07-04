
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        private readonly CodeAction m_OffAction;
        private readonly CodeAction m_OnAction;

        public CodeDevice(int declarationLineNumber, int pass, deviceType_t deviceType, CodeRoom codeRoom, string identifier, char houseCode, int deviceCode, CodeAction /*ActionCode*/ offAction, CodeAction /*ActionCode*/ onAction)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdDevice)
        {
            Debug.Assert(identifier.Length <= 30);
            Debug.Assert(houseCode >= 'A' && houseCode <= 'P');
            Debug.Assert(deviceCode >= 1 && deviceCode <= 16);

            m_DeviceType = deviceType;
            m_CodeRoom = codeRoom;
            m_HouseCode = houseCode;
            m_DeviceCode = deviceCode;
            m_OffAction = offAction;
            m_OnAction = onAction;

            m_Entries.Add(this);
            m_NoEntries++;

            if (m_DeviceType == deviceType_t.devicePIRSensor)
            {
                this.NoteUsage();
            }
        }

        public CodeDevice(int declarationLineNumber, int pass, deviceType_t deviceType, CodeRoom codeRoom, string identifier, char houseCode, string macAddress, CodeAction /*ActionCode*/ offAction, CodeAction /*ActionCode*/ onAction)
            : base(declarationLineNumber, pass, identifier, m_NoEntries, IdentifierTypeEnum.IdDevice)
        {
            Debug.Assert(identifier.Length <= 30);
            Debug.Assert(macAddress.Length == 26);

            m_DeviceType = deviceType;
            m_CodeRoom = codeRoom;
            m_HouseCode = houseCode;
            m_MacAddress = macAddress;
            m_OffAction = offAction;
            m_OnAction = onAction;

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

        public CodeAction /*ActionCode*/ OffAction
        {
            get { return m_OffAction; }
        }


        public CodeAction /*ActionCode*/ OnAction
        {
            get { return m_OnAction; }
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
