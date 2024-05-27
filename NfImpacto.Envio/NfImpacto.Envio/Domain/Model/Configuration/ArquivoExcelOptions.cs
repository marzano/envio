using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfImpacto.Envio.Domain.Model.Configuration
{
    public class ArquivoExcelOptions
    {
        public const string ArquivoExcel = "ArquivoExcel";

        public string? CaminhoOrigem { get; set; }
        public string? CaminhoProcessando { get; set; }
        public string? CaminhoProcessado { get; set; }
        public string? CaminhoErro { get; set; }
        public string? CaminhoCnpjs { get; set; }
    }
}
