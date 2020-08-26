/*
    \file CodeCalendar.cs
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
using Compiler2.Calculations;
using compiler2.Compile;
using compiler2.Generate;

namespace compiler2.Code
{
    class CodeCalendar
    {
        public const int SECONDSPERDAY = (3600 * 24);
        //public const int TICKSPERSECOND = 10000000;
        private const int BACKTRACK_DAYS = 2;
        private const int MAX_RUNTIME = 366;
        private const int TOTAL_DAY_ATTRIBUTE_DAYS = (BACKTRACK_DAYS + 1 + MAX_RUNTIME*2);

        public static readonly TimeSpan SUNRISE_TIMESPAN = new TimeSpan(0, 0, -1);
        public static readonly TimeSpan SUNSET_TIMESPAN = new TimeSpan(0, 0, -2);

        double latitude = 51.019570;//+ve is °N - Timsbury
        double longitude = -1.505470;//+ve is °E - Timsbury



        static private readonly dayEnum[] dayOfWeekMap =
        {
            dayEnum.DAY_SUN,
            dayEnum.DAY_MON,
            dayEnum.DAY_TUE,
            dayEnum.DAY_WED,
            dayEnum.DAY_THU,
            dayEnum.DAY_FRI,
            dayEnum.DAY_SAT,
        };

		dayEnum[] muDayAttributes = new dayEnum[TOTAL_DAY_ATTRIBUTE_DAYS];
		int[] /*time_t*/ mtSunRise = new int[TOTAL_DAY_ATTRIBUTE_DAYS];
		int[] /*time_t*/ mtSunSet = new int[TOTAL_DAY_ATTRIBUTE_DAYS];

        public struct CalendarEntry
        {
            public DateTime day;
            public dayEnum dayEnum;
            public int sunRise;
            public int sunSet;
        }

        public static int NoCalendarEntries
        {
            get { return TOTAL_DAY_ATTRIBUTE_DAYS; }
        }

        public void SetDay(DateTime day, dayEnum dayflag)
        {
	        int index = (int) ((day.Ticks - mMidnightStartDay.Ticks)/ TimeSpan.TicksPerSecond / SECONDSPERDAY);

	        if (index >= 0 && index < TOTAL_DAY_ATTRIBUTE_DAYS)
	        {
		        muDayAttributes[index] |= dayflag;
	        }
        }

        private DateTime mMidnightStartDay;

        public CodeCalendar()
        {
            mMidnightStartDay = DateTime.Today - new TimeSpan(BACKTRACK_DAYS, 0, 0, 0);
            //TODO temporary calculation 
            //mMidnightStartDay = new DateTime(2010, 4, 1, 0, 0, 0) - new TimeSpan(BACKTRACK_DAYS, 0, 0, 0); // TODO Remove
	        for (int iDayNo = 0; iDayNo < TOTAL_DAY_ATTRIBUTE_DAYS; iDayNo++)
	        {
		        DateTime dateTime = mMidnightStartDay + new TimeSpan(iDayNo, 0, 0, 0);

		        Debug.Assert(dateTime.Second == 0);  //really is midnight!
		        Debug.Assert(dateTime.Minute == 0);
		        Debug.Assert(dateTime.Hour == 0);
                SunSetRise sunSetRise = new SunSetRise(latitude, longitude, dateTime);

                mtSunRise[iDayNo] = (int) Math.Round(sunSetRise.Sunrise.TotalMilliseconds /1000.0);
		        mtSunSet[iDayNo] = (int) Math.Round(sunSetRise.Sunset.TotalMilliseconds/1000.0);

		        // set day of week bit
                muDayAttributes[iDayNo] = dayOfWeekMap[(int) dateTime.DayOfWeek];

		        // deduce non working days
                if (muDayAttributes[iDayNo] == dayEnum.DAY_SAT ||
                    muDayAttributes[iDayNo] == dayEnum.DAY_SUN)
		        {
                    muDayAttributes[iDayNo] |= dayEnum.DAY_NONWORK;
		        }
	        }
        }

        public bool CompleteDefinition()
        {
            bool resultOK = true;
	        int iDayNo;
            dayEnum summerWinterTimeAttributes = 0;

	        // pre-pass to deduce whether we are currently in winter or summer by
	        // looking forward to the first time change
	        iDayNo = 0;
	        while (iDayNo < TOTAL_DAY_ATTRIBUTE_DAYS && summerWinterTimeAttributes == 0)
	        {
                if ((muDayAttributes[iDayNo] & dayEnum.DAY_BST) != 0)
		        {
                    summerWinterTimeAttributes = dayEnum.DAY_GMT;
		        }


                if ((muDayAttributes[iDayNo] & dayEnum.DAY_GMT) != 0)
		        {
                    summerWinterTimeAttributes = dayEnum.DAY_BST;
		        }

		        iDayNo++;
	        }

            if (summerWinterTimeAttributes == 0)
            {
                resultOK = false;
                summerWinterTimeAttributes = dayEnum.DAY_GMT;
            }


	        for (iDayNo = 0; iDayNo < TOTAL_DAY_ATTRIBUTE_DAYS; iDayNo++)
	        {
		        // deduce working days
                if ((muDayAttributes[iDayNo] & dayEnum.DAY_NONWORK) == 0)
		        {
                    muDayAttributes[iDayNo] |= dayEnum.DAY_WORK;
		        }

		        // deduce first working day of the week
		        if (iDayNo > 0 &&
                    (muDayAttributes[iDayNo - 1] & dayEnum.DAY_NONWORK) != 0 &&
                    (muDayAttributes[iDayNo] & dayEnum.DAY_WORK) != 0)
		        {
                    muDayAttributes[iDayNo] |= dayEnum.DAY_FIRSTWORK;
		        }

		        // populate with BST/GMT attributes
                if ((muDayAttributes[iDayNo] & dayEnum.DAY_BST) != 0 ||
                     (muDayAttributes[iDayNo] & dayEnum.DAY_GMT) != 0)
		        {
			        // change over time from Winter to Summer or vice versa
                    summerWinterTimeAttributes = muDayAttributes[iDayNo] & (dayEnum.DAY_BST | dayEnum.DAY_GMT);
		        }
		        else
		        {
			        // set winter/summer time attribute
			        muDayAttributes[iDayNo] |= summerWinterTimeAttributes;
		        }

        #if DEBUG2
		        time_t dateTime;
		        struct tm * localDateTime;

		        dateTime = mMidnightStartDay + iDayNo * SECONDSPERDAY;
		        localDateTime = gmtime(&dateTime);

		        printf("day[%3d] %02d/%02d/%4d %03x ", iDayNo,
			        localDateTime->tm_mday, localDateTime->tm_mon +1, 1900 + localDateTime->tm_year,
			        muDayAttributes[iDayNo]);

		        int i;
		        for (i = 0; i < 16; i++)
		        {
			        if ((muDayAttributes[iDayNo] & (1 << i)) != 0)
			        {
				        printf("%s", calendarAttributeDescriptions[i]);
			        }
		        }
		        printf("\n");
        #endif
            }

            return resultOK;
        }

        public CalendarEntry getCalendarEntry(int entryNo)
        {
            Debug.Assert(entryNo >= 0 && entryNo <= TOTAL_DAY_ATTRIBUTE_DAYS);
            CalendarEntry calendarEntry;

            calendarEntry.day = mMidnightStartDay + new TimeSpan(entryNo, 0, 0, 0);
            calendarEntry.sunRise = mtSunRise[entryNo];
            calendarEntry.sunSet = mtSunSet[entryNo];
            calendarEntry.dayEnum = muDayAttributes[entryNo];

            return calendarEntry;
        }


    }
}
