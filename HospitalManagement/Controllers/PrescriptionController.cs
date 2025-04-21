using Hl7.Fhir.Serialization;
using HospitalManagement.Service;
using HospitalManagement.ViewModel.EPS;
using HospitalManagement.ViewModel.PrescriptionRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

namespace HospitalManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly string _epsPrepareEndpoint = "https://sandbox.api.service.nhs.uk/electronic-prescriptions/FHIR/R4/$prepare";
        /*private readonly string _epsPrepareEndpoint = "https://int.api.service.nhs.uk/electronic-prescriptions/FHIR/R4/$prepare";*/
        private readonly HttpClient _httpClient;
        private readonly IPrescriptionService _fhirResourceBuilder;
        private readonly FhirJsonSerializer _fhirJsonSerializer;

        public PrescriptionController(IPrescriptionService fhirResourceBuilder, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _fhirResourceBuilder = fhirResourceBuilder;
            _fhirJsonSerializer = new FhirJsonSerializer();
        }

        [HttpPost("EncodeAcutePrescription")]
        public async Task<IActionResult> EncodeAcutePrescriptionAndReturnSign([FromBody] AcutePrescriptionRequest request)
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

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                "application/json");

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer g1112R_ccQ1Ebbb4gtHBP1aaaNM");
                _httpClient.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

                var response = await _httpClient.PostAsync(_epsPrepareEndpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(responseContent);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}