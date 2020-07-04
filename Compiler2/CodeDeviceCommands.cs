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
