namespace TesterWeb.Domain
{

    public class CardPayResponse
    {
        public string AC{get;set;}
        public string AMT{get;set;}
        public string CC{get;set;}
        public string CID{get;set;}
        public Currency CURR{get;set;}
        public string ECDSA { get; set; }
        public string ECDSA_KEY { get; set; }
        public string HMAC { get; set; }
        public string RC { get; set; }
        public PaymentResult RES {get;set;}
        public string TID {get;set;}
        public string TIMESTAMP { get; set; }
        public PaymentResult TRES {get;set;}
        public string TXN {get;set;}
        public string VS {get;set;}

        public string GetInputForHMAC()
        {
            return AMT
                + (int)CURR
                + VS
                + TXN
                + RES
                + AC
                + (TRES != PaymentResult.None ? TRES : "")
                + CID
                + CC
                + RC
                + TID
                + TIMESTAMP;
        }
    }
}