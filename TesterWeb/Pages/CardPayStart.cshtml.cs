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
            P.E2E = Request.Query["E2E"] == "Y";
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

            // these values must match the request, but can be modified to test some invalid responses
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

            // MID comes from request
            // ECDSA_KEY can be selected (first with private key is pre-selected)
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

            if (string.IsNullOrEmpty(P.HMAC))
            {
                OnPostCalculateHMAC();
            }

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

        public void OnPostSetResultFail()
        {
            EnsureLoaded();

            var random = new Random();
            const string alphaNumeric = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ ";

            R.RES = PaymentResult.FAIL;
            R.AC = "";
            R.CC = "";
            R.RC = string.Join("", Enumerable.Range(1, 2)
                        .Select(_=>alphaNumeric[random.Next(alphaNumeric.Length - 1)]));
            R.TID = random.NextInt64(0, 10_000_000_000).ToString(); // up to 10 digits

            if (P.TPAY)
            {
                R.TRES = PaymentResult.FAIL;
                R.CID = "";
            }
            else
            {
                R.TRES = PaymentResult.None;
                R.CID = "";
            }

            // prepare all next steps
            ModelState.Clear();
            OnPostCalculateHMAC();
            OnPostGenerateUrl();
        }

        public void OnPostSetResultOk()
        {
            EnsureLoaded();

            var random = new Random();
            const string alphaNumeric = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ ";

            R.RES = PaymentResult.OK;
            R.AC = string.Join("", Enumerable.Range(1, 6)
                        .Select(_=>alphaNumeric[random.Next(alphaNumeric.Length)]))
                        .Trim();
            R.CC = $"{random.Next(1000, 10000):0000} {random.Next(0, 100):00}** **** {random.Next(0, 10000):0000}";
            R.RC = "";
            R.TID = random.NextInt64(0, 10_000_000_000).ToString(); // up to 10 digits

            if (P.TPAY)
            {
                R.TRES = PaymentResult.OK;
                R.CID = "xxx";
            }
            else
            {
                R.TRES = PaymentResult.None;
                R.CID = "";
            }

            // prepare all next steps
            ModelState.Clear();
            OnPostCalculateHMAC();
            OnPostGenerateUrl();
        }
    }
}