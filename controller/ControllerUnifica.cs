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
                Console.WriteLine("Ocorreu um erro ao ler o arquivo XML: " + ex.Message);
            }
            return null;
        }

        private static Transacao RetornaTransacao(ControllerParametros categoria, STMTTRN trnscOFX)
        {
            return new Transacao
            {
                NrDoc = trnscOFX.FITID,
                Data = trnscOFX.DTPOSTED,
                Tipo = trnscOFX.TRNTYPE,
                Descricao = trnscOFX.MEMO,
                Valor = trnscOFX.TRNAMT,
                Categoria = categoria.RetornaCategoria(trnscOFX.MEMO)
            };
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
                finance.Id = ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKACCTFROM.BANKID;
                finance.Banco = ofx.SIGNONMSGSRSV1.SONRS.FI.ORG;
                foreach (STMTTRN trnscOFX in ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.STMTTRN.Where(x =>
                    x.DTPOSTED >= ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.DTSTART &&
                    x.DTPOSTED <= ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.DTEND
                    ).ToList())
                {
                    finance.Transacoes.Add(RetornaTransacao(ConfigParametros, trnscOFX));
                }
            }
            else
            {
                Banco banco = ConfigParametros.parametros.Bancos.Where(b => b.TipoArquivo == "OFX").FirstOrDefault();
                finance.Id = banco.Codigo;
                finance.Banco = banco.Descricao;
                finance.DataFechamento = ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND;
                DateTime dtvcto = finance.DataFechamento.AddMonths(1);
                if (banco.DiaVencimento > finance.DataFechamento.Day)
                    dtvcto = finance.DataFechamento;
                finance.DataVencimento = new DateTime(dtvcto.Year, dtvcto.Month, banco.DiaVencimento);
                if (ofx.SIGNONMSGSRSV1.SONRS.FI != null)
                    finance.Banco = ofx.SIGNONMSGSRSV1.SONRS.FI.ORG;
                foreach (STMTTRN trnscOFX in ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.STMTTRN.Where(x =>
                    x.DTPOSTED >= ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTSTART &&
                    x.DTPOSTED <= ofx.CREDITCARDMSGSRSV1.CCSTMTTRNRS.CCSTMTRS.BANKTRANLIST.DTEND
                    ).ToList())
                {
                    finance.Transacoes.Add(RetornaTransacao(ConfigParametros, trnscOFX));
                }
            }
            File.Delete(caminhoArquivoXML);
            return finance;
        }
        public static void UnificarArquivos(string[] LstArquivos)
        {
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
                else
                {
                    lstFinance.AddRange(ControllerFaturaExcel.CarregaArqExcelSantander(arquivo, parametros));
                }
            }
            XmlSerializer serializer = new(typeof(List<Finance>));
            using StreamWriter streamWriter = new(Path.Combine(Directory.GetCurrentDirectory(), "Finance.xml"));
            serializer.Serialize(streamWriter, lstFinance);
        }
    }
}
