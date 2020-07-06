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

namespace compiler2.Compile
{
    enum TokenEnum
    {
        // ReSharper disable InconsistentNaming
        token_flag,
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
        //token_action,
        token_action_body,
        token_end,
        token_set_device,
        token_on,
        token_off,
        token_delayed,
        token_duration,
        token_refreshDevices, //bring external devices up to date with internal state.
        token_resynchClock, //bring internal clock into synch with host's real time clock
        token_dim1,
        token_dim2,
        token_dim3,
        token_dim4,
        token_dim5,
        token_dim6,
        token_dim7,
        token_dim8,
        token_dim9,
        token_dim10,
        token_dim11,
        token_dim12,
        token_dim13,
        token_dim14,
        token_dim15,
        token_dim16,
        token_dim17,
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

    class LexicalAnalyser
    {
        private const int MAX_TOKEN_SIZE = 16;
        private static readonly Dictionary<string, TokenEnum> m_reservedWordDictionary = new Dictionary<string, TokenEnum>();
        private static readonly Dictionary<TokenEnum, string> m_TokenDictionary = new Dictionary<TokenEnum, string>();

        static LexicalAnalyser()
        {
	         m_reservedWordDictionary.Add("FLAG", TokenEnum.token_flag);
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
	         m_reservedWordDictionary.Add("BODY", TokenEnum.token_action_body);
	         m_reservedWordDictionary.Add("END", TokenEnum.token_end);
	         m_reservedWordDictionary.Add("SETDEVICE", TokenEnum.token_set_device);
	         m_reservedWordDictionary.Add("ON", TokenEnum.token_on);
	         m_reservedWordDictionary.Add("OFF", TokenEnum.token_off);
             m_reservedWordDictionary.Add("DELAYED", TokenEnum.token_delayed);
             m_reservedWordDictionary.Add("DURATION", TokenEnum.token_duration);
             m_reservedWordDictionary.Add("REFRESHDEVICES", TokenEnum.token_refreshDevices);
             m_reservedWordDictionary.Add("RESYNCHCLOCK", TokenEnum.token_resynchClock);

             m_reservedWordDictionary.Add("DIM1", TokenEnum.token_dim1);
	         m_reservedWordDictionary.Add("DIM2", TokenEnum.token_dim2);
	         m_reservedWordDictionary.Add("DIM3", TokenEnum.token_dim3);
	         m_reservedWordDictionary.Add("DIM4", TokenEnum.token_dim4);
	         m_reservedWordDictionary.Add("DIM5", TokenEnum.token_dim5);
	         m_reservedWordDictionary.Add("DIM6", TokenEnum.token_dim6);
	         m_reservedWordDictionary.Add("DIM7", TokenEnum.token_dim7);
	         m_reservedWordDictionary.Add("DIM8", TokenEnum.token_dim8);
	         m_reservedWordDictionary.Add("DIM9", TokenEnum.token_dim9);
	         m_reservedWordDictionary.Add("DIM10", TokenEnum.token_dim10);
	         m_reservedWordDictionary.Add("DIM11", TokenEnum.token_dim11);
	         m_reservedWordDictionary.Add("DIM12", TokenEnum.token_dim12);
	         m_reservedWordDictionary.Add("DIM13", TokenEnum.token_dim13);
	         m_reservedWordDictionary.Add("DIM14", TokenEnum.token_dim14);
	         m_reservedWordDictionary.Add("DIM15", TokenEnum.token_dim15);
	         m_reservedWordDictionary.Add("DIM16", TokenEnum.token_dim16);
	         m_reservedWordDictionary.Add("DIM17", TokenEnum.token_dim17);
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
	         m_reservedWordDictionary.Add("BST", TokenEnum.token_bst);
	         m_reservedWordDictionary.Add("GMT", TokenEnum.token_gmt);
	         m_reservedWordDictionary.Add("SUNRISE", TokenEnum.token_sunrise);
	         m_reservedWordDictionary.Add("SUNSET", TokenEnum.token_sunset);
	         m_reservedWordDictionary.Add("TIMER", TokenEnum.token_timer);
	         m_reservedWordDictionary.Add("SEQUENCE", TokenEnum.token_sequence);
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
        }

        public static string GetSpelling(TokenEnum tokenEnum)
        {
            string spelling = m_TokenDictionary[tokenEnum];
            return spelling;
        }

        public static string GetSpellings(HashSet<TokenEnum> tokenEnumHashSet)
        {
            StringBuilder spellings = new StringBuilder();
            foreach (TokenEnum tokenEnum in tokenEnumHashSet)
            {
                if (spellings.Length > 0)
                {
                    spellings.Append(", ");
                }
                spellings.Append(m_TokenDictionary[tokenEnum]);
            }
            return spellings.ToString();
        }

        const char EOF = (char)0xFFFF;
        private char m_PreviousCh = '?';
        private char m_ch;  // current character being processed
        private int m_CharPosition = 0;
        private string inputFile;
        TokenEnum token = TokenEnum.token_error;
        private string m_TokenValue;                   //string, name, etc
        private int m_TokenIntegerValue;             // value of integer token
        private DateTime m_TokenDateValue;             // seconds since 00:00:00 on 1 Jan 1970
        private TimeSpan m_TokenTimeValue;             // seconds positive and negative permitted
        private int m_LineNumber = 1;
        private string m_Line;
        private int m_ErrorCount = 0;
        private int m_WarningCount = 0;

        private readonly int _pass;

        public LexicalAnalyser(int pass)
        {
            Debug.Assert(pass >= 1 && pass <= 2);
            _pass = pass;

            TextReader textReader = new StreamReader("smart.txt");
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
                        // check for house code
                        else if (m_TokenValue.Length == 1 && m_TokenValue[0] >= 'A' && m_TokenValue[0] <= 'P')
				        {
					         token = TokenEnum.token_house_code;
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

					         if (m_ch == ':')
					         {
						          token = TokenEnum.token_time_of_day;
					         }
					         if (m_ch == '/')
					         {
						          token = TokenEnum.token_date;
					         }
				        } while ((m_ch >= '0' && m_ch <= '9') ||
							         m_ch == ':' ||
							         m_ch == '/');

                        //parse int, date or time value
				        switch (token)
				        {
					        case TokenEnum.token_integer:
						        m_TokenIntegerValue = int.Parse(m_TokenValue);
						        break;

					        case TokenEnum.token_date:
						        {
                                    m_TokenDateValue = DateTime.Parse(m_TokenValue);
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

                                    m_TokenTimeValue = TimeSpan.Parse(m_TokenValue);


                                    if (negative)
							        {
                                        m_TokenTimeValue = -m_TokenTimeValue;
							        }


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

        public void LogError(string message)
        {
            if (_pass > 1)
            {
                m_ErrorCount++;
                Console.WriteLine(m_Line);
                Console.WriteLine(message + " at " + m_TokenValue + " on line " + m_LineNumber.ToString());
            }
        }

        public void LogWarn(string message)
        {
            if (_pass > 1)
            {
                m_WarningCount++;
                Console.WriteLine(message + " at " + m_TokenValue + " on line " + m_LineNumber.ToString());
            }
        }


        public int IntValue
        {
            get
            {
                return m_TokenIntegerValue;
            }
        }

        public DateTime DateValue
        {
            get
            {
                Debug.Assert(token == TokenEnum.token_date);
                return m_TokenDateValue;
            }
        }

        public TimeSpan TimeSpanValue
        {
            get
            {
                Debug.Assert(token == TokenEnum.token_time_of_day);
                return m_TokenTimeValue;
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
    }
}
