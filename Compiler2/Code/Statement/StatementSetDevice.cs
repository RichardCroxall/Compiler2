/*
    \file StatementSetDevice.cs
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
