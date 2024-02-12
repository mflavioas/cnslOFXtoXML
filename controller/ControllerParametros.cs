// <summary>
/// Classe controle para modelo de configurações do Banco e Categorias do arquivo financeiro.
/// </summary>
/// <author>Flavio Alves</author>
/// <created>2024-02-12</created>
/// <version>1.0</version>

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
                Console.WriteLine("Ocorreu um erro ao ler o arquivo de parametros: " + ex.Message);
            }
            return new Parametros();
        }

        public Categoria RetornaCategoria(string Descricao, string CodigoBanco)
        {
            Banco banco = parametros.Bancos.FirstOrDefault(b => b.Codigo == CodigoBanco);
            foreach (Categoria categoria in banco.Categorias)
            {
                foreach (string valor in categoria.Valores.Split(';'))
                {
                    if (Descricao.Contains(valor.Trim(), StringComparison.CurrentCultureIgnoreCase))
                        return categoria;
                }
            }
            return new Categoria { Descricao = "Indefinido", Grupo = "NDF" };
        }
    }
}
