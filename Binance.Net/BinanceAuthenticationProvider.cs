﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;

namespace Binance.Net
{
    internal class BinanceAuthenticationProvider: AuthenticationProvider
    {
        private readonly HMACSHA256 encryptor;
        private readonly ArrayParametersSerialization arraySerialization;

        public BinanceAuthenticationProvider(ApiCredentials credentials, ArrayParametersSerialization arraySerialization) : base(credentials)
        {
            if (credentials.Secret == null)
                throw new ArgumentException("No valid API credentials provided. Key/Secret needed.");

            encryptor = new HMACSHA256(Encoding.ASCII.GetBytes(credentials.Secret.GetString()));
            this.arraySerialization = arraySerialization;
        }

        public override Dictionary<string, object> AddAuthenticationToParameters(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed)
        {
            if (!signed)
                return parameters;

            string signData;
            if (method == HttpMethod.Get || method == HttpMethod.Delete)
            {
                signData = parameters.CreateParamString(true, arraySerialization);
            }
            else
            {

                var formData = HttpUtility.ParseQueryString(string.Empty);
                foreach (var kvp in parameters.OrderBy(p => p.Key))
                    formData.Add(kvp.Key, kvp.Value.ToString());
                signData = formData.ToString();
            }

            parameters.Add("signature", ByteToString(encryptor.ComputeHash(Encoding.UTF8.GetBytes(signData))));
            return parameters;
        }

        public override Dictionary<string, string> AddAuthenticationToHeaders(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed)
        {
            if (Credentials.Key == null)
                throw new ArgumentException("No valid API credentials provided. Key/Secret needed.");

            return new Dictionary<string, string> {{"X-MBX-APIKEY", Credentials.Key.GetString()}};
        }
        
        public override string Sign(string toSign)
        {
            throw new NotImplementedException();
        }
    }
}
