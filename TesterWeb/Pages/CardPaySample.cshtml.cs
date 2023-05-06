using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class CardPaySample : PageModel
    {
        public CardPaySample(ILogger<CardPaySample> logger)
        {
            _logger = logger;
        }

        private bool _isLoaded;
        private readonly ILogger<CardPaySample> _logger;

        public List<SelectListItem> AvailableMIDs{get;set;} = new();

        [BindProperty]
        public string HMAC_input { get; set; }

        [BindProperty]
        public string PaymentUrl {get;set;}

        [BindProperty]
        public CardPayRequest P {get;set;} = new();

        private void EnsureLoaded(bool refresh = false)
        {
            if (_isLoaded && !refresh)
            {
                return;
            }

            AvailableMIDs = Keys.All["merchants"].AsObject()
                .Select(node => new SelectListItem{ Value = node.Key, Text = node.Key })
                .ToList();

            _isLoaded= true;
        }

        public void OnGet()
        {
            EnsureLoaded();

            // put in some defaults
            P.MID = "9999";
            // P.RURL = "https://moja.tatrabanka.sk/cgi-bin/e-commerce/start/example.jsp";
            P.RURL = "./CardPayResult";
            P.TIMESTAMP = "01092014125505";
            P.REM = "";

            P.AMT = "1234.50";
            P.CURR = Currency.EUR;
            P.VS = "1111";

            P.NAME = "Jan Pokusny";
            P.IPC = "1.2.3.4";
        }

        public void OnPostCalculateHMAC()
        {
            EnsureLoaded();

            HMAC_input = P.GetInputForHMAC();

            P.HMAC = Crypto.ConvertByteArrayToHexString(
                Crypto.CalculateHMAC(HMAC_input, Keys.KEYo(P.MID)));

            ModelState.Clear();
        }

        public void OnPostGenerateUrl()
        {
            EnsureLoaded();

            var sb = new StringBuilder();
            sb.Append("./CardPayStart");
            sb.AddQuery("MID", P.MID);
            sb.AddQuery("AMT", P.AMT);
            sb.AddQuery("CURR", ((int)P.CURR).ToString());
            sb.AddQuery("VS", P.VS);
            sb.AddQueryOptional("E2E", P.E2E ? "Y" : "");
            // TXN
            sb.AddQuery("RURL", P.RURL);
            sb.AddQuery("IPC", P.IPC);
            sb.AddQuery("NAME", P.NAME);
            sb.AddQueryOptional("REM", P.REM);
            // TPAY
            // CID
            // ECID
            // TDS_*
            sb.AddQuery("TIMESTAMP", P.TIMESTAMP);
            sb.AddQuery("HMAC", P.HMAC);
            // AREDIR
            // LANG

            PaymentUrl = sb.ToString();

            ModelState.Clear();
        }
    }
}