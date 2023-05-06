namespace TesterWeb.Domain
{
    public class CardPayRequest
    {
        public string AMT{get;set;}
        public string CID {get;set;}
        public Currency CURR{get;set;}
        public bool E2E{ get; set; }
        public string ECID {get;set;}
        public string IPC { get; set; }
        public string HMAC { get; set; }
        public string MID {get;set;}
        public string NAME { get; set; }
        public string REM { get; set; }
        public string RURL {get;set;}
        public string TIMESTAMP { get; set; }
        public bool TPAY { get; set; }
        public string TXN {get;set;}
        public string VS {get;set;}

        public string GetInputForHMAC()
        {
            return MID
                + AMT
                + (int)CURR
                + VS
                + (E2E == true ? "Y" : "")
                + TXN
                + RURL
                + IPC
                + NAME
                + REM
                + (TPAY ? "Y" : "")
                + CID
                + ECID
                + TIMESTAMP;
        }
    }
}