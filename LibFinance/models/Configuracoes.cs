// <summary>
/// Classe modelo de arquivo de configurações do padrao e tratamento de tags do OFX.
/// </summary>
/// <author>Flavio Alves</author>
/// <created>2024-02-12</created>
/// <version>1.0</version>

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
