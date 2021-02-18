using System;
using AutoMapper;
using BD_Labo_02_01_Vaccin.Models;

namespace BD_Labo_02_01_Vaccin.DTO
{
    public class AutoMapping : Profile
    {
        public AutoMapping() {
            CreateMap<VaccinRegistration, VaccinRegistrationDTO>();
        }
    }
}
