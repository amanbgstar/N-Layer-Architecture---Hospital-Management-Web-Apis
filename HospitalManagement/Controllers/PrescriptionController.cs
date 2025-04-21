using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using HospitalManagement.Service;
using HospitalManagement.ViewModel.PrescriptionRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
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
        private readonly ILogger<PrescriptionController> _logger;

        public PrescriptionController(IPrescriptionService fhirResourceBuilder, IHttpClientFactory httpClientFactory, ILogger<PrescriptionController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _fhirResourceBuilder = fhirResourceBuilder;
            _fhirJsonSerializer = new FhirJsonSerializer();
            _logger = logger;
        }

        [HttpPost("ConvertToFhirREsource")]
        public async Task<IActionResult> ConvertToFhirREsource([FromBody] AcutePrescriptionRequest request)
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

        /// <summary>
        /// {
        ///"signedInfo": "your-base64-signedinfo",
        ///"mode": "test"
        ///}
        ///
        /// {
        /// "signedInfo": "your-base64-signedinfo",
        /// "mode": "production",
        /// "certificateBase64": "base64-encoded-pfx",
        /// "certificatePassword": "your-pfx-password"
        ///}
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("signAfterEncode")]
        public IActionResult SignPrescription([FromBody] SigningRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SignedInfo))
                {
                    return BadRequest("SignedInfo is required");
                }

                byte[] signature;
                X509Certificate2 cert;

                if (request.Mode == "test")
                {
                    // Use self-signed certificate for testing
                    (signature, cert) = SignWithTestCertificate(request.SignedInfo);
                }
                else
                {
                    // Use production certificate
                    if (string.IsNullOrEmpty(request.CertificateBase64))
                    {
                        return BadRequest("Certificate is required for production mode");
                    }

                    (signature, cert) = SignWithProductionCertificate(
                        request.SignedInfo,
                        request.CertificateBase64,
                        request.CertificatePassword);
                }

                var response = new SignatureResponse
                {
                    SignatureValue = Convert.ToBase64String(signature),
                    Certificate = Convert.ToBase64String(cert.Export(X509ContentType.Cert)),
                    SignatureAlgorithm = "RSA-SHA256",
                    KeyInfo = new ViewModel.PrescriptionRequest.KeyInfo
                    {
                        X509Data = new X509Data
                        {
                            X509Certificate = Convert.ToBase64String(cert.Export(X509ContentType.Cert))
                        }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing prescription");
                return StatusCode(500, $"Error signing prescription: {ex.Message}");
            }
        }

        [HttpGet("test-certificate")]
        public IActionResult GenerateTestCertificate()
        {
            try
            {
                var password = Guid.NewGuid().ToString();
                var cert = CreateSelfSignedCertificate();
                var pfxBytes = cert.Export(X509ContentType.Pfx, password);

                return Ok(new
                {
                    Certificate = Convert.ToBase64String(pfxBytes),
                    Password = password,
                    Thumbprint = cert.Thumbprint
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test certificate");
                return StatusCode(500, $"Error generating test certificate: {ex.Message}");
            }
        }

        private (byte[] signature, X509Certificate2 cert) SignWithTestCertificate(string base64SignedInfo)
        {
            var cert = CreateSelfSignedCertificate();
            using (RSA privateKey = cert.GetRSAPrivateKey())
            {
                var dataToSign = Convert.FromBase64String(base64SignedInfo);
                var signature = privateKey.SignData(dataToSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return (signature, cert);
            }
        }

        private (byte[] signature, X509Certificate2 cert) SignWithProductionCertificate(
            string base64SignedInfo, string certificateBase64, string password)
        {
            var certBytes = Convert.FromBase64String(certificateBase64);
            var cert = new X509Certificate2(certBytes, password, X509KeyStorageFlags.Exportable);

            using (RSA privateKey = cert.GetRSAPrivateKey())
            {
                if (privateKey == null)
                {
                    throw new Exception("Certificate does not contain a private key");
                }

                var dataToSign = Convert.FromBase64String(base64SignedInfo);
                var signature = privateKey.SignData(dataToSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return (signature, cert);
            }
        }

        private X509Certificate2 CreateSelfSignedCertificate()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    "CN=NHS EPS Test Certificate, O=National Health Service, C=GB",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                // Set validity (2 years)
                var now = DateTimeOffset.UtcNow;
                var cert = request.CreateSelfSigned(now, now.AddYears(2));

                return cert;
            }
        }


        [HttpPost("CreateProvenance")]
        public IActionResult CreateProvenance([FromBody] ProvenanceRequest request)
        {
            try
            {
                string provenanceUuid = Guid.NewGuid().ToString();

                // Validate input
                if (string.IsNullOrEmpty(request.SignatureValue) ||
                    string.IsNullOrEmpty(request.Certificate) ||
                    string.IsNullOrEmpty(request.TargetReference) ||
                    string.IsNullOrEmpty(request.AgentReference))
                {
                    return BadRequest("SignatureValue, Certificate, TargetReference and AgentReference are required");
                }

                // Create FHIR Provenance resource
                var provenance = new Provenance
                {
                    Id = provenanceUuid,
                    Recorded = DateTimeOffset.UtcNow,
                    Target = new List<ResourceReference>
            {
                new ResourceReference($"urn:uuid:{request.TargetReference}")
            },
                    Agent = new List<Provenance.AgentComponent>
            {
                new Provenance.AgentComponent
                {
                    Who = new ResourceReference($"urn:uuid:{request.AgentReference}")
                }
            },
                    Signature = new List<Hl7.Fhir.Model.Signature>
            {
                new Hl7.Fhir.Model.Signature
                {
                    Type = new List<Coding>
                    {
                        new Coding
                        {
                            System = "urn:iso-astm:E1762-95:2013",
                            Code = "1.2.840.10065.1.12.1.1",
                            Display = "Author's Signature"
                        }
                    },
                    When = DateTimeOffset.UtcNow,
                    Who = new ResourceReference($"urn:uuid:{request.AgentReference}"),
                    SigFormat = "application/pkcs7-signature",
                    Data = Convert.FromBase64String(request.SignatureValue)
                }
            }
                };

                // Serialize the Provenance resource to JSON
                var sb = new StringBuilder();
                using (var writer = new JsonTextWriter(new StringWriter(sb)))
                {
                    var fhirSerializer = new FhirJsonSerializer(new SerializerSettings
                    {
                        Pretty = true,
                        AppendNewLine = true
                    });
                    fhirSerializer.Serialize(provenance, writer);
                }

                // Create the complete response structure
                var response = new
                {
                    fullUrl = $"urn:uuid:{provenanceUuid}",
                    resource = JsonConvert.DeserializeObject(sb.ToString())
                };

                // Return as JSON
                return Content(JsonConvert.SerializeObject(response, Formatting.Indented), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Provenance resource");
                return StatusCode(500, $"Error creating Provenance resource: {ex.Message}");
            }

        }
    }
}