class Program
{
    static void Main(string[] args)
    {
        // Verifica se o caminho do arquivo OFX foi fornecido como argumento
        if (args.Length == 0)
        {
            Console.WriteLine("Por favor, forneça o caminho do arquivo OFX/XLS como argumento.");
            return;
        }
        cnslOFXtoXML.controller.ControllerUnifica.UnificarArquivos(args);
    }
}