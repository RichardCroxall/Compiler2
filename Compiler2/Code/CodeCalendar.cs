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

        private const int JAN =       -1;
        private const int FEB = (JAN + 31);
        private const int MAR = (FEB + 29);
        private const int APR = (MAR + 31);
        private const int MAY = (APR + 30);
        private const int JUN = (MAY + 31);
        private const int JUL = (JUN + 30);
        private const int AUG = (JUL + 31);
        private const int SEP = (AUG + 31);
        private const int OCT = (SEP + 30);
        private const int NOV = (OCT + 31);
        private const int DEC = (NOV + 30);

        // define static method to make it easier to define dates and times of
        // sunrise and sunset at Poole Harbour.
        private static int TIME(int hour, int minute)
        {
            Debug.Assert(hour >= 0 && hour <= 23);
            Debug.Assert(minute >= 0 && minute <= 59);
            return ((hour) * 3600 + (minute) * 60);
        }

        struct astronomy_t
        {
	        public int	iDayOfYear;
            public int /*time_t*/	tSunRise;
            public int /*time_t*/	tSunSet;

            public astronomy_t(int dayOfYear, int sunrise, int sunset)
            {
                iDayOfYear = dayOfYear;
                tSunRise = sunrise;
                tSunSet = sunset;
            }
        };


        static astronomy_t[] pooleDayLight = new astronomy_t[]
        {
	        new astronomy_t(JAN +  1, TIME( 8, 10), TIME(16, 13)),
	        new astronomy_t(JAN + 15, TIME( 8,  4), TIME(16, 31)),
	        new astronomy_t(FEB +  1, TIME( 7, 44), TIME(16, 59)),
	        new astronomy_t(FEB + 15, TIME( 7, 21), TIME(17, 24)),
	        new astronomy_t(MAR +  1, TIME( 6, 52), TIME(17, 49)),
	        new astronomy_t(MAR + 15, TIME( 6, 22), TIME(18, 12)),
	        new astronomy_t(APR +  1, TIME( 5, 44), TIME(18, 40)),
	        new astronomy_t(APR + 15, TIME( 5, 14), TIME(19,  3)),
	        new astronomy_t(MAY +  1, TIME( 4, 43), TIME(19, 28)),
	        new astronomy_t(MAY + 15, TIME( 4, 20), TIME(19, 50)),
	        new astronomy_t(JUN +  1, TIME( 4,  1), TIME(20, 11)),
	        new astronomy_t(JUN + 15, TIME( 3, 54), TIME(20, 22)),
	        new astronomy_t(JUL +  1, TIME( 3, 59), TIME(20, 24)),
	        new astronomy_t(JUL + 15, TIME( 4, 12), TIME(20, 15)),
	        new astronomy_t(AUG +  1, TIME( 4, 34), TIME(19, 53)),
	        new astronomy_t(AUG + 15, TIME( 4, 55), TIME(19, 28)),
	        new astronomy_t(SEP +  1, TIME( 5, 21), TIME(18, 53)),
	        new astronomy_t(SEP + 15, TIME( 5, 43), TIME(18, 22)),
	        new astronomy_t(OCT +  1, TIME( 6,  8), TIME(17, 46)),
	        new astronomy_t(OCT + 15, TIME( 6, 30), TIME(17, 46)),
	        new astronomy_t(NOV +  1, TIME( 6, 59), TIME(17, 16)),
	        new astronomy_t(NOV + 15, TIME( 7, 23), TIME(16, 43)),
	        new astronomy_t(DEC +  1, TIME( 7, 47), TIME(16, 22)),
	        new astronomy_t(DEC + 15, TIME( 8,  3), TIME(16,  3)),
	        new astronomy_t(DEC + 31, TIME( 8, 10), TIME(16, 13))
        };
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

        static int /*time_t*/ getSunRise(int yearDayNo)
        {
	        int i = 1;
	        Debug.Assert(yearDayNo >= 0 && yearDayNo <= 365);

            while (i < pooleDayLight.Length && yearDayNo > pooleDayLight[i].iDayOfYear)
	        {
		        i++;
	        }

	        return pooleDayLight[i - 1].tSunRise +
		        (yearDayNo -  pooleDayLight[i - 1].iDayOfYear) *
		        (pooleDayLight[i].tSunRise - pooleDayLight[i - 1].tSunRise)/
		        (pooleDayLight[i].iDayOfYear - pooleDayLight[i - 1].iDayOfYear);
        }

        static int /*time_t*/ getSunSet(int yearDayNo)
        {
	        int i = 1;
	        Debug.Assert(yearDayNo >= 0 && yearDayNo <= 365);

	        while (i <pooleDayLight.Length && yearDayNo > pooleDayLight[i].iDayOfYear)
	        {
		        i++;
	        }
	        return pooleDayLight[i - 1].tSunSet +
		        ((yearDayNo -  pooleDayLight[i - 1].iDayOfYear) *
		        (pooleDayLight[i].tSunSet - pooleDayLight[i - 1].tSunSet))/
		        (pooleDayLight[i].iDayOfYear - pooleDayLight[i - 1].iDayOfYear);
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

		        mtSunRise[iDayNo] = getSunRise(dateTime.DayOfYear - 1);
		        mtSunSet[iDayNo] = getSunSet(dateTime.DayOfYear - 1);

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

        public void CompleteDefinition()
        {
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
	        Debug.Assert(summerWinterTimeAttributes != 0);


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
