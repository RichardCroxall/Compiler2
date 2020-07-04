using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using compiler2.Compile;
using compiler2.Generate;

namespace compiler2.Code.Statement
{
    class StatementSetDevice : StatementBase
    {
        private readonly CodeDevice[] m_CodeDevices;
        private readonly CodeDeviceCommands m_CodeDeviceCommands;
        private readonly TimeSpan m_DelayTimeSpan;
        private readonly TimeSpan m_DurationTimeSpan;

        public StatementSetDevice(CodeDevice[] codeDevices, CodeDeviceCommands codeDeviceCommands,  TimeSpan delayTimeSpan, TimeSpan durationTimeSpan)
            : base(TokenEnum.token_set_device)
        {
            //Debug.Assert(delayTimeSpan <= durationTimeSpan);
            m_CodeDevices = codeDevices;
            m_CodeDeviceCommands = codeDeviceCommands;
            m_DelayTimeSpan = delayTimeSpan;
            m_DurationTimeSpan = durationTimeSpan;
        }


        public CodeDevice[] CodeDeviceValues
        {
            get { return m_CodeDevices; }
        }

        public CodeDeviceCommands GetCodeDeviceCommands
        {
            get { return m_CodeDeviceCommands; }
        }

        public TimeSpan DelayTimeSpan { get { return m_DelayTimeSpan; } }
        public TimeSpan DurationTimeSpan { get { return m_DurationTimeSpan; } }
    }

}
