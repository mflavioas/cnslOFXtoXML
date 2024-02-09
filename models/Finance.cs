using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;

namespace cnslOFXtoXML.models
{
    public class Transacao
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public DateTime Data { get; set; }
        public double Valor { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public string GetLinhaCSV()
        {  
            return string.Concat(Id, ";", Tipo, ";", Data.ToString("ddMMyyyy"), ";", Descricao, ";", Categoria, ";", Valor);
        }
    }
    public class Finance
    {
        public int Id { get; set; }
        public string Banco { get; set; }
        public List<Transacao> Transacoes { get; set; }
        public string GetLinhaCSV()
        {
            return string.Concat(Id, ";", Banco, ";");
        }
    }
}
