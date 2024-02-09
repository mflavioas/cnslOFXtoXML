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

        private static Finance ConverteOFXToFinance(string caminhoArquivoXML)
        {
            OFX? ofx = LerArqXML(caminhoArquivoXML);
            Finance finance = new() { Transacoes = [] };
            if (ofx == null)
            {
                return finance;
            }
            ControllerCategoria categoria = new();
            finance.Id = ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKACCTFROM.BANKID;
            finance.Banco = ofx.SIGNONMSGSRSV1.SONRS.FI.ORG;
            foreach (STMTTRN trnscOFX in ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.STMTTRN.Where(x =>
                x.DTPOSTED >= ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.DTSTART &&
                x.DTPOSTED <= ofx.BANKMSGSRSV1.STMTTRNRS.STMTRS.BANKTRANLIST.DTEND
                ).ToList())
            {
                finance.Transacoes.Add(new Transacao
                {
                    Id = trnscOFX.FITID,
                    Data = trnscOFX.DTPOSTED,
                    Tipo = trnscOFX.TRNTYPE,
                    Descricao = trnscOFX.MEMO,
                    Valor = trnscOFX.TRNAMT,
                    Categoria = categoria.RetornaCategoria(trnscOFX.MEMO)
                });
            }
            File.Delete(caminhoArquivoXML);
            return finance;
        }
        public static void UnificarXMLOFX(string[] arqs)
        {
            List<Finance> lstCSV = [];
            foreach (string arq in arqs)
            {
                lstCSV.Add(ConverteOFXToFinance(arq));
            }
            XmlSerializer serializer = new(typeof(List<Finance>));
            using StreamWriter streamWriter = new(Path.Combine(Directory.GetCurrentDirectory(), "Finance.xml"));
            serializer.Serialize(streamWriter, lstCSV);
        }
    }
}
