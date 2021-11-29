using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using Tecware.Printer.Enums;

namespace ImpressaoPDV.API
{
    public class REST
    {
        public REST(int idOrder, AmbAPIEnum amb_API, EndPointEnum endPoint, string token)
        {
            IdOrder = idOrder;
            Amb_API = amb_API;
            Token = token;
            End_Point = endPoint;
        }

        public int IdOrder { get; set; }
        public string Token { get; set; }
        public AmbAPIEnum Amb_API { get; set; }
        public EndPointEnum End_Point { get; set; }

        public enum AmbAPIEnum
        {
            PROD = 1,
            DEV
        }

        public enum EndPointEnum
        {
            COMANDA = 1,
            SETOR,
            NOTA
        }

        public static Dictionary<string, string> AmbAPI = new Dictionary<string, string>
        {
            { "PROD", "http://api.tecware.com.br/api/"},
            { "DEV", "https://teste-api.tecware.com.br/api/"}
        };


        public static Dictionary<string, string> EndPoint = new Dictionary<string, string>
        {
            { "COMANDA", "PDV/GerarImpressaoComanda/{_key}"},
            { "SETOR", "Lojas/GerarImpressaoSetor/{_key}"},
            { "NOTA", "NotasFiscais/GerarImpressaoNFe/{_key}"},
            { "LOJA", "Lojas/LojaLogged"}
        };

        public Response Get()
        {
            var client = new RestClient(
                AmbAPI.FirstOrDefault(c => c.Key == Amb_API.ToString())
                .Value);

            var request = new RestRequest(EndPoint.
                FirstOrDefault(c => c.Key == End_Point.ToString())
                .Value.Replace("{_key}", IdOrder.ToString()), Method.GET);

            client.Authenticator = new JwtAuthenticator(Token);

            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new Exception(response.Content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("Token expired!");

            var layout = response.RawBytes;

            request = new RestRequest("Lojas/LojaLogged", Method.GET);

            response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new Exception(response.Content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("Token expired!");

            var responseAPI = JsonConvert.DeserializeObject<Response>(response.Content);

            if(responseAPI == null)
                throw new Exception("Ambiente informado instável!");

            responseAPI.Layout = layout;

            return responseAPI;
        }


        public class Response
        {
            public bool Sucesso { get; set; }
            public Loja Data { get; set; }
            public byte[] Layout { get; set; }
        }

        public class Loja
        {
            public int Codigo { get; set; }
            public int ExCode { get; set; }
            public string Nome { get; set; }
            public int Empresa { get; set; }
            public int ListaProduto { get; set; }
            public int ListaAtributo { get; set; }
            public string NomeEmpresa { get; set; }
            public int UltimaNF { get; set; }
            public string UsuarioGO2GO { get; set; }
            public string SenhaGO2GO { get; set; }
            public string Logo { get; set; }
            public string ImpressoraComanda { get; set; }
            public PrinterType TipoImpressoraComanda { get; set; }
            public string ImpressoraFiscal { get; set; }
            public PrinterType TipoImpressoraFiscal { get; set; }
        }
    }
}
