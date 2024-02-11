using cnslOFXtoXML.models;
using System.Drawing;
using System.Drawing.Text;
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
                Console.WriteLine("Ocorreu um erro ao ler o arquivo XML: " + ex.Message);
            }
            return null;
        }
        private static Finance ConverteOFXToFinance(string caminhoArquivoXML, ControllerParametros ConfigParametros)
        {
            OFX? ofx = LerArqXML(caminhoArquivoXML);
            Finance finance = new() { Transacoes = [] };
            if (ofx == null)
            {
                return finance;
            }

            if (ofx.BANKMSGSRSV1 != null)
            {
                foreach (STMTTRN trnscOFX in ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.STMTTRN.Where(x =>
                    x.DTPOSTED >= ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.DTSTART &&
                    x.DTPOSTED <= ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.DTEND
                    ).ToList())
                {
                    finance.Transacoes.Add(new Transacao
                    {
                        NrBco = ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKACCTFROM.BANKID,
                        Banco = ofx.SIGNONMSGSRSV1.SONRS.FI.ORG,
                        DataFechamento = trnscOFX.DTPOSTED,
                        DataVencimento = trnscOFX.DTPOSTED,
                        NrDoc = trnscOFX.FITID,
                        Data = trnscOFX.DTPOSTED,
                        Tipo = "Extrato",
                        Descricao = trnscOFX.MEMO,
                        Valor = trnscOFX.TRNAMT,
                        Categoria = ConfigParametros.RetornaCategoria(trnscOFX.MEMO)
                    });
                }
            }
            else
            {
                Banco banco = ConfigParametros.parametros.Bancos.Where(b => b.TipoArquivo == "OFX").FirstOrDefault();
                DateTime dtvcto = ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND.AddMonths(1);
                if (banco.DiaVencimento > ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND.Day)
                    dtvcto = ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND;
                foreach (STMTTRN trnscOFX in ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.STMTTRN.Where(x =>
                    x.DTPOSTED >= ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTSTART &&
                    x.DTPOSTED <= ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND
                    ).ToList())
                {
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
                        Categoria = ConfigParametros.RetornaCategoria(trnscOFX.MEMO)
                    });
                }
            }
            File.Delete(caminhoArquivoXML);
            return finance;
        }

        private static void GerarArquivoXMLUnificado(List<Finance> lstFinance, string DirOutput)
        {
            XmlSerializer serializer = new(typeof(List<Finance>));
            using StreamWriter streamWriter = new(Path.Combine(DirOutput, $"Base_Financeiro_{DateTime.Now:yyyyMMddfff}.xml"));
            serializer.Serialize(streamWriter, lstFinance);
        }

        public static void UnificarArquivos(string[] LstParametrosEntrada)
        {
            List<string> LstArquivos = [];
            foreach (string parentr in LstParametrosEntrada)
            {
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
                        lstFinance.Add(ConverteOFXToFinance(arqXML, parametros));
                    }
                }
                else if (Path.GetExtension(arquivo).Equals(".XML", StringComparison.CurrentCultureIgnoreCase))
                {
                    lstFinance.AddRange(ControllerFinanceXML.LerArqXMLUnificado(arquivo));
                }
                else
                {
                    lstFinance.AddRange(ControllerFaturaExcel.CarregaArqExcelSantander(arquivo, parametros));
                }
            }
            GerarArquivoXMLUnificado(lstFinance, OutPut);
            ControllerFaturaExcel.GravarArquivoExcel(lstFinance, OutPut);
        }
    }
}
