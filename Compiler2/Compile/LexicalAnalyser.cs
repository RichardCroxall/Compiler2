/*
    \file LexicalAnalyser.cs
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
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using Compiler2.Compile;

namespace compiler2.Compile
{
    public enum TokenEnum
    {
        // ReSharper disable InconsistentNaming
        token_bool,
        token_const,
        token_int,
        token_enum,
        token_device,
        token_timeout,
        token_lamp,
        token_appliance,
        token_applianceLamp,
        token_hueLamp,
        token_sensor,
        token_remote,
        token_housecode,
        token_room,
        token_rules,
        //token_action,
        token_procedure,
        token_end,
        token_set_device,
        token_onProcedure,
        token_offProcedure,
        token_delayed,
        token_duration,
        token_refreshDevices, //bring external devices up to date with internal state.
        token_resynchClock, //bring internal clock into synch with host's real time clock
        token_device_state,
        token_colour_loop,
        token_if,
        token_not,
        token_then,
        token_else,
        token_endif,
        token_and,
        token_or,
        token_call_action,
        token_message,
        token_reset, //timer
        token_day,
        token_holiday,
        token_bst,
        token_gmt,
        token_sunrise,
        token_sunset,
        token_timer,
        token_sequence,
        token_assert,
        token_mon,
        token_tue,
        token_wed,
        token_thu,
        token_fri,
        token_sat,
        token_sun,
        token_all,
        token_working,
        token_non_working,
        token_first_working,
        token_nonFirst_working,
        token_event,
        token_false,
        token_true,
        token_eof,
        token_identifier,
        token_integer,
        token_rgb_colour,
        token_string,
        token_time_of_day,
        token_date,
        token_house_code,
        token_semicolon,
        token_leftParent,
        token_rightParent,
        token_comma,
        token_dot,
        token_equals,
        token_equalsequals,
        token_notequals,
        token_lessthan,
        token_lessthanEquals,
        token_greaterthan,
        token_greaterthanEquals,
        token_plusPlus,
        token_minusMinus,
        token_plus,
        token_minus,
        token_times,
        token_div,
        token_mod,
        token_error,
        // ReSharper restore InconsistentNaming
    }

    public enum TypeEnum
    {
        OtherType,
        RoomType,
        UnitType,
        DeviceType,
        HouseCodeType,
        TimerType,
        EnumType,
        IntType,
        BoolType,
        StringType,
        DayType,
        DateType,
        TimeType,
        ColourType,
        ColourLoopType,
        DeviceStateType
    };

    class LexicalAnalyser
    {
        private const int MAX_TOKEN_SIZE = 16;
        private static readonly Dictionary<string, TokenEnum> m_reservedWordDictionary = new Dictionary<string, TokenEnum>();
        private static readonly Dictionary<TokenEnum, string> m_TokenDictionary = new Dictionary<TokenEnum, string>();

        private static readonly Dictionary<TypeEnum, string> m_TypeDictionary = new Dictionary<TypeEnum, string>();

        static LexicalAnalyser()
        {
	         m_reservedWordDictionary.Add("BOOL", TokenEnum.token_bool);
             m_reservedWordDictionary.Add("CONST", TokenEnum.token_const);
             m_reservedWordDictionary.Add("INT", TokenEnum.token_int);
             m_reservedWordDictionary.Add("ENUM", TokenEnum.token_enum);
             m_reservedWordDictionary.Add("DEVICE", TokenEnum.token_device);
             m_reservedWordDictionary.Add("TIMEOUT", TokenEnum.token_timeout);
	         m_reservedWordDictionary.Add("LAMP", TokenEnum.token_lamp);
	         m_reservedWordDictionary.Add("APPLIANCE", TokenEnum.token_appliance);
             m_reservedWordDictionary.Add("APPLIANCELAMP", TokenEnum.token_applianceLamp);
             m_reservedWordDictionary.Add("HUELAMP", TokenEnum.token_hueLamp);
             m_reservedWordDictionary.Add("SENSOR", TokenEnum.token_sensor);
             m_reservedWordDictionary.Add("REMOTE", TokenEnum.token_remote);
             //m_reservedWordDictionary.Add("ACTION", TokenEnum.token_action);
	         m_reservedWordDictionary.Add("PROCEDURE", TokenEnum.token_procedure);
	         m_reservedWordDictionary.Add("END", TokenEnum.token_end);
	         m_reservedWordDictionary.Add("SETDEVICE", TokenEnum.token_set_device);
             m_reservedWordDictionary.Add("ONPROCEDURE", TokenEnum.token_onProcedure);
             m_reservedWordDictionary.Add("OFFPROCEDURE", TokenEnum.token_offProcedure);

             m_reservedWordDictionary.Add("DELAYED", TokenEnum.token_delayed);
             m_reservedWordDictionary.Add("DURATION", TokenEnum.token_duration);
             m_reservedWordDictionary.Add("REFRESHDEVICES", TokenEnum.token_refreshDevices);
             m_reservedWordDictionary.Add("RESYNCHCLOCK", TokenEnum.token_resynchClock);

             m_reservedWordDictionary.Add("COLOURLOOP", TokenEnum.token_colour_loop);
             m_reservedWordDictionary.Add("COLORLOOP", TokenEnum.token_colour_loop);
	         m_reservedWordDictionary.Add("IF", TokenEnum.token_if);
             m_reservedWordDictionary.Add("THEN", TokenEnum.token_then);
             m_reservedWordDictionary.Add("ELSE", TokenEnum.token_else);
	         m_reservedWordDictionary.Add("ENDIF", TokenEnum.token_endif);
	         m_reservedWordDictionary.Add("AND", TokenEnum.token_and);
             m_reservedWordDictionary.Add("OR", TokenEnum.token_or);
             m_reservedWordDictionary.Add("CALL", TokenEnum.token_call_action);
	         m_reservedWordDictionary.Add("MESSAGE", TokenEnum.token_message);
             m_reservedWordDictionary.Add("RESET", TokenEnum.token_reset);
             m_reservedWordDictionary.Add("DAY", TokenEnum.token_day);
	         m_reservedWordDictionary.Add("HOLIDAY", TokenEnum.token_holiday);
	         m_reservedWordDictionary.Add("HOUSECODE", TokenEnum.token_housecode);
             m_reservedWordDictionary.Add("ROOM", TokenEnum.token_room);
             m_reservedWordDictionary.Add("RULES", TokenEnum.token_rules);
             m_reservedWordDictionary.Add("BST", TokenEnum.token_bst);
	         m_reservedWordDictionary.Add("GMT", TokenEnum.token_gmt);
	         m_reservedWordDictionary.Add("SUNRISE", TokenEnum.token_sunrise);
	         m_reservedWordDictionary.Add("SUNSET", TokenEnum.token_sunset);
	         m_reservedWordDictionary.Add("TIMER", TokenEnum.token_timer);
	         m_reservedWordDictionary.Add("SEQUENCE", TokenEnum.token_sequence);
             m_reservedWordDictionary.Add("ASSERT", TokenEnum.token_assert);
            m_reservedWordDictionary.Add("MON", TokenEnum.token_mon);
	         m_reservedWordDictionary.Add("TUE", TokenEnum.token_tue);
	         m_reservedWordDictionary.Add("WED", TokenEnum.token_wed);
	         m_reservedWordDictionary.Add("THU", TokenEnum.token_thu);
	         m_reservedWordDictionary.Add("FRI", TokenEnum.token_fri);
	         m_reservedWordDictionary.Add("SAT", TokenEnum.token_sat);
	         m_reservedWordDictionary.Add("SUN", TokenEnum.token_sun);
	         m_reservedWordDictionary.Add("ALL", TokenEnum.token_all);
	         m_reservedWordDictionary.Add("WORKING", TokenEnum.token_working);
	         m_reservedWordDictionary.Add("NONWORKING", TokenEnum.token_non_working);
	         m_reservedWordDictionary.Add("FIRSTWORKING", TokenEnum.token_first_working);
             m_reservedWordDictionary.Add("NONFIRSTWORKING", TokenEnum.token_nonFirst_working);
             m_reservedWordDictionary.Add("EVENT", TokenEnum.token_event);
	         m_reservedWordDictionary.Add("FALSE", TokenEnum.token_false);
	         m_reservedWordDictionary.Add("TRUE", TokenEnum.token_true);
             m_reservedWordDictionary.Add("NOT", TokenEnum.token_not);
             m_reservedWordDictionary.Add(";", TokenEnum.token_semicolon);
             m_reservedWordDictionary.Add(",", TokenEnum.token_comma);
             m_reservedWordDictionary.Add(".", TokenEnum.token_dot);
             m_reservedWordDictionary.Add("(", TokenEnum.token_leftParent);
             m_reservedWordDictionary.Add(")", TokenEnum.token_rightParent);
             m_reservedWordDictionary.Add("=", TokenEnum.token_equals);
             m_reservedWordDictionary.Add("*", TokenEnum.token_times);
             m_reservedWordDictionary.Add("/", TokenEnum.token_div);
             m_reservedWordDictionary.Add("%", TokenEnum.token_mod);

             m_TokenDictionary.Add(TokenEnum.token_equalsequals, "==");
             m_TokenDictionary.Add(TokenEnum.token_notequals, "!=");
             m_TokenDictionary.Add(TokenEnum.token_lessthan, "<");
             m_TokenDictionary.Add(TokenEnum.token_lessthanEquals, "<=");
             m_TokenDictionary.Add(TokenEnum.token_greaterthan, ">");
             m_TokenDictionary.Add(TokenEnum.token_greaterthanEquals, ">=");
             m_TokenDictionary.Add(TokenEnum.token_plusPlus, "++");
             m_TokenDictionary.Add(TokenEnum.token_minusMinus, "--");
             m_TokenDictionary.Add(TokenEnum.token_plus, "+");
             m_TokenDictionary.Add(TokenEnum.token_minus, "-");
             m_TokenDictionary.Add(TokenEnum.token_identifier, "<identifier>");
             m_TokenDictionary.Add(TokenEnum.token_integer, "<integer>");
             m_TokenDictionary.Add(TokenEnum.token_string, "<string>");
             m_TokenDictionary.Add(TokenEnum.token_time_of_day, "<time>");
             m_TokenDictionary.Add(TokenEnum.token_date, "<date>");
             m_TokenDictionary.Add(TokenEnum.token_house_code, "<house code>");
             m_TokenDictionary.Add(TokenEnum.token_rgb_colour, "<colour>");
             m_TokenDictionary.Add(TokenEnum.token_device_state, "<device state>");
             m_TokenDictionary.Add(TokenEnum.token_error, "?");
             m_TokenDictionary.Add(TokenEnum.token_eof, "End-Of-File");


            foreach(string reservedWord in m_reservedWordDictionary.Keys)
            {
                //allow for multiple spellings of COLOURLOOP/COLORLOOP.
                if (!m_TokenDictionary.ContainsKey(m_reservedWordDictionary[reservedWord]))
                {
                    m_TokenDictionary.Add(m_reservedWordDictionary[reservedWord], reservedWord);
                }
            }

            //allow for lower case keywords
            List<string> upperCaseKeys = m_reservedWordDictionary.Keys.ToList();
            foreach (string reservedWord in upperCaseKeys)
            {
                if (char.IsLetter(reservedWord[0]))
                {
                    TokenEnum token = m_reservedWordDictionary[reservedWord];
                    m_reservedWordDictionary.Add(reservedWord.ToLower(), token);
                }
            }

            m_TypeDictionary.Add(TypeEnum.OtherType, "other-type");
            m_TypeDictionary.Add(TypeEnum.RoomType, "room type");
            m_TypeDictionary.Add(TypeEnum.UnitType, "device-unit type");
            m_TypeDictionary.Add(TypeEnum.DeviceType, "device type");
            m_TypeDictionary.Add(TypeEnum.HouseCodeType, "house-code type");
            m_TypeDictionary.Add(TypeEnum.TimerType, "timer type");
            m_TypeDictionary.Add(TypeEnum.EnumType, "enum type");
            m_TypeDictionary.Add(TypeEnum.IntType, "int");
            m_TypeDictionary.Add(TypeEnum.BoolType, "boolean");
            m_TypeDictionary.Add(TypeEnum.StringType, "string");
            m_TypeDictionary.Add(TypeEnum.DayType, "day type");
            m_TypeDictionary.Add(TypeEnum.DateType, "date type");
            m_TypeDictionary.Add(TypeEnum.TimeType, "time-of-day type");
            m_TypeDictionary.Add(TypeEnum.ColourType, "colour type");
            m_TypeDictionary.Add(TypeEnum.ColourLoopType, "colour loop type");
            m_TypeDictionary.Add(TypeEnum.DeviceStateType, "device state type");
        }

        public static string GetSpelling(TokenEnum tokenEnum)
        {
            string spelling = m_TokenDictionary[tokenEnum];
            return spelling;
        }

        public static string GetSpelling(TypeEnum typeEnum)
        {
            string spelling = m_TypeDictionary[typeEnum];
            return spelling;
        }

        public static string GetSpellings(HashSet<TokenEnum> tokenEnumHashSet)
        {
            StringBuilder spellings = new StringBuilder();
            int counter = 0;
            foreach (TokenEnum tokenEnum in tokenEnumHashSet)
            {
                if (spellings.Length > 0)
                {
                    if (counter < tokenEnumHashSet.Count - 1)
                    {
                        spellings.Append(", ");
                    }
                    else
                    {
                        spellings.Append(" or ");
                    }
                }

                if (char.IsLetter(m_TokenDictionary[tokenEnum][0]))
                {
                    spellings.Append(m_TokenDictionary[tokenEnum]);
                }
                else
                {
                    spellings.Append(string.Format("'{0}'", m_TokenDictionary[tokenEnum]));

                }

                counter++;
            }
            return spellings.ToString();
        }

        const char EOF = (char)0xFFFF;
        private char m_PreviousCh = '?';
        private char m_ch;  // current character being processed
        private int m_CharPosition = 0;
        private int m_TokenStartCharPosition = 0;
        private string inputFile;
        TokenEnum token = TokenEnum.token_error;
        private string m_TokenValue;                   //string, name, etc
        private int m_TokenIntegerValue;             // value of integer token, also days since 00:00:00 on 1 Jan 1970, also seconds +/- since midnight.
        private int m_LineNumber = 1;
        private int m_PreviousTokenLineNumber = 0;
        private string m_Line;
        private string m_PreviousErrorLine = "";
        private int m_previousErrorLineNumber = 0;
        private int m_ErrorCount = 0;
        private int m_WarningCount = 0;
        private TypeEnum m_typeEnum = TypeEnum.OtherType;

        private readonly int _pass;

        public LexicalAnalyser(string sourceFile, int pass)
        {
            Debug.Assert(pass >= 1 && pass <= 2);
            _pass = pass;

            TextReader textReader = new StreamReader(sourceFile);
            Debug.Assert(textReader != null);
            inputFile = textReader.ReadToEnd() + EOF;

            NextCh();
        }

        private void NextCh()
        {
            m_PreviousCh = m_ch;
            m_ch = inputFile[m_CharPosition++];
            if (m_ch == '\n')
            {
                m_LineNumber++;
                m_Line = "";
            }
            else if (m_ch == '\r')
            {
            }
            else if (m_ch == '\t')
            {
                m_Line += "    ".Substring(0, 4 - m_Line.Length % 4);
            }
            else
            {
                m_Line += m_ch;
            }
        }


        private void SkipAnyWhiteSpace()
        {
	         while (m_ch == ' ' || m_ch == '\t' || m_ch == '\f' || m_ch == '\n' || m_ch == '\r')
	         {
		          NextCh();
	         }
        }

        public TokenEnum Readtoken()
        {
	         int tokenSize = 0;
             token = TokenEnum.token_error;
             m_TokenValue = string.Empty;
             m_typeEnum = TypeEnum.OtherType;

            SkipAnyWhiteSpace();
	        //skip any comments
	         while (m_ch== '/')
	         {
		          NextCh();
                  if (m_ch == '/')
                  {
                      //found comment, skip to end of line or file
                      while (m_ch != '\n' && m_ch != '\r' && m_ch != EOF)
                      {
                          NextCh();
                      }

                      // skip white space to next significant character
                      SkipAnyWhiteSpace();
                  }
                  else
                  {
                      LogError("single slash found");
                  }
	         }

             m_PreviousTokenLineNumber = m_LineNumber;
             m_TokenStartCharPosition = m_CharPosition;
	         switch(m_ch)
	         {
		          case 'a':
		          case 'b':
		          case 'c':
		          case 'd':
		          case 'e':
		          case 'f':
		          case 'g':
		          case 'h':
		          case 'i':
		          case 'j':
		          case 'k':
		          case 'l':
		          case 'm':
		          case 'n':
		          case 'o':
		          case 'p':
		          case 'q':
		          case 'r':
		          case 's':
		          case 't':
		          case 'u':
		          case 'v':
		          case 'w':
		          case 'x':
		          case 'y':
		          case 'z':
		          case 'A':
		          case 'B':
		          case 'C':
		          case 'D':
		          case 'E':
		          case 'F':
		          case 'G':
		          case 'H':
		          case 'I':
		          case 'J':
		          case 'K':
		          case 'L':
		          case 'M':
		          case 'N':
		          case 'O':
		          case 'P':
		          case 'Q':
		          case 'R':
		          case 'S':
		          case 'T':
		          case 'U':
		          case 'V':
		          case 'W':
		          case 'X':
		          case 'Y':
		          case 'Z':
				        // collect an identifier with usual rules
				        do
				        {
					         m_TokenValue += m_ch;
					         Debug.Assert(tokenSize < MAX_TOKEN_SIZE);
					         NextCh();
				        } while ((m_ch >= 'A' && m_ch <= 'Z') ||
							        (m_ch >= 'a' && m_ch <= 'z') ||
							        (m_ch >= '0' && m_ch <= '9') ||
							        (m_ch == '_' )                );

				        // assume for moment that it isn't reserved word
				        token = TokenEnum.token_identifier;

				        // check for reserved words
                        if (m_reservedWordDictionary.ContainsKey(m_TokenValue))
                        {
                            token = m_reservedWordDictionary[m_TokenValue];
                        }
                        // check for colours
                        else if (Colours.ContainsKey(m_TokenValue))
                        {
                            token = TokenEnum.token_rgb_colour;
                            m_TokenIntegerValue = Colours.RGBColour(m_TokenValue);
                        }
                        // check for device states
                        else if (DeviceStates.ContainsKey(m_TokenValue))
                        {
                            token = TokenEnum.token_device_state;
                            m_TokenIntegerValue = DeviceStates.DeviceStateValue(m_TokenValue);
                        }
                        // check for house code
                    else if (m_TokenValue.Length == 1 && m_TokenValue[0] >= 'A' && m_TokenValue[0] <= 'P')
				        {
					         token = TokenEnum.token_house_code;
                             m_typeEnum = TypeEnum.HouseCodeType;
				        }
                    //otherwise a normal identifier.
                    break;

		          case '-':
		          case '0':
		          case '1':
		          case '2':
		          case '3':
		          case '4':
		          case '5':
		          case '6':
		          case '7':
		          case '8':
		          case '9':
				        token = TokenEnum.token_integer;
				        do
				        {
					         m_TokenValue += m_ch;
					         Debug.Assert(tokenSize < MAX_TOKEN_SIZE);
					         NextCh();
                             m_typeEnum = TypeEnum.IntType;

                            if (m_ch == ':')
					         {
						          token = TokenEnum.token_time_of_day;
                                  m_typeEnum = TypeEnum.TimeType;
                            }
                            if (m_ch == '/')
					         {
						          token = TokenEnum.token_date;
                                  m_typeEnum = TypeEnum.DateType;
					         }
                    } while ((m_ch >= '0' && m_ch <= '9') ||
							         m_ch == ':' ||
							         m_ch == '/');

                        //parse int, date or time value
				        switch (token)
				        {
					        case TokenEnum.token_integer:
						        m_TokenIntegerValue = int.Parse(m_TokenValue);
                                m_typeEnum = TypeEnum.IntType;
						        break;

					        case TokenEnum.token_date:
                            {
                                DateTime epochDateTime = new DateTime(1970, 1, 1);
                                DateTime tokenDateValue = DateTime.Parse(m_TokenValue);
                                m_TokenIntegerValue = tokenDateValue.Subtract(epochDateTime).Days;
                                m_typeEnum = TypeEnum.DateType;
						        }
                            break;

					        case TokenEnum.token_time_of_day:
						        {
                                    // if there was an initial minus sign, convert time to negative.
                                    bool negative = false;
                                    if (m_TokenValue[0] == '-')
                                    {
                                        negative = true;
                                        m_TokenValue = m_TokenValue.Substring(1);
                                    }

                                    TimeSpan tokenTimeValue = TimeSpan.Parse(m_TokenValue);


                                    if (negative)
							        {
                                        tokenTimeValue = -tokenTimeValue;
							        }

                                    m_TokenIntegerValue = (int) tokenTimeValue.TotalSeconds;
                                    m_typeEnum = TypeEnum.TimeType;

                            }
                            break;
					        }
				        break;

		          case '"':
                        token = TokenEnum.token_string;
				        // skip opening quote
				        NextCh();
				        do
				        {
					         m_TokenValue += m_ch;
					         Debug.Assert(tokenSize < MAX_TOKEN_SIZE);
                             m_typeEnum = TypeEnum.StringType;
					         NextCh();

				        } while (m_ch != '"' && m_ch!= EOF);

				        // don't save closing quote
				        NextCh();


				        break;

		          case ';':
                  case ',':
                  case '.':
                  case '(':
                  case ')':
                  case '*':
                  case '/':
                  case '%':
                        m_TokenValue += m_ch;
				        NextCh();

                        token = m_reservedWordDictionary[m_TokenValue];
                        break;

                  case '!':
                        m_TokenValue += m_ch;
                        NextCh();
                        if (m_ch == '=')
                        {
                            m_TokenValue += m_ch;
                            NextCh();
                            token = TokenEnum.token_notequals;
                        }
                        else
                        {
                            token = TokenEnum.token_not;
                        }
                        break;

                  case '=':
                        m_TokenValue += m_ch;
                        token = TokenEnum.token_equals;
                        NextCh();
                        if (m_ch == '=')
                        {
                            m_TokenValue += m_ch;
                            token = TokenEnum.token_equalsequals;
                        }
                        NextCh();
                        break;


                  case '<':
                        m_TokenValue += m_ch;
                        NextCh();
                        if (m_ch == '=')
                        {
                            m_TokenValue += m_ch;
                            NextCh();
                            token = TokenEnum.token_lessthanEquals;
                        }
                        else if (m_ch == '>')
                        {
                            m_TokenValue += m_ch;
                            NextCh();
                            token = TokenEnum.token_notequals;
                        }
                        else
                        {
                            token = TokenEnum.token_lessthan;
                        }
                        break;

                  case '>':
                        m_TokenValue += m_ch;
                        NextCh();
                        if (m_ch == '=')
                        {
                            m_TokenValue += m_ch;
                            NextCh();
                            token = TokenEnum.token_greaterthanEquals;
                        }
                        else
                        {
                            token = TokenEnum.token_greaterthan;
                        }
                        break;


                case '&':
                    m_TokenValue += m_ch;
                    NextCh();
                    if (m_ch == '&')
                    {
                        m_TokenValue += m_ch;
                        NextCh();
                        token = TokenEnum.token_and;
                    }
                    else
                    {
                        token = TokenEnum.token_error;
                        LogError("bad token");
                    }
                    break;

                  case '|':
                      m_TokenValue += m_ch;
                      NextCh();
                      if (m_ch == '|')
                      {
                          m_TokenValue += m_ch;
                          NextCh();
                          token = TokenEnum.token_or;
                      }
                      else
                      {
                          token = TokenEnum.token_error;
                          LogError("bad token");
                      }
                      break;
                       
                case EOF:
				        token = TokenEnum.token_eof;
				        break;


		          default:
				        m_TokenValue += m_ch;
				        NextCh();

				        token = TokenEnum.token_error;
                        LogError("bad token");
				        break;
	         }

        #if DEBUG2
	         printf("token = %d, %s\n", (int) token, tokenValue);
        #endif

	         return token;
        }

        public String PositionArrows()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int tokenLength = Math.Max(0, m_CharPosition - m_TokenStartCharPosition);

            for (int i = 0; i < m_Line.Length - tokenLength - 1; i++)
            {
                stringBuilder.Append('.');
            }

            for (int i = 0; i < tokenLength; i++)
            {
                stringBuilder.Append('^');
            }

            return stringBuilder.ToString();
        }

        public void LogError(string message, int pass = 2)
        {
            if (_pass == pass)
            {
                m_ErrorCount++;

                //only report error line and positions once
                if (m_previousErrorLineNumber != m_LineNumber ||
                    m_PreviousErrorLine != m_Line)
                {
                    Console.WriteLine(m_Line);
                    Console.WriteLine(PositionArrows());
                }

                Console.WriteLine("Error: " + message + " at '" + m_TokenValue + "' on line " + m_LineNumber.ToString());
                m_previousErrorLineNumber = m_LineNumber;
                m_PreviousErrorLine = m_Line;
            }
        }

        public void LogPass1Error(string message)
        {
            LogError(message, 1);
        }
        public void LogWarn(string message)
        {
            if (_pass > 1)
            {
                m_WarningCount++;
                Console.WriteLine("Warning: " + message + " at " + m_TokenValue + " on line " + m_LineNumber.ToString());
            }
        }


        public int IntValue
        {
            get
            {
                return m_TokenIntegerValue;
            }
        }

        public string TokenValue
        {
            get
            {
                return m_TokenValue;
            }
        }

        public int ErrorCount
        {
            get { return m_ErrorCount; }
        }

        public int WarningCount
        {
            get { return m_WarningCount; }
        }

        public int Pass { get { return _pass; } }
        public int LineNumber { get { return m_LineNumber; } }

        public int PreviousTokenLineNumber { get { return m_PreviousTokenLineNumber; } }

        public TypeEnum GetTypeEnum { get { return m_typeEnum;} }
    }
}
