﻿using System;
using System.Linq;
using NUnit.Framework;
using Sensus.Tools.Scripts;

namespace Sensus.Tools.Tests.Scripts
{
    [TestFixture]
    public class ScheduleTriggerTests
    {
        [Test]
        public void Deserialize1PointTest()
        {
            var schedule = new ScheduleTrigger();

            schedule.DeserializeWindows("10:00");

            Assert.AreEqual(1, schedule.WindowCount);
            Assert.AreEqual("10:00", schedule.SerlializeWindows());
        }

        [Test]
        public void Deserialize1WindowTest()
        {
            var schedule = new ScheduleTrigger();

            schedule.DeserializeWindows("10:00-10:30");

            Assert.AreEqual(1, schedule.WindowCount);
            Assert.AreEqual("10:00-10:30", schedule.SerlializeWindows());
        }

        [Test]
        public void Deserialize1PointTrailingCommaTest()
        {
            var schedule = new ScheduleTrigger();

            schedule.DeserializeWindows("10:00,");

            Assert.AreEqual(1, schedule.WindowCount);
            Assert.AreEqual("10:00", schedule.SerlializeWindows());
        }

        [Test]
        public void Deserialize1Point1WindowTest()
        {
            var schedule = new ScheduleTrigger();

            schedule.DeserializeWindows("10:00,10:10-10:20");

            Assert.AreEqual(2, schedule.WindowCount);
            Assert.AreEqual("10:00, 10:10-10:20", schedule.SerlializeWindows());
        }

        [Test]
        public void Deserialize1Point1WindowSpacesTest()
        {
            var schedule = new ScheduleTrigger();

            schedule.DeserializeWindows("10:00,                10:10-10:20");

            Assert.AreEqual(2, schedule.WindowCount);
            Assert.AreEqual("10:00, 10:10-10:20", schedule.SerlializeWindows());
        }

        [Test]
        public void SchedulesAllFutureTest()
        {
            var schedule = new ScheduleTrigger();

            schedule.DeserializeWindows("10:00, 10:10-10:20");

            var startDate = new DateTime(1986, 4, 18, 0, 0, 0);
            var afterDate = new DateTime(1986, 4, 18, 0, 0, 0);

            var schedules = schedule.SchedulesAfter(startDate, afterDate).Take(6).ToArray();

            Assert.AreEqual(new TimeSpan(0, 10, 0, 0), schedules[0].TimeUntil);
            Assert.IsTrue(new TimeSpan(0, 10, 10, 0) <= schedules[1].TimeUntil && schedules[1].TimeUntil <= new TimeSpan(0, 10, 20, 0));

            Assert.AreEqual(new TimeSpan(1, 10, 0, 0), schedules[2].TimeUntil);
            Assert.IsTrue(new TimeSpan(1, 10, 10, 0) <= schedules[3].TimeUntil && schedules[3].TimeUntil <= new TimeSpan(1, 10, 20, 0));

            Assert.AreEqual(new TimeSpan(2, 10, 0, 0), schedules[4].TimeUntil);
            Assert.IsTrue(new TimeSpan(2, 10, 10, 0) <= schedules[5].TimeUntil && schedules[5].TimeUntil <= new TimeSpan(2, 10, 20, 0));
        }

        [Test]
        public void SchedulesOneFutureTest()
        {
            var schedule = new ScheduleTrigger();

            schedule.DeserializeWindows("10:00, 10:20-10:30");

            var startDate = new DateTime(1986, 4, 18, 10, 10, 0);
            var afterDate = new DateTime(1986, 4, 18, 10, 10, 0);

            var schedules = schedule.SchedulesAfter(startDate, afterDate).Take(6).ToArray();

            Assert.IsTrue(new TimeSpan(0, 0, 10, 0) <= schedules[0].TimeUntil && schedules[0].TimeUntil <= new TimeSpan(0, 0, 20, 0));
            Assert.AreEqual(new TimeSpan(0, 23, 50, 0), schedules[1].TimeUntil);

            Assert.IsTrue(new TimeSpan(1, 0, 10, 0) <= schedules[2].TimeUntil && schedules[2].TimeUntil <= new TimeSpan(1, 0, 20, 0));
            Assert.AreEqual(new TimeSpan(1, 23, 50, 0), schedules[3].TimeUntil);

            Assert.IsTrue(new TimeSpan(2, 0, 10, 0) <= schedules[4].TimeUntil && schedules[4].TimeUntil <= new TimeSpan(2, 0, 20, 0));
            Assert.AreEqual(new TimeSpan(2, 23, 50, 0), schedules[5].TimeUntil);
        }

        [Test]
        public void SchedulesAfterOneDayTest()
        {
            var schedule = new ScheduleTrigger();

            schedule.DeserializeWindows("10:00, 10:20-10:30");

            var startDate = new DateTime(1986, 4, 18, 10, 10, 0);
            var afterDate = new DateTime(1986, 4, 19, 10, 10, 0);

            var schedules = schedule.SchedulesAfter(startDate, afterDate).Take(6).ToArray();

            Assert.IsTrue(new TimeSpan(1, 0, 10, 0) <= schedules[0].TimeUntil && schedules[0].TimeUntil <= new TimeSpan(1, 0, 20, 0));
            Assert.AreEqual(new TimeSpan(1, 23, 50, 0), schedules[1].TimeUntil);

            Assert.IsTrue(new TimeSpan(2, 0, 10, 0) <= schedules[2].TimeUntil && schedules[2].TimeUntil <= new TimeSpan(2, 0, 20, 0));
            Assert.AreEqual(new TimeSpan(2, 23, 50, 0), schedules[3].TimeUntil);

            Assert.IsTrue(new TimeSpan(3, 0, 10, 0) <= schedules[4].TimeUntil && schedules[4].TimeUntil <= new TimeSpan(3, 0, 20, 0));
            Assert.AreEqual(new TimeSpan(3, 23, 50, 0), schedules[5].TimeUntil);
        }

        [Test]
        public void SchedulesPullsOnlySevenDays()
        {
            var schedule = new ScheduleTrigger();

            schedule.DeserializeWindows("10:00");

            var startDate = new DateTime(1986, 4, 18, 0, 0, 0);
            var afterDate = new DateTime(1986, 4, 19, 0, 0, 0);

            var scheduleCount = schedule.SchedulesAfter(startDate, afterDate).Count();

            Assert.AreEqual(7, scheduleCount);
        }
    }
}