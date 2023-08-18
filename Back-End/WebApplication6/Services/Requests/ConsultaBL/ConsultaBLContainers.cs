﻿using System.Net;
using System.Threading.Tasks;
using System;
using WebApplication6.Repositories;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WebApplication6.DTO.ConsultaBL;

namespace WebApplication6.Services.ConsultaBL
{
    public class ConsultaBLContainersRequest
    {
        private readonly CookieRepository _cookieRepository;

        public ConsultaBLContainersRequest(CookieRepository cookieRepository)
        {
            _cookieRepository = cookieRepository;
        }

        public async Task<object> ConsultaBL(string filCod, string usnCod, string cntCod)
        {
            var url = "https://capital-homologacao.conexos.cloud/api-etl/capital/consultaContainersBL";

            if (string.IsNullOrEmpty(filCod) || string.IsNullOrEmpty(usnCod))
            {
                throw new ArgumentException("Credenciais inválidas");
            }

            var cookieContainer = new CookieContainer();
            var uri = new Uri("https://capital-homologacao.conexos.cloud");

            var cookieDTO = _cookieRepository.RecuperarCookie();

            if (cookieDTO == null)
            {
                throw new ArgumentException("Cookie não encontrado");
            }

            var cookie = new Cookie
            {
                Name = cookieDTO.Name,
                Value = cookieDTO.Value,
                Domain = cookieDTO.Domain,
                Path = cookieDTO.Path,
                Expires = cookieDTO.Expires,
            };

            cookieContainer.Add(uri, cookie);
            using (var httpClient = new HttpClient(new HttpClientHandler { CookieContainer = cookieContainer }))
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.Add("Cnx-filCod", filCod);
                httpClient.DefaultRequestHeaders.Add("Cnx-usnCod", usnCod);

                var queryParams = $"?cntCod={cntCod}";
                var requestUrl = url + queryParams;

                var response = await httpClient.GetAsync(requestUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var obj = JsonConvert.DeserializeObject<CnxListResponseConhecimentoContainersDTO>(responseContent);

                    return obj;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ArgumentException("Acesso não autorizado");
                }
                else
                {
                    Console.WriteLine($"Ocorreu um erro: {response.StatusCode}");
                    return null;
                }

            }
        }
    }
}