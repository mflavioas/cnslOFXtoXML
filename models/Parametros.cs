using System.Xml.Serialization;

namespace cnslOFXtoXML.models
{
    [XmlRoot(ElementName = "categoria")]
    public class Categoria
    {

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

        [XmlAttribute(AttributeName = "tparq")]
        public string TipoArquivo { get; set; }

        [XmlAttribute(AttributeName = "ignorar")]
        public string Ignorar { get; set; }
    }

    [XmlRoot(ElementName = "Parametros")]
    public class Parametros
    {

        [XmlElement(ElementName = "categoria")]
        public List<Categoria> Categorias { get; set; }
        
        [XmlElement(ElementName = "banco")]
        public List<Banco> Bancos { get; set; }
    }


}
