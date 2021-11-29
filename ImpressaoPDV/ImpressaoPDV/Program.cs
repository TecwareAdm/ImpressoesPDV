using ImpressaoPDV.API;
using System;
using System.Runtime.InteropServices;
using Tecware.Printer;
using Tecware.Printer.Enums;

namespace ImpressaoPDV
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_OPEN = 5;

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            // Hide window
            ShowWindow(handle, SW_HIDE);

            // Open window
            // ShowWindow(handle, SW_OPEN);

            string printerName = string.Empty;
            string codeOrder = string.Empty;
            string typePrinter = string.Empty;
            string typeAction = string.Empty;
            string token = string.Empty;
            string amb = string.Empty;

            //Example Values
            /*args = new string[1];
            args.SetValue("impressoestitaniumpdv:codeOrder=284401,typeAction=COMANDA,amb=PROD,token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiMSIsImp0aSI6ImZiMWZmMTE5LTNiNWQtNDFjYy1iNTZiLTZmYjMxM2E2NzQzMyIsImlhdCI6IjE2MzYzODA1MTgiLCJzdWIiOiJURVNURV9PTEdBX1JJIiwidXNlciI6InRlY3dhcmUuYWRtaW4iLCJwYXNzd29yZCI6ImJvc3MiLCJjb21wYW55IjoiVEVTVEVfT0xHQV9SSSIsImxvamFJZCI6IjEwIiwidHlwZSI6IlVTVUFSSU9QQURSQU8iLCJleHAiOjE2MzY0NjY5MTgsImlzcyI6Imh0dHA6Ly90ZWN3YXJlLmNvbS5ici8ifQ.a5Ku6nZsnpOiyE0mt4Q7JGmi4lWM3i4UpwIAixCqj4I", 0);
            */

            if (args.Length == 0 || args[0] == string.Empty)
                throw new Exception("values empty!");

            foreach (var item in args[0].Replace("impressoestitaniumpdv:", string.Empty)?.Split(","))
            {
                var itemAndValue = item.Split("=");

                switch (itemAndValue[0])
                {
                    case "printerName":
                        printerName = itemAndValue[1];
                        break;
                    case "codeOrder":
                        codeOrder = itemAndValue[1];
                        break;
                    case "token":
                        token = itemAndValue[1];
                        break;
                    case "typeAction":
                        typeAction = itemAndValue[1];
                        break;
                    case "typePrinter":
                        typePrinter = itemAndValue[1];
                        break;
                    case "amb":
                        amb = itemAndValue[1];
                        break;
                }
            }

            if (codeOrder == string.Empty ||
                token == string.Empty ||
                typeAction == string.Empty
            )
                throw new Exception($@"
                        Values: printerName = {printerName}; 
                        codeOrder = {codeOrder}; 
                        typePrinter = {typePrinter}; 
                        token = {token}
                        typeAction = {typeAction}

                  some value is empty!");

            var endpoint = ((REST.EndPointEnum)Enum.Parse(typeof(REST.EndPointEnum), typeAction));

            REST api = new REST(int.Parse(codeOrder), ((REST.AmbAPIEnum)Enum.Parse(typeof(REST.AmbAPIEnum), amb)), endpoint, token);

            var response = api.Get();

            PrinterType type;
            if (typePrinter != null && typePrinter != string.Empty)
                type = ((PrinterType)Enum.Parse(typeof(PrinterType), typePrinter));

            if (response.Data == null)
                throw new Exception("Loja não encontrada");

            switch (endpoint)
            {
                case REST.EndPointEnum.COMANDA:
                    if (response.Data.ImpressoraComanda == null || response.Data.ImpressoraComanda == string.Empty)
                        throw new Exception("Nome da impressora comanda vazio ou nulo!");

                    printerName = response.Data.ImpressoraComanda;
                    type = response.Data.TipoImpressoraComanda;
                    break;
                case REST.EndPointEnum.NOTA:
                    if (response.Data.ImpressoraFiscal == null || response.Data.ImpressoraFiscal == string.Empty)
                        throw new Exception("Nome da impressora fiscal vazio ou nulo!");

                    printerName = response.Data.ImpressoraFiscal;
                    type = response.Data.TipoImpressoraFiscal;
                    break;
                default:
                    type = PrinterType.Epson;
                    break;
            }

            Printer printer = new Printer(printerName, type);

            printer.Set(response.Layout);
            printer.PartialPaperCut();
            printer.PrintDocument();
        }
    }
}
