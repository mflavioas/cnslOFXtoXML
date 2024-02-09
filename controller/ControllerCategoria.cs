using cnslOFXtoXML.models;
using System.Drawing;
using System.Xml.Serialization;

namespace cnslOFXtoXML.controller
{
    public class ControllerCategoria
    {
        private Categorias Categorias { get; set; }

        public ControllerCategoria()
        {
            Categorias = LerArqCategoria();
        }
        private Categorias LerArqCategoria()
        {
            string caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "config\\Categorias.xml");
            try
            {
                XmlSerializer serializer = new(typeof(Categorias));
                using StreamReader reader = new(caminhoArquivo);
                return serializer.Deserialize(reader) as Categorias;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro ao ler o arquivo de categorias: " + ex.Message);
            }
            return new Categorias();
        }

        public string RetornaCategoria(string Descricao)
        {
            foreach (Categoria categoria in Categorias.categoria) 
            {
                foreach (string valor in categoria.Valores.Split(';'))
                {
                    if (Descricao.Contains(valor))
                        return categoria.Descricao;
                }
            }
            return "NDF";
        }
    }
}
