﻿using System;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace Android.Gms.SafetyNet
{
    public static class ISafetyNetApiAttestationResultExtensions
    {
        public static string DecodeJwsResult (this ISafetyNetApiAttestationResult result, byte[] originalNonce)
        {
            return JWT.JsonWebToken.Decode (result.JwsResult, originalNonce);
        }

        public static async Task<bool> ValidateWithGoogle (this ISafetyNetApiAttestationResult result, string validationApiKey)
        {
            const string url = "https://www.googleapis.com/androidcheck/v1/attestations/verify?key=";

            var http = new HttpClient ();
            var jsonReq = "{ \"signedAttestation\": \"" + result.JwsResult + "\" }";
            var content = new StringContent (jsonReq, Encoding.Default, "application/json");

            var response = await http.PostAsync (url + validationApiKey, content);
            var data = await response.Content.ReadAsStringAsync ();

            response.EnsureSuccessStatusCode ();

            var json = JsonObject.Parse (data);
            if (json.ContainsKey ("isValidSignature"))
                return json ["isValidSignature"].ToString ().Trim ('"').Equals ("true");
            
            return false;
        }
    }

    public static class Nonce
    {
        static readonly RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider ();

        public static byte[] Generate (int size = 16)
        {            
            var buffer = new byte[size];
            rnd.GetNonZeroBytes (buffer);
            return buffer;
        }
    }


}


