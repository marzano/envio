using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfImpacto.Envio.Schedule
{
    public class ScheduleControl
    {
        private readonly List<CrontabSchedule> _crontabSchedules = new List<CrontabSchedule>();

        public ScheduleControl(string cronExpressions)
        {
            const char separator = ';';
            _crontabSchedules = cronExpressions
                                    .Split(separator)
                                    .Select(x => CrontabSchedule.Parse(x)).ToList();
        }

        public DateTime GetNext()
        {
            return _crontabSchedules.Select(x =>
                    x.GetNextOccurrence(DateTime.Now)).OrderBy(x => x).First();
        }

    }
}
