using cnslOFXtoXML.models;
using System.Xml.Serialization;

namespace cnslOFXtoXML.controller
{
    public class ControllerFinanceXML
    {
        public static List<Finance> LerArqXMLUnificado(string arqXML)
        {
            try
            {
                XmlSerializer serializer = new(typeof(List<Finance>));
                using StreamReader reader = new(arqXML);
                return serializer.Deserialize(reader) as List<Finance>;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro ao ler o arquivo unificado: " + ex.Message);
            }
            return [];
        }
    }
}
