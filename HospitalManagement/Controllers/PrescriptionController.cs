using Hl7.Fhir.Serialization;
using HospitalManagement.Service;
using HospitalManagement.ViewModel.PrescriptionRequest;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _fhirResourceBuilder;
        private readonly FhirJsonSerializer _fhirJsonSerializer;

        public PrescriptionController(IPrescriptionService fhirResourceBuilder)
        {
            _fhirResourceBuilder = fhirResourceBuilder;
            _fhirJsonSerializer = new FhirJsonSerializer();
        }

        [HttpPost("EncodeAcutePrescription")]
        public IActionResult EncodeAcutePrescription([FromBody] AcutePrescriptionRequest request)
        {
            try
            {
                // Validate input
                if (request == null)
                    return BadRequest("Request body cannot be null");

                if (request.Medications == null || request.Medications.Count == 0)
                    return BadRequest("At least one medication is required");

                // Generate FHIR bundle
                var bundle = _fhirResourceBuilder.CreateAcutePrescriptionBundle(request);

                // Serialize to JSON
                var json = _fhirJsonSerializer.SerializeToString(bundle);

                return Ok(json);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}