// <summary>
/// Classe controller para capturar arquivo Santander xlsx da opção copiar fatura no site e colado direto no excel em todas as abas mensais.
/// </summary>
/// <author>Flavio Alves</author>
/// <created>2024-02-12</created>
/// <version>1.0</version>

using cnslOFXtoXML.models;
using OfficeOpenXml;

namespace cnslOFXtoXML.controller
{
    public class ControllerFaturaExcel
    {
        public static List<Finance> CarregaArqExcelSantander(string arquivo, ControllerParametros ConfigParametros)
        {
            List<Finance> lstFinances = [];
            Banco banco = ConfigParametros.parametros.Bancos.FirstOrDefault(b => b.Codigo == "033");
            if (File.Exists(arquivo))
            {
                Console.WriteLine($"Carregando registros da planilha: {arquivo}");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(new FileInfo(arquivo));
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    Console.WriteLine($"Lendo registros na aba: {sheet.Name}");
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
                                Categoria categoria = ConfigParametros.RetornaCategoria(descricao, banco.Codigo);
                                finance.Transacoes.Add(new()
                                {
                                    NrBco = banco.Codigo,
                                    Banco = banco.Descricao,
                                    NrDoc = row.ToString(),
                                    Tipo = string.Concat(sheet.Cells[Base0Linha - 3, 1].Value, " - ", sheet.Cells[Base0Linha - 3, 2].Value),
                                    Data = Convert.ToDateTime(sheet.Cells[row, 1].Value),
                                    Descricao = descricao,
                                    Valor = Convert.ToDouble(vlr) * -1,
                                    Grupo = categoria.Grupo,
                                    Categoria = categoria.Descricao
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
                Console.WriteLine($"Arquivo({arquivo}) convertido com sucesso para XML");
                File.Delete(Path.ChangeExtension(arquivo, ".xml"));
            }
            else
            {
                Console.WriteLine("Arquivo não encontrado.");
            }
            return lstFinances;
        }

        public static void GravarArquivoExcel(List<Finance> lstFinance, string DirOutput)
        {
            if (lstFinance is null)
            {
                return;
            }

            string filePath = Path.Combine(DirOutput, $"Financeiro_{DateTime.Now:yyyyMMdd_HHmmfff}.xlsx");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Financeiro");
                int row = 1;
                int col = 1;
                // Escreve os cabeçalhos
                worksheet.Cells[row, col++].Value = "NrBco";
                worksheet.Cells[row, col++].Value = "Banco";
                worksheet.Cells[row, col++].Value = "Tipo";
                worksheet.Cells[row, col++].Value = "NrDoc";
                worksheet.Cells[row, col++].Value = "Data Movimento";
                worksheet.Cells[row, col++].Value = "Valor";
                worksheet.Cells[row, col++].Value = "Descrição";
                worksheet.Cells[row, col++].Value = "Grupo";
                worksheet.Cells[row, col++].Value = "Categoria";
                worksheet.Cells[row, col++].Value = "Data Fechamento";
                worksheet.Cells[row, col++].Value = "Data Vencimento";

                row++;
                // Preenche os dados detalhe
                foreach (Finance finance in lstFinance)
                {
                    foreach (Transacao transacao in finance.Transacoes)
                    {
                        col = 1;
                        worksheet.Cells[row, col++].Value = transacao.NrBco;
                        worksheet.Cells[row, col++].Value = transacao.Banco;
                        worksheet.Cells[row, col++].Value = transacao.Tipo;
                        worksheet.Cells[row, col++].Value = transacao.NrDoc;
                        worksheet.Cells[row, col++].Value = transacao.Data;
                        worksheet.Cells[row, col++].Value = transacao.Valor;
                        worksheet.Cells[row, col++].Value = transacao.Descricao;
                        worksheet.Cells[row, col++].Value = transacao.Grupo;
                        worksheet.Cells[row, col++].Value = transacao.Categoria;
                        worksheet.Cells[row, col++].Value = transacao.DataFechamento;
                        worksheet.Cells[row, col++].Value = transacao.DataVencimento;
                        row++;
                    }
                }
                // Salva as mudanças no arquivo
                package.Save();
            }

            Console.WriteLine($"Arquivo: {filePath} criado com sucesso!");
        }
    }
}
