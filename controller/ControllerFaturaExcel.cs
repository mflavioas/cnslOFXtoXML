using cnslOFXtoXML.models;
using OfficeOpenXml;
using System;

namespace cnslOFXtoXML.controller
{
    public class ControllerFaturaExcel
    {
        public static List<Finance> CarregaArqExcelSantander(string arquivo, ControllerParametros ConfigParametros)
        {
            List<Finance> lstFinances = [];
            Banco banco = ConfigParametros.parametros.Bancos.FirstOrDefault(b => b.TipoArquivo == "XLS");
            if (File.Exists(arquivo))
            {
                Console.WriteLine($"Registros da planilha: {arquivo}");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(new FileInfo(arquivo));
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    Console.WriteLine($"Registros na aba: {sheet.Name}");
                    Finance finance = new() { Transacoes = [] };
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
                                    NrBco = banco.Codigo,
                                    Banco = banco.Descricao,
                                    NrDoc = row.ToString(),
                                    Tipo = string.Concat(sheet.Cells[Base0Linha - 3, 1].Value, " - ", sheet.Cells[Base0Linha - 3, 2].Value),
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
                    DateTime dtvcto = MaiorMov.AddMonths(1);
                    if (banco.DiaVencimento > MaiorMov.Day)
                        dtvcto = MaiorMov;
                    foreach (Transacao transacao in finance.Transacoes)
                    {
                        transacao.DataFechamento = MaiorMov;
                        transacao.DataVencimento = new DateTime(dtvcto.Year, dtvcto.Month, banco.DiaVencimento);
                    }
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

        public static void GravarArquivoExcel(List<Finance> Lstfinance)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Financeiro.xlsx");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Financeiro");
                int row = 1;
                // Escreve os cabeçalhos
                worksheet.Cells[row, 1].Value = "NrBco";
                worksheet.Cells[row, 2].Value = "Banco";
                worksheet.Cells[row, 3].Value = "Tipo";
                worksheet.Cells[row, 4].Value = "NrDoc";
                worksheet.Cells[row, 5].Value = "Data";
                worksheet.Cells[row, 6].Value = "Valor";
                worksheet.Cells[row, 7].Value = "Descrição";
                worksheet.Cells[row, 8].Value = "Categoria";
                worksheet.Cells[row, 9].Value = "Data Fechamento";
                worksheet.Cells[row, 10].Value = "Data Vencimento";

                row++;
                foreach (Finance finance in Lstfinance)
                {
                    // Preenche os dados detalhe
                    foreach (Transacao transacao in finance.Transacoes)
                    {
                        worksheet.Cells[row, 1].Value = transacao.NrBco;
                        worksheet.Cells[row, 2].Value = transacao.Banco;
                        worksheet.Cells[row, 3].Value = transacao.Tipo;
                        worksheet.Cells[row, 4].Value = transacao.NrDoc;
                        worksheet.Cells[row, 5].Value = transacao.Data;
                        worksheet.Cells[row, 6].Value = transacao.Valor;
                        worksheet.Cells[row, 7].Value = transacao.Descricao;
                        worksheet.Cells[row, 8].Value = transacao.Categoria;
                        worksheet.Cells[row, 9].Value = transacao.DataFechamento;
                        worksheet.Cells[row, 10].Value = transacao.DataVencimento;
                        row++;
                    }
                }
                // Salva as mudanças no arquivo
                package.Save();
            }

            Console.WriteLine("Arquivo do Excel criado com sucesso!");
        }
    }
}
