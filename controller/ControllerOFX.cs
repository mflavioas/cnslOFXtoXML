using System.Text.RegularExpressions;
using System.Text;
using System.Xml.Serialization;
using cnslOFXtoXML.models;

namespace cnslOFXtoXML
{
    public class ControllerOFX
    {
        private static Configuracoes? LerArqConfig()
        {
            string caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "config\\config.xml");
            try
            {
                XmlSerializer serializer = new(typeof(Configuracoes));
                using StreamReader reader = new(caminhoArquivo);
                return serializer.Deserialize(reader) as Configuracoes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro ao ler o arquivo de configurações: " + ex.Message);
            }
            return null;
        }
        private static string RetornaLinhaXML(string linha, Configuracoes? configuracoes)
        {
            if (configuracoes == null)
                throw new Exception("Arquivo de configurações inválido.");
            string ret = linha;
            foreach (Chave chave in configuracoes.Chave)
            {
                if (linha.Contains(chave.Nometag))
                {
                    if (chave.Tipo == "data")
                    {
                        string dataHora = linha.Substring(linha.IndexOf(">") + 1, 14);
                        DateTime data = DateTime.ParseExact(dataHora, "yyyyMMddHHmmss", null);
                        ret = $"<{chave.Nometag}>{data:yyyy-MM-dd}</{chave.Nometag}>";
                        break;
                    }
                    ret = string.Concat(Regex.Replace(linha, @"\s+", " "), "</", chave.Nometag, ">");
                    break;
                }
            }
            return ret;
        }
        public static string ConverterOFXToXML(string arquivoOFX)
        {
            if (!File.Exists(arquivoOFX))
            {
                Console.WriteLine("O arquivo OFX especificado não existe.");
                return string.Empty;
            }
            try
            {
                Configuracoes? configuracoes = LerArqConfig();
                string[] conteudoOFX = File.ReadAllLines(arquivoOFX);
                bool arqOFX = false;
                StringBuilder stringBuilder = new();
                foreach (string linha in conteudoOFX)
                {
                    if (linha.Contains("<OFX>"))
                        arqOFX = true;
                    if (arqOFX)
                    {
                        stringBuilder.AppendLine(RetornaLinhaXML(linha, configuracoes));
                    }
                }
                string novoArquivoXML = Path.ChangeExtension(arquivoOFX, ".xml");
                if (File.Exists(novoArquivoXML))
                    File.Delete(novoArquivoXML);
                File.WriteAllText(novoArquivoXML, stringBuilder.ToString());
                Console.WriteLine("Arquivo OFX convertido com sucesso para XML: " + novoArquivoXML);
                return novoArquivoXML;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro ao converter o arquivo OFX para XML: " + ex.Message);
            }
            return string.Empty;
        }
    }
}
