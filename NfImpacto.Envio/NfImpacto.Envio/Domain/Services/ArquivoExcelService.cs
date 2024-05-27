using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.Extensions.Options;
using NfImpacto.Envio.Domain.Model.Configuration;
using NfImpacto.Envio.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfImpacto.Envio.Domain.Services
{
    public interface IArquivoExcelService 
    {
        DataSet? CarregarDadosArquivos();
        IXLWorkbook CarregarWorkbookTemplate();
        void Processado();
        void GerarArquivoPorCnpj(IXLWorkbook workbook, string cnpj);
        void Erro();
    }

    public class ArquivoExcelService : IArquivoExcelService
    {
        private readonly IOptions<ArquivoExcelOptions> _arquivoExcelOptions;
        private string _arquivoProcessando;

        public ArquivoExcelService(IOptions<ArquivoExcelOptions> arquivoExcelOptions)
        {
            _arquivoExcelOptions = arquivoExcelOptions;
            if (arquivoExcelOptions.Value == null
                || string.IsNullOrWhiteSpace(arquivoExcelOptions.Value.CaminhoOrigem))
                throw new NoNullAllowedException("Configurações do arquivo são obrigatórias");
        }

        public DataSet? CarregarDadosArquivos()
        {
            var caminho = FileSystemHelper.CombineDirectory(_arquivoExcelOptions.Value.CaminhoOrigem, "");
            FileSystemHelper.CreateDirectoryIfNotExists(caminho);

            FileSystemInfo arqInfo = new DirectoryInfo(caminho).GetFileSystemInfos().Where(x => x is FileInfo)
                                                                    .OrderBy(x => x.CreationTime).FirstOrDefault();

            if (arqInfo != null) 
            {
                return CarregarDadosArquivo(arqInfo.FullName);
            }

            return null;
        }

        private DataSet? CarregarDadosArquivo(string filePath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            DataSet? dados = null;
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    dados = reader.AsDataSet();
                }
            }
            FileSystemHelper.CreateDirectoryIfNotExists(_arquivoExcelOptions.Value.CaminhoProcessando);
            _arquivoProcessando = Path.GetFileName(filePath);
            var caminhoPara = FileSystemHelper.CombineFile(_arquivoExcelOptions.Value.CaminhoProcessando, _arquivoProcessando);
            FileSystemHelper.MoveFile(filePath, caminhoPara);
            return dados;
        }

        public IXLWorkbook CarregarWorkbookTemplate()
        {
            var wkb = new XLWorkbook();
            wkb.Worksheets.Add("NOTA FISCAL");
            wkb.Worksheets.Add("1ª E 2ª VIAS, MULTA");
            wkb.Worksheets.Add("Reajuste_Particularidade");
            return wkb;
        }

        public void GerarArquivoPorCnpj(IXLWorkbook workbook, string cnpj)
        {
            FileSystemHelper.CreateDirectoryIfNotExists(_arquivoExcelOptions.Value.CaminhoCnpjs);
            var arquivoSalvar = string.Format("{0}-{1}.xlsx", cnpj, DateTime.Now.ToString("yyyyMMddHHmmsss"));
            var caminhoSalvar = FileSystemHelper.CombineFile(_arquivoExcelOptions.Value.CaminhoCnpjs, arquivoSalvar);
            workbook.SaveAs(caminhoSalvar);
        }

        public void Processado()
        {
            FileSystemHelper.CreateDirectoryIfNotExists(_arquivoExcelOptions.Value.CaminhoProcessado);
            var caminhoDe = FileSystemHelper.CombineFile(_arquivoExcelOptions.Value.CaminhoProcessando, _arquivoProcessando);
            var caminhoPara = FileSystemHelper.CombineFile(_arquivoExcelOptions.Value.CaminhoProcessado, _arquivoProcessando);
            FileSystemHelper.MoveFile(caminhoDe, caminhoPara);
            _arquivoProcessando = string.Empty;
        }

        public void Erro()
        {
            FileSystemHelper.CreateDirectoryIfNotExists(_arquivoExcelOptions.Value.CaminhoErro);
            var caminhoDe = FileSystemHelper.CombineFile(_arquivoExcelOptions.Value.CaminhoProcessando, _arquivoProcessando);
            var caminhoPara = FileSystemHelper.CombineFile(_arquivoExcelOptions.Value.CaminhoErro, _arquivoProcessando);
            FileSystemHelper.MoveFile(caminhoDe, caminhoPara);
            _arquivoProcessando = string.Empty;
        }

    }
}
