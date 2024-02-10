using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;

namespace cnslOFXtoXML.models
{
    public class Transacao
    {
        public string NrDoc { get; set; }
        public string Tipo { get; set; }
        public DateTime Data { get; set; }
        public double Valor { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public string NrBco { get; set; }
        public string Banco { get; set; }
        public DateTime DataFechamento { get; set; }
        public DateTime DataVencimento { get; set; }
    }
    public class Finance
    {
        public List<Transacao> Transacoes { get; set; }
    }
}
