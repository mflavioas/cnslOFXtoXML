// <summary>
/// Classe controller para tratar o arquivo de Base XML gerado para subir sempre os novos unificados.
/// </summary>
/// <author>Flavio Alves</author>
/// <created>2024-02-12</created>
/// <version>1.0</version>

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
                Console.WriteLine("Ocorreu um erro ao ler o arquivo base unificado: " + ex.Message);
            }
            return [];
        }
    }
}
