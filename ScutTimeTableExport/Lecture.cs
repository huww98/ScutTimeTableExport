using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScutTimeTableExport
{
    internal struct LessonTime
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    internal class Lecture
    {
        private static Regex timeRegex =
            new Regex(@"周([一二三四五六日])第(\d{1,2}(?:,\d{1,2})*)节\{第(\d{1,2})-(\d{1,2})周(?:(\|单周)?|(\|双周)?)}",
                RegexOptions.Compiled);

        private static Dictionary<string, DayOfWeek> dayOfWeekName = new Dictionary<string, DayOfWeek>
        {
            {"日", DayOfWeek.Sunday},
            {"一", DayOfWeek.Monday},
            {"二", DayOfWeek.Tuesday},
            {"三", DayOfWeek.Wednesday},
            {"四", DayOfWeek.Thursday},
            {"五", DayOfWeek.Friday},
            {"六", DayOfWeek.Saturday},
        };

        private static Dictionary<int, LessonTime> lessonTime = new Dictionary<int, LessonTime>
        {
            {1,  new LessonTime{ StartTime = new TimeSpan(8, 50,0), EndTime = new TimeSpan(9, 35,0) } },
            {2,  new LessonTime{ StartTime = new TimeSpan(9, 40,0), EndTime = new TimeSpan(10,25,0) } },
            {3,  new LessonTime{ StartTime = new TimeSpan(10,40,0), EndTime = new TimeSpan(11,25,0) } },
            {4,  new LessonTime{ StartTime = new TimeSpan(11,30,0), EndTime = new TimeSpan(12,15,0) } },
            {5,  new LessonTime{ StartTime = new TimeSpan(14,00,0), EndTime = new TimeSpan(14,45,0) } },
            {6,  new LessonTime{ StartTime = new TimeSpan(14,50,0), EndTime = new TimeSpan(15,35,0) } },
            {7,  new LessonTime{ StartTime = new TimeSpan(15,45,0), EndTime = new TimeSpan(16,30,0) } },
            {8,  new LessonTime{ StartTime = new TimeSpan(16,35,0), EndTime = new TimeSpan(17,20,0) } },
            {9,  new LessonTime{ StartTime = new TimeSpan(19,00,0), EndTime = new TimeSpan(19,45,0) } },
            {10, new LessonTime{ StartTime = new TimeSpan(19,50,0), EndTime = new TimeSpan(20,35,0) } },
            {11, new LessonTime{ StartTime = new TimeSpan(20,40,0), EndTime = new TimeSpan(21,25,0) } },
        };

        public string Name { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int[] IndexOfLessons { get; set; }
        public int StartWeek { get; set; }
        public int WeekInterval { get; set; }
        public int RepeatTime { get; set; }
        public string Teacher { get; set; }
        public string Address { get; set; }

        public TimeSpan StartTime => lessonTime[IndexOfLessons[0]].StartTime;
        public TimeSpan EndTime => lessonTime[IndexOfLessons[IndexOfLessons.Length - 1]].EndTime;

        public Lecture(string str)
        {
            var lines = str.Split('\n');
            Name = lines[0];

            var match = timeRegex.Match(lines[1]);
            DayOfWeek = dayOfWeekName[match.Groups[1].Value];
            IndexOfLessons = match.Groups[2].Value.Split(',').Select(s => int.Parse(s)).ToArray();
            StartWeek = int.Parse(match.Groups[3].Value);
            var endWeek = int.Parse(match.Groups[4].Value);

            bool oddWeek = match.Groups[5].Success;
            bool evenWeek = match.Groups[6].Success;
            if (oddWeek)
            {
                WeekInterval = 2;
                if (StartWeek % 2 == 0)
                {
                    StartWeek++;
                }
                if (endWeek % 2 == 0)
                {
                    endWeek--;
                }
            }
            if (evenWeek)
            {
                WeekInterval = 2;
                if (StartWeek % 2 == 1)
                {
                    StartWeek++;
                }
                if (endWeek % 2 == 1)
                {
                    endWeek--;
                }
            }
            else
            {
                WeekInterval = 1;
            }
            RepeatTime = (endWeek - StartWeek) / WeekInterval + 1;

            Teacher = lines[2];
            Address = lines[3];
        }
    }
}