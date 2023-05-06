using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using TesterWeb.Domain;
using TesterWeb.Utils;

namespace TesterWeb.Pages
{
    public class CardPayStart : PageModel
    {
        public CardPayStart(ILogger<CardPayStart> logger)
        {
            _logger = logger;
        }

        private bool _isLoaded;

        private readonly ILogger<CardPayStart> _logger;

        public List<SelectListItem> AvailableECDSA_KEYs{get;set;} = new ();

        [BindProperty]
        public CardPayRequest P{get;set;} = new();
        public string P_HMAC_expected{get;set;}
        public string P_HMAC_input{get;set;}

        [BindProperty]
        public CardPayResponse R{get;set;} = new();

        [BindProperty]
        public string R_HMAC_input { get; set; }

        [BindProperty]
        public string ReturnUrl {get;set;}

        private void EnsureLoaded(bool refresh = false)
        {
            if (_isLoaded && !refresh)
            {
                return;
            }

            AvailableECDSA_KEYs = Keys.All["bank-keys"].AsObject()
                .Where(node => node.Value["PRIVATE_PEM"] != null)
                .Select(node => new SelectListItem{ Value = node.Key, Text = node.Key })
                .ToList();

            P_HMAC_input = P.GetInputForHMAC();
            P_HMAC_expected = Crypto.ConvertByteArrayToHexString(
                Crypto.CalculateHMAC(P_HMAC_input, Keys.KEYo(P.MID)));

            _isLoaded= true;
        }

        public void OnGet()
        {
            P.AMT = Request.Query["AMT"];
            // CID
            P.CURR = string.IsNullOrEmpty(Request.Query["CURR"])
                ? Currency.None
                : Enum.Parse<Currency>(Request.Query["CURR"]);
            // E2E
            // ECID
            P.IPC = Request.Query["IPC"];
            P.HMAC = Request.Query["HMAC"];
            P.MID = Request.Query["MID"];
            P.NAME = Request.Query["NAME"];
            P.REM = Request.Query["REM"];
            P.RURL = Request.Query["RURL"];
            P.TIMESTAMP = Request.Query["TIMESTAMP"];
            // TPAY
            // TXN
            P.VS = Request.Query["VS"];

            R.AMT = P.AMT;
            R.CURR = P.CURR;
            R.VS = P.VS;
            R.TXN = P.TXN;
            R.TIMESTAMP = P.TIMESTAMP;

            EnsureLoaded();
        }

        public void OnPostCalculateHMAC()
        {
            EnsureLoaded();

            R_HMAC_input = R.GetInputForHMAC();
            R.HMAC = Crypto.ConvertByteArrayToHexString(
                Crypto.CalculateHMAC(R_HMAC_input, Keys.KEYo(P.MID)));

            if (string.IsNullOrEmpty(R.ECDSA_KEY))
            {
                R.ECDSA_KEY = AvailableECDSA_KEYs[0].Value;
            }

            string R_ECDSA_input = R_HMAC_input + R.HMAC;
            R.ECDSA = Crypto.ConvertByteArrayToHexString(
                Crypto.CalculateSignature(R_ECDSA_input, Keys.PRIVATE_PEM(R.ECDSA_KEY)));

            ModelState.Clear();
        }

        public void OnPostGenerateUrl()
        {
            EnsureLoaded();

            var sb = new StringBuilder();
            sb.Append(P.RURL);
            sb.AddQuery("AMT", R.AMT);
            sb.AddQuery("CURR", ((int)R.CURR).ToString());
            sb.AddQuery("VS", R.VS);
            sb.AddQueryOptional("TXN", R.TXN);
            sb.AddQuery("RES", R.RES.ToString());
            sb.AddQueryOptional("AC", R.AC);
            sb.AddQueryOptional("TRES", R.TRES == PaymentResult.None ? "" : R.TRES.ToString());
            sb.AddQueryOptional("CID", R.CID);
            sb.AddQueryOptional("CC", R.CC);
            sb.AddQueryOptional("RC", R.RC);
            sb.AddQueryOptional("TID", R.TID);
            sb.AddQuery("TIMESTAMP", R.TIMESTAMP);
            sb.AddQuery("HMAC", R.HMAC);
            sb.AddQuery("ECDSA_KEY", R.ECDSA_KEY);
            sb.AddQuery("ECDSA", R.ECDSA);

            ReturnUrl = sb.ToString();

            ModelState.Clear();
        }
    }
}