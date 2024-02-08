using System.Xml.Serialization;

namespace cnslOFXtoXML.models
{
    [XmlRoot(ElementName = "STATUS")]
    public class STATUS
    {

        [XmlElement(ElementName = "CODE")]
        public int CODE { get; set; }

        [XmlElement(ElementName = "SEVERITY")]
        public string SEVERITY { get; set; }
    }

    [XmlRoot(ElementName = "FI")]
    public class FI
    {

        [XmlElement(ElementName = "ORG")]
        public string ORG { get; set; }

        [XmlElement(ElementName = "FID")]
        public string FID { get; set; }
    }

    [XmlRoot(ElementName = "SONRS")]
    public class SONRS
    {

        [XmlElement(ElementName = "STATUS")]
        public STATUS STATUS { get; set; }

        [XmlElement(ElementName = "DTSERVER")]
        public string DTSERVER { get; set; }

        [XmlElement(ElementName = "LANGUAGE")]
        public string LANGUAGE { get; set; }

        [XmlElement(ElementName = "FI")]
        public FI FI { get; set; }
    }

    [XmlRoot(ElementName = "SIGNONMSGSRSV1")]
    public class SIGNONMSGSRSV1
    {

        [XmlElement(ElementName = "SONRS")]
        public SONRS SONRS { get; set; }
    }

    [XmlRoot(ElementName = "BANKACCTFROM")]
    public class BANKACCTFROM
    {

        [XmlElement(ElementName = "BANKID")]
        public int BANKID { get; set; }

        [XmlElement(ElementName = "ACCTID")]
        public int ACCTID { get; set; }

        [XmlElement(ElementName = "ACCTTYPE")]
        public string ACCTTYPE { get; set; }
    }

    [XmlRoot(ElementName = "STMTTRN")]
    public class STMTTRN
    {

        [XmlElement(ElementName = "TRNTYPE")]
        public string TRNTYPE { get; set; }

        [XmlElement(ElementName = "DTPOSTED")]
        public DateTime DTPOSTED { get; set; }

        [XmlElement(ElementName = "TRNAMT")]
        public double TRNAMT { get; set; }

        [XmlElement(ElementName = "FITID")]
        public int FITID { get; set; }

        [XmlElement(ElementName = "CHECKNUM")]
        public int CHECKNUM { get; set; }

        [XmlElement(ElementName = "PAYEEID")]
        public int PAYEEID { get; set; }

        [XmlElement(ElementName = "MEMO")]
        public string MEMO { get; set; }
    }

    [XmlRoot(ElementName = "BANKTRANLIST")]
    public class BANKTRANLIST
    {

        [XmlElement(ElementName = "DTSTART")]
        public DateTime DTSTART { get; set; }

        [XmlElement(ElementName = "DTEND")]
        public DateTime DTEND { get; set; }

        [XmlElement(ElementName = "STMTTRN")]
        public List<STMTTRN> STMTTRN { get; set; }
    }

    [XmlRoot(ElementName = "LEDGERBAL")]
    public class LEDGERBAL
    {

        [XmlElement(ElementName = "BALAMT")]
        public double BALAMT { get; set; }

        [XmlElement(ElementName = "DTASOF")]
        public DateTime DTASOF { get; set; }
    }

    [XmlRoot(ElementName = "STMTRS")]
    public class STMTRS
    {

        [XmlElement(ElementName = "CURDEF")]
        public string CURDEF { get; set; }

        [XmlElement(ElementName = "BANKACCTFROM")]
        public BANKACCTFROM BANKACCTFROM { get; set; }

        [XmlElement(ElementName = "BANKTRANLIST")]
        public BANKTRANLIST BANKTRANLIST { get; set; }

        [XmlElement(ElementName = "LEDGERBAL")]
        public LEDGERBAL LEDGERBAL { get; set; }
    }

    [XmlRoot(ElementName = "STMTTRNRS")]
    public class STMTTRNRS
    {

        [XmlElement(ElementName = "TRNUID")]
        public int TRNUID { get; set; }

        [XmlElement(ElementName = "STATUS")]
        public STATUS STATUS { get; set; }

        [XmlElement(ElementName = "STMTRS")]
        public STMTRS STMTRS { get; set; }
    }

    [XmlRoot(ElementName = "BANKMSGSRSV1")]
    public class BANKMSGSRSV1
    {

        [XmlElement(ElementName = "STMTTRNRS")]
        public STMTTRNRS STMTTRNRS { get; set; }
    }

    [XmlRoot(ElementName = "OFX")]
    public class OFX
    {

        [XmlElement(ElementName = "SIGNONMSGSRSV1")]
        public SIGNONMSGSRSV1 SIGNONMSGSRSV1 { get; set; }

        [XmlElement(ElementName = "BANKMSGSRSV1")]
        public BANKMSGSRSV1 BANKMSGSRSV1 { get; set; }
    }
}
