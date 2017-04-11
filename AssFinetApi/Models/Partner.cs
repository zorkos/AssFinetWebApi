using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssFinetApi.Models
{
    public class Partner
    {
        public Partner(){ }

        public Partner( string partnerID, string name, string partnernummerVM, string strasse, string postleitzahl, string ort, string kommunikationsadresse )
        {
            PartnerID = partnerID;
            Name = name;
            PartnernummerVM = partnernummerVM;
            Postleitzahl = postleitzahl;
            Ort = ort;
            Kommunikationsadresse = kommunikationsadresse;
            Strasse = strasse;
        }

        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.Identity )] 
        public string PartnerID { get; set; }
        public string Name { get; set; }
        public string PartnernummerVM { get; set; }
        public string Strasse { get; set; }
        public string Postleitzahl { get; set; }
        public string Ort { get; set; }
        public string Kommunikationsadresse { get; set; }
    }
}