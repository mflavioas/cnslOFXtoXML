using cnslOFXtoXML.models;
using System.Xml.Serialization;

namespace cnslOFXtoXML.controller
{
    public class ControllerParametros
    {
        public Parametros parametros { get; set; }

        public ControllerParametros()
        {
            parametros = LerArqCategoria();
        }
        private Parametros LerArqCategoria()
        {
            string caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "config\\Parametros.xml");
            try
            {
                XmlSerializer serializer = new(typeof(Parametros));
                using StreamReader reader = new(caminhoArquivo);
                return serializer.Deserialize(reader) as Parametros;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro ao ler o arquivo de categorias: " + ex.Message);
            }
            return new Parametros();
        }

        public string RetornaCategoria(string Descricao)
        {
            foreach (Categoria categoria in parametros.Categorias) 
            {
                foreach (string valor in categoria.Valores.Split(';'))
                {
                    if (Descricao.Contains(valor.Trim(), StringComparison.CurrentCultureIgnoreCase))
                        return categoria.Descricao;
                }
            }
            return "Indefinido";
        }
    }
}
