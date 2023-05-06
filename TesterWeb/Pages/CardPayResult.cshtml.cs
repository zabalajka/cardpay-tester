using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TesterWeb.Domain;
using TesterWeb.Utils;

namespace TesterWeb.Pages
{
    public class CardPayResult : PageModel
    {
        public CardPayResult(ILogger<CardPayResult> logger)
        {
            _logger = logger;
        }

        private bool _isLoaded;
        private readonly ILogger<CardPayResult> _logger;

        [BindProperty]
        public CardPayResponse R {get;set;} = new();

        public bool R_ECDSA_isValid{get;set;}
        public string R_HMAC_expected{get;set;}
        public string R_HMAC_input{get;set;}

        private void EnsureLoaded(bool refresh = false)
        {
            if (_isLoaded && !refresh)
            {
                return;
            }

            R_HMAC_input = R.GetInputForHMAC();
            R_HMAC_expected = Crypto.ConvertByteArrayToHexString(
                Crypto.CalculateHMAC(R_HMAC_input, Keys.KEYo(State.LastMID)));

            string R_ECDSA_input = R_HMAC_input + R_HMAC_expected;
            R_ECDSA_isValid = Crypto.IsSignatureValid(R_ECDSA_input,
                Crypto.ConvertHexStringToByteArray(R.ECDSA),
                Keys.PUBLIC_PEM(R.ECDSA_KEY));

            _isLoaded= true;
        }

        public void OnGet()
        {
            R.AC = Request.Query["AC"];
            R.AMT = Request.Query["AMT"];
            R.CC = Request.Query["CC"];
            R.CID = Request.Query["CID"];
            R.CURR = string.IsNullOrEmpty(Request.Query["CURR"])
                ? Currency.None
                : Enum.Parse<Currency>(Request.Query["CURR"]);
            R.ECDSA = Request.Query["ECDSA"];
            R.ECDSA_KEY = Request.Query["ECDSA_KEY"];
            R.HMAC = Request.Query["HMAC"];
            R.RC = Request.Query["RC"];
            R.RES = string.IsNullOrEmpty(Request.Query["RES"])
                ? PaymentResult.None
                : Enum.Parse<PaymentResult>(Request.Query["RES"]);
            R.TID = Request.Query["TID"];
            R.TIMESTAMP = Request.Query["TIMESTAMP"];
            R.TRES = string.IsNullOrEmpty(Request.Query["TRES"])
                ? PaymentResult.None
                : Enum.Parse<PaymentResult>(Request.Query["TRES"]);
            R.TXN = Request.Query["TXN"];
            R.VS = Request.Query["VS"];

            EnsureLoaded();
        }
    }
}