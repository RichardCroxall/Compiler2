/*
    \file CodeDeviceCommands.cs
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
using compiler2.Generate;
using System;
using System.Diagnostics;

namespace compiler2.Code
{
    public class CodeDeviceCommands
    {
        private device_state_t device_state = device_state_t.stateUnknown;
        private int colour = 0;
        private bool colourLoop = false;

        public CodeDeviceCommands()
        {
        }

        public void SetDeviceState(device_state_t deviceState)
        {
            Debug.Assert(deviceState >= device_state_t.stateOff);
            Debug.Assert(deviceState <= device_state_t.stateDim17);
            device_state = deviceState;
        }

        public void SetColour(int colourParam)
        {
            Debug.Assert(colourParam > 0 && colourParam <= 0xFFFFFF);
            colour = colourParam;
        }

        public void SetColourLoop()
        {
            colourLoop = true;
        }

        public device_state_t GetDeviceState
        {
            get { return device_state; }
        }

        public int GetColour 
        {
            get { return colour; }
        }

        public bool GetColourLoop
        {
            get { return colourLoop; }
        }
    }
}
