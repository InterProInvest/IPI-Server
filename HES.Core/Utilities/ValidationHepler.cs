using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hideez.SDK.Communication.Utils;

namespace HES.Core.Utilities
{
    public static class ValidationHepler
    {
        public static string VerifyUrls(string urls)
        {
            if (urls == null)
            {
                return null;
            }

            List<string> verifiedUrls = new List<string>();
            foreach (var url in urls.Split(";"))
            {
                if (!UrlUtils.TryGetDomain(url, out string domain))
                {
                    throw new Exception($"'{url}' incorrect url.");
                }

                verifiedUrls.Add(domain);
            }

            var result = string.Join(";", verifiedUrls.ToArray());
            return result;
        }

        public static string VerifyOtpSecret(string otp)
        {
            if (otp == null)
            {
                return null;
            }

            var valid = Regex.IsMatch(otp.Replace(" ", ""), @"^[a-zA-Z0-9]+$");

            if (!valid)
            {
                throw new Exception("Otp secret is not valid");
            }

            return otp;
        }

        public static string GetModelStateErrors(ModelStateDictionary ModelState)
        {
            return string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
        }
    }
}