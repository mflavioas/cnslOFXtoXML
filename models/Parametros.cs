// <summary>
/// Classe modelo de configurações do Banco e Categorias do arquivo financeiro.
/// </summary>
/// <author>Flavio Alves</author>
/// <created>2024-02-12</created>
/// <version>1.0</version>

using System.Xml.Serialization;

namespace cnslOFXtoXML.models
{
    [XmlRoot(ElementName = "categoria")]
    public class Categoria
    {
        [XmlAttribute(AttributeName = "grupo")]
        public string Grupo { get; set; }

        [XmlAttribute(AttributeName = "descricao")]
        public string Descricao { get; set; }

        [XmlAttribute(AttributeName = "valores")]
        public string Valores { get; set; }
    }

    [XmlRoot(ElementName = "banco")]
    public class Banco
    {

        [XmlAttribute(AttributeName = "descricao")]
        public string Descricao { get; set; }

        [XmlAttribute(AttributeName = "codigo")]
        public string Codigo { get; set; }
        
        [XmlAttribute(AttributeName = "diavcto")]
        public int DiaVencimento { get; set; }

        [XmlAttribute(AttributeName = "ignorar")]
        public string Ignorar { get; set; }

        [XmlElement(ElementName = "categoria")]
        public List<Categoria> Categorias { get; set; }
    }

    [XmlRoot(ElementName = "Parametros")]
    public class Parametros
    {
        [XmlElement(ElementName = "banco")]
        public List<Banco> Bancos { get; set; }
    }


}
