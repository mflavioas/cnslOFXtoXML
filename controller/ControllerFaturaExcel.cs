using cnslOFXtoXML.models;
using OfficeOpenXml;

namespace cnslOFXtoXML.controller
{
    public class ControllerFaturaExcel
    {
        public static List<Finance> CarregaArqExcelSantander(string arquivo, ControllerParametros ConfigParametros)
        {
            List<Finance> lstFinances = new();
            Banco banco = ConfigParametros.parametros.Bancos.Where(b => b.TipoArquivo == "XLS").FirstOrDefault();
            if (File.Exists(arquivo))
            {
                Console.WriteLine($"Registros da planilha: {arquivo}");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(new FileInfo(arquivo));
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    Console.WriteLine($"Registros na aba: {sheet.Name}");
                    Finance finance = new()
                    {
                        Transacoes = [],
                        Id = banco.Codigo,
                        Banco = banco.Descricao
                    };
                    string[] ignorar = banco.Ignorar.Split(";");
                    int Base0Linha = 0;
                    for (int row = 1; row <= sheet.Dimension.Rows; row++)
                    {
                        try
                        {
                            string vlr = sheet.Cells[row, 4].Value.ToString().Replace(".", "");
                            if (Base0Linha == 0)
                            {
                                Base0Linha = row;
                            }
                            string descricao = sheet.Cells[row, 2].Value.ToString();
                            foreach (string ignore in ignorar)
                            {
                                if (descricao.Contains(ignore, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    descricao = string.Empty;
                                    break;
                                }
                            }
                            if (Convert.ToDateTime(sheet.Cells[row, 1].Value) > DateTime.Now.AddYears(-5) && !string.IsNullOrWhiteSpace(descricao))
                            {
                                finance.Transacoes.Add(new()
                                {
                                    NrDoc = row.ToString(),
                                    Tipo = string.Concat(sheet.Cells[Base0Linha - 3, 1].Value, " - ",sheet.Cells[Base0Linha - 3, 2].Value),
                                    Data = Convert.ToDateTime(sheet.Cells[row, 1].Value),
                                    Descricao = descricao,
                                    Valor = Convert.ToDouble(vlr) * -1,
                                    Categoria = ConfigParametros.RetornaCategoria(descricao)
                                });
                            }
                        }
                        catch 
                        {
                            Base0Linha = 0;
                            continue;
                        }
                    }
                    DateTime MaiorMov = finance.Transacoes.Max(x => x.Data);

                    finance.DataFechamento = MaiorMov;
                    DateTime dtvcto = finance.DataFechamento.AddMonths(1);
                    if (banco.DiaVencimento > finance.DataFechamento.Day)
                        dtvcto = finance.DataFechamento;
                    finance.DataVencimento = new DateTime(dtvcto.Year, dtvcto.Month, banco.DiaVencimento);

                    lstFinances.Add(finance);
                }
                File.Delete(Path.ChangeExtension(arquivo, ".xml"));
            }
            else
            {
                Console.WriteLine("Arquivo não encontrado.");
            }
            return lstFinances;
        }
    }
}
