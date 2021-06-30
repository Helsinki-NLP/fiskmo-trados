﻿using Newtonsoft.Json;
using OpusMTInterface;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatTranslationProvider
{
    public class OpusCatMtServiceConnection
    {
        internal OpusCatConnectionStatus Status { get; set; }

        internal enum OpusCatConnectionStatus
        {
            Ok,
            NoConnection,
            ServiceError
        }

        public OpusCatMtServiceConnection()
        {
        }
        
        internal string Translate(
            string host,
            string port,
            string source, 
            string sourceLangCode, 
            string targetLangCode, 
            string modelTag)
        {

            IRestResponse translationResponse =
                this.SendRequest(host, port, "TranslateJson", Method.GET,
                new Dictionary<string, string>()
                    {
                        { "input",source },
                        { "srcLangCode", sourceLangCode },
                        { "trgLangCode", targetLangCode },
                        { "modelTag", modelTag }
                    }
                );
            
            if (translationResponse.IsSuccessful)
            {
                Translation translationObject = JsonConvert.DeserializeObject<Translation>(translationResponse.Content);
                return translationObject.translation;
            }
            else
            {
                throw new Exception($"Problem fetching translation from OPUS-CAT MT Engine: {translationResponse.StatusCode}");
                //return null;
            }
        }

        private IRestResponse SendRequest(
            string host, 
            string port, 
            string endPoint,
            Method httpMethod,
            Dictionary<string,string> queryParameters = null)
        {
            var client = new RestClient(
                $"http://{host}:{port}/MtRestService/{endPoint}");
            client.Timeout = -1;
            var request = new RestRequest(httpMethod);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("tokenCode", "0");

            if (queryParameters != null)
            {
                foreach (var queryParameter in queryParameters)
                {
                    request.AddQueryParameter(queryParameter.Key, queryParameter.Value);
                }
            }

            return client.Execute(request);
        }

        internal List<string> ListSupportedLanguages(string host, string port)
        {
            IRestResponse supportedLanguageResponse = 
                this.SendRequest(host,port,"ListSupportedLanguagePairs",Method.GET);

            if (supportedLanguageResponse.IsSuccessful)
            {
                List<string> supportedLanguages = JsonConvert.DeserializeObject<List<string>>(supportedLanguageResponse.Content);
                return supportedLanguages;
            }
            else
            {
                throw new Exception($"Problem fetching list of supported language pairs from OPUS-CAT MT Engine: {supportedLanguageResponse.StatusCode}");
            }
        }

        internal IEnumerable<string> GetLanguagePairModelTags(string host, string port, string sourceLangCode, string targetLangCode)
        {
            IRestResponse modelTagResponse =
                this.SendRequest(host, port, "GetLanguagePairModelTags", Method.GET,
                new Dictionary<string, string>()
                    {
                        { "srcLangCode", sourceLangCode },
                        { "trgLangCode", targetLangCode }
                    }
                );
            
            if (modelTagResponse.IsSuccessful)
            {
                List<string> languagePairTags = JsonConvert.DeserializeObject<List<string>>(modelTagResponse.Content);
                return languagePairTags;
            }
            else
            {
                throw new Exception($"Problem fetching tags for a language pair from OPUS-CAT MT Engine: {modelTagResponse.StatusCode}");
            }
        }

        public HttpStatusCode PreOrderBatch(
            string host,
            string port,
            List<string> source,
            string sourceLangCode,
            string targetLangCode,
            string modelTag)
        {

            var client = new RestClient(
            $"http://{host}:{port}/MtRestService/PreOrderBatch");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(source);
            request.AddQueryParameter("tokenCode", "0");
            request.AddQueryParameter("srcLangCode", sourceLangCode);
            request.AddQueryParameter("trgLangCode", targetLangCode);
            request.AddQueryParameter("modelTag", modelTag);

            IRestResponse preOrderResponse = client.Execute(request);

            if (preOrderResponse.IsSuccessful)
            {
                return preOrderResponse.StatusCode;
            }
            else
            {
                throw new Exception($"Problem preordering from OPUS-CAT MT Engine: {preOrderResponse.StatusCode}");
            }
        }
    }
}