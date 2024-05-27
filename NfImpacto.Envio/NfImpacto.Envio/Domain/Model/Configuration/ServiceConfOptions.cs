using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfImpacto.Envio.Domain.Model.Configuration
{
    public class ServiceConfOptions
    {
        public const string ServiceConf = "ServiceConf";
        public string? InstrumentationKey { get; set; }
        public string? Environment { get; set; }
        public string? CurrentAppName { get; set; }
        public string? CrontabDayCommands { get; set; }
        public double? DelayRecurrenceInMinutes { get; set; }
    }
}
