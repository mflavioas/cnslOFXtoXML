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

    [XmlRoot(ElementName = "Categorias")]
    public class Categorias
    {

        [XmlElement(ElementName = "categoria")]
        public List<Categoria> categoria { get; set; }
    }


}
