// <summary>
/// Classe teste para tratar padrao XSD dos arquivos OFX.
/// </summary>
/// <author>Flavio Alves</author>
/// <created>2024-02-12</created>
/// <version>1.0</version>
using System.Xml;
using System.Xml.Schema;

namespace cnslOFXtoXML.controller
{
    public class ControllerXSD
    {
        public static void CarregarArquivos(string[] args)
        {
            try
            {
                // Caminho para o arquivo XML que você quer carregar
                string xmlFilePath = RemoveCabecalhoOFX("C:\\Users\\mflav\\source\\repos\\mflavioas\\cnslOFXtoXML\\bin\\Debug\\net8.0\\nubank-2023-09.ofx");

                // Configurando as definições do leitor XML para validar com base nos XSDs
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;

                foreach (string arqXSD in Directory.GetFiles("C:\\Users\\mflav\\source\\repos\\mflavioas\\cnslOFXtoXML\\bin\\Debug\\net8.0\\OFX.v2.3\\"))
                {
                    if (arqXSD.Contains("OFX_"))
                        settings.Schemas.Add(null, arqXSD);
                }

                // Adicionando os esquemas de estrutura e tipos
                //settings.Schemas.Add(null, structureXsdFilePath);
                //settings.Schemas.Add(null, typesXsdFilePath);

                settings.ValidationEventHandler += ValidationCallback;

                // Criando o leitor XML com as configurações definidas
                using (XmlReader reader = XmlReader.Create(xmlFilePath, settings))
                {
                    // Ler o documento XML, isso disparará o evento de validação
                    while (reader.Read())
                    {
                        // Verificar se é um elemento de início (tag de abertura)
                        //if (reader.NodeType == XmlNodeType.Element)
                        ////if (reader.Value != null)
                        //{
                        //    // Exibir o nome do elemento (tag)
                        //    Console.WriteLine($"Tag: {reader.Name} Valor: {reader.Value}");
                        //}
                    }
                }

                Console.WriteLine("Documento XML carregado e validado com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }

            Console.ReadLine();
        }

        private static string RemoveCabecalhoOFX(string arq)
        {
            // Caminho para o arquivo de saída (após a remoção das primeiras linhas)
            string outputFile = Path.ChangeExtension(arq, "xml");
            try
            {
                // Flag indicando se a linha com "<OFX>" foi encontrada
                bool foundOFX = false;

                // Criando um leitor para o arquivo de entrada
                using (StreamReader reader = new StreamReader(arq))
                {
                    // Criando um escritor para o arquivo de saída
                    using (StreamWriter writer = new StreamWriter(outputFile))
                    {
                        string line;

                        // Ler cada linha do arquivo de entrada
                        while ((line = reader.ReadLine()) != null)
                        {
                            // Se encontrarmos a linha que contém "<OFX>", definimos a flag como verdadeira
                            if (!foundOFX && line.Contains("<OFX>"))
                            {
                                foundOFX = true;
                            }

                            // Se a flag for verdadeira, escrevemos a linha no arquivo de saída
                            if (foundOFX)
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }
                }

                Console.WriteLine("Linhas iniciais removidas com sucesso até o <OFX>.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }
            return outputFile;
        }

        // Método para lidar com eventos de validação
        private static void ValidationCallback(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                Console.WriteLine($"Erro de validação: {e.Message}");
                throw new Exception("Falha na validação do XML.");
            }
            else
            {
                Console.WriteLine($"Aviso de validação: {e.Message}");
            }
        }
    }
}
