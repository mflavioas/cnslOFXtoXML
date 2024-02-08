class Program
{
    static void Main(string[] args)
    {
        // Verifica se o caminho do arquivo OFX foi fornecido como argumento
        if (args.Length == 0)
        {
            Console.WriteLine("Por favor, forneça o caminho do arquivo OFX como argumento.");
            return;
        }
        List<string> listArqXML = [];
        foreach (string arg in args)
        {
            string arqXML = cnslOFXtoXML.source.ControllerOFX.ConverterOFXToXML(arg);
            if (!string.IsNullOrWhiteSpace(arqXML))
            {
                listArqXML.Add(arqXML);
            }
        }
        cnslOFXtoXML.controller.ControllerUnifica.UnificarXMLOFX(listArqXML.ToArray());
    }
}