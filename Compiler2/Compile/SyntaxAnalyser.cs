using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Compiler.Code.Statement;
using compiler2.Code;
using compiler2.Code.ExpressValue;
using compiler2.Code.Statement;
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
        static readonly HashSet<TokenEnum> m_Device;
        static readonly HashSet<TokenEnum> m_BooleanLiteral;
        static readonly HashSet<TokenEnum> m_DeviceLevel;
        static readonly HashSet<TokenEnum> m_DeviceSetCommands;
        static readonly HashSet<TokenEnum> m_ValidActionStarters;
        static readonly HashSet<TokenEnum> m_ValidActionAndDeclarationStarters;
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
                TokenEnum.token_flag, TokenEnum.token_int, TokenEnum.token_enum, TokenEnum.token_const, TokenEnum.token_action_body, TokenEnum.token_timer, TokenEnum.token_day, 
                TokenEnum.token_refreshDevices, TokenEnum.token_resynchClock);


            m_ExpectedDeclarationEnders = TokenSet.Set(TokenEnum.token_semicolon);
            m_Device = TokenSet.Set(TokenEnum.token_lamp, TokenEnum.token_appliance, TokenEnum.token_applianceLamp, TokenEnum.token_hueLamp, TokenEnum.token_sensor, TokenEnum.token_remote);

            m_BooleanLiteral = TokenSet.Set(TokenEnum.token_false, TokenEnum.token_true);

            m_DeviceLevel = TokenSet.Set(TokenEnum.token_on, TokenEnum.token_dim1, TokenEnum.token_dim2, TokenEnum.token_dim3,
                TokenEnum.token_dim4, TokenEnum.token_dim5, TokenEnum.token_dim6, TokenEnum.token_dim7, TokenEnum.token_dim8,
                TokenEnum.token_dim9, TokenEnum.token_dim10, TokenEnum.token_dim11, TokenEnum.token_dim12, TokenEnum.token_dim13, 
                TokenEnum.token_dim14, TokenEnum.token_dim15, TokenEnum.token_dim16, TokenEnum.token_dim17, TokenEnum.token_off);
            m_DeviceSetCommands = TokenSet.Set(TokenEnum.token_on, TokenEnum.token_dim1, TokenEnum.token_dim2, TokenEnum.token_dim3,
                TokenEnum.token_dim4, TokenEnum.token_dim5, TokenEnum.token_dim6, TokenEnum.token_dim7, TokenEnum.token_dim8,
                TokenEnum.token_dim9, TokenEnum.token_dim10, TokenEnum.token_dim11, TokenEnum.token_dim12, TokenEnum.token_dim13,
                TokenEnum.token_dim14, TokenEnum.token_dim15, TokenEnum.token_dim16, TokenEnum.token_dim17, TokenEnum.token_off,
                TokenEnum.token_rgb_colour, TokenEnum.token_colour_loop);

            m_ValidActionStarters = TokenSet.Set(TokenEnum.token_if, TokenEnum.token_call_action, TokenEnum.token_set_device, TokenEnum.token_reset, 
                TokenEnum.token_refreshDevices, TokenEnum.token_resynchClock, TokenEnum.token_identifier);
            m_ValidActionAndDeclarationStarters = TokenSet.Set(m_ValidActionStarters, m_ValidDeclarationStarters);

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
                m_LexicalAnalyser.LogError("Syntax Error. Expected one of " + LexicalAnalyser.GetSpellings(valid));
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
                m_LexicalAnalyser.LogError("expected " + LexicalAnalyser.GetSpelling(expectedTokenEnum));
            }
            return result;
        }

        private string AcceptNewIdentifier()
        {
            string result = null;
            if (m_Token == TokenEnum.token_identifier)
            {
                if (m_IdDictionary.ContainsKey(m_LexicalAnalyser.TokenValue) && 
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

        private CodeAction AcceptNewActionIdentifier()
        {
            CodeAction resultCodeAction = null;
            if (m_Token == TokenEnum.token_identifier)
            {
                if (m_IdDictionary.ContainsKey(m_LexicalAnalyser.TokenValue))
                {
                    CodeAction codeAction = m_IdDictionary[m_LexicalAnalyser.TokenValue] as CodeAction;
                    if (codeAction != null)
                    {
                        codeAction.NoteUsage();
                        resultCodeAction = codeAction;
                    }
                    else
                    {
                        m_LexicalAnalyser.LogError("New identifier expected");
                    }
                }
                else
                {
                    resultCodeAction = null;
                }
                AcceptToken(TokenEnum.token_identifier);
            }
            return resultCodeAction;
        }

        public void Parse()
        {
            while (m_Token != TokenEnum.token_eof)
            {
                if (m_ValidDeclarationStarters.Contains(m_Token))
                {
                    switch (m_Token)
                    {
                        case TokenEnum.token_room:
                            Rooms(m_ExpectedDeclarationEnders);
                            break;

                        //case TokenEnum.token_action:
                            //ForwardAction(m_ExpectedDeclarationEnders);
                            //break;

                        case TokenEnum.token_housecode:
                            HouseCode(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_device:
                            Device(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_timeout:
                            Timeout(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_flag:
                            Flag(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_const:
                            Const(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_int:
                            Integer(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_enum:
                            Enumerated(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_action_body:
                            Action(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_day:
                            Day(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_timer:
                            Timer(m_ExpectedDeclarationEnders);
                            break;

                        case TokenEnum.token_refreshDevices:
                            RefreshDevices();
                            break;

                        case TokenEnum.token_resynchClock:
                            ResynchClock();
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
            m_CodeCalendar.CompleteDefinition();
            CheckUsage();
       }


        private void CheckUsage()
        {
            foreach (CodeBase codeBase in m_IdDictionary.Values)
            {
                if (codeBase.UseCount == 0 && codeBase.IdentifierType != IdentifierTypeEnum.IdHouseCode)
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
                    m_LexicalAnalyser.LogError("unknown identifier");
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
                    m_LexicalAnalyser.LogError("found wrong type");
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
                if (id != null && 
                    !m_IdDictionary.ContainsKey(id))
                {
                    m_IdDictionary.Add(id, new CodeRoom(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, id, value++));
                }
                if (m_Token != TokenEnum.token_semicolon)
                {
                    AcceptToken(TokenEnum.token_comma);
                }
            }
            SkipTo(expectedDeclarationStarters);
        }


        /*
        private void ForwardAction(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_action);
            string actionId = AcceptNewIdentifier();
            if (actionId != null)
            {
                m_IdDictionary.Add(actionId, new CodeAction(actionId));
            }
            SkipTo(expectedDeclarationStarters);
        }
         */

        /*
    housecode_list	: housecode
            | housecode_list ';' housecode 
            ;

    housecode	:
            | TOKEN_HOUSECODE housecode_id TOKEN_HOUSE_CODE
                    off_action_id on_action_id 
            | TOKEN_HOUSECODE error
                { yyerror("error in housecode"); }
            ;

    housecode_id	: TOKEN_IDENTIFIER
            ;

    off_action_id	: TOKEN_IDENTIFIER
            ;

    on_action_id	: TOKEN_IDENTIFIER
            ;


         */
        private void HouseCode(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_housecode);
            string houseCodeId = AcceptNewIdentifier();

            string houseCode = AcceptToken(TokenEnum.token_house_code);
            if (CodeHouseCode.GetEntry(houseCode[0]) != null)
            {
                m_LexicalAnalyser.LogError("Housecode already defined");
            }
            CodeAction offAction = OffOnAction(TokenEnum.token_off);
            CodeAction onAction = OffOnAction(TokenEnum.token_on);

            if (houseCodeId != null && houseCode.Length == 1 && offAction != null && onAction != null)
            {
                m_IdDictionary.Add(houseCodeId, new CodeHouseCode(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, houseCode[0], houseCodeId, offAction, onAction));
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
                    m_LexicalAnalyser.LogError("Unknown identifier");
                }
            }
            return resultCodeBase;
        }

        private CodeAction OffOnAction(TokenEnum preparationTokenEnum)
        {
            CodeAction codeAction = null;
            if (m_Token == preparationTokenEnum)
            {
                AcceptToken(preparationTokenEnum);
                codeAction = AcceptExistingIdentifier(IdentifierTypeEnum.IdAction) as CodeAction;
            }
            return codeAction;
        }


        private void Device(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_device);
            TokenEnum tokenDeviceType = GetDeviceTokenType(expectedDeclarationStarters);

            //identifier is now "roomname . devicename".
            CodeRoom codeRoom = AcceptExistingIdentifier(IdentifierTypeEnum.IdRoom) as CodeRoom;
            AcceptToken(TokenEnum.token_dot);

            string deviceId = AcceptNewIdentifier();

            string houseCode = AcceptToken(TokenEnum.token_house_code);
            if (houseCode == null || CodeHouseCode.GetEntry(houseCode[0]) == null)
            {
                m_LexicalAnalyser.LogError("Housecode not defined");
            }

            switch (tokenDeviceType)
            {
                case TokenEnum.token_lamp:
                case TokenEnum.token_appliance:
                case TokenEnum.token_applianceLamp:
                case TokenEnum.token_sensor:
                case TokenEnum.token_remote:
                {
                    int unitCode = int.Parse(AcceptToken(TokenEnum.token_integer));

                    CodeAction optionalCodeOffAction = OffOnAction(TokenEnum.token_off);
                    CodeAction optionalCodeOnAction = OffOnAction(TokenEnum.token_on);

                    if (deviceId != null && codeRoom != null && m_LexicalAnalyser.Pass == 2)
                    {
                        CodeDevice codeDevice = new CodeDevice(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass,
                            GenerateDevice.MapDevice(tokenDeviceType), codeRoom, deviceId, houseCode[0], unitCode,
                            optionalCodeOffAction, optionalCodeOnAction);
                        ((CodeRoom) m_IdDictionary[codeRoom.Identifier]).CodeDeviceDictionary.Add(deviceId, codeDevice);
                    }
                    break;
                }

                case TokenEnum.token_hueLamp:
                {
                    string hueId = AcceptToken(TokenEnum.token_string);
                    CodeAction optionalCodeOffAction = OffOnAction(TokenEnum.token_off);
                    CodeAction optionalCodeOnAction = OffOnAction(TokenEnum.token_on);

                    if (deviceId != null && codeRoom != null && m_LexicalAnalyser.Pass == 2)
                    {
                        CodeDevice codeDevice = new CodeDevice(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass,
                            GenerateDevice.MapDevice(tokenDeviceType), codeRoom, deviceId, houseCode[0], hueId, optionalCodeOffAction,
                            optionalCodeOnAction);
                        ((CodeRoom) m_IdDictionary[codeRoom.Identifier]).CodeDeviceDictionary.Add(deviceId, codeDevice);
                    }

                    break;
                }
            }
            SkipTo(expectedDeclarationStarters);
        }

        private TimeSpan AcceptTimeSpan()
        {
            TimeSpan timespan = new TimeSpan(0L);

            if (m_Token == TokenEnum.token_time_of_day)
            {
                //TODO only works if you use exact format 00:00:00 (i.e. leading zeros on 2 digits and all valid numbers)
                string hours = m_LexicalAnalyser.TokenValue.Substring(0, 2);
                string minutes = m_LexicalAnalyser.TokenValue.Substring(3, 2);
                string seconds = m_LexicalAnalyser.TokenValue.Substring(6, 2);
                timespan = new TimeSpan(int.Parse(hours), int.Parse(minutes), int.Parse(seconds));
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
            CodeAction timeoutAction = OffOnAction(TokenEnum.token_off);

            if (timeoutId != null &&
                timeoutAction != null)
            {
                CodeTimeout codeTimeout = new CodeTimeout(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, timeoutId, defaultDurationTimeSpan, timeoutAction);
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
                m_LexicalAnalyser.LogError("Expected " + expected);
            }
            return resultTokenEnum;
        }

        private Expression IdentifierListExpression(string[] indentifiers)
        {
            Expression expression1 = new Expression(new Value(5));
            if (m_IdDictionary.ContainsKey(indentifiers[0]))
            {
                CodeBase codebase = m_IdDictionary[indentifiers[0]];
                switch (codebase.IdentifierType)
                {
                    case IdentifierTypeEnum.IdConst:
                        CodeConst codeConst = CodeConst.GetEntry(codebase.EntryNo);
                        expression1 = new Expression(new Value(codeConst.Value));

                        if (indentifiers.Length > 1)
                        {
                            m_LexicalAnalyser.LogError(string.Format("Dotted identifier '{0}' not allowed here", indentifiers[0]));
                        }
                        break;

                    case IdentifierTypeEnum.IdFlag:
                        expression1 = new Expression(new Value(CodeFlag.GetEntry(codebase.EntryNo)));

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

                    case IdentifierTypeEnum.IdAction:
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

        private Expression Factor(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            Expression expression1 = null;

            switch (m_Token)
            {
                case TokenEnum.token_integer:
                    expression1 = new Expression(new Value(m_LexicalAnalyser.IntValue));
                    NextToken();
                    break;

                case TokenEnum.token_false:
                case TokenEnum.token_true:
                    expression1 = new Expression(new Value(m_Token == TokenEnum.token_true ? 1 : 0));
                    NextToken();
                    break;


                case TokenEnum.token_on:
                case TokenEnum.token_dim1:
                case TokenEnum.token_dim2:
                case TokenEnum.token_dim3:
                case TokenEnum.token_dim4:
                case TokenEnum.token_dim5:
                case TokenEnum.token_dim6:
                case TokenEnum.token_dim7:
                case TokenEnum.token_dim8:
                case TokenEnum.token_dim9:
                case TokenEnum.token_dim10:
                case TokenEnum.token_dim11:
                case TokenEnum.token_dim12:
                case TokenEnum.token_dim13:
                case TokenEnum.token_dim14:
                case TokenEnum.token_dim15:
                case TokenEnum.token_dim16:
                case TokenEnum.token_dim17:
                case TokenEnum.token_off:
                    expression1 = new Expression(new Value((int)GenerateDevice.MapDeviceState(m_Token)));
                    NextToken();
                    break;


                //literal constant values
                case TokenEnum.token_string:
                case TokenEnum.token_time_of_day:
                case TokenEnum.token_date:
                case TokenEnum.token_house_code:
                    expression1 = new Expression(new Value(m_LexicalAnalyser.IntValue)); //TODO write some real code here.
                    NextToken(); //TODO Fix
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
                    }
                    break;

                case TokenEnum.token_minus: //unary '-' operator
                    {
                        NextToken();
                        Expression expression2 = Factor(validTokenHashSet, expressionTypeExpected);
                        expression1 = new Expression(UnaryOperator.Negate, expression2);
                    }
                    break;

                case TokenEnum.token_leftParent:
                    NextToken(); //leftParent
                    expression1 = ParseExpression(validTokenHashSet, expressionTypeExpected); //TODO add ')' to expected set
                    AcceptToken(TokenEnum.token_rightParent);
                    break;

                default:
                    m_LexicalAnalyser.LogError("Expected literal constant, variable, or '(' expression ')'");
                    expression1 = new Expression(new Value(5));
                    break;
            }
            return expression1;
        }

        private Expression Term(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            Expression expression1 = Factor(validTokenHashSet, expressionTypeExpected);
            while (m_FactorOperators.Contains(m_Token)) // '*', '/', '%'
            {
                BinaryOperator binaryOperator = m_BinaryOperatorDictionary[m_Token];
                NextToken();
                Expression expression2 = Factor(validTokenHashSet, expressionTypeExpected);
                expression1 = new Expression(expression1, binaryOperator, expression2);
            }
            return expression1;
        }

        private Expression SimpleExpression(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            Expression expression1 = Term(validTokenHashSet, expressionTypeExpected);
            while (m_TermOperators.Contains(m_Token)) // binary '+', '-'
            {
                BinaryOperator binaryOperator = m_BinaryOperatorDictionary[m_Token];
                NextToken();
                Expression expression2 = Term(validTokenHashSet, expressionTypeExpected);
                expression1 = new Expression(expression1, binaryOperator, expression2);
            }
            return expression1;
        }

        private Expression ComparisonExpression(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            Expression expression1 = SimpleExpression(validTokenHashSet, expressionTypeExpected);
            while (m_ComparisonOperators.Contains(m_Token)) // binary <=, <, !=, ==, >=, >
            {
                BinaryOperator binaryOperator = m_BinaryOperatorDictionary[m_Token];
                NextToken();
                Expression expression2 = SimpleExpression(validTokenHashSet, expressionTypeExpected);
                expression1 = new Expression(expression1, binaryOperator, expression2);
            }
            return expression1;
        }

        private Expression LogicalOrExpression(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            Expression expression1 = ComparisonExpression(validTokenHashSet, expressionTypeExpected);
            while (m_Token == TokenEnum.token_or) 
            {
                NextToken();
                Expression expression2 = ComparisonExpression(validTokenHashSet, expressionTypeExpected);
                expression1 = new Expression(expression1, BinaryOperator.LogicalOr, expression2);
            }
            return expression1;
        }

        private Expression ParseExpression(HashSet<TokenEnum> validTokenHashSet, ExpressionTypeEnum expressionTypeExpected)
        {
            Expression expression1 = LogicalOrExpression(validTokenHashSet, expressionTypeExpected);
            while (m_Token == TokenEnum.token_and)
            {
                NextToken();
                Expression expression2 = LogicalOrExpression(validTokenHashSet, expressionTypeExpected);
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
            return value;
        }

        private void Flag(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_flag);
            string flagId = AcceptNewIdentifier();
            TokenEnum boolValue = AcceptValidToken(TokenSet.Set(TokenEnum.token_false, TokenEnum.token_true), "TRUE/FALSE");
            if (flagId != null && m_LexicalAnalyser.Pass == 1)
            {
                m_IdDictionary.Add(flagId, new CodeFlag(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, flagId, boolValue.Equals(TokenEnum.token_true)));
            }

            SkipTo(expectedDeclarationStarters);
        }

        private void Integer(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_int);
            string intId = AcceptNewIdentifier();
            AcceptToken(TokenEnum.token_equals);
            int value = ConstantIntegerExpression();
            if (intId != null && m_LexicalAnalyser.Pass == 1)
            {
                m_IdDictionary.Add(intId, new CodeFlag(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, intId, value));
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
                m_IdDictionary.Add(intId, new CodeConst(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, intId, value));
            }

            SkipTo(expectedDeclarationStarters);
        }

        private void Enumerated(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            int value = 0;

            AcceptToken(TokenEnum.token_enum);
            string enumId = AcceptNewIdentifier(); //Not currently used
            AcceptToken(TokenEnum.token_leftParent);
            while (m_Token == TokenEnum.token_identifier)
            {
                string id = AcceptNewIdentifier();
                if (id != null && m_LexicalAnalyser.Pass == 1)
                {
                    m_IdDictionary.Add(id, new CodeConst(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, id, value++));
                }
                if (m_Token != TokenEnum.token_rightParent)
                {
                    AcceptToken(TokenEnum.token_comma);
                }
            }
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

            Expression expression = ParseExpression(followers, ExpressionTypeEnum.TypeBoolean); //TODO add then to followers
            AcceptToken(TokenEnum.token_then);

            List<StatementBase> thenStatements = Statements(TokenSet.Set(followers, TokenEnum.token_else, TokenEnum.token_endif));
            List<StatementBase> elseStatements = null;
            if (m_Token == TokenEnum.token_else)
            {
                AcceptToken(TokenEnum.token_else);
                AcceptToken(TokenEnum.token_semicolon);
                elseStatements = Statements(TokenSet.Set(followers, TokenEnum.token_endif));
            }
            AcceptToken(TokenEnum.token_endif);

            statementIf = new StatementIf(expression, thenStatements, elseStatements);
            return statementIf;
        }

        private StatementCall CallStatement()
        {
            AcceptToken(TokenEnum.token_call_action);
            CodeAction codeAction = AcceptExistingIdentifier(IdentifierTypeEnum.IdAction) as CodeAction;

            StatementCall statementCall = new StatementCall(codeAction);
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
                    Debug.Assert(false);
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


        private StatementSetDevice SetDevice()
        {
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
                }

            }
            TokenEnum deviceTokenEnum = AcceptRoomDeviceOrDeviceType();

            CodeDeviceCommands codeDeviceCommands = new CodeDeviceCommands();
            TokenEnum deviceLevelTokenEnum = TokenEnum.token_error;
            do
            {
                TokenEnum deviceFeatureTokenEnum = AcceptValidToken(m_DeviceSetCommands, "OFF/ON/DIM1 etc");
                if (m_DeviceSetCommands.Contains(deviceFeatureTokenEnum))
                {
                    switch (deviceFeatureTokenEnum)
                    {
                        case TokenEnum.token_off:
                        case TokenEnum.token_on:
                        case TokenEnum.token_dim1:
                        case TokenEnum.token_dim2:
                        case TokenEnum.token_dim3:
                        case TokenEnum.token_dim4:
                        case TokenEnum.token_dim5:
                        case TokenEnum.token_dim6:
                        case TokenEnum.token_dim7:
                        case TokenEnum.token_dim8:
                        case TokenEnum.token_dim9:
                        case TokenEnum.token_dim10:
                        case TokenEnum.token_dim11:
                        case TokenEnum.token_dim12:
                        case TokenEnum.token_dim13:
                        case TokenEnum.token_dim14:
                        case TokenEnum.token_dim15:
                        case TokenEnum.token_dim16:
                        case TokenEnum.token_dim17:
                            deviceLevelTokenEnum = deviceFeatureTokenEnum;
                            codeDeviceCommands.SetDeviceState(GenerateDevice.MapDeviceState(deviceLevelTokenEnum));
                            break;

                        case TokenEnum.token_rgb_colour:
                            codeDeviceCommands.SetColour(m_LexicalAnalyser.IntValue);
                            break;

                        case TokenEnum.token_colour_loop:
                            codeDeviceCommands.SetColourLoop();
                            break;

                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
            } while (m_DeviceSetCommands.Contains(m_Token));

            CodeDevice[] codeDevices = CalculateHouseDotDevices(codeRoom, deviceTokenEnum, deviceIdentifier, codeDeviceCommands.GetDeviceState);

            //a duration only makes sense if the device is being switched on.
            TimeSpan delayTimeSpan = AcceptTimeSpan(TokenEnum.token_delayed, new TimeSpan(0L));
            TimeSpan durationTimeSpan = new TimeSpan(0, 0, 0);
            if (GenerateDevice.MapDeviceStateContainsKey(deviceLevelTokenEnum))
            {
                int defaultDurationHours = deviceLevelTokenEnum == TokenEnum.token_off ? 0 : 12;  //default on/dimmed for 12 hours, off in 0 hours.
                durationTimeSpan = AcceptTimeSpan(TokenEnum.token_duration, new TimeSpan(defaultDurationHours, 0, 0));

                if (durationTimeSpan < delayTimeSpan && deviceLevelTokenEnum != TokenEnum.token_off)
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


        private StatementAssignment AssignmentStatement(CodeFlag codeFlag)
        {
            //int value = -1;

            AcceptToken(TokenEnum.token_equals);
            Expression expression = ParseExpression(m_ExpectedDeclarationEnders, ExpressionTypeEnum.TypeBoolean); //TODO use the type of the variable
            /*
            if (m_Token == TokenEnum.token_false || m_Token == TokenEnum.token_true)
            {
                value = m_Token == TokenEnum.token_true ? 1 : 0;
                NextToken();
            }
            else
            {
                if (m_Token == TokenEnum.token_integer)
                {
                    value = m_LexicalAnalyser.IntValue;
                    AcceptToken(TokenEnum.token_integer);
                }
                else
                {
                    if (m_Token == TokenEnum.token_identifier)
                    {
                        CodeConst codeConst = AcceptExistingIdentifier(IdentifierTypeEnum.IdConst) as CodeConst;
                        if (codeConst != null)
                        {
                            value = codeConst.Value;
                        }
                    }
                    else
                    {
                        m_LexicalAnalyser.LogError("value expected");
                    }
                }
            }
             */

            StatementAssignment statementAssignment = new StatementAssignment(codeFlag, expression);
            return statementAssignment;
        }

        private StatementIncrementDecrement IncrementDecrement(CodeFlag codeFlag)
        {
            int value = 0;

            if (m_Token == TokenEnum.token_plusPlus || m_Token == TokenEnum.token_minusMinus)
            {
                value = m_Token == TokenEnum.token_plusPlus ? +1 : -1;
                NextToken();
            }

            StatementIncrementDecrement statementIncrementDecrement = new StatementIncrementDecrement(codeFlag, value);
            return statementIncrementDecrement;
        }

        private List<StatementBase> Statements(HashSet<TokenEnum> expectedFollowers)
        {
            HashSet<TokenEnum> validEnds = TokenSet.Set(expectedFollowers, m_ValidActionAndDeclarationStarters);

            List<StatementBase> statementList = new List<StatementBase>();

            while (m_ValidActionStarters.Contains(m_Token))
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
                                case IdentifierTypeEnum.IdFlag:
                                    CodeFlag codeFlag = AcceptExistingIdentifier(IdentifierTypeEnum.IdFlag) as CodeFlag;
                                    switch (m_Token)
                                    {
                                        case TokenEnum.token_equals:
                                            statementList.Add(AssignmentStatement(codeFlag));
                                            break;

                                        case TokenEnum.token_plusPlus:
                                        case TokenEnum.token_minusMinus:
                                            statementList.Add(IncrementDecrement(codeFlag));
                                            break;

                                        default:
                                            m_LexicalAnalyser.LogError("expected '=', '++' or '--'");
                                            break;
                                    }
                                    
                                    break;

                                case IdentifierTypeEnum.IdDevice:
                                case IdentifierTypeEnum.IdAction:
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
                            m_LexicalAnalyser.LogError("unknown identifier");
                            AcceptToken(TokenEnum.token_identifier);
                        }
                        break;

                    case TokenEnum.token_set_device:
                        statementList.Add(SetDevice());
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

                    default:
                        Debug.Assert(false);
                        break;

                }
                AcceptToken(TokenEnum.token_semicolon);
                SkipTo(validEnds);

            }
            return statementList;
        }

        private void Action(HashSet<TokenEnum> expectedDeclarationStarters)
        {
            AcceptToken(TokenEnum.token_action_body);

            CodeAction codeAction = null;
            if (m_Token == TokenEnum.token_identifier)
            {
                //Check for forward declared action name
                if (m_IdDictionary.ContainsKey(m_LexicalAnalyser.TokenValue))
                {
                    CodeBase codeBase = m_IdDictionary[m_LexicalAnalyser.TokenValue];
                    if (codeBase.IdentifierType == IdentifierTypeEnum.IdAction)
                    {
                        codeBase.NoteUsage();
                        codeAction = (CodeAction)codeBase;
                    }
                    else
                    {
                        m_LexicalAnalyser.LogError("New identifier expected/wrong type");
                    }
                }
                else
                {
                    codeAction = new CodeAction(m_LexicalAnalyser.LineNumber, m_LexicalAnalyser.Pass, m_LexicalAnalyser.TokenValue);
                    m_IdDictionary.Add(m_LexicalAnalyser.TokenValue, codeAction);
                }
                AcceptToken(TokenEnum.token_identifier);
            }

            AcceptToken(TokenEnum.token_semicolon);

            codeAction.SetStatementList(
                Statements(TokenSet.Set(expectedDeclarationStarters, TokenSet.Set(m_ValidActionStarters, TokenEnum.token_end))));
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
							sequenceFireTime = m_LexicalAnalyser.TimeSpanValue;
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
                        eventTimeSpan = m_LexicalAnalyser.TimeSpanValue;
                        if (eventTimeSpan.CompareTo(new TimeSpan(-12, 0, 0)) < 0 ||
                            eventTimeSpan.CompareTo(new TimeSpan(23, 59, 0)) > 0)
                        {
                            m_LexicalAnalyser.LogError("event timespan not in range -12:00 .. 23:59");
                        }
                    }
                    AcceptToken(TokenEnum.token_time_of_day);
                    CodeAction codeAction = AcceptExistingIdentifier(IdentifierTypeEnum.IdAction) as CodeAction;
                    if (eventTimeSpan > TimeSpan.MinValue && codeAction != null)
                    {
                        eventList.Add(new CodeEvent(eventTimeSpan, codeAction));
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


        void ResetFlagStatement()
        {
            //TIMEOUT ConservatoryEmptyTimer 00:10:00 NoOneInConservatory;

            AcceptToken(TokenEnum.token_timeout);
            string timoutDescription = AcceptToken(TokenEnum.token_string);
            AcceptToken(TokenEnum.token_time_of_day);

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
