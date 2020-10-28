/*
    \file SyntaxAnalyser.cs
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
using System.Security.Cryptography;
using System.Text;
using Compiler.Code.Statement;
using compiler2.Code;
using compiler2.Code.ExpressValue;
using compiler2.Code.Statement;
using Compiler2.Code.Statement;
using compiler2.Generate;

namespace compiler2.Compile
{
    public enum ExpressionTypeEnum
    {
        TypeBoolean,
        TypeEnum,
        TypeInteger,
        TypeDeviceState,
        TypeDateTime,
        TypeTimeInterval,
    }

    class SyntaxAnalyser
    {
        static readonly HashSet<TokenEnum> m_ValidDeclarationStarters;
        static readonly HashSet<TokenEnum> m_ExpectedDeclarationEnders;
        static readonly HashSet<TokenEnum> m_StartersAndEnders;
        static readonly HashSet<TokenEnum> m_Device;
        static readonly HashSet<TokenEnum> m_BooleanLiteral;
        static readonly HashSet<TokenEnum> m_DeviceLevel;
        static readonly HashSet<TokenEnum> m_DeviceSetCommands;
        static readonly HashSet<TokenEnum> m_ValidProcedureStarters;
        static readonly HashSet<TokenEnum> m_ValidProcedureAndDeclarationStarters;
        static readonly HashSet<TokenEnum> m_ValidDayAttribute;
        static readonly HashSet<TokenEnum> m_ValidDay;
        static readonly HashSet<TokenEnum> m_SequenceFiringTime;
        static readonly HashSet<TokenEnum> m_BooleanOperators;
        static readonly HashSet<TokenEnum> m_ComparisonOperators;
        static readonly HashSet<TokenEnum> m_FactorOperators;
        static readonly HashSet<TokenEnum> m_TermOperators;

        static readonly Dictionary<TokenEnum, BinaryOperator> m_BinaryOperatorDictionary = new Dictionary<TokenEnum, BinaryOperator>();
        static SyntaxAnalyser()
        {
            m_ValidDeclarationStarters = TokenSet.Set(TokenEnum.token_room, /*TokenEnum.token_action, */ TokenEnum.token_housecode, TokenEnum.token_device, TokenEnum.token_timeout,
                TokenEnum.token_bool, TokenEnum.token_int, TokenEnum.token_enum, TokenEnum.token_const, TokenEnum.token_procedure, TokenEnum.token_timer, TokenEnum.token_day);


            m_ExpectedDeclarationEnders = TokenSet.Set(TokenEnum.token_semicolon, TokenEnum.token_end);
            m_ExpectedDeclarationEnders = TokenSet.Set(TokenEnum.token_semicolon, TokenEnum.token_dot);

            //combine starters and enders to a single set.
            m_StartersAndEnders = new HashSet<TokenEnum>();
            foreach (TokenEnum tokenEnum in m_ValidDeclarationStarters)
            {
                m_StartersAndEnders.Add(tokenEnum);
            }
            foreach (TokenEnum tokenEnum in m_ExpectedDeclarationEnders)
            {
                m_StartersAndEnders.Add(tokenEnum);
            }


            m_Device = TokenSet.Set(TokenEnum.token_lamp, TokenEnum.token_appliance, TokenEnum.token_applianceLamp, TokenEnum.token_hueLamp, TokenEnum.token_sensor, TokenEnum.token_remote);

            m_BooleanLiteral = TokenSet.Set(TokenEnum.token_false, TokenEnum.token_true);

            m_DeviceLevel = TokenSet.Set(TokenEnum.token_device_state);
            m_DeviceSetCommands = TokenSet.Set(TokenEnum.token_device_state, TokenEnum.token_rgb_colour, TokenEnum.token_colour_loop);

            m_ValidProcedureStarters = TokenSet.Set(TokenEnum.token_if, TokenEnum.token_call_action, TokenEnum.token_set_device, TokenEnum.token_reset, 
                TokenEnum.token_refreshDevices, TokenEnum.token_resynchClock, TokenEnum.token_identifier);
            m_ValidProcedureAndDeclarationStarters = TokenSet.Set(m_ValidProcedureStarters, m_ValidDeclarationStarters);

            m_ValidDayAttribute = TokenSet.Set(TokenEnum.token_gmt, TokenEnum.token_bst, TokenEnum.token_holiday);
            m_ValidDay = TokenSet.Set(TokenEnum.token_mon, TokenEnum.token_tue, TokenEnum.token_wed, TokenEnum.token_thu, TokenEnum.token_fri,
                TokenEnum.token_sat, TokenEnum.token_sun, TokenEnum.token_first_working, TokenEnum.token_nonFirst_working, TokenEnum.token_working, TokenEnum.token_non_working, TokenEnum.token_all);
            m_SequenceFiringTime = TokenSet.Set(TokenEnum.token_sunrise, TokenEnum.token_sunset, TokenEnum.token_time_of_day);
            m_BooleanOperators = TokenSet.Set(TokenEnum.token_equalsequals, TokenEnum.token_notequals,
                TokenEnum.token_greaterthan, TokenEnum.token_greaterthanEquals, TokenEnum.token_lessthan, TokenEnum.token_lessthanEquals);

            m_ComparisonOperators = TokenSet.Set(TokenEnum.token_equalsequals, TokenEnum.token_lessthan,
                                             TokenEnum.token_lessthanEquals, TokenEnum.token_greaterthan,
                                             TokenEnum.token_greaterthanEquals, TokenEnum.token_notequals);

            m_FactorOperators = TokenSet.Set(TokenEnum.token_times, TokenEnum.token_div, TokenEnum.token_mod);
            m_TermOperators = TokenSet.Set(TokenEnum.token_plus, TokenEnum.token_minus);

            m_BinaryOperatorDictionary.Add(TokenEnum.token_equalsequals, BinaryOperator.Equal);
            m_BinaryOperatorDictionary.Add(TokenEnum.token_notequals, BinaryOperator.NotEqual);
            m_BinaryOperatorDictionary.Add(TokenEnum.token_greaterthan, BinaryOperator.GreaterThan);
            m_BinaryOperatorDictionary.Add(TokenEnum.token_greaterthanEquals, BinaryOperator.GreaterThanEquals);
            m_BinaryOperatorDictionary.Add(TokenEnum.token_lessthan, BinaryOperator.LessThan);
            m_BinaryOperatorDictionary.Add(TokenEnum.token_lessthanEquals, BinaryOperator.LessThanEqual);

        }


        private readonly LexicalAnalyser m_LexicalAnalyser;
        private TokenEnum m_Token;
        private readonly Dictionary<string, CodeBase> m_IdDictionary;
        private readonly CodeCalendar m_CodeCalendar = new CodeCalendar();

        internal CodeCalendar CodeCalendarValue
        {
            get { return m_CodeCalendar; }
        } 


        public SyntaxAnalyser(LexicalAnalyser lexicalAnalyser, Dictionary<string, CodeBase> idDictionary)
        {
            m_IdDictionary = idDictionary;
            m_LexicalAnalyser = lexicalAnalyser;
            NextToken();
        }

        private void NextToken()
        {
            Debug.Assert(m_Token != TokenEnum.token_eof);
            m_Token = m_LexicalAnalyser.Readtoken();
        }

        private void SkipTo(HashSet<TokenEnum> valid)
        {
            if (!valid.Contains(m_Token))
            {
                String oneOf = valid.Count > 1 ? "one of " : "";
                m_LexicalAnalyser.LogError("Syntax Error. Expected " + oneOf + LexicalAnalyser.GetSpellings(valid));
                while (!valid.Contains(m_Token) && m_Token != TokenEnum.token_eof)
                {
                    NextToken();
                }
            }
        }


        private string AcceptToken(TokenEnum expectedTokenEnum)
        {
            string result = null;
            if (m_Token == expectedTokenEnum)
            {
                result = m_LexicalAnalyser.TokenValue;
                NextToken();
            }
            else
            {
                m_LexicalAnalyser.LogError("expected '" + LexicalAnalyser.GetSpelling(expectedTokenEnum) + "'");
            }
            return result;
        }

        private string AcceptNewIdentifier()
        {
            string result = null;
            if (m_Token == TokenEnum.token_identifier)
            {
                if (m_IdDictionary.ContainsKey(m_LexicalAnalyser.TokenValue) &&
                    m_IdDictionary[m_LexicalAnalyser.TokenValue] != null &&
                    m_IdDictionary[m_LexicalAnalyser.TokenValue].DeclarationLineNumber != m_LexicalAnalyser.LineNumber)
                {
                    m_LexicalAnalyser.LogError("New identifier expected");
                }
                else
                {
                    result = m_LexicalAnalyser.TokenValue;
                }
                AcceptToken(TokenEnum.token_identifier);
            }
            return result;
        }

        private CodeProcedure AcceptNewActionIdentifier()
        {
            CodeProcedure resultCodeProcedure = null;
            if (m_Token == TokenEnum.token_identifier)
            {
                if (m_IdDictionary.ContainsKey(m_LexicalAnalyser.TokenValue))
                {
                    CodeProcedure codeProcedure = m_IdDictionary[m_LexicalAnalyser.TokenValue] as CodeProcedure;
                    if (codeProcedure != null)
                    {
                        codeProcedure.NoteUsage();
                        resultCodeProcedure = codeProcedure;
                    }
                    else
                    {
                        m_LexicalAnalyser.LogError("New identifier expected");
                    }
                }
                else
                {
                    resultCodeProcedure = null;
                }
                AcceptToken(TokenEnum.token_identifier);
            }
            return resultCodeProcedure;
        }

        public void Parse()
        {
            AcceptToken(TokenEnum.token_rules);
            string rulesName = AcceptNewIdentifier();

            while (m_Token != TokenEnum.token_eof && m_Token != TokenEnum.token_end)
            {
                //Console.WriteLine(m_LexicalAnalyser.LineNumber);
                if (m_ValidDeclarationStarters.Contains(m_Token))
                {
                    switch (m_Token)
                    {
                        case TokenEnum.token_room:
                            Rooms(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_housecode:
                            HouseCode(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_device:
                            Device(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_timeout:
                            Timeout(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_bool:
                            Bool(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_const:
                            Const(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_int:
                            Integer(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_enum:
                            Enumerated(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_procedure:
                            Procedure(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_day:
                            Day(m_StartersAndEnders);
                            break;

                        case TokenEnum.token_timer:
                            Timer(m_StartersAndEnders);
                            break;

                        default:
                            Debug.Assert(false);
                            break;
                    }
                    AcceptToken(TokenEnum.token_semicolon);
                }
                else
                {
                    SkipTo(m_ValidDeclarationStarters);
                }
            }
            AcceptToken(TokenEnum.token_end);
            AcceptToken(TokenEnum.token_dot);

            if (!m_CodeCalendar.CompleteDefinition())
            {
                m_LexicalAnalyser.LogWarn("No definitions for Winter and summer time found");
            }

            if (m_LexicalAnalyser.Pass == 2)
            {
                CheckUsage();
            }
        }


        private void CheckUsage()
        {
            foreach (CodeBase codeBase in m_IdDictionary.Values)
            {
                if (codeBase.UseCount == 0 && 
                    codeBase.IdentifierType != IdentifierTypeEnum.IdHouseCode &&
                    codeBase.IdentifierType != IdentifierTypeEnum.IdEnum)
                {
                    m_LexicalAnalyser.LogWarn(codeBase.Identifier + " not used");
                }
            }
        }

        private CodeBase AcceptExistingIdentifier()
        {
            CodeBase resultCodeBase = null;

            if (m_Token == TokenEnum.token_identifier)
            {
                if (m_IdDictionary.ContainsKey(m_LexicalAnalyser.TokenValue))
                {
                    resultCodeBase = m_IdDictionary[m_LexicalAnalyser.TokenValue];
                    resultCodeBase.NoteUsage();
                }
                else
                {
                    m_LexicalAnalyser.LogError("unknown identifier (2)");
                    if (m_LexicalAnalyser.Pass > 1)
                    {
                        //try to prevent cascading errors
                        m_IdDictionary.Add(m_LexicalAnalyser.TokenValue, new CodeUndefined(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, m_LexicalAnalyser.TokenValue));
                    }
                }
                AcceptToken(TokenEnum.token_identifier);
            }
            else
            {
                m_LexicalAnalyser.LogError("identifier expected");
            }
            return resultCodeBase;
        }

        private CodeDevice AcceptDeviceIdentifier(CodeRoom codeRoom)
        {
            CodeDevice codeDevice = null;
            if (m_Token == TokenEnum.token_identifier)
            {
                if (codeRoom != null && 
                    codeRoom.CodeDeviceDictionary.ContainsKey(m_LexicalAnalyser.TokenValue))
                {
                    codeDevice = codeRoom.CodeDeviceDictionary[m_LexicalAnalyser.TokenValue];
                }
                else
                {
                    m_LexicalAnalyser.LogError("unknown device(1)");
                }
            }
            AcceptToken(TokenEnum.token_identifier);

            return codeDevice;
        }

        private CodeBase AcceptExistingIdentifier(IdentifierTypeEnum wantedIdentifierTypeEnum)
        {
            CodeBase resultCodeBase = null;

            CodeBase tempCodeBase = AcceptExistingIdentifier();
            if (tempCodeBase != null)
            {
                if (tempCodeBase.IdentifierType == wantedIdentifierTypeEnum)
                {
                    resultCodeBase = tempCodeBase;
                }
                else
                {
                    if (tempCodeBase.IdentifierType != IdentifierTypeEnum.IdUndefined)
                    {
                        m_LexicalAnalyser.LogError("found wrong type");
                    }
                }
            }

            return resultCodeBase;
        }

        private TokenEnum GetDeviceTokenType(HashSet<TokenEnum> expectedTokenHashSet)
        {
            TokenEnum tokenEnum = TokenEnum.token_error;

            HashSet<TokenEnum> expectedHere = new HashSet<TokenEnum>(expectedTokenHashSet);
            expectedHere.UnionWith(m_Device);
            SkipTo(expectedHere);

            if (m_Device.Contains(m_Token))
            {
                tokenEnum = m_Token;
                NextToken();
            }
            return tokenEnum;
        }

        private void Rooms(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            int value = 0;

            AcceptToken(TokenEnum.token_room);
            while (m_Token == TokenEnum.token_identifier)
            {
                string id = AcceptNewIdentifier();
                if (id != null)
                {
                    if (!m_IdDictionary.ContainsKey(id))
                    {
                        m_IdDictionary.Add(id,
                            new CodeRoom(m_LexicalAnalyser.PreviousTokenLineNumber, m_LexicalAnalyser.Pass, id, value++));
                    }
                    else
                    {
                        m_LexicalAnalyser.LogPass1Error(String.Format("Room identifier '{0}' already used", id));
                    }
                }

                if (m_Token != TokenEnum.token_semicolon &&
                    !expectedDeclarationStarters.Contains(m_Token))
                {
                    AcceptToken(TokenEnum.token_comma);
                }
            }
            SkipTo(expectedDeclarationStarters);
        }


        private void HouseCode(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_housecode);
            int identifierLineNumber = m_LexicalAnalyser.LineNumber;
            string houseCodeId = AcceptNewIdentifier();

            string houseCode = AcceptToken(TokenEnum.token_house_code);
            if (houseCode == null)
            {
                m_LexicalAnalyser.LogError("Housecode not found");
            }
            else
            {
                if (CodeHouseCode.GetEntry(houseCode[0]) != null)
                {
                    m_LexicalAnalyser.LogPass1Error(string.Format("Housecode '{0}' already defined", houseCode[0]));
                }
            }

            CodeProcedure offProcedure = OffOnAction(TokenEnum.token_offProcedure);
            CodeProcedure onProcedure = OffOnAction(TokenEnum.token_onProcedure);

            if (houseCodeId != null &&
                houseCode != null && houseCode.Length == 1)
            {
                if (m_LexicalAnalyser.Pass == 1)
                {
                    if (!m_IdDictionary.ContainsKey(houseCodeId))
                    {
                        m_IdDictionary.Add(houseCodeId, null);
                    }
                    else
                    {
                        m_LexicalAnalyser.LogPass1Error(string.Format("Housecode name '{0}' already defined",
                            houseCodeId));
                    }
                }
                else //pass == 2
                {
                    CodeHouseCode codeHouseCode = new CodeHouseCode(identifierLineNumber, m_LexicalAnalyser.Pass,
                        houseCode[0], houseCodeId, offProcedure, onProcedure);
                    //update housecode with actual code now that we know values for the on & off actions.
                    m_IdDictionary[houseCodeId] = codeHouseCode;
                }
            }
        }
        /*
            device_list	: device
                    | device_list ';' device 
                    ;

            device		:
                    | TOKEN_DEVICE device_type device_id device_name TOKEN_HOUSE_CODE TOKEN_INTEGER optional_on_off_action
                    | TOKEN_DEVICE error ';'
                        { yyerror("error in device"); }
                    ;

            device_id	: TOKEN_IDENTIFIER
                    ;

            device_name	: TOKEN_STRING
                    ;

            optional_on_off_action	:
                        | off_action_id on_action_id 
                        ;

            device_type	: TOKEN_APPLIANCE
                    | TOKEN_LAMP
                    ;

         */

        private CodeBase AcceptOptionalIdentifier(IdentifierTypeEnum expectedIdentifierTypeEnum)
        {
            CodeBase resultCodeBase = null;

            if (m_Token == TokenEnum.token_identifier)
            {
                if (m_IdDictionary.ContainsKey(m_LexicalAnalyser.TokenValue))
                {
                    CodeBase codeBase = m_IdDictionary[m_LexicalAnalyser.TokenValue];
                    if (codeBase.IdentifierType == expectedIdentifierTypeEnum)
                    {
                        codeBase.NoteUsage();
                        resultCodeBase = codeBase;
                    }
                    else
                    {
                        m_LexicalAnalyser.LogError("Wrong type");
                    }
                    AcceptToken(TokenEnum.token_identifier);
                }
                else
                { 
                    m_LexicalAnalyser.LogError("Unknown identifier (3)");
                }
            }
            return resultCodeBase;
        }

        private CodeProcedure OffOnAction(TokenEnum preparationTokenEnum)
        {
            CodeProcedure codeProcedure = null;
            if (m_Token == preparationTokenEnum)
            {
                AcceptToken(preparationTokenEnum);
                codeProcedure = AcceptExistingIdentifier(IdentifierTypeEnum.IdProcedure) as CodeProcedure;
            }
            return codeProcedure;
        }

        private static bool IsValidMacAddress(string macAddress)
        {
            bool valid = macAddress != null && macAddress.Length == 26;

            if (valid)
            {
                for (int i = 0; i < macAddress.Length && valid; i++)
                {
                    if (i % 3 == 2)
                    {
                        if (i == 23)
                        {
                            valid = macAddress[i] == '-';
                        }
                        else
                        {
                            valid = macAddress[i] == ':';
                        }
                    }
                    else
                    {
                        valid = Char.IsDigit(macAddress[i]) ||
                                macAddress[i] >= 'a' && macAddress[i] <= 'f';
                    }
                }
            }
            return valid;
        }

        private void Device(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            const string dummyMacAddress = "00:00:00:00:00:00:00:00-00";

            AcceptToken(TokenEnum.token_device);
            TokenEnum tokenDeviceType = GetDeviceTokenType(expectedDeclarationStarters);

            //identifier is now "roomname . devicename".
            CodeRoom codeRoom = AcceptExistingIdentifier(IdentifierTypeEnum.IdRoom) as CodeRoom;
            AcceptToken(TokenEnum.token_dot);

            string deviceId = AcceptNewIdentifier();

            if (codeRoom!= null && codeRoom.CodeDeviceDictionary.ContainsKey(deviceId))
            {
                m_LexicalAnalyser.LogPass1Error(string.Format("Duplicate device name '{0}' in code room '{1}'",
                    deviceId, codeRoom.Identifier));
            }


            string houseCode = AcceptToken(TokenEnum.token_house_code);
            if (houseCode == null || CodeHouseCode.GetEntry(houseCode[0]) == null)
            {
                m_LexicalAnalyser.LogError(string.Format("Housecode '{0}' not defined", houseCode == null ? '?' : houseCode[0]));
            }

            switch (tokenDeviceType)
            {
                case TokenEnum.token_lamp:
                case TokenEnum.token_appliance:
                case TokenEnum.token_applianceLamp:
                case TokenEnum.token_sensor:
                case TokenEnum.token_remote:
                {
                    int unitCode = 1;
                    String tokenValue = AcceptToken(TokenEnum.token_integer);
                    if (tokenValue != null)
                    {
                        unitCode = int.Parse(tokenValue);
                    }

                    CodeProcedure optionalCodeOffProcedure = OffOnAction(TokenEnum.token_offProcedure);
                    CodeProcedure optionalCodeOnProcedure = OffOnAction(TokenEnum.token_onProcedure);

                    if (houseCode != null && deviceId != null && codeRoom != null && m_LexicalAnalyser.Pass == 2)
                    {
                        CodeDevice codeDevice = new CodeDevice(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass,
                            GenerateDevice.MapDevice(tokenDeviceType), codeRoom, deviceId, houseCode[0], unitCode,
                            optionalCodeOffProcedure, optionalCodeOnProcedure);
                        if (!codeRoom.CodeDeviceDictionary.ContainsKey(deviceId))
                        {
                            codeRoom.CodeDeviceDictionary.Add(deviceId, codeDevice);
                        }
                        else
                        {
                            //Debug.Assert(false);
                        }
                    }
                    break;
                }

                case TokenEnum.token_hueLamp:
                {
                    string macAddress = AcceptToken(TokenEnum.token_string);
                    if (!IsValidMacAddress(macAddress))
                    {
                        m_LexicalAnalyser.LogError(string.Format("MAC Address '{0}' must be in the form '"+ dummyMacAddress + "'", macAddress));
                        macAddress = dummyMacAddress;
                    }
                    CodeProcedure optionalCodeOffProcedure = OffOnAction(TokenEnum.token_offProcedure);
                    CodeProcedure optionalCodeOnProcedure = OffOnAction(TokenEnum.token_onProcedure);

                    if (houseCode != null && deviceId != null && codeRoom != null && m_LexicalAnalyser.Pass == 2)
                    {
                        CodeDevice codeDevice = new CodeDevice(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass,
                            GenerateDevice.MapDevice(tokenDeviceType), codeRoom, deviceId, houseCode[0], macAddress, optionalCodeOffProcedure,
                            optionalCodeOnProcedure);

                        if (!codeRoom.CodeDeviceDictionary.ContainsKey(deviceId))
                        {
                            codeRoom.CodeDeviceDictionary.Add(deviceId, codeDevice);
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                    }

                    break;
                }
            }
            SkipTo(expectedDeclarationStarters);
        }

        private bool IsValidTimeOfDay(string timeOfDay)
        {
            bool valid = timeOfDay != null && timeOfDay.Length == 8;

            if (valid)
            {
                for (int i = 0; i < timeOfDay.Length && valid; i++)
                {
                    if (i % 3 == 2)
                    {
                        valid = timeOfDay[i] == ':';
                    }
                    else
                    {
                        valid = Char.IsDigit(timeOfDay[i]);
                    }
                }
            }
            return valid;
        }

        private TimeSpan AcceptTimeSpan()
        {
            TimeSpan timespan = new TimeSpan(0L);

            if (m_Token == TokenEnum.token_time_of_day)
            {
                if (IsValidTimeOfDay(m_LexicalAnalyser.TokenValue))
                {
                    //TODO only works if you use exact format 00:00:00 (i.e. leading zeros on 2 digits and all valid numbers)
                    string hours = m_LexicalAnalyser.TokenValue.Substring(0, 2);
                    string minutes = m_LexicalAnalyser.TokenValue.Substring(3, 2);
                    string seconds = m_LexicalAnalyser.TokenValue.Substring(6, 2);
                    timespan = new TimeSpan(int.Parse(hours), int.Parse(minutes), int.Parse(seconds));
                }
                else
                {
                    m_LexicalAnalyser.LogError("Time must be of form '00:00:00'");
                }
            }
            AcceptToken(TokenEnum.token_time_of_day);
            return timespan;
        }

        private void Timeout(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            //TIMEOUT ConservatoryEmptyTimer 00:10:00 NoOneInConservatory;
            AcceptToken(TokenEnum.token_timeout);
            string timeoutId = AcceptNewIdentifier(); //todo note down timeout.

            TimeSpan defaultDurationTimeSpan = AcceptTimeSpan();
            CodeProcedure timeoutProcedure = OffOnAction(TokenEnum.token_offProcedure);

            if (timeoutId != null &&
                timeoutProcedure != null)
            {
                CodeTimeout codeTimeout = new CodeTimeout(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, timeoutId, defaultDurationTimeSpan, timeoutProcedure);
                m_IdDictionary.Add(timeoutId, codeTimeout);
            }

            SkipTo(expectedDeclarationStarters);
        }

        /*
    flag_list	: flag
            | flag_list ';' flag 
            ;

    flag		: TOKEN_FLAG flag_id default_flag_value
            | TOKEN_FLAG error
                { yyerror("error in flag"); }
            ;

    flag_id		: TOKEN_IDENTIFIER
            ;


    default_flag_value	: TOKEN_TRUE
                | TOKEN_FALSE
                ;

         */

        private TokenEnum AcceptValidToken(HashSet<TokenEnum> validTokenHashSet, string expected)
        {
            TokenEnum resultTokenEnum = TokenEnum.token_error;

            if (validTokenHashSet.Contains(m_Token))
            {
                resultTokenEnum = m_Token;
                NextToken();
            }
            else
            {
                m_LexicalAnalyser.LogError("Expected '" + expected + "'");
            }
            return resultTokenEnum;
        }

        private Expression IdentifierListExpression(string[] indentifiers)
        {
            Expression expression1 = new Expression(new Value(5));
            if (m_IdDictionary.ContainsKey(indentifiers[0]))
            {
                CodeBase codebase = m_IdDictionary[indentifiers[0]];
                codebase?.NoteUsage();
                switch (codebase.IdentifierType)
                {
                    case IdentifierTypeEnum.IdConst:
                        CodeConst codeConst = CodeConst.GetEntry(codebase.EntryNo);

                        expression1 = new Expression(new Value(codeConst));

                        if (indentifiers.Length > 1)
                        {
                            m_LexicalAnalyser.LogError(string.Format("Dotted identifier '{0}' not allowed here", indentifiers[0]));
                        }
                        break;

                    case IdentifierTypeEnum.IdBool:
                        expression1 = new Expression(new Value(CodeVariable.GetEntry(codebase.EntryNo)));

                        if (indentifiers.Length > 1)
                        {
                            m_LexicalAnalyser.LogError(string.Format("Dotted identifier '{0}' not allowed here", indentifiers[0]));
                        }
                        break;

                    case IdentifierTypeEnum.IdRoom:
                        if (indentifiers.Length == 2)
                        {
                            CodeRoom codeRoom = CodeRoom.GetEntry(codebase.EntryNo);
                            if (codeRoom.CodeDeviceDictionary.ContainsKey(indentifiers[1]))
                            {
                                CodeDevice codeDevice = codeRoom.CodeDeviceDictionary[indentifiers[1]];
                                expression1 = new Expression(new Value(codeDevice));
                            }
                            else
                            {
                                m_LexicalAnalyser.LogError(string.Format("Device '{0}' not found in room", indentifiers[1]));
                            }
                            
                        }
                        else
                        {
                            m_LexicalAnalyser.LogError(string.Format("single Dotted identifier '{0}' not allowed here", indentifiers[0]));
                        }

                        break;

                    case IdentifierTypeEnum.IdProcedure:
                    case IdentifierTypeEnum.IdHouseCode:
                    case IdentifierTypeEnum.IdTimeout:
                    default:
                        m_LexicalAnalyser.LogError(string.Format("This type of identifier '{0}' not allowed here", indentifiers[0]));

                        break;
                }
            }
            else
            {
                m_LexicalAnalyser.LogError(string.Format("Unknown identifier '{0}'", indentifiers[0]));
            }
            return expression1;
        }

        private bool ExpectedExpressionType(TypeEnum foundTypeEnum, TypeEnum expectedTypeEnum)
        {
            bool ok = true;

            if (foundTypeEnum != expectedTypeEnum)
            {
                m_LexicalAnalyser.LogError(string.Format("expresion type was {0} but expected {1}",
                    LexicalAnalyser.GetSpelling(foundTypeEnum), LexicalAnalyser.GetSpelling(expectedTypeEnum)));
                ok = false;
            }
            return ok;
        }

        private bool ExpectedRhs(TokenEnum operatorTokenEnum, TypeEnum rightTypeEnum, TypeEnum expectedTypeEnum)
        {
            bool ok = true;

            if (rightTypeEnum != expectedTypeEnum)
            {
                m_LexicalAnalyser.LogError(string.Format("rhs of operator {0} was {1} but expected {2}", 
                    LexicalAnalyser.GetSpelling(operatorTokenEnum), LexicalAnalyser.GetSpelling(rightTypeEnum), LexicalAnalyser.GetSpelling(expectedTypeEnum)));
                ok = false;
            }

            return ok;
        }

        private bool ExpectedLhs(TokenEnum operatorTokenEnum, TypeEnum leftTypeEnum, TypeEnum expectedTypeEnum)
        {
            bool ok = true;

            if (leftTypeEnum != expectedTypeEnum)
            {
                m_LexicalAnalyser.LogError(string.Format("lhs of operator {0} was {1} but expected {2}",
                    LexicalAnalyser.GetSpelling(operatorTokenEnum), LexicalAnalyser.GetSpelling(leftTypeEnum), LexicalAnalyser.GetSpelling(expectedTypeEnum)));
                ok = false;
            }

            return ok;
        }

        private bool CheckExpressionType(TypeEnum leftTypeEnum, TokenEnum operatorTokenEnum, TypeEnum rightTypeEnum)
        {
            bool ok = false; //by default

            switch (operatorTokenEnum)
            {
                case TokenEnum.token_not: //unary NOT operator
                    Debug.Assert(leftTypeEnum == TypeEnum.OtherType);
                    ok = ExpectedRhs(operatorTokenEnum, rightTypeEnum, TypeEnum.BoolType);
                    break;

                case TokenEnum.token_minus: //unary '-' operator or binary
                {
                    if (leftTypeEnum == TypeEnum.OtherType)
                    {
                        ok = ExpectedRhs(operatorTokenEnum, rightTypeEnum, TypeEnum.IntType);
                    }
                    else
                    {
                        ok = ExpectedLhs(operatorTokenEnum, leftTypeEnum, TypeEnum.IntType) &&
                             ExpectedRhs(operatorTokenEnum, rightTypeEnum, TypeEnum.IntType);
                    }
                    break;
                }

                case TokenEnum.token_equalsequals:
                case TokenEnum.token_notequals:
                case TokenEnum.token_greaterthan:
                case TokenEnum.token_greaterthanEquals:
                case TokenEnum.token_lessthan:
                case TokenEnum.token_lessthanEquals:
                    //TODO these can be other types later.
                    ok = ExpectedLhs(operatorTokenEnum, leftTypeEnum, TypeEnum.IntType) &&
                         ExpectedRhs(operatorTokenEnum, rightTypeEnum, TypeEnum.IntType);
                    break;


                case TokenEnum.token_plus:
                case TokenEnum.token_times:
                case TokenEnum.token_div:
                case TokenEnum.token_mod:
                    ok = ExpectedLhs(operatorTokenEnum, leftTypeEnum, TypeEnum.IntType) &&
                         ExpectedRhs(operatorTokenEnum, rightTypeEnum, TypeEnum.IntType);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            return ok;
        }
        private Expression Factor(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            Expression expression1 = null;

            TokenEnum operatorTokenEnum = m_Token;
            switch (m_Token)
            {
                case TokenEnum.token_integer:
                    expression1 = new Expression(new Value(m_LexicalAnalyser.IntValue));
                    NextToken();
                    break;

                case TokenEnum.token_false:
                case TokenEnum.token_true:
                    expression1 = new Expression(new Value(m_Token == TokenEnum.token_true));
                    NextToken();
                    break;


                case TokenEnum.token_device_state:
                    Debug.Assert(m_LexicalAnalyser.IntValue >= (int)device_state_t.stateOff && m_LexicalAnalyser.IntValue <= (int)device_state_t.stateDim17);
                    expression1 = new Expression(new Value(m_LexicalAnalyser.IntValue));
                    NextToken();
                    break;


                //literal constant values
                case TokenEnum.token_string:
                    expression1 = new Expression(new Value(m_LexicalAnalyser.IntValue)); //TODO write some real code here.
                    NextToken();
                    break;

                case TokenEnum.token_time_of_day:
                    expression1 = new Expression(new Value(m_LexicalAnalyser.GetTypeEnum, m_LexicalAnalyser.IntValue)); //TODO write some real code here.
                    NextToken();
                    break;

                case TokenEnum.token_date:
                    expression1 = new Expression(new Value(m_LexicalAnalyser.GetTypeEnum, m_LexicalAnalyser.IntValue)); //TODO write some real code here.
                    NextToken();
                    break;

                case TokenEnum.token_house_code:
                    expression1 = new Expression(new Value(m_LexicalAnalyser.IntValue)); //TODO write some real code here.
                    NextToken();
                    break;

                case TokenEnum.token_identifier: //variable
                    {
                        List<string> identifierList = new List<string>();
                        identifierList.Add(m_LexicalAnalyser.TokenValue);
                        NextToken();
                        if (m_Token == TokenEnum.token_dot) //TODO should be while later??
                        {
                            NextToken();
                            if (m_Token == TokenEnum.token_identifier)
                            {
                                identifierList.Add(m_LexicalAnalyser.TokenValue);

                            }
                            AcceptToken(TokenEnum.token_identifier);
                        }
                        expression1 = IdentifierListExpression(identifierList.ToArray());
                    }
                    break;

                //TODO function call

                case TokenEnum.token_not: //unary NOT operator
                    {
                        NextToken();
                        Expression expression2 = Factor(validTokenHashSet, expressionTypeExpected);
                        expression1 = new Expression(UnaryOperator.Not, expression2);
                        bool ok = CheckExpressionType(TypeEnum.OtherType, operatorTokenEnum, expression2.GetTypeEnum);
                    }
                    break;

                case TokenEnum.token_minus: //unary '-' operator
                    {
                        NextToken();
                        Expression expression2 = Factor(validTokenHashSet, expressionTypeExpected);
                        expression1 = new Expression(UnaryOperator.Negate, expression2);
                        bool ok = CheckExpressionType(TypeEnum.OtherType, operatorTokenEnum, expression2.GetTypeEnum);
                    }
                    break;

                case TokenEnum.token_leftParent:
                {
                    HashSet<TokenEnum> rightParentExpectedHere = new HashSet<TokenEnum>(validTokenHashSet);
                    rightParentExpectedHere.Add(TokenEnum.token_rightParent);

                    NextToken(); //leftParent
                    expression1 = ParseExpression(rightParentExpectedHere, expressionTypeExpected);
                    AcceptToken(TokenEnum.token_rightParent);
                    break;
                }

                default:
                    m_LexicalAnalyser.LogError("Expected literal constant, variable, or '(' expression ')'");
                    expression1 = new Expression(new Value(5));
                    break;
            }

            SkipTo(validTokenHashSet);
            return expression1;
        }

        private Expression Term(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            HashSet<TokenEnum> expectedHere = new HashSet<TokenEnum>(validTokenHashSet);
            expectedHere.UnionWith(m_FactorOperators);

            Expression expression1 = Factor(expectedHere, expressionTypeExpected);
            while (m_FactorOperators.Contains(m_Token)) // '*', '/', '%'
            {
                BinaryOperator binaryOperator = m_BinaryOperatorDictionary[m_Token];
                NextToken();
                Expression expression2 = Factor(expectedHere, expressionTypeExpected);
                expression1 = new Expression(expression1, binaryOperator, expression2);
            }
            return expression1;
        }

        private Expression SimpleExpression(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            HashSet<TokenEnum> expectedHere = new HashSet<TokenEnum>(validTokenHashSet);
            expectedHere.UnionWith(m_TermOperators);

            Expression expression1 = Term(expectedHere, expressionTypeExpected);
            while (m_TermOperators.Contains(m_Token)) // binary '+', '-'
            {
                BinaryOperator binaryOperator = m_BinaryOperatorDictionary[m_Token];
                NextToken();
                Expression expression2 = Term(expectedHere, expressionTypeExpected);
                expression1 = new Expression(expression1, binaryOperator, expression2);
            }
            return expression1;
        }

        private Expression ComparisonExpression(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            HashSet<TokenEnum> expectedHere = new HashSet<TokenEnum>(validTokenHashSet);
            expectedHere.UnionWith(m_ComparisonOperators);

            Expression expression1 = SimpleExpression(expectedHere, expressionTypeExpected);
            while (m_ComparisonOperators.Contains(m_Token)) // binary <=, <, !=, ==, >=, >
            {
                BinaryOperator binaryOperator = m_BinaryOperatorDictionary[m_Token];
                NextToken();
                Expression expression2 = SimpleExpression(expectedHere, expressionTypeExpected);
                expression1 = new Expression(expression1, binaryOperator, expression2);
            }
            return expression1;
        }

        private Expression LogicalOrExpression(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            HashSet<TokenEnum> expectedHere = new HashSet<TokenEnum>(validTokenHashSet);
            expectedHere.Add(TokenEnum.token_or);

            Expression expression1 = ComparisonExpression(expectedHere, expressionTypeExpected);
            while (m_Token == TokenEnum.token_or) 
            {
                NextToken();
                Expression expression2 = ComparisonExpression(expectedHere, expressionTypeExpected);
                expression1 = new Expression(expression1, BinaryOperator.LogicalOr, expression2);
            }
            return expression1;
        }

        private Expression ParseExpression(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            HashSet<TokenEnum> expectedHere = new HashSet<TokenEnum>(validTokenHashSet);
            expectedHere.Add(TokenEnum.token_and);

            Expression expression1 = LogicalOrExpression(expectedHere, expressionTypeExpected);
            while (m_Token == TokenEnum.token_and)
            {
                NextToken();
                Expression expression2 = LogicalOrExpression(expectedHere, expressionTypeExpected);
                expression1 = new Expression(expression1, BinaryOperator.LogicalAnd, expression2);
            }
            return expression1;
        }

        private int ConstantIntegerExpression()
        {
            //TODO this really should use the expression parser, but afterwards we would have to do constant folding, so not worth it for now.
            int value = -1;

            if (m_Token == TokenEnum.token_integer)
            {
                value = m_LexicalAnalyser.IntValue;
                AcceptToken(TokenEnum.token_integer);
            }
            else if (m_Token == TokenEnum.token_identifier)
            {
                CodeConst codeConst = AcceptExistingIdentifier(IdentifierTypeEnum.IdConst) as CodeConst;
                if (codeConst != null)
                {
                    value = codeConst.Value;
                }
            }
            else
            {
                m_LexicalAnalyser.LogError("Expression not found");
            }
            return value;
        }

        private void Bool(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_bool);
            int identifierLineNumber = m_LexicalAnalyser.LineNumber;
            string boolId = AcceptNewIdentifier();
            if (boolId == null)
            {
                m_LexicalAnalyser.LogError("Variable on LHS of assignment not found");
            }
            AcceptToken(TokenEnum.token_equals);
            //todo in future accept boolean constant expression but do automatic constant folding.
            TokenEnum boolValue = AcceptValidToken(TokenSet.Set(TokenEnum.token_false, TokenEnum.token_true), "TRUE/FALSE");
            if (boolId != null && m_LexicalAnalyser.Pass == 1)
            {
                m_IdDictionary.Add(boolId, new CodeVariable(identifierLineNumber, m_LexicalAnalyser.Pass, boolId, boolValue.Equals(TokenEnum.token_true)));
            }

            SkipTo(expectedDeclarationStarters);
        }

        private void Integer(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_int);
            int identifierLineNumber = m_LexicalAnalyser.LineNumber;
            string intId = AcceptNewIdentifier();
            if (intId == null)
            {
                m_LexicalAnalyser.LogError("Variable on LHS of assignment not found");
            }
            AcceptToken(TokenEnum.token_equals);
            int value = ConstantIntegerExpression();
            if (intId != null && m_LexicalAnalyser.Pass == 1)
            {
                m_IdDictionary.Add(intId, new CodeVariable(identifierLineNumber, m_LexicalAnalyser.Pass, intId, value));
            }

            SkipTo(expectedDeclarationStarters);
        }


        private void Const(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_const);
            string intId = AcceptNewIdentifier();
            AcceptToken(TokenEnum.token_equals);
            int value = ConstantIntegerExpression();
            if (intId != null)
            {
                m_IdDictionary.Add(intId, new CodeConst(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, intId, value, TypeEnum.IntType));
            }

            SkipTo(expectedDeclarationStarters);
        }

        private void Enumerated(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            HashSet <TokenEnum> expectedPlusRightParent = new HashSet<TokenEnum>(expectedDeclarationStarters);
            expectedPlusRightParent.Add(TokenEnum.token_rightParent);

            HashSet<TokenEnum> expectedPlusRightParentIdentifierAndComma = new HashSet<TokenEnum>(expectedPlusRightParent);
            expectedPlusRightParentIdentifierAndComma.Add(TokenEnum.token_identifier);
            expectedPlusRightParentIdentifierAndComma.Add(TokenEnum.token_comma);

            int value = 0;

            AcceptToken(TokenEnum.token_enum);
            string enumId = AcceptNewIdentifier();
            if (enumId != null)
            {
                if (m_IdDictionary.ContainsKey(enumId))
                {
                    if (m_IdDictionary[enumId].DeclarationLineNumber != m_LexicalAnalyser.PreviousTokenLineNumber)
                    {
                        m_LexicalAnalyser.LogError(string.Format(" duplicate declaration of '{0}'", enumId));
                    }
                }
                else
                {
                    m_IdDictionary.Add(enumId, new CodeEnum(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, enumId, value, TypeEnum.IntType /*EnumType */));
                }
            }

            AcceptToken(TokenEnum.token_leftParent);
            while (m_Token == TokenEnum.token_identifier || m_Token == TokenEnum.token_comma)
            {
                string id = AcceptNewIdentifier();
                if (id != null && m_LexicalAnalyser.Pass == 1)
                {
                    if (m_IdDictionary.ContainsKey(id))
                    {
                        m_LexicalAnalyser.LogError(string.Format("duplicate enum value '{0}'", id));
                    }
                    else
                    {
                        m_IdDictionary.Add(id, new CodeConst(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, id, value++, TypeEnum.IntType /*EnumType */));
                    }
                }
                else
                {
                    m_LexicalAnalyser.LogPass1Error("identifier not found");
                }

                SkipTo(expectedPlusRightParentIdentifierAndComma);
                if (m_Token != TokenEnum.token_rightParent)
                {
                    AcceptToken(TokenEnum.token_comma);
                }
            }
            SkipTo(expectedPlusRightParent);
            AcceptToken(TokenEnum.token_rightParent);

            SkipTo(expectedDeclarationStarters);
        }

        /*
    action_id	: TOKEN_IDENTIFIER
            ;

    action_list : action
        | action_list ';' action 
        ;

    action		: TOKEN_BODY action_id ';' action_item_list TOKEN_END 
            | TOKEN_BODY error
                { yyerror("error in action"); }
            ;

    action_item_list : action_item
        | action_item_list ';' action_item
        ;

    action_item	: if_item
            | if_device_item
            | call_item
            | assign_flag_item
            | set_device
            |
            ;

    if_item		: TOKEN_IF flag_id ';' action_item_list optional_else_list TOKEN_ENDIF
            ;

    optional_else_list	:
                | TOKEN_ELSE action_item_list
                ;


    if_device_item	: TOKEN_IF_ON device_id ';' action_item_list optional_else_list TOKEN_ENDIF
            ;

    call_item	: TOKEN_CALL action_id
            ;

    assign_flag_item	: TOKEN_SET flag_id
                | TOKEN_UNSET flag_id
                ;

    set_device	: TOKEN_SET_DEVICE device_id device_level
            ;


    device_level	: TOKEN_OFF
            | TOKEN_DIM1
            | TOKEN_DIM2
            | TOKEN_DIM3
            | TOKEN_DIM4
            | TOKEN_DIM5
            | TOKEN_DIM6
            | TOKEN_DIM7
            | TOKEN_DIM8
            | TOKEN_DIM9
            | TOKEN_DIM10
            | TOKEN_DIM11
            | TOKEN_DIM12
            | TOKEN_DIM13
            | TOKEN_DIM14
            | TOKEN_DIM15
            | TOKEN_DIM16
            | TOKEN_DIM17
            ;
         */

        private StatementIf IfStatement(HashSet<TokenEnum> followers)
        {
            StatementIf statementIf = null;

            AcceptToken(TokenEnum.token_if);

            Expression expression = ParseExpression(TokenSet.Set(followers, TokenEnum.token_then), ExpressionTypeEnum.TypeBoolean);
            AcceptToken(TokenEnum.token_then);

            List<StatementBase> thenStatements = Statements(TokenSet.Set(followers, TokenEnum.token_else, TokenEnum.token_endif));
            List<StatementBase> elseStatements = null;
            if (m_Token == TokenEnum.token_else)
            {
                AcceptToken(TokenEnum.token_else);
                elseStatements = Statements(TokenSet.Set(followers, TokenEnum.token_endif));
            }
            AcceptToken(TokenEnum.token_endif);

            statementIf = new StatementIf(expression, thenStatements, elseStatements);
            return statementIf;
        }

        private StatementCall CallStatement()
        {
            AcceptToken(TokenEnum.token_call_action);
            CodeProcedure codeProcedure = AcceptExistingIdentifier(IdentifierTypeEnum.IdProcedure) as CodeProcedure;

            StatementCall statementCall = new StatementCall(codeProcedure);
            return statementCall;
        }

        private CodeDevice[] GetCodeDevices(CodeRoom codeRoom, TokenEnum tokenEnum, device_state_t futureDeviceState)
        {
            Debug.Assert(tokenEnum == TokenEnum.token_lamp || tokenEnum == TokenEnum.token_appliance);

            CodeDevice[] codeDevices = null;
            List<CodeDevice> codeDeviceList = new List<CodeDevice>();


            if (codeRoom != null)
            {
                foreach (CodeDevice codeDevice in codeRoom.CodeDeviceDictionary.Values)
                {
                    switch (tokenEnum)
                    {
                        case TokenEnum.token_appliance:
                            if (codeDevice.DeviceType == deviceType_t.deviceAppliance ||
                                codeDevice.DeviceType == deviceType_t.deviceLamp ||
                                codeDevice.DeviceType == deviceType_t.deviceApplianceLamp)
                            {
                                codeDeviceList.Add(codeDevice);
                            }
                            break;

                        case TokenEnum.token_lamp:
                            if (codeDevice.DeviceType == deviceType_t.deviceLamp ||
                                codeDevice.DeviceType == deviceType_t.deviceHueLamp ||
                                (codeDevice.DeviceType == deviceType_t.deviceApplianceLamp && (futureDeviceState == device_state_t.stateOn ||
                                                                                               futureDeviceState == device_state_t.stateOff) ) )
                            {
                                 codeDeviceList.Add(codeDevice);
                            }
                            break;

                        case TokenEnum.token_hueLamp:
                            Debug.Assert(false);
                            break;

                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
                codeDevices = codeDeviceList.ToArray();
            }
            return codeDevices;
        }

        private TokenEnum AcceptRoomDeviceOrDeviceType()
        {
            TokenEnum returnTokenEnum = m_Token;
            switch (m_Token)
            {
                case TokenEnum.token_appliance:
                    AcceptToken(TokenEnum.token_appliance);
                    break;

                case TokenEnum.token_lamp:
                    AcceptToken(TokenEnum.token_lamp);
                    break;

                case TokenEnum.token_identifier:
                    AcceptToken(TokenEnum.token_identifier);
                    break;

                default:
                    m_LexicalAnalyser.LogError("device is not lamp, lamp-appliance or appliance");
                    break;
            }
            return returnTokenEnum;
        }

        private CodeDevice[] CalculateHouseDotDevices(CodeRoom codeRoom, TokenEnum deviceTokenEnum, string deviceIdentifier, device_state_t futureDeviceState)
        {
            CodeDevice[] codeDevices = null;
            switch (deviceTokenEnum)
            {
                case TokenEnum.token_appliance:
                    codeDevices = GetCodeDevices(codeRoom, TokenEnum.token_appliance, futureDeviceState);
                    break;

                case TokenEnum.token_lamp:
                    codeDevices = GetCodeDevices(codeRoom, TokenEnum.token_lamp, futureDeviceState);
                    break;

                case TokenEnum.token_identifier:
                    if (codeRoom != null &&
                        codeRoom.CodeDeviceDictionary.ContainsKey(deviceIdentifier))
                    {
                        codeDevices = new CodeDevice[1];
                        codeDevices[0] = codeRoom.CodeDeviceDictionary[deviceIdentifier];
                    }
                    else
                    {
                        //ignore on 1st pass
                    }
                    break;

                default:
                    if (string.IsNullOrWhiteSpace(deviceIdentifier))
                    {
                        m_LexicalAnalyser.LogError("device identifier missing");
                    }
                    else
                    {
                        m_LexicalAnalyser.LogError("unknown syntax error");
                    }
                    break;
            }
            return codeDevices;
        }
        /*
        private CodeDevice AcceptHouseDotDevice()
        {
            CodeRoom codeRoom = AcceptExistingIdentifier(IdentifierTypeEnum.IdRoom) as CodeRoom;
            AcceptToken(TokenEnum.token_dot);
            CodeDevice codeDevice = AcceptDeviceIdentifier(codeRoom);
            return codeDevice;
        }
         */


        private StatementSetDevice SetDevice(HashSet<TokenEnum> expectedFollowers)
        {
            HashSet<TokenEnum> expectedAfterDevice = new HashSet<TokenEnum>(expectedFollowers);
            expectedAfterDevice.UnionWith(m_DeviceSetCommands);
            expectedAfterDevice.Add(TokenEnum.token_delayed);
            expectedAfterDevice.Add(TokenEnum.token_duration);

            AcceptToken(TokenEnum.token_set_device);

            CodeRoom codeRoom = AcceptExistingIdentifier(IdentifierTypeEnum.IdRoom) as CodeRoom;
            AcceptToken(TokenEnum.token_dot);

            string deviceIdentifier = "";
            if (m_Token == TokenEnum.token_identifier)
            {
                deviceIdentifier = m_LexicalAnalyser.TokenValue;
                if (codeRoom == null ||
                    !codeRoom.CodeDeviceDictionary.ContainsKey(m_LexicalAnalyser.TokenValue))
                {
                    m_LexicalAnalyser.LogError("unknown device(2)");

                    //reduce further errors by declaring this device
                    if (codeRoom != null && m_LexicalAnalyser.Pass == 2)
                    {
                        codeRoom.CodeDeviceDictionary.Add(m_LexicalAnalyser.TokenValue, new CodeDevice(
                            m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass,
                            deviceType_t.deviceLamp, codeRoom, m_LexicalAnalyser.TokenValue, 'A', 1, null, null));
                    }
                }

            }
            TokenEnum deviceTokenEnum = AcceptRoomDeviceOrDeviceType();

            CodeDeviceCommands codeDeviceCommands = new CodeDeviceCommands();
            TokenEnum deviceLevelTokenEnum = TokenEnum.token_error;
            SkipTo(expectedAfterDevice);
            do
            {
                int tokenValue = m_LexicalAnalyser.IntValue;
                TokenEnum deviceFeatureTokenEnum = AcceptValidToken(m_DeviceSetCommands, "OFF/ON/DIM1 etc");
                if (m_DeviceSetCommands.Contains(deviceFeatureTokenEnum))
                {
                    switch (deviceFeatureTokenEnum)
                    {
                        case TokenEnum.token_device_state:
                            deviceLevelTokenEnum = deviceFeatureTokenEnum;
                            codeDeviceCommands.SetDeviceState((device_state_t)tokenValue);
                            break;

                        case TokenEnum.token_rgb_colour:
                            codeDeviceCommands.SetColour(tokenValue);
                            break;

                        case TokenEnum.token_colour_loop:
                            codeDeviceCommands.SetColourLoop();
                            break;

                        default:
                            m_LexicalAnalyser.LogError("expected device state attribute");
                            break;
                    }
                }
                SkipTo(expectedAfterDevice);
            } while (m_DeviceSetCommands.Contains(m_Token));

            CodeDevice[] codeDevices = CalculateHouseDotDevices(codeRoom, deviceTokenEnum, deviceIdentifier, codeDeviceCommands.GetDeviceState);

            //a duration only makes sense if the device is being switched on.
            TimeSpan delayTimeSpan = AcceptTimeSpan(TokenEnum.token_delayed, new TimeSpan(0L));
            TimeSpan durationTimeSpan = new TimeSpan(0, 0, 0);
            if (deviceLevelTokenEnum == TokenEnum.token_device_state)
            {
                int defaultDurationHours = codeDeviceCommands.GetDeviceState == device_state_t.stateOff ? 0 : 12;  //default on/dimmed for 12 hours, off in 0 hours.
                durationTimeSpan = AcceptTimeSpan(TokenEnum.token_duration, new TimeSpan(defaultDurationHours, 0, 0));

                if (durationTimeSpan < delayTimeSpan && codeDeviceCommands.GetDeviceState != device_state_t.stateOff)
                {
                    m_LexicalAnalyser.LogError("Duration must be greater than the initial delay");
                }
            }

            StatementSetDevice statementSetDevice = new StatementSetDevice(codeDevices, codeDeviceCommands, delayTimeSpan, durationTimeSpan);

            return statementSetDevice;
        }

        private StatementRefreshDevices RefreshDevices()
        {
            AcceptToken(TokenEnum.token_refreshDevices);
            StatementRefreshDevices statementRefreshDevices = new StatementRefreshDevices();

            return statementRefreshDevices;
        }

        private StatementResynchClock ResynchClock()
        {
            AcceptToken(TokenEnum.token_resynchClock);
            StatementResynchClock resynchClock = new StatementResynchClock();

            return resynchClock;
        }

        private StatementResetTimeout ResetTimer()
        {
            StatementResetTimeout statementResetTimeout = null;

            AcceptToken(TokenEnum.token_reset);
            CodeTimeout codeTimeout = AcceptExistingIdentifier(IdentifierTypeEnum.IdTimeout) as CodeTimeout;
            if (codeTimeout != null)
            {
                TimeSpan durationTimeSpan = AcceptTimeSpan(TokenEnum.token_duration, codeTimeout.DefaultDurationTimeSpan);
                statementResetTimeout = new StatementResetTimeout(codeTimeout, durationTimeSpan);
            }

            return statementResetTimeout;
        }

        TimeSpan AcceptTimeSpan(TokenEnum preparationTokenEnum, TimeSpan defaultDuration)
        {
            TimeSpan timeSpan = defaultDuration;
            if (m_Token == preparationTokenEnum)
            {
                AcceptToken(preparationTokenEnum);
                timeSpan = AcceptTimeSpan();
            }

            return timeSpan;
        }


        private StatementAssignment AssignmentStatement(CodeVariable codeVariable)
        {
            //int value = -1;

            AcceptToken(TokenEnum.token_equals);
            Expression expression = ParseExpression(m_ExpectedDeclarationEnders, ExpressionTypeEnum.TypeBoolean); //TODO use the type of the variable

            StatementAssignment statementAssignment = new StatementAssignment(codeVariable, expression);

            bool ok = ExpectedRhs(TokenEnum.token_equals, expression.GetTypeEnum, codeVariable.GetTypeEnum);
            return statementAssignment;
        }

        private StatementIncrementDecrement IncrementDecrement(CodeVariable codeVariable)
        {
            int value = 0;

            if (m_Token == TokenEnum.token_plusPlus || m_Token == TokenEnum.token_minusMinus)
            {
                value = m_Token == TokenEnum.token_plusPlus ? +1 : -1;
                NextToken();
            }

            StatementIncrementDecrement statementIncrementDecrement = new StatementIncrementDecrement(codeVariable, value);
            return statementIncrementDecrement;
        }

        private StatementAssert AssertStatement()
        {
            AcceptToken(TokenEnum.token_assert);
            Expression expression = ParseExpression(m_ExpectedDeclarationEnders, ExpressionTypeEnum.TypeBoolean);

            StatementAssert statementAssert = new StatementAssert(expression);

            bool ok = ExpectedExpressionType(expression.GetTypeEnum, TypeEnum.BoolType);
            return statementAssert;
        }


        private List<StatementBase> Statements(HashSet<TokenEnum> expectedFollowers)
        {
            HashSet<TokenEnum> validEnds = TokenSet.Set(expectedFollowers, m_ValidProcedureAndDeclarationStarters);
            HashSet<TokenEnum> validEndsIncludingSemicolon = TokenSet.Set(validEnds, TokenEnum.token_semicolon);

            List<StatementBase> statementList = new List<StatementBase>();

            while (m_ValidProcedureStarters.Contains(m_Token))
            {
                switch (m_Token)
                {
                    case TokenEnum.token_if:
                        statementList.Add(IfStatement(expectedFollowers));
                        break;

                    case TokenEnum.token_call_action:
                        statementList.Add(CallStatement());
                        break;

                    case TokenEnum.token_identifier:
                        if (m_IdDictionary.ContainsKey(m_LexicalAnalyser.TokenValue))
                        {
                            switch (m_IdDictionary[m_LexicalAnalyser.TokenValue].IdentifierType)
                            {
                                case IdentifierTypeEnum.IdBool:
                                    CodeVariable codeVariable = AcceptExistingIdentifier(IdentifierTypeEnum.IdBool) as CodeVariable;
                                    switch (m_Token)
                                    {
                                        case TokenEnum.token_equals:
                                            statementList.Add(AssignmentStatement(codeVariable));
                                            break;

                                        case TokenEnum.token_plusPlus:
                                        case TokenEnum.token_minusMinus:
                                            statementList.Add(IncrementDecrement(codeVariable));
                                            break;

                                        default:
                                            m_LexicalAnalyser.LogError("expected '=', '++' or '--'");
                                            break;
                                    }
                                    
                                    break;

                                case IdentifierTypeEnum.IdDevice:
                                case IdentifierTypeEnum.IdProcedure:
                                case IdentifierTypeEnum.IdHouseCode:
                                case IdentifierTypeEnum.IdTimeout: //TODO is this in the right place??
                                default:
                                    m_LexicalAnalyser.LogError("identifier type not allowed in assignment/incr/decr");
                                    AcceptToken(TokenEnum.token_identifier);
                                    break;
                            }
                        }
                        else
                        {
                            m_LexicalAnalyser.LogError("unknown identifier (1)");
                            AcceptToken(TokenEnum.token_identifier);
                        }
                        break;

                    case TokenEnum.token_set_device:
                        statementList.Add(SetDevice(expectedFollowers));
                        break;

                    case TokenEnum.token_refreshDevices:
                        statementList.Add(RefreshDevices());
                        break;

                    case TokenEnum.token_resynchClock:
                        statementList.Add(ResynchClock());
                        break;

                    case TokenEnum.token_reset:
                        statementList.Add(ResetTimer());
                        break;

                    case TokenEnum.token_assert:
                        statementList.Add( AssertStatement());
                        break;

                    default:
                        Debug.Assert(false);
                        break;

                }
                SkipTo(validEndsIncludingSemicolon);
                AcceptToken(TokenEnum.token_semicolon);
                SkipTo(validEnds);

            }
            return statementList;
        }

        private void Procedure(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            HashSet<TokenEnum> expectedDeclarationStartersAndEnd = TokenSet.Set(expectedDeclarationStarters);
            expectedDeclarationStartersAndEnd.Add(TokenEnum.token_end);
            AcceptToken(TokenEnum.token_procedure);

            CodeProcedure codeProcedure = null;
            if (m_Token == TokenEnum.token_identifier)
            {
                //Check for forward declared action name
                if (m_IdDictionary.ContainsKey(m_LexicalAnalyser.TokenValue))
                {
                    CodeBase codeBase = m_IdDictionary[m_LexicalAnalyser.TokenValue];
                    if (codeBase.IdentifierType == IdentifierTypeEnum.IdProcedure)
                    {
                        codeBase.NoteUsage();
                        codeProcedure = (CodeProcedure)codeBase;
                    }
                    else
                    {
                        m_LexicalAnalyser.LogError("New identifier expected/wrong type");
                    }
                }
                else
                {
                    codeProcedure = new CodeProcedure(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, m_LexicalAnalyser.TokenValue);
                    m_IdDictionary.Add(m_LexicalAnalyser.TokenValue, codeProcedure);
                }
                AcceptToken(TokenEnum.token_identifier);
            }
            else
            {
                m_LexicalAnalyser.LogError("procedure name expected");
            }

            if (codeProcedure != null)
            {
                codeProcedure.SetStatementList(
                    Statements(TokenSet.Set(expectedDeclarationStarters, TokenSet.Set(m_ValidProcedureStarters, TokenEnum.token_end))));
            }

            SkipTo(expectedDeclarationStartersAndEnd);
            AcceptToken(TokenEnum.token_end);
        }

        /*
    day_list	: day
        | day_list ';' day 
        ;


    day		: TOKEN_DAY TOKEN_DATE day_attribute_list ';'
            | TOKEN_HOUSECODE error ';'
                { yyerror("error in day"); }
            ;

    day_attribute_list	: day_attribute
                | day_attribute_list day_attribute 
                ;

    day_attribute	: TOKEN_HOLIDAY
            | TOKEN_BST
            | TOKEN_GMT
            ;

         */
        private void Day(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_day);
            string date = AcceptToken(TokenEnum.token_date);
            if (date != null)
            {
                DateTime dateTime = DateTime.Parse(date);
                while (m_ValidDayAttribute.Contains(m_Token))
                {
                    switch (m_Token)
                    {
                        case TokenEnum.token_holiday:
                            m_CodeCalendar.SetDay(dateTime, dayEnum.DAY_NONWORK);
                            break;

                        case TokenEnum.token_bst:
                            m_CodeCalendar.SetDay(dateTime, dayEnum.DAY_BST);
                            break;

                        case TokenEnum.token_gmt:
                            m_CodeCalendar.SetDay(dateTime, dayEnum.DAY_GMT);
                            break;

                        default:
                            Debug.Assert(false);
                            break;
                    }
                    TokenEnum tokenEnum = AcceptValidToken(m_ValidDayAttribute, "GMT/BST/HOLIDAY");
                }
            }
        }

        /*
    timer_list	: timer
        | timer_list ';' timer 
        ;


    timer 		: TOKEN_TIMER TOKEN_IDENTIFER ';' sequence_list
            | TOKEN_HOUSECODE error ';' sequence_list
                { yyerror("error in timer"); }
            ;

    sequence_list	: sequence
            | sequence_list sequence 
            ;

    sequence	: TOKEN_SEQUENCE TOKEN_STRING TOKEN_TIME TOKEN_D ';'
            | TOKEN_SEQUENCE error ';'
                { yyerror("error in sequence"); }
            ;

         */

        private dayEnum MapTokenToDaysToFire(TokenEnum tokenEnum)
        {
            dayEnum daysToFire = 0;

            switch (tokenEnum)
            {
                case TokenEnum.token_mon:
                    daysToFire |= dayEnum.DAY_MON;
                    break;

                case TokenEnum.token_tue:
                    daysToFire |= dayEnum.DAY_TUE;
                    break;

                case TokenEnum.token_wed:
                    daysToFire |= dayEnum.DAY_WED;
                    break;

                case TokenEnum.token_thu:
                    daysToFire |= dayEnum.DAY_THU;
                    break;

                case TokenEnum.token_fri:
                    daysToFire |= dayEnum.DAY_FRI;
                    break;

                case TokenEnum.token_sat:
                    daysToFire |= dayEnum.DAY_SAT;
                    break;

                case TokenEnum.token_sun:
                    daysToFire |= dayEnum.DAY_SUN;
                    break;

                case TokenEnum.token_all:
                    daysToFire |= dayEnum.DAY_MON | dayEnum.DAY_TUE | dayEnum.DAY_WED |
                                dayEnum.DAY_THU | dayEnum.DAY_FRI | dayEnum.DAY_SAT | dayEnum.DAY_SUN;
                    break;

                case TokenEnum.token_working:
                    daysToFire |= dayEnum.DAY_WORK;
                    break;

                case TokenEnum.token_non_working:
                    daysToFire |= dayEnum.DAY_NONWORK;
                    break;

                case TokenEnum.token_first_working:
                    daysToFire |= dayEnum.DAY_NONFIRSTWORK;
                    break;

                case TokenEnum.token_nonFirst_working:
                    daysToFire |= dayEnum.DAY_FIRSTWORK;
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            return daysToFire;
        }

        private void Timer(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            List<CodeSequence> sequenceList = new List<CodeSequence>();

            AcceptToken(TokenEnum.token_timer);
            string timerDescription = AcceptToken(TokenEnum.token_string);
            AcceptToken(TokenEnum.token_semicolon);
            while (m_Token == TokenEnum.token_sequence)
            {
                List<CodeEvent> eventList = new List<CodeEvent>();

                AcceptToken(TokenEnum.token_sequence);
                string seqDescription = AcceptToken(TokenEnum.token_string);

                TimeSpan sequenceFireTime = TimeSpan.MinValue;
                if (m_SequenceFiringTime.Contains(m_Token))
                {
                    switch(m_Token)
					{
                        case TokenEnum.token_time_of_day:
							sequenceFireTime = new TimeSpan(0, 0, 0,m_LexicalAnalyser.IntValue);;
                            if (sequenceFireTime.CompareTo(new TimeSpan(0, 0, 0)) < 0 ||
                                sequenceFireTime.CompareTo(new TimeSpan(23, 59, 0)) > 0)
                            {
                                m_LexicalAnalyser.LogError("sequence timespan not in range 00:00 .. 23:59");
                            }
                            break;
                        case TokenEnum.token_sunrise:
                            sequenceFireTime = CodeCalendar.SUNRISE_TIMESPAN;
							break;

                        case TokenEnum.token_sunset:
                            sequenceFireTime = CodeCalendar.SUNSET_TIMESPAN;
							break;

						default:
							Debug.Assert(false);
                            break;
						}
                }
                TokenEnum timeTokenEnum = AcceptValidToken(m_SequenceFiringTime, "SUNRISE/SUNSET/<time> expected");
                dayEnum daysToFire = MapTokenToDaysToFire(AcceptValidToken(m_ValidDay, "FIRSTWORKING/MON/TUE/WED/ etc"));
                AcceptToken(TokenEnum.token_semicolon);

                while (m_Token == TokenEnum.token_event)
                {
                    AcceptToken(TokenEnum.token_event);
                    TimeSpan eventTimeSpan = TimeSpan.MinValue;
                    if (m_Token == TokenEnum.token_time_of_day)
                    {
                        eventTimeSpan = new TimeSpan(0, 0, 0, m_LexicalAnalyser.IntValue); ;
                        if (eventTimeSpan.CompareTo(new TimeSpan(-12, 0, 0)) < 0 ||
                            eventTimeSpan.CompareTo(new TimeSpan(23, 59, 0)) > 0)
                        {
                            m_LexicalAnalyser.LogError("event timespan not in range -12:00 .. 23:59");
                        }
                    }
                    AcceptToken(TokenEnum.token_time_of_day);
                    CodeProcedure codeProcedure = AcceptExistingIdentifier(IdentifierTypeEnum.IdProcedure) as CodeProcedure;
                    if (eventTimeSpan > TimeSpan.MinValue && codeProcedure != null)
                    {
                        eventList.Add(new CodeEvent(eventTimeSpan, codeProcedure));
                    }
                    AcceptToken(TokenEnum.token_semicolon);
                    SkipTo(TokenSet.Set(expectedDeclarationStarters, TokenEnum.token_end, TokenEnum.token_sequence, TokenEnum.token_event));
                }
                SkipTo(TokenSet.Set(expectedDeclarationStarters, TokenEnum.token_end, TokenEnum.token_sequence));

                sequenceList.Add(new CodeSequence(seqDescription, sequenceFireTime, daysToFire, eventList));
            }
            SkipTo(TokenSet.Set(expectedDeclarationStarters, TokenEnum.token_end));
            AcceptToken(TokenEnum.token_end);
            CodeTimer codeTimer = new CodeTimer(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, timerDescription, sequenceList);
        }


 /*
    goal		: forward_action_list housecode_list device_list flag_list action_list day_list timer_list
            | error
                { yyerror("error in program"); }
            ;


    forward_action_list : forward_action
            | forward_action_list ';' forward_action 
            ;

    forward_action	: 
            | TOKEN_ACTION action_id 
            | TOKEN_ACTION error 
                { yyerror("error in forward action identifier"); }
            ;



         */

        public int ErrorCount
        {
            get { return m_LexicalAnalyser.ErrorCount; }
        }

        public int WarningCount
        {
            get { return m_LexicalAnalyser.WarningCount; }
        }

    }

}
