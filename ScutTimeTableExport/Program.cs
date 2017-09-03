using AngleSharp;
using AngleSharp.Dom.Html;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScutTimeTableExport
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("请前往http://jw2005.scuteo.com/进行登录（最好启用隐私模式），将登录后的网址粘贴于本程序中");
            Console.WriteLine("请注意，上面的网址不是一般登录教务系统时使用的网址");
            Console.WriteLine("警告：在平时使用教务系统时，请勿将此网址。登录后的网址包含有您的个人敏感信息，请勿泄漏");

            var address = Console.ReadLine();

            Regex regex = new Regex(@"http://xsweb\.scuteo\.com/\((.*?)\)/xs_main\.aspx\?xh=(\d{12})", RegexOptions.IgnoreCase);

            var match = regex.Match(address);
            if (!match.Success)
            {
                Console.WriteLine("网址未能识别");
                return;
            }

            var token = match.Groups[1].Value;
            var studnetNum = match.Groups[2].Value;

            var queryAddress = $"http://xsweb.scuteo.com/({token})/xskbcx.aspx?xh={studnetNum}&gnmkdm=N121603";

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            var main = context.OpenAsync(address).Result;
            var lectureTable = context.OpenAsync(queryAddress).Result;

            var table = lectureTable.GetElementById("Table1") as IHtmlTableElement;
            var lectureTexts = table.Rows
                .SelectMany(r => r.Cells.Where(c => c.TextContent.Length > 5))
                .Select(c => c.InnerHtml.Replace("<br>", "\n"));

            var lectures = new List<Lecture>();
            foreach (var lt in lectureTexts)
            {
                lectures.Add(new Lecture(lt));
            }

            var calendar = new Calendar();
            DateTime SchoolBeginTime = new DateTime(2017, 9, 3);

            foreach (var l in lectures)
            {
                var date = SchoolBeginTime
                    + TimeSpan.FromDays(7 * (l.StartWeek - 1))
                    + TimeSpan.FromDays((int)l.DayOfWeek);
                var e = new Event
                {
                    DtStart = new CalDateTime(date + l.StartTime),
                    DtEnd = new CalDateTime(date + l.EndTime),
                    Summary = l.Name,
                    Location = l.Address,
                    RecurrenceRules = new List<IRecurrencePattern>
                    {
                        new RecurrencePattern(FrequencyType.Weekly, l.WeekInterval){Count = l.RepeatTime}
                    },
                    Description = $"老师:{l.Teacher}",
                };
                calendar.Events.Add(e);
            }

            var serializer = new CalendarSerializer(calendar);

            const string fileName = "课表.ics";
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                serializer.Serialize(calendar, stream, Encoding.UTF8);
            }
            Console.WriteLine($"处理完成，发现课程{lectures.Count}门，已导出到“{fileName}”");
        }
    }
}