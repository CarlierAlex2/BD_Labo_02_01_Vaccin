using System;
using System.ComponentModel.DataAnnotations;

namespace BD_Labo_02_01_Vaccin.Models
{
    public class VaccinRegistration
    {
        public Guid RegistratieID {get; set;}
        [Required(ErrorMessage="Naam is verplicht!")]
        public string Naam {get; set;}
        [Required(ErrorMessage="Voornaam is verplicht!")]
        public string Voornaam {get; set;}
        [Required(ErrorMessage="E-mail is verplicht!")]
        [EmailAddress(ErrorMessage="Ongeldig e-mailadres!")]
        public string Email {get; set;}
        [Range(18,120)]
        public int Leeftijd {get; set;}
        public string Datum {get; set;}
        public Guid TypeID {get; set;}
        public Guid LocatieID {get; set;}
    }
}
