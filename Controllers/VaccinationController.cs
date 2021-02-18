using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BD_Labo_02_01_Vaccin.Models;
using BD_Labo_02_01_Vaccin.DTO;
using BD_Labo_02_01_Vaccin.Configurations;

//CSV Helper ------------------------------
using System.Globalization;
using System.IO;
using CsvHelper;
using Microsoft.Extensions.Options;
using CsvHelper.Configuration;

using Microsoft.AspNetCore.Mvc.Versioning;
using AutoMapper;

namespace BD_Labo_02_01_Vaccin.Controllers
{
    [ApiController]
    [Route("api")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class VaccinationController : ControllerBase
    {
        private CSVSettings _settings;
        private static List<VaccinRegistration> _listRegistrations = new List<VaccinRegistration>();
        private static List<Locatie> _listLocations;
        private static List<VaccinType> _listTypes;
        private readonly ILogger<VaccinationController> _logger;
        private readonly IMapper _mapper;

        public VaccinationController(ILogger<VaccinationController> logger, IOptions<CSVSettings> settings, IMapper mapper)
        {
            _settings = settings.Value;
            _logger = logger;
            _mapper = mapper;

            Initialize();

            _logger.LogInformation("ctor");
        }

        private void Initialize()
        {
            if(_listLocations == null || _listLocations.Count <= 0)
                _listLocations = ReadLocationsCSV();
            if(_listTypes == null || _listTypes.Count <= 0)
                _listTypes = ReadTypesCSV();
            if(_listRegistrations == null || _listRegistrations.Count <= 0)
                _listRegistrations = ReadRegistrationsCSV();

            //CreateTestData();
        }

        private void CreateTestData()
        {
            if(_listTypes == null || _listTypes.Count <= 0)
            {
                _listTypes.Add(new VaccinType(){TypeID = Guid.NewGuid(), Naam = "Modera"});
            }

            if(_listLocations == null || _listLocations.Count <= 0)
            {
                _listLocations.Add(new Locatie(){LocatieID = Guid.NewGuid(), Naam = "Kortrijk EXPO"});
            }

            if(_listRegistrations == null || _listRegistrations.Count <= 0)
            {
                _listRegistrations.Add(new VaccinRegistration(){
                    RegistratieID = Guid.NewGuid(), 
                    Naam = "Droomer",
                    Voornaam = "Bent",
                    Email = "bert.droomer@student.howest.be",
                    Leeftijd = 25,
                    Datum = "18/02/2021",
                    TypeID = _listTypes[0].TypeID,
                    LocatieID = _listLocations[0].LocatieID
                    });
            }
        }

        //=============================
        #region Brands - CSV
        // ===================================================================================================================
        private List<Locatie> ReadLocationsCSV()
        {   
            var config = new CsvConfiguration(CultureInfo.InvariantCulture){
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            using (var reader = new StreamReader(_settings.CSVLocations))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<Locatie>();
                return records.ToList<Locatie>();
            }            
        }

        private List<VaccinType> ReadTypesCSV()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture){
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            using (var reader = new StreamReader(_settings.CSVVaccins))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<VaccinType>();
                return records.ToList<VaccinType>();
            }   
        }

        private List<VaccinRegistration> ReadRegistrationsCSV()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture){
                HasHeaderRecord = true,
                Delimiter = ";"
            };
            
            using (var reader = new StreamReader(_settings.CSVRegistrations))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<VaccinRegistration>();
                return records.ToList<VaccinRegistration>();
            }   
        }

        private void WriteToRegistrationsCSV(VaccinRegistration registration)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture){
                HasHeaderRecord = true,
                Delimiter = ";"
            };
            
            using (var writer = new StreamWriter(_settings.CSVRegistrations))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(_listRegistrations);
            }
        }
        #endregion

        //=============================
        #region Brands - Get
        // ===================================================================================================================
        [HttpGet]
        [Route("registrations")]
        [MapToApiVersion("1.0")]
        public ActionResult<List<VaccinRegistration>> GetRegistrationsV1(string date = ""){
            _logger.LogInformation("GetRegistrationsV1");
            if(date == null || date.Length <= 0)
                return new OkObjectResult(_listRegistrations);

            var filteredList = _listRegistrations.FindAll(delegate(VaccinRegistration vac){
                return vac.Datum == date;
                });
            
            return new OkObjectResult(filteredList);
        }

        [HttpGet]
        [Route("registrations")]
        [MapToApiVersion("2.0")]
        public ActionResult<List<VaccinRegistration>> GetRegistrationsV2(string date = ""){
            _logger.LogInformation("GetRegistrationsV2");
            if(date == null || date.Length <= 0)
            {
                var returnedListFull = _mapper.Map<List<VaccinRegistrationDTO>>(_listRegistrations);
                return new OkObjectResult(returnedListFull);
            }

            var filteredList = _listRegistrations.FindAll(delegate(VaccinRegistration vac){
                return vac.Datum == date;
                });
            
            var returnedList = _mapper.Map<List<VaccinRegistrationDTO>>(filteredList);
            return new OkObjectResult(returnedList);
        }

        [HttpGet]
        [Route("locations")]
        public ActionResult<List<Locatie>> GetLocations(){
            return new OkObjectResult(_listLocations);
        }

        [HttpGet]
        [Route("vaccins")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
        public ActionResult<List<VaccinType>> GetVaccinTypes(){
            return new OkObjectResult(_listTypes);
        }
        #endregion


        //=============================
        #region Brands - Post
        // ===================================================================================================================
        [HttpPost]
        [Route("registration")]
        public ActionResult<VaccinRegistration> AddBrand(VaccinRegistration registration){
            if(registration == null)
                return new BadRequestResult();

            if(_listTypes.Where(vt => vt.TypeID == registration.TypeID).Count() == 0)
                return new BadRequestResult();
            if(_listLocations.Where(loc => loc.LocatieID == registration.LocatieID).Count() == 0)
                return new BadRequestResult();
            
            registration.RegistratieID = Guid.NewGuid();
            _listRegistrations.Add(registration);
            WriteToRegistrationsCSV(registration);
            return new OkObjectResult(registration);
        }
        #endregion
    }
}
