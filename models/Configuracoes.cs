using System.Xml.Serialization;

namespace cnslOFXtoXML.models
{
    [XmlRoot(ElementName = "chave")]
    public class Chave
    {

        [XmlAttribute(AttributeName = "nometag")]
        public required string Nometag { get; set; }

        [XmlAttribute(AttributeName = "tipo")]
        public required string Tipo { get; set; }
    }

    [XmlRoot(ElementName = "parametros")]
    public class Configuracoes
    {

        [XmlElement(ElementName = "chave")]
        public required List<Chave> Chave { get; set; }
    }
}
