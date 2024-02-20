// <summary>
/// Classe Controller do sistema para unificar os arquivos lidos/convertidos de XLSX, OFX, XML para base XML e saida Excel.
/// </summary>
/// <author>Flavio Alves</author>
/// <created>2024-02-12</created>
/// <version>1.0</version>

using cnslOFXtoXML.models;
using LibFinance.models;
using System.Text.RegularExpressions;
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
        private static Finance LancamentoProjecoes(List<Finance> finances)
        {
            List<Transacao> retTransacaos = [];
            foreach (Finance finance in finances)
            {
                foreach (Transacao transacao in finance.Transacoes)
                {
                    DateTime MaxFechamento = finance.Transacoes.Where(b => b.NrBco == transacao.NrBco).Max(d => d.DataFechamento);
                    DateTime DtCorte = new(MaxFechamento.Year, MaxFechamento.Month, 1);
                    int qtdMaxParcFut = finance.Transacoes.Where(b => b.NrBco == transacao.NrBco).Max(q => q.QtdParcelasFuturas);
                    if (transacao.LancamentoFuturo == Constantes.C_SIM && transacao.DataVencimento > DtCorte)
                    {
                        int QtdLnctoFut = transacao.QtdParcelasFuturas;
                        if (QtdLnctoFut == 0)
                        {
                            QtdLnctoFut = qtdMaxParcFut;
                        }
                        for (int i = 0; i < QtdLnctoFut; i++)
                        {
                            Transacao lncmtno = (Transacao)transacao.Clone();
                            if (transacao.NrParcela > 0)
                            {
                                lncmtno.NrParcela += i + 1;
                                lncmtno.QtdParcelasFuturas = lncmtno.QtdParcelas - lncmtno.NrParcela;
                            }
                            lncmtno.DataVencimento = transacao.DataVencimento.AddMonths(i + 1);
                            lncmtno.Projecao = Constantes.C_SIM;
                            retTransacaos.Add(lncmtno);
                        }
                    }
                }
            }
            if (retTransacaos.Count > 0)
            {
                Finance retProjecoes = new() { Transacoes = [] };
                retProjecoes.Transacoes.AddRange(retTransacaos);
                return retProjecoes;
            }
            return new Finance { Transacoes = [] };
        }
        private static List<Finance> RegularizaFinanceiro(List<Finance> finances, ControllerParametros ConfigParametros)
        {
            List<Finance> retLstFinance = [];
            Finance retFinance = new() { Transacoes = [] };
            foreach (Finance finance in finances)
            {
                foreach (Transacao transacao in finance.Transacoes)
                {
                    if (transacao.Projecao == Constantes.C_SIM)
                        continue;
                    Transacao retTransacao = (Transacao)transacao.Clone();
                    Banco banco = ConfigParametros.parametros.Bancos.Where(b => b.Codigo == transacao.NrBco).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(transacao.Grupo) && transacao.Grupo != Constantes.C_NDF)
                        continue;
                    Categoria categoria = ConfigParametros.RetornaCategoria(transacao.Descricao, transacao.NrBco);
                    foreach (string diaFech in ConfigParametros.parametros.Fechamento.Dias.Split(';'))
                    {
                        int diaFechamento = Convert.ToInt32(diaFech);
                        if (transacao.Valor > 0 && categoria.DiaTeto != null && categoria.DiaTeto > 0)
                        {

                            if (retTransacao.DataFechamento.Day < categoria.DiaTeto)
                            {
                                if (diaFechamento > categoria.DiaTeto)
                                {
                                    retTransacao.DiaDechamentoCaixa = diaFechamento;
                                    break;
                                }
                            }
                            else
                            {
                                if (categoria.DiaTeto > diaFechamento)
                                {
                                    retTransacao.DiaDechamentoCaixa = diaFechamento;
                                    break;
                                }
                            }
                        }
                        else
                        if (retTransacao.DataVencimento.Day <= diaFechamento)
                        {
                            retTransacao.DiaDechamentoCaixa = diaFechamento;
                            break;
                        }
                    }
                    if (categoria.DiaTeto != null && categoria.DiaTeto > 0 && transacao.DataVencimento.Day > categoria.DiaTeto)
                    {
                        DateTime dtvcto = transacao.DataFechamento.AddMonths(1);
                        retTransacao.DataVencimento = new DateTime(dtvcto.Year, dtvcto.Month, 1);
                    }
                    retTransacao.Grupo = categoria.Grupo;
                    retTransacao.Categoria = categoria.Descricao;
                    retTransacao.LancamentoFuturo = Constantes.C_NAO;
                    retTransacao.Projecao = Constantes.C_NAO;
                    retTransacao.NrParcela = 1;
                    retTransacao.QtdParcelas = 1;
                    retTransacao.QtdParcelasFuturas = 0;
                    Regex regex = new(banco.RegexParcelado);
                    Match match = regex.Match(transacao.Descricao);
                    if (match.Success)
                    {
                        retTransacao.NrParcela = Convert.ToInt32(match.Groups[1].Value);
                        retTransacao.QtdParcelas = Convert.ToInt32(match.Groups[2].Value);
                        retTransacao.QtdParcelasFuturas = retTransacao.QtdParcelas - retTransacao.NrParcela;
                        if (retTransacao.QtdParcelasFuturas < 0)
                        {
                            retTransacao.NrParcela = 1;
                            retTransacao.QtdParcelas = 1;
                            retTransacao.QtdParcelasFuturas = 0;
                        }
                    }
                    if (categoria.Projetar != null || retTransacao.QtdParcelasFuturas > 0)
                    {
                        retTransacao.LancamentoFuturo = Constantes.C_SIM;
                    }
                    retFinance.Transacoes.Add(retTransacao);
                }

            }
            retLstFinance.Add(retFinance);
            return retLstFinance;
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
            using StreamWriter streamWriter = new(Path.Combine(DirOutput, $"Base_Financeiro_{DateTime.Now.ToString(Constantes.C_FRMT_DATA)}.xml"));
            serializer.Serialize(streamWriter, lstFinance);
        }
        public static void UnificarArquivos(string[] LstParametrosEntrada)
        {
            List<string> LstArquivos = [];
            string CodigoBanco = Constantes.C_COD_BCO_DEFAULT;
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
            string DiretorioSaida = Path.Combine(Directory.GetCurrentDirectory(), "output");
            if (!Directory.Exists(DiretorioSaida))
                Directory.CreateDirectory(DiretorioSaida);
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
            Console.WriteLine("Regularizando financeiro...");
            List<Finance> ExtratoFinance = RegularizaFinanceiro(lstFinance, parametros);
            Console.WriteLine("Lançando projeções futuras...");
            Finance projecao = LancamentoProjecoes(ExtratoFinance);
            if (projecao.Transacoes.Count > 0)
            {
                ExtratoFinance.Add(projecao);
            }
            Console.WriteLine("Gerando arquivo final...");
            GerarArquivoXMLUnificado(ExtratoFinance, DiretorioSaida);
            ControllerFaturaExcel.GravarArquivoExcel(ExtratoFinance, DiretorioSaida);
        }
    }
}
