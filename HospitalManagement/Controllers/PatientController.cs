using HospitalManagement.Service;
using HospitalManagement.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpPost("Register")]
        public IActionResult RegisterPatient ( [FromBody] PatientVM patient)
        {
            if (patient == null)
            {
                return BadRequest("Enter Some Data !");
            }
            var result = _patientService.AddPatient(patient);
            if (result == null)
            {
                return BadRequest("Something Went Wrong!");
            }
            return Ok(result);
        }

        [HttpPost("LogIn")]
        public IActionResult LogIn (string username, string bg)
        {
            if (username == null || bg == null)
            {
                return BadRequest("Username and Date Of Birth is Required");
            }
            var result = _patientService.Login(username, bg);
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetAllPatient()
        {
            var data = _patientService.AllData();
            return Ok(data);
        }
    }
}
