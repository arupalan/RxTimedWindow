using System;
using System.Collections.Generic;

namespace AlanAamy.Net.RxTimedWindow
{
    public sealed class DateTimeHelper
    {
        public readonly DateTime RunDateTime;
        public readonly Dictionary<int, string> UtcIndexToLocalHourMap = new Dictionary<int, string>();

        public DateTimeHelper(DateTime runDate, TimeZoneInfo timeZoneInfo)
        {
            RunDateTime = runDate;
            var dateTimeStart =
                new DateTime(runDate.Year, runDate.Month, runDate.Day, 0, 0, 0, DateTimeKind.Unspecified).Date.AddHours(
                    -1.0);
            var dateTimeEnd = dateTimeStart.AddDays(1.0);
            var dateTimeUtcStart = TimeZoneInfo.ConvertTimeToUtc(dateTimeStart, timeZoneInfo);
            var dateTimeUtcEnd = TimeZoneInfo.ConvertTimeToUtc(dateTimeEnd, timeZoneInfo);
            var index = 0;
            for (var dateTime = dateTimeUtcStart; dateTime < dateTimeUtcEnd; dateTime = dateTime.AddHours(1.0))
            {
                UtcIndexToLocalHourMap.Add(++index,
                    TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo).ToString(@"HH:00"));
            }
        }
    }

    public static class DateTimeHelperExtensions
    {
        public static DateTimeHelper Reset(this DateTimeHelper dateTimeHelper, DateTime now, TimeZoneInfo timeZoneInfo)
        {
            var dateRollOver =
                new DateTime(dateTimeHelper.RunDateTime.Year, dateTimeHelper.RunDateTime.Month,
                    dateTimeHelper.RunDateTime.Day, 0, 0, 0, DateTimeKind.Unspecified).Date.AddDays(1);
            if (DateTime.Compare(now, dateRollOver) > 0)
            {
                return new DateTimeHelper(now, timeZoneInfo);
            }
            return dateTimeHelper;
        }
    }
}