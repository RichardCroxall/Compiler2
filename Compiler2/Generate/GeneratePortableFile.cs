/*
    \file GeneratePortableFile.cs
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
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Compiler.Code.Statement;
using compiler2.Code;
using compiler2.Code.ExpressValue;
using compiler2.Code.Statement;
using compiler2.Compile;


namespace compiler2.Generate
{

    class GeneratePortableFile
    {
        public const int MaxRuntimeIdentifierLength = 30;
        const string MagicCode = "SmartCode";
        const int NullAction = (-1);


        enum codeActionItem_t : ushort
        {
            Stop,
            PushDeviceState, //parameters housecode, devicecode. current device state left on stack
            SetDeviceState, //parameters housecode, devicecode, new device state, delay time, duration time
            GetTimeoutState,//parameter timeout entry no
            ResetTimeout,//parameter timeout entry no
            AbsoluteInteger, //on stack
            DecrementInteger, //on stack
            IncrementInteger, //on stack
            Add2Integers, //2 integers from on stack, result back on stack
            Subtract2Integers, //2 integers from on stack, result back on stack
            Multiply2Integers, //2 integers from on stack, result back on stack
            Divide2Integers, //2 integers from on stack, result back on stack
            Modulus2Integers, //2 integers from on stack, result back on stack
            NegateInteger, //on stack
            Compare2IntegersLessThan, //2 integers from on stack, result back on stack
            Compare2IntegersLessThanEqual, //2 integers from on stack, result back on stack
            Compare2IntegersEqual, //2 integers from on stack, result back on stack
            Compare2IntegersGreaterThanEqual, //2 integers from on stack, result back on stack
            Compare2IntegersGreatThan, //2 integers from on stack, result back on stack
            Compare2IntegersNotEqual, //2 integers from on stack, result back on stack
            NotBoolean, //on stack
            Or2Booleans,//2 boolean values from on stack, result back on stack
            And2Booleans,//2 boolean values from on stack, result back on stack
            PushConstant,//Fixed value left stack
            PushVariableValue,//value of variable left stack
            StoreVariable,//1 value taken from stack, result stored in variable
            JumpUnconditional,
            JumpIfTrue,//1 value taken from stack, 
            JumpIfFalse,//1 value taken from stack, 
            CallSystemProcedure,//1 parameter is entry number of system procedure
            CallUserProcedure,//1 parameter is entry number of user procedure
            ReturnFromUserProcedure,//return to previous calling user procedure.
            SetRefreshDevices, //send X-10 codes to refresh all devices.
            SetResynchClock, //Bring internal clock instep with host's realtime clock.
        };

        static readonly Dictionary<UnaryOperator, codeActionItem_t> m_UnaryOperatorDictionary = new Dictionary<UnaryOperator, codeActionItem_t>();
        static readonly Dictionary<BinaryOperator, codeActionItem_t> m_BinaryOperatorDictionary = new Dictionary<BinaryOperator, codeActionItem_t>();

        static GeneratePortableFile()
        {
            m_UnaryOperatorDictionary.Add(UnaryOperator.Not, codeActionItem_t.NotBoolean);
            m_UnaryOperatorDictionary.Add(UnaryOperator.Negate, codeActionItem_t.NegateInteger);

            m_BinaryOperatorDictionary.Add(BinaryOperator.Plus, codeActionItem_t.Add2Integers);
            m_BinaryOperatorDictionary.Add(BinaryOperator.Minus, codeActionItem_t.Subtract2Integers);
            m_BinaryOperatorDictionary.Add(BinaryOperator.Times, codeActionItem_t.Multiply2Integers);
            m_BinaryOperatorDictionary.Add(BinaryOperator.Div, codeActionItem_t.Divide2Integers);
            m_BinaryOperatorDictionary.Add(BinaryOperator.Mod, codeActionItem_t.Modulus2Integers);
            m_BinaryOperatorDictionary.Add(BinaryOperator.LessThan, codeActionItem_t.Compare2IntegersLessThan);
            m_BinaryOperatorDictionary.Add(BinaryOperator.LessThanEqual, codeActionItem_t.Compare2IntegersLessThanEqual);
            m_BinaryOperatorDictionary.Add(BinaryOperator.Equal, codeActionItem_t.Compare2IntegersEqual);
            m_BinaryOperatorDictionary.Add(BinaryOperator.GreaterThanEquals, codeActionItem_t.Compare2IntegersGreaterThanEqual);
            m_BinaryOperatorDictionary.Add(BinaryOperator.GreaterThan, codeActionItem_t.Compare2IntegersGreatThan);
            m_BinaryOperatorDictionary.Add(BinaryOperator.NotEqual, codeActionItem_t.Compare2IntegersNotEqual);
            m_BinaryOperatorDictionary.Add(BinaryOperator.LogicalOr, codeActionItem_t.Or2Booleans);
            m_BinaryOperatorDictionary.Add(BinaryOperator.LogicalAnd, codeActionItem_t.And2Booleans);
        }


        private StreamWriter m_StreamWriter;
        private readonly CodeCalendar m_CodeCalendar;


        public GeneratePortableFile(CodeCalendar codeCalendar)
        {
            m_CodeCalendar = codeCalendar;
        }

        private string LimitIdentifierLength(string identifier)
        {
            return identifier.Length <= MaxRuntimeIdentifierLength
                ? identifier
                : identifier.Substring(0, MaxRuntimeIdentifierLength);
        }

        private void WriteHeader()
        {
            m_StreamWriter.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                MagicCode,
                CodeCalendar.NoCalendarEntries,
                CodeHouseCode.NoHouseCodeEntries,
                CodeDevice.NoDeviceEntries,
                CodeFlag.NoFlagEntries,
                CodeTimer.NoTimerEntries,
                CodeAction.NoActionEntries,
                CodeTimeout.NoTimeoutEntries,
                CodeCalendar.NoCalendarEntries + CodeHouseCode.NoHouseCodeEntries + CodeDevice.NoDeviceEntries + CodeFlag.NoFlagEntries + CodeTimer.NoTimerEntries + CodeAction.NoActionEntries + CodeTimeout.NoTimeoutEntries);
        }

        private void WriteCalendarEntries()
        {
            DateTime unixStartDateTime = new DateTime(1970, 1, 1, 0, 0, 0);

            for (int entry = 0; entry < CodeCalendar.NoCalendarEntries; entry++)
            {
                CodeCalendar.CalendarEntry calendarEntry = m_CodeCalendar.getCalendarEntry(entry);
                m_StreamWriter.WriteLine("{0} {1} {2} {3}",
                    ((calendarEntry.day.Ticks - unixStartDateTime.Ticks)/TimeSpan.TicksPerSecond),
                    calendarEntry.sunRise,
                    calendarEntry.sunSet,
                    (int) calendarEntry.dayEnum);
            }

        }

        private void WriteHouseCodeEntries()
        {
            for (int entry = 0; entry < CodeHouseCode.NoHouseCodeEntries; entry++)
            {
                CodeHouseCode codeHouseCode = CodeHouseCode.GetEntry(entry);
                m_StreamWriter.WriteLine("{0} {1} {2} {3}",
                    (int)GenerateDevice.MapHouseCode(codeHouseCode.HouseCode),
                    codeHouseCode.OffAction.EntryNo,
                    codeHouseCode.OnAction.EntryNo,
                    LimitIdentifierLength(codeHouseCode.Name));
            }
        }

        private void WriteDeviceEntries()
        {
            const string dummyMacAddress = "00:00:00:00:00:00:00:00-00";

            for (int entry = 0; entry < CodeDevice.NoDeviceEntries; entry++)
            {
                CodeDevice codeDevice = CodeDevice.GetEntry(entry);
                switch (codeDevice.DeviceType)
                {
                    case deviceType_t.deviceHueLamp:
                        m_StreamWriter.WriteLine("{0} {1} {2} {3} {4} {5} {6}.{7}",
                            (int)codeDevice.DeviceType,
                            (int) GenerateDevice.MapHouseCode(codeDevice.HouseCode),
                            "-1", //Dummy DeviceCode
                        codeDevice.OffAction == null ? NullAction : codeDevice.OffAction.EntryNo,
                        codeDevice.OnAction == null ? NullAction : codeDevice.OnAction.EntryNo,
                        codeDevice.MacAddress,
                        codeDevice.CodeRoomValue.Identifier,
                        LimitIdentifierLength(codeDevice.Identifier));
                        break;

                    case deviceType_t.deviceAppliance:
                    case deviceType_t.deviceApplianceLamp:
                    case deviceType_t.deviceIRRemote:
                    case deviceType_t.deviceLamp:
                    case deviceType_t.devicePIRSensor:
                        m_StreamWriter.WriteLine("{0} {1} {2} {3} {4} {5} {6}.{7}",
                            (int)codeDevice.DeviceType,
                            (int) GenerateDevice.MapHouseCode(codeDevice.HouseCode),
                            (int) GenerateDevice.MapDeviceCode(codeDevice.DeviceCode),
                            codeDevice.OffAction == null ? NullAction : codeDevice.OffAction.EntryNo,
                            codeDevice.OnAction == null ? NullAction : codeDevice.OnAction.EntryNo,
                            dummyMacAddress,
                            codeDevice.CodeRoomValue.Identifier,
                            LimitIdentifierLength(codeDevice.Identifier));
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        private void WriteFlagEntries()
        {
            for (int entry = 0; entry < CodeFlag.NoFlagEntries; entry++)
            {
                CodeFlag codeFlag = CodeFlag.GetEntry(entry);
                m_StreamWriter.WriteLine("{0} {1}",
                    codeFlag.InitialValue,
                    LimitIdentifierLength(codeFlag.Identifier));
            }
        }

        private void WriteEvent(CodeEvent codeEvent)
        {
            m_StreamWriter.WriteLine("{0} {1}",
                codeEvent.CodeActionValue.EntryNo,
                (int) (codeEvent.TimeSpanValue.Ticks / TimeSpan.TicksPerSecond));
        }

        private void WriteSequence(CodeSequence codeSequence)
        {
            m_StreamWriter.WriteLine("{0} {1} {2} {3}",
                                     codeSequence.EventList.Count,
                                     (int) (codeSequence.TimeSpan.Ticks/TimeSpan.TicksPerSecond), //sequence fire time
                                     (int) codeSequence.DaysToFire,
                                     LimitIdentifierLength(codeSequence.Description));

            foreach (CodeEvent codeEvent in codeSequence.EventList)
            {
                WriteEvent(codeEvent);
            }
        }

        private void WriteTimer(CodeTimer codeTimer)
        {
            m_StreamWriter.WriteLine("{0} {1}",
                codeTimer.SequenceList.Count,
                LimitIdentifierLength(codeTimer.Identifier));

            foreach (CodeSequence codeSequence in codeTimer.SequenceList)
            {
                WriteSequence(codeSequence);
            }
        }

        private void WriteTimerEntries()
        {
            for (int entry = 0; entry < CodeTimer.NoTimerEntries; entry++)
            {
                WriteTimer(CodeTimer.GetEntry(entry));
           }
        }

        private void WriteActionEntries()
        {
            for (int entry = 0; entry < CodeAction.NoActionEntries; entry++)
            {
                CodeAction codeAction = CodeAction.GetEntry(entry);

                m_StreamWriter.WriteLine("{0} {1} {2}",
                    InstructionCount(codeAction.StatementList),
                    ProgramWordCount(codeAction.StatementList),
                    LimitIdentifierLength(codeAction.Identifier));

                if (codeAction != null)
                {
                    WriteStatementList(codeAction.StatementList);
                }
            }
        }

        private void WriteCodeActionPush(Value value)
        {
            if (value != null)
            {
                switch (value.ValueType)
                {
                    case SimpleValueType.Variable:
                        m_StreamWriter.WriteLine("{0} {1}", (int) codeActionItem_t.PushVariableValue, value.CodeFlag.EntryNo); //push value from variable
                        break;

                    case SimpleValueType.SimpleConstant:
                        m_StreamWriter.WriteLine("{0} {1}", (int) codeActionItem_t.PushConstant, value.IntegerValue); //push value from parameter
                        break;

                    case SimpleValueType.Device:
                    {
                        CodeDevice codeDevice = value.CodeDevice;
                        m_StreamWriter.WriteLine("{0} {1}", (int) codeActionItem_t.PushDeviceState, value.CodeDevice.EntryNo); //push value from parameter
                        break;
                    }

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
            else
            {
                Console.WriteLine(string.Format("push NULL"));
            }
        }

        private void WriteCodeActionUnaryOperator(UnaryOperator unaryOperator)
        {
            m_StreamWriter.WriteLine("{0}", (int) m_UnaryOperatorDictionary[unaryOperator]); //map unary operator to VM instruction set
        }

        private void WriteCodeActionBinaryOperator(BinaryOperator binaryOperator)
        {
            m_StreamWriter.WriteLine("{0}", (int)m_BinaryOperatorDictionary[binaryOperator]); //map binary operator to VM instruction set
        }


        private void WriteCodeActionExpression(Expression expression)
        {
            switch (expression.ExpressionType)
            {
                case ExpressionType.SimpleValue:
                    WriteCodeActionPush(expression.Value);
                    break;

                case ExpressionType.UnaryExpression:
                    WriteCodeActionExpression(expression.Expression1);
                    WriteCodeActionUnaryOperator(expression.UnaryOperator);
                    break;

                case ExpressionType.BinaryExpression:
                    WriteCodeActionExpression(expression.Expression1);
                    WriteCodeActionExpression(expression.Expression2);
                    WriteCodeActionBinaryOperator(expression.BinaryOperator);
                    break;
            }
        }

        private void WriteCodeActionItemIfRecord(Expression expression, int skipCount)
        {
            WriteCodeActionExpression(expression); //leaves single integer/boolean value on stack

            m_StreamWriter.WriteLine("{0} {1}",
                    (int)codeActionItem_t.JumpIfFalse,
                    skipCount);
        }

        private void WriteCodeActionItemGotoRecord(int skipCount)
        {
            m_StreamWriter.WriteLine("{0} {1}",
                    (int)codeActionItem_t.JumpUnconditional,
                    skipCount);
        }

        

        private void WriteCodeActionItemRecord(StatementSetDevice statementDevice)
        {
            foreach (CodeDevice codeDevice in statementDevice.CodeDeviceValues)
            {
                switch (codeDevice.DeviceType)
                {
                    case deviceType_t.deviceAppliance:
                        Debug.Assert(statementDevice.GetCodeDeviceCommands.GetDeviceState == device_state_t.stateOff ||
                                     statementDevice.GetCodeDeviceCommands.GetDeviceState == device_state_t.stateOn);
                        break;

                    case deviceType_t.deviceApplianceLamp:
                        if (statementDevice.GetCodeDeviceCommands.GetDeviceState != device_state_t.stateOff &&
                            statementDevice.GetCodeDeviceCommands.GetDeviceState != device_state_t.stateOn)
                        {
                            statementDevice.GetCodeDeviceCommands.SetDeviceState(device_state_t.stateOn);
                        }
                        break;

                    case deviceType_t.deviceHueLamp:
                        break;

                    case deviceType_t.deviceLamp:
                        break;

                    case deviceType_t.deviceIRRemote:
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
                m_StreamWriter.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                    (int)codeActionItem_t.SetDeviceState,
                    codeDevice.EntryNo,
                    (int)statementDevice.GetCodeDeviceCommands.GetDeviceState,
                    statementDevice.GetCodeDeviceCommands.GetColour,
                    statementDevice.GetCodeDeviceCommands.GetColourLoop ? "1" : "0",
                    (int)statementDevice.DelayTimeSpan.TotalSeconds,
                    (int)statementDevice.DurationTimeSpan.TotalSeconds);
            }
        }

        private void WriteCodeActionItemRecord(StatementRefreshDevices statementRefreshDevices)
        {
            m_StreamWriter.WriteLine("{0}",
                (int)codeActionItem_t.SetRefreshDevices);

        }

        private void WriteCodeActionItemRecord(StatementResynchClock statementResynchClock)
        {
            m_StreamWriter.WriteLine("{0}",
                (int)codeActionItem_t.SetResynchClock);

        }

        private void WriteCodeActionItemRecord(StatementResetTimeout statementResetTimeout)
        {
            m_StreamWriter.WriteLine("{0} {1} {2}",
                (int)codeActionItem_t.ResetTimeout,
                statementResetTimeout.CodeTimeoutValue.EntryNo,
                (int)statementResetTimeout.TimeSpanValue.TotalSeconds);
        }


        private void WriteCodeActionItemRecord(StatementAssignment statementAssignment)
        {
            WriteCodeActionExpression(statementAssignment.Expression); //leaves single integer/boolean value on stack

            m_StreamWriter.WriteLine("{0} {1}",
                (int)codeActionItem_t.StoreVariable,
                statementAssignment.CodeFlag.EntryNo);
        }

        private void WriteCodeActionItemRecord(StatementCall statementCall)
        {
            m_StreamWriter.WriteLine("{0} {1}",
                (int)codeActionItem_t.CallUserProcedure,
                statementCall.CodeActionValue.EntryNo);
        }

        //

        private int InstructionCount(Value value)
        {
            int count = 0;
            if (value != null)
            {
                switch (value.ValueType)
                {
                    case SimpleValueType.Variable:
                        count = 1; //needs 1 instruction to push a variable
                        break;

                    case SimpleValueType.SimpleConstant:
                        count = 1; //needs 1 instruction to push a constant value
                        break;

                    case SimpleValueType.Device:
                        count = 1; //needs 1 instruction to push a device state
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
            else
            {
                Debug.Assert(false);
            }
            return count;
        }

        private int InstructionCount(UnaryOperator unaryOperator)
        {
            return 1;//a binary operator needs just one instruction
        }

        private int InstructionCount(BinaryOperator binaryOperator)
        {
            return 1;//a binary operator needs just one instruction
        }


        private int InstructionCount(Expression expression)
        {
            int count = 0;
            switch (expression.ExpressionType)
            {
                case ExpressionType.SimpleValue:
                    count = InstructionCount(expression.Value);
                    break;

                case ExpressionType.UnaryExpression:
                    count = InstructionCount(expression.Expression1) + InstructionCount(expression.UnaryOperator);
                    break;

                case ExpressionType.BinaryExpression:
                    count = InstructionCount(expression.Expression1) + InstructionCount(expression.Expression2) + InstructionCount(expression.BinaryOperator);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            return count;
        }

        private int InstructionCount(List<StatementBase> statementList)
        {
            int instructionCount = 0;
            if (statementList != null)
            {
                foreach (StatementBase statementBase in statementList)
                {
                    switch (statementBase.TokenEnumValue)
                    {
                        case TokenEnum.token_if:
                            {
                                StatementIf statementIf = (StatementIf)statementBase;
                                instructionCount += InstructionCount(statementIf.Condition);
                                instructionCount++;
                                instructionCount += InstructionCount(statementIf.ThenStatementList);

                                if (statementIf.ElseStatementList != null)
                                {
                                    instructionCount++;
                                    instructionCount += InstructionCount(statementIf.ElseStatementList);
                                }
                            }
                            break;

                        case TokenEnum.token_equals: //assignment to variable
                            {
                                StatementAssignment statementAssignment = (StatementAssignment) statementBase;
                                instructionCount += InstructionCount(statementAssignment.Expression);
                                instructionCount++;
                            }
                            break;

                        case TokenEnum.token_call_action:
                            {
                                instructionCount++;
                            }
                            break;

                        case TokenEnum.token_set_device:
                            {
                                StatementSetDevice statementSetDevice = (StatementSetDevice) statementBase;
                                instructionCount += statementSetDevice.CodeDeviceValues.Length;
                            }
                            break;

                        case TokenEnum.token_refreshDevices:
                        case TokenEnum.token_resynchClock:
                        case TokenEnum.token_reset:
                            {
                                instructionCount++;
                            }
                            break;


                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
            }
            return instructionCount;
        }


        private int ProgramWordCount(Value value)
        {
            int count = 0;
            if (value != null)
            {
                switch (value.ValueType)
                {
                    case SimpleValueType.Variable:
                        count = 2;//needs 2 programWord to push a variable
                        break;

                    case SimpleValueType.SimpleConstant:
                        count = 2; //needs 1 programWord to push a constant value
                        break;

                    case SimpleValueType.Device:
                        count = 2; //needs 1 programWord to push a device state
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
            else
            {
                Debug.Assert(false);
            }
            return count;
        }

        private int ProgramWordCount(UnaryOperator unaryOperator)
        {
            return 1;//a binary operator needs just one programWord
        }

        private int ProgramWordCount(BinaryOperator binaryOperator)
        {
            return 1;//a binary operator needs just one programWord
        }


        private int ProgramWordCount(Expression expression)
        {
            int count = 0;
            switch (expression.ExpressionType)
            {
                case ExpressionType.SimpleValue:
                    count = ProgramWordCount(expression.Value);
                    break;

                case ExpressionType.UnaryExpression:
                    count = ProgramWordCount(expression.Expression1) + ProgramWordCount(expression.UnaryOperator);
                    break;

                case ExpressionType.BinaryExpression:
                    count = ProgramWordCount(expression.Expression1) + ProgramWordCount(expression.Expression2) + ProgramWordCount(expression.BinaryOperator);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            return count;
        }

        private int ProgramWordCount(List<StatementBase> statementList)
        {
            int programWordCount = 0;
            if (statementList != null)
            {
                foreach (StatementBase statementBase in statementList)
                {
                    switch (statementBase.TokenEnumValue)
                    {
                        case TokenEnum.token_if:
                            {
                                StatementIf statementIf = (StatementIf)statementBase;
                                programWordCount += ProgramWordCount(statementIf.Condition);
                                programWordCount += 2; //if false conditional jump + offset.
                                programWordCount += ProgramWordCount(statementIf.ThenStatementList);

                                if (statementIf.ElseStatementList != null)
                                {
                                    programWordCount += 2; //if  unconditional jump + offset.
                                    programWordCount += ProgramWordCount(statementIf.ElseStatementList);
                                }
                            }
                            break;

                        case TokenEnum.token_equals: //assignment to variable
                            {
                                StatementAssignment statementAssignment = (StatementAssignment)statementBase;
                                programWordCount += ProgramWordCount(statementAssignment.Expression);
                                programWordCount += 2; //including variable location
                            }
                            break;

                        case TokenEnum.token_call_action:
                            {
                                programWordCount+=2; //including routine number
                            }
                            break;

                        case TokenEnum.token_set_device:
                            {
                                StatementSetDevice statementSetDevice = (StatementSetDevice)statementBase;
                                programWordCount += statementSetDevice.CodeDeviceValues.Length * 7; //set device, device no, device state, colour, colourLoop, delay, duration, 
                            }
                            break;

                        case TokenEnum.token_refreshDevices:
                        {
                            StatementRefreshDevices statementRefreshDevices = (StatementRefreshDevices) statementBase;
                            programWordCount += 1; //refresh devices, 
                        }
                            break;

                        case TokenEnum.token_resynchClock:
                        {
                            StatementResynchClock statementResynchClock = (StatementResynchClock)statementBase;
                            programWordCount += 1; //refresh devices, 
                        }
                            break;

                        case TokenEnum.token_reset:
                            {
                                programWordCount += 3; //including timeout number and duration.
                            }
                            break;


                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
            }
            return programWordCount;
        }




        private void WritePush(Value value)
        {
            if (value != null)
            {
                switch (value.ValueType)
                {
                    case SimpleValueType.Variable:
                        Console.WriteLine(string.Format("PUSH variable {0}", value.CodeFlag.EntryNo));
                        break;

                    case SimpleValueType.SimpleConstant:
                        Console.WriteLine(string.Format("PUSH {0}", value.IntegerValue));
                        break;

                    case SimpleValueType.Device:
                        {
                            CodeDevice codeDevice = value.CodeDevice;
                            Console.WriteLine(string.Format("PUSH device housecode {0}, devicecode{1}", codeDevice.HouseCode, codeDevice.DeviceCode));
                        }
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
            else
            {
                Console.WriteLine(string.Format("push NULL"));
            }
        }

        private void WriteUnaryOperator(UnaryOperator unaryOperator)
        {
            Console.WriteLine(string.Format("Unary OP {0}", unaryOperator.ToString()));
        }

        private void WriteBinaryOperator(BinaryOperator binaryOperator)
        {
            Console.WriteLine(string.Format("Binary OP {0}", binaryOperator.ToString()));
        }


        private void WriteExpressionEvaluation(Expression expression)
        {
            switch (expression.ExpressionType)
            {
                case ExpressionType.SimpleValue:
                    WritePush(expression.Value);
                    break;

                case ExpressionType.UnaryExpression:
                    WriteExpressionEvaluation(expression.Expression1);
                    WriteUnaryOperator(expression.UnaryOperator);
                    break;

                case ExpressionType.BinaryExpression:
                    WriteExpressionEvaluation(expression.Expression1);
                    WriteExpressionEvaluation(expression.Expression2);
                    WriteBinaryOperator(expression.BinaryOperator);
                   break;

            }
        }
        private void WriteStatementList(List<StatementBase> statementList)
        {
            if (statementList != null)
            {
                foreach (StatementBase statementBase in statementList)
                {
                    switch (statementBase.TokenEnumValue)
                    {
                        case TokenEnum.token_equals:
                        {
                            StatementAssignment statementAssignment = (StatementAssignment)statementBase;
                            WriteCodeActionItemRecord(statementAssignment);
                            break;
                        }

                        case TokenEnum.token_call_action:
                        {
                            StatementCall statementCall = (StatementCall)statementBase;
                            WriteCodeActionItemRecord(statementCall);
                            break;
                        }

                        case TokenEnum.token_if:
                        {
                            StatementIf statementIf = (StatementIf)statementBase;
                            Expression condExpression = statementIf.Condition;

                            int elseCaseSkipByActionItems = ProgramWordCount(statementIf.ThenStatementList);

                            // allow 1 extra to skip over the 'ELSE' goto
                            if (statementIf.ElseStatementList != null)
                            {
                                elseCaseSkipByActionItems += 2; //skip over next jump unconditional
                            }
                            WriteCodeActionItemIfRecord(statementIf.Condition, elseCaseSkipByActionItems);
                            WriteStatementList(statementIf.ThenStatementList);

                            if (statementIf.ElseStatementList != null)
                            {
                                // write goto to skip around ELSE case.
                                WriteCodeActionItemGotoRecord(ProgramWordCount(statementIf.ElseStatementList));

                                WriteStatementList(statementIf.ElseStatementList);
                            }
                            break;
                        }

                        case TokenEnum.token_set_device:
                        {
                            StatementSetDevice statementSetDevice = (StatementSetDevice)statementBase;
                            WriteCodeActionItemRecord(statementSetDevice);
                            break;
                        }

                        case TokenEnum.token_refreshDevices:
                        {
                            StatementRefreshDevices statementRefreshDevices = (StatementRefreshDevices) statementBase;
                            WriteCodeActionItemRecord(statementRefreshDevices);
                            break;
                        }

                        case TokenEnum.token_resynchClock:
                        {
                            StatementResynchClock statementResynchClock = (StatementResynchClock)statementBase;
                            WriteCodeActionItemRecord(statementResynchClock);
                            break;
                        }
                        case TokenEnum.token_reset:
                        {
                            StatementResetTimeout statementResetTimeout = (StatementResetTimeout)statementBase;
                            WriteCodeActionItemRecord(statementResetTimeout);
                            break;
                        }

                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
            }
        }


        private void WriteTimeoutEntries()
        {
            for (int entry = 0; entry < CodeTimeout.NoTimeoutEntries; entry++)
            {
                CodeTimeout timeout = CodeTimeout.GetEntry(entry);
                m_StreamWriter.WriteLine("{0} {1} {2}",
                    (int) timeout.DefaultDurationTimeSpan.TotalSeconds,
                    timeout.OffAction == null ? NullAction : timeout.OffAction.EntryNo,
                    LimitIdentifierLength(timeout.Identifier));
            }
        }


        private void WriteEndMarker()
        {
            m_StreamWriter.WriteLine("****************");
        }

        public void WriteRuntimeFile()
        {
            m_StreamWriter = new System.IO.StreamWriter(File.Open(@"D:\usr\richard\projects\Smart8r\Smart8r\smart.smt", FileMode.Create));
            WriteHeader();
            WriteCalendarEntries(); WriteEndMarker();
            WriteHouseCodeEntries(); WriteEndMarker();
            WriteDeviceEntries(); WriteEndMarker();
            WriteFlagEntries(); WriteEndMarker();
            WriteTimerEntries(); WriteEndMarker();
            WriteActionEntries(); WriteEndMarker();
            WriteTimeoutEntries(); WriteEndMarker();
            m_StreamWriter.Close();
        }
    }
}
