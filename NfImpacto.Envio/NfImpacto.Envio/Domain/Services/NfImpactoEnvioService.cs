using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using NfImpacto.Envio.Domain.Model.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfImpacto.Envio.Domain.Services
{
    public interface INfImpactoEnvioService 
    {
        void Executar();
    }

    public class NfImpactoEnvioService : INfImpactoEnvioService
    {
        private readonly DatabaseOptions _databaseOptions;
        private readonly ServiceConfOptions _serviceConfOptions;
        private readonly TelemetryClient _telemetryClient;
        private readonly IArquivoExcelService _arquivoExcelService;
        public NfImpactoEnvioService(IOptions<ServiceConfOptions> serviceConfOptions,
                        IOptions<DatabaseOptions> databaseOptions,
                        TelemetryClient telemetryClient, 
                        IArquivoExcelService arquivoExcelService) 
        {
            _serviceConfOptions = serviceConfOptions.Value ?? throw new ArgumentNullException(nameof(serviceConfOptions));
            _databaseOptions = databaseOptions.Value ?? throw new ArgumentNullException(nameof(databaseOptions));
            _telemetryClient = telemetryClient;
            _arquivoExcelService = arquivoExcelService;
        }

        public void Executar()
        {
            _telemetryClient.TrackEvent($"{_serviceConfOptions.CurrentAppName}-{_serviceConfOptions.Environment} NfImpactoEnvioService | Carregando Arquivo");
            var dados = _arquivoExcelService.CarregarDadosArquivos();

            if (dados != null)
            {
                try
                {
                    
                    var dv1 = dados.Tables[0].AsEnumerable().ToList();
                    var dv2 = dados.Tables[1].AsEnumerable().ToList();
                    var dv3 = dados.Tables[2].AsEnumerable().ToList();
                    var cnpjs = dv1.Select(x => x.ItemArray[2].ToString().Trim()).ToList();

                    cnpjs.RemoveRange(0, 2);
                    cnpjs = cnpjs.Distinct().ToList();

                    _telemetryClient.TrackEvent($"{_serviceConfOptions.CurrentAppName}-{_serviceConfOptions.Environment} NfImpactoEnvioService | CNPJs unicos {cnpjs.Count}");

                    foreach (var cnpj in cnpjs)
                    {
                        GerarArquivoCnpj(cnpj, dv1, dv2, dv3);
                    }
                    _arquivoExcelService.Processado();
                    _telemetryClient.TrackEvent($"{_serviceConfOptions.CurrentAppName}-{_serviceConfOptions.Environment} NfImpactoEnvioService | Processado com sucesso");
                }
                catch (Exception ex)
                {
                    _arquivoExcelService.Erro();
                    _telemetryClient.TrackEvent($"{_serviceConfOptions.CurrentAppName}-{_serviceConfOptions.Environment} NfImpactoEnvioService | Processado com erro. {ex.Message}");
                    _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { $"{_serviceConfOptions.CurrentAppName}-{_serviceConfOptions.Environment} NfImpactoEnvioService | EXCEPTION", "" } });
                }
            }
        }

        private void GerarArquivoCnpj(string cnpj, List<DataRow> dv1, List<DataRow> dv2, List<DataRow> dv3)
        {
            var wkb = _arquivoExcelService.CarregarWorkbookTemplate();
            var rows1 = dv1.Where(x => x.ItemArray[2].ToString().Trim() == cnpj).ToList();
            rows1.Insert(0, dv1[0]);
            rows1.Insert(1, dv1[1]);
            var wks1 = wkb.Worksheet(1);
            for (var r1 = 0; r1 < rows1.Count; r1++)
            {
                var row1 = rows1[r1];
                for (var c = 0; c < row1.ItemArray.Length; c++)
                {
                    wks1.Cell(r1 + 1, c + 1).Value = row1.ItemArray[c].ToString();
                }
            }

            var rows2 = dv2.Where(x => x.ItemArray[4].ToString().Trim() == cnpj).ToList();
            rows2.Insert(0, dv2[0]);
            rows2.Insert(1, dv2[1]);
            var wks2 = wkb.Worksheet(2);
            for (var r2 = 0; r2 < rows2.Count; r2++)
            {
                var row2 = rows2[r2];
                for (var c = 0; c < row2.ItemArray.Length; c++)
                {
                    wks2.Cell(r2 + 1, c + 1).Value = row2.ItemArray[c].ToString();
                }
            }

            var rows3 = dv3.Where(x => x.ItemArray[6].ToString().Trim() == cnpj).ToList();
            rows3.Insert(0, dv3[0]);
            var wks3 = wkb.Worksheet(3);
            for (var r3 = 0; r3 < rows3.Count; r3++)
            {
                var row3 = rows3[r3];
                for (var c = 0; c < row3.ItemArray.Length; c++)
                {
                    wks3.Cell(r3 + 1, c + 1).Value = row3.ItemArray[c].ToString();
                }
            }

            _arquivoExcelService.GerarArquivoPorCnpj(wkb, cnpj);
            wkb.Dispose();

        }
    }
}
