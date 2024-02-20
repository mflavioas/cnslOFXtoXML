/// <summary>
/// Classe modelo do arquivo financeiro final.
/// </summary>
/// <author>Flavio Alves</author>
/// <created>2024-02-12</created>
/// <version>1.0</version>

namespace cnslOFXtoXML.models
{
    public class Transacao : ICloneable
    {
        public string NrDoc { get; set; }
        public string Tipo { get; set; }
        public DateTime Data { get; set; }
        public double Valor { get; set; }
        public string Descricao { get; set; }
        public string Grupo { get; set; }
        public string Categoria { get; set; }
        public string NrBco { get; set; }
        public string Banco { get; set; }
        public DateTime DataFechamento { get; set; }
        public DateTime DataVencimento { get; set; }
        public string LancamentoFuturo { get; set; }
        public int QtdParcelas { get; set; }
        public int NrParcela { get; set; }
        public int QtdParcelasFuturas { get; set; }
        public string Projecao { get; set; }
        public int DiaDechamentoCaixa { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
    public class Finance
    {
        public List<Transacao> Transacoes { get; set; }
    }
}
