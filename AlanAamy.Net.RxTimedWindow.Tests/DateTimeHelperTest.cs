using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AlanAamy.Net.RxTimedWindow.Tests
{
    [TestFixture]
    public sealed class DateTimeHelperTest
    {
        [Test]
        public void DateTimeHelperDayRollOver1AM_OnDaylightSavingStart()
        {
            //Arrange
            var dateStart = DateTime.ParseExact("2015/03/28 23:30", "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
            var dateRollOverPre1am = DateTime.ParseExact("2015/03/29", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            var dateRollOverpsot1am = DateTime.ParseExact("2015/03/29 01:00", "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
            var dateTimeHelper = new DateTimeHelper(dateStart, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
            var expectedReset = new Dictionary<int, string>
            {
                {1, "23:00"},{2,"00:00"},{3, "01:00"},{4,"02:00"},{5, "03:00"},{6,"04:00"},
                {7, "05:00"},{8,"06:00"},{9, "07:00"},{10,"08:00"},{11, "09:00"},{12,"10:00"},
                {13,"11:00"},{14,"12:00"},{15, "13:00"},{16,"14:00"},{17, "15:00"},{18,"16:00"},
                {19,"17:00"},{20,"18:00"},{21, "19:00"},{22,"20:00"},{23, "21:00"},{24,"22:00"}
            };

            var expectedReset1 = new Dictionary<int, string>
            {
                {1, "23:00"},{2,"00:00"},{3, "02:00"},{4,"03:00"},{5, "04:00"},{6,"05:00"},
                {7, "06:00"},{8,"07:00"},{9, "08:00"},{10,"09:00"},{11, "10:00"},{12,"11:00"},
                {13,"12:00"},{14,"13:00"},{15, "14:00"},{16,"15:00"},{17, "16:00"},{18,"17:00"},
                {19,"18:00"},{20,"19:00"},{21, "20:00"},{22,"21:00"},{23, "22:00"}
            };

            //Act
            var dateTimeHelperReset = dateTimeHelper.Reset(dateRollOverPre1am,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

            //Assert
            CollectionAssert.AreEquivalent(expectedReset,dateTimeHelperReset.UtcIndexToLocalHourMap);

            var dateTimeHelperReset1 = dateTimeHelperReset.Reset(dateRollOverpsot1am,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

            //Assert
            CollectionAssert.AreEquivalent(expectedReset1, dateTimeHelperReset1.UtcIndexToLocalHourMap);

        }

        [Test]
        public void DateTimeHelperDayRollOver2AM_OnDaylightSavingEnd()
        {
            //Arrange
            var dateStart = DateTime.ParseExact("2015/10/24 23:30", "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
            var dateRollOverPre1am = DateTime.ParseExact("2015/10/25", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            var dateRollOverpsot1am = DateTime.ParseExact("2015/10/25 02:00", "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
            var dateTimeHelper = new DateTimeHelper(dateStart, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
            var expectedReset = new Dictionary<int, string>
            {
                {1, "23:00"},{2,"00:00"},{3, "01:00"},{4,"02:00"},{5, "03:00"},{6,"04:00"},
                {7, "05:00"},{8,"06:00"},{9, "07:00"},{10,"08:00"},{11, "09:00"},{12,"10:00"},
                {13,"11:00"},{14,"12:00"},{15, "13:00"},{16,"14:00"},{17, "15:00"},{18,"16:00"},
                {19,"17:00"},{20,"18:00"},{21, "19:00"},{22,"20:00"},{23, "21:00"},{24,"22:00"}
            };

            var expectedReset1 = new Dictionary<int, string>
            {
                {1, "23:00"},{2,"00:00"},{3, "01:00"},{4,"01:00"},{5, "02:00"},{6,"03:00"},
                {7, "04:00"},{8,"05:00"},{9, "06:00"},{10,"07:00"},{11, "08:00"},{12,"09:00"},
                {13,"10:00"},{14,"11:00"},{15, "12:00"},{16,"13:00"},{17, "14:00"},{18,"15:00"},
                {19,"16:00"},{20,"17:00"},{21, "18:00"},{22,"19:00"},{23, "20:00"},{24,"21:00"},
                {25,"22:00"}
            };

            //Act
            var dateTimeHelperReset = dateTimeHelper.Reset(dateRollOverPre1am,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

            //Assert
            CollectionAssert.AreEquivalent(expectedReset, dateTimeHelperReset.UtcIndexToLocalHourMap);

            var dateTimeHelperReset1 = dateTimeHelperReset.Reset(dateRollOverpsot1am,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

            //Assert
            CollectionAssert.AreEquivalent(expectedReset1, dateTimeHelperReset1.UtcIndexToLocalHourMap);

        }
    }
}
