using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfImpacto.Envio.Domain.Model.Configuration
{
    public class DatabaseOptions
    {
        public const string Database = "Database";
        public string? ConnectionString { get; set; }
    }
}
