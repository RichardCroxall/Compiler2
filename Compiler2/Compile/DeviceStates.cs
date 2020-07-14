/*
    \file DeviceStates.cs
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
using System.Text;
using compiler2.Generate;

namespace Compiler2.Compile
{
    public static class DeviceStates
    {
        public const int ON = (int) device_state_t.stateOn;
        public const int OFF = (int) device_state_t.stateOff;
        public const int DIM1 = (int) device_state_t.stateDim1;
        public const int DIM2 = (int) device_state_t.stateDim2;
        public const int DIM3 = (int) device_state_t.stateDim3;
        public const int DIM4 = (int) device_state_t.stateDim4;
        public const int DIM5 = (int) device_state_t.stateDim5;
        public const int DIM6 = (int) device_state_t.stateDim6;
        public const int DIM7 = (int) device_state_t.stateDim7;
        public const int DIM8 = (int) device_state_t.stateDim8;
        public const int DIM9 = (int) device_state_t.stateDim9;
        public const int DIM10 = (int) device_state_t.stateDim10;
        public const int DIM11 = (int) device_state_t.stateDim11;
        public const int DIM12 = (int) device_state_t.stateDim12;
        public const int DIM13 = (int) device_state_t.stateDim13;
        public const int DIM14 = (int) device_state_t.stateDim14;
        public const int DIM15 = (int) device_state_t.stateDim15;
        public const int DIM16 = (int) device_state_t.stateDim16;
        public const int DIM17 = (int) device_state_t.stateDim17;

        private static readonly Dictionary<string, int> DeviceStateName = new Dictionary<string, int>();

        static DeviceStates()
        {
            DeviceStateName.Add("ON", ON);
            DeviceStateName.Add("OFF", OFF);
            DeviceStateName.Add("DIM1", DIM1);
            DeviceStateName.Add("DIM2", DIM2);
            DeviceStateName.Add("DIM3", DIM3);
            DeviceStateName.Add("DIM4", DIM4);
            DeviceStateName.Add("DIM5", DIM5);
            DeviceStateName.Add("DIM6", DIM6);
            DeviceStateName.Add("DIM7", DIM7);
            DeviceStateName.Add("DIM8", DIM8);
            DeviceStateName.Add("DIM9", DIM9);
            DeviceStateName.Add("DIM10", DIM10);
            DeviceStateName.Add("DIM11", DIM11);
            DeviceStateName.Add("DIM12", DIM12);
            DeviceStateName.Add("DIM13", DIM13);
            DeviceStateName.Add("DIM14", DIM14);
            DeviceStateName.Add("DIM15", DIM15);
            DeviceStateName.Add("DIM16", DIM16);
            DeviceStateName.Add("DIM17", DIM17);

        }

        public static bool ContainsKey(string deviceStateName)
        {
            return DeviceStateName.ContainsKey(deviceStateName);
        }

        public static int DeviceStateValue(string deviceStateName)
        {
            return DeviceStateName[deviceStateName];
        }
    }
}
