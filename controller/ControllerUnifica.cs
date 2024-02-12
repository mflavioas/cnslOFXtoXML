// <summary>
/// Classe Controller do sistema para unificar os arquivos lidos/convertidos de XLSX, OFX, XML para base XML e saida Excel.
/// </summary>
/// <author>Flavio Alves</author>
/// <created>2024-02-12</created>
/// <version>1.0</version>

using cnslOFXtoXML.models;
using System.Xml.Serialization;

namespace cnslOFXtoXML.controller
{
    public class ControllerUnifica
    {
        private static OFX? LerArqXML(string caminhoArquivo)
        {
            try
            {
                XmlSerializer serializer = new(typeof(OFX));
                using StreamReader reader = new(caminhoArquivo);
                return serializer.Deserialize(reader) as OFX;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro ao ler o arquivo({caminhoArquivo}) OFX convertido para XML: {ex.Message}");
            }
            return null;
        }
        private static bool IgnorarLinhaDescricao(string Descricao, string[] LstIgnorar)
        {
            string descricao = Descricao;
            foreach (string ignore in LstIgnorar)
            {

                if (descricao.Contains(ignore, StringComparison.CurrentCultureIgnoreCase))
                {
                    descricao = string.Empty;
                    break;
                }
            }
            return string.IsNullOrWhiteSpace(descricao);
        }
        private static Finance ConverteOFXToFinance(string caminhoArquivoXML, ControllerParametros ConfigParametros, string CodigoBanco)
        {
            OFX? ofx = LerArqXML(caminhoArquivoXML);
            Finance finance = new() { Transacoes = [] };
            if (ofx == null)
            {
                return finance;
            }
            try
            {
                if (ofx.BANKMSGSRSV1 != null)
                {
                    Banco banco = ConfigParametros.parametros.Bancos.Where(b => b.Codigo == ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKACCTFROM.BANKID.PadRight(4, '0').Substring(1, 3)).FirstOrDefault();
                    string[] ignorar = banco.Ignorar.Split(";");
                    foreach (STMTTRN trnscOFX in ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.STMTTRN.Where(x =>
                        x.DTPOSTED >= ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.DTSTART &&
                        x.DTPOSTED <= ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.DTEND
                        ).ToList())
                    {
                        if (IgnorarLinhaDescricao(trnscOFX.MEMO, ignorar))
                            continue;
                        Categoria categoria = ConfigParametros.RetornaCategoria(trnscOFX.MEMO, banco.Codigo);
                        finance.Transacoes.Add(new Transacao
                        {
                            NrBco = banco.Codigo,
                            Banco = banco.Descricao,
                            DataFechamento = trnscOFX.DTPOSTED,
                            DataVencimento = trnscOFX.DTPOSTED,
                            NrDoc = trnscOFX.FITID,
                            Data = trnscOFX.DTPOSTED,
                            Tipo = "Extrato",
                            Descricao = trnscOFX.MEMO,
                            Valor = trnscOFX.TRNAMT,
                            Grupo = categoria.Grupo,
                            Categoria = categoria.Descricao
                        });
                    }
                }
                else
                {
                    Banco banco = ConfigParametros.parametros.Bancos.Where(b => b.Codigo == CodigoBanco).FirstOrDefault();
                    string[] ignorar = banco.Ignorar.Split(";");
                    DateTime dtvcto = ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND.AddMonths(1);
                    if (banco.DiaVencimento > ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND.Day)
                        dtvcto = ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND;
                    foreach (STMTTRN trnscOFX in ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.STMTTRN.Where(x =>
                        x.DTPOSTED >= ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTSTART &&
                        x.DTPOSTED <= ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND
                        ).ToList())
                    {
                        if (IgnorarLinhaDescricao(trnscOFX.MEMO, ignorar))
                            continue;
                        Categoria categoria = ConfigParametros.RetornaCategoria(trnscOFX.MEMO, banco.Codigo);
                        finance.Transacoes.Add(new Transacao
                        {
                            NrBco = banco.Codigo,
                            Banco = banco.Descricao,
                            DataFechamento = ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND,
                            DataVencimento = new DateTime(dtvcto.Year, dtvcto.Month, banco.DiaVencimento),
                            NrDoc = trnscOFX.FITID,
                            Data = trnscOFX.DTPOSTED,
                            Tipo = "Fatura",
                            Descricao = trnscOFX.MEMO,
                            Valor = trnscOFX.TRNAMT,
                            Grupo = categoria.Grupo,
                            Categoria = categoria.Descricao
                        });
                    }
                }
            }
            finally
            {
                File.Delete(caminhoArquivoXML);
            }
            return finance;
        }

        private static void GerarArquivoXMLUnificado(List<Finance> lstFinance, string DirOutput)
        {
            XmlSerializer serializer = new(typeof(List<Finance>));
            using StreamWriter streamWriter = new(Path.Combine(DirOutput, $"Base_Financeiro_{DateTime.Now:yyyyMMdd_HHmmfff}.xml"));
            serializer.Serialize(streamWriter, lstFinance);
        }

        public static void UnificarArquivos(string[] LstParametrosEntrada)
        {
            List<string> LstArquivos = [];
            string CodigoBanco = "260";
            foreach (string parentr in LstParametrosEntrada)
            {
                if (parentr.Contains("Banco="))
                {
                    string[] bco = parentr.Split('=');
                    CodigoBanco = bco[1];
                    continue;
                }

                if (File.Exists(parentr))
                    LstArquivos.Add(parentr);
                else if (Directory.Exists(parentr))
                {
                    foreach (string arqv in Directory.GetFiles(parentr))
                    {
                        LstArquivos.Add(arqv);
                    }
                }
            }
            string OutPut = Path.Combine(Directory.GetCurrentDirectory(), "output");
            if (!Directory.Exists(OutPut))
                Directory.CreateDirectory(OutPut);
            List<Finance> lstFinance = [];
            ControllerParametros parametros = new();
            foreach (string arquivo in LstArquivos)
            {
                if (Path.GetExtension(arquivo).Equals(".OFX", StringComparison.CurrentCultureIgnoreCase))
                {
                    string arqXML = ControllerOFX.ConverterOFXToXML(arquivo);
                    if (!string.IsNullOrWhiteSpace(arqXML))
                    {
                        lstFinance.Add(ConverteOFXToFinance(arqXML, parametros, CodigoBanco));
                    }
                }
                else if (Path.GetExtension(arquivo).Equals(".XML", StringComparison.CurrentCultureIgnoreCase))
                {
                    lstFinance.AddRange(ControllerFinanceXML.LerArqXMLUnificado(arquivo));
                }
                else if (Path.GetExtension(arquivo).Equals(".XLSX", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (arquivo.Contains("Santander", StringComparison.CurrentCultureIgnoreCase))
                        lstFinance.AddRange(ControllerFaturaExcel.CarregaArqExcelSantander(arquivo, parametros));
                    else
                        throw new Exception($"XLS não implementado ou não possui o nome do Banco no nome do arquivo.");
                }
                else
                    throw new Exception($"Não implementado importação para este formato de arquivo: {Path.GetExtension(arquivo)}.");
            }
            GerarArquivoXMLUnificado(lstFinance, OutPut);
            ControllerFaturaExcel.GravarArquivoExcel(lstFinance, OutPut);
        }
    }
}
