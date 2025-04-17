using HospitalManagement.ViewModel.EPS;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace HospitalManagement.Controllers.EPS
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _epsPrepareEndpoint = "https://sandbox.api.service.nhs.uk/electronic-prescriptions/FHIR/R4/$prepare";

        public PrescriptionController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("prepare")]
        public async Task<IActionResult> PreparePrescription([FromBody] PrescriptionPrepareRequest request)
        {
            try
            {
                var fhirBundle = CreateFhirBundle(request);

                var content = new StringContent(
                    JsonConvert.SerializeObject(fhirBundle),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

                
                var response = await _httpClient.PostAsync(_epsPrepareEndpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JsonConvert.DeserializeObject<dynamic>(responseContent));
                }
                else
                {
                    return StatusCode((int)response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPrescription([FromBody] dynamic signedPrescription)
        {
            try
            {
                var processEndpoint = "https://sandbox.api.service.nhs.uk/electronic-prescriptions/FHIR/R4/$process-message";

                var content = new StringContent(
                    JsonConvert.SerializeObject(signedPrescription),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

                var response = await _httpClient.PostAsync(processEndpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JsonConvert.DeserializeObject<dynamic>(responseContent));
                }
                else
                {
                    return StatusCode((int)response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private FhirBundle CreateFhirBundle(PrescriptionPrepareRequest request)
        {
            var bundle = new FhirBundle
            {
                Meta = new Meta
                {
                    Profile = new List<string>
                    {
                        "https://fhir.nhs.uk/StructureDefinition/NHSDigital-Bundle-prescription-order"
                    }
                }
            };

            var prescriptionId = Guid.NewGuid().ToString();

            var messageHeader = new MessageHeader
            {
                EventCoding = "prescription-order",
                Source = new Source
                {
                    Name = "PrescribingSystem",
                    Software = "YourSystem",
                    Version = "1.0",
                    Endpoint = "https://example.org/source"
                },
                Destination = new List<Destination>
                {
                    new Destination
                    {
                        Endpoint = "https://example.org/destination"
                    }
                }
            };

            bundle.Entry.Add(new BundleEntry
            {
                Resource = messageHeader,
                FullUrl = $"urn:uuid:{messageHeader.Id}"
            });

            var patient = new FhirPatient
            {
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        System = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "https://fhir.nhs.uk/Id/nhs-number",
                                    Code = request.Patient.NhsNumber
                                }
                            }
                        },
                        Value = request.Patient.NhsNumber
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Family = request.Patient.FamilyName,
                        Given = request.Patient.GivenNames,
                        Prefix = new List<string> { request.Patient.Title }
                    }
                },
                Gender = request.Patient.Gender,
                BirthDate = request.Patient.DateOfBirth,
                Address = new List<FhirAddress>
                {
                    new FhirAddress
                    {
                        Line = request.Patient.Address.Lines,
                        City = request.Patient.Address.City,
                        District = request.Patient.Address.District,
                        PostalCode = request.Patient.Address.PostalCode
                    }
                }
            };

            bundle.Entry.Add(new BundleEntry
            {
                Resource = patient,
                FullUrl = $"urn:uuid:{patient.Id}"
            });

            var practitioner = new FhirPractitioner
            {
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        System = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "https://fhir.nhs.uk/Id/sds-user-id",
                                    Code = request.Practitioner.ProfessionalCode
                                }
                            }
                        },
                        Value = request.Practitioner.ProfessionalCode
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Family = request.Practitioner.FamilyName,
                        Given = request.Practitioner.GivenNames,
                        Prefix = new List<string> { request.Practitioner.Title }
                    }
                }
            };

            bundle.Entry.Add(new BundleEntry
            {
                Resource = practitioner,
                FullUrl = $"urn:uuid:{practitioner.Id}"
            });

            var practitionerRole = new PractitionerRole
            {
                Practitioner = new References { Reference = $"urn:uuid:{practitioner.Id}" },
                Organization = new References { Reference = $"urn:uuid:{Guid.NewGuid()}" }, // Will add organization below
                Code = new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "https://fhir.nhs.uk/CodeSystem/NHSDigital-PractitionerRole",
                                Code = "R0260",
                                Display = "Prescriber"
                            }
                        }
                    }
                }
            };

            bundle.Entry.Add(new BundleEntry
            {
                Resource = practitionerRole,
                FullUrl = $"urn:uuid:{practitionerRole.Id}"
            });

            var organization = new FhirOrganization
            {
                Id = Guid.NewGuid().ToString(),
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        System = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "https://fhir.nhs.uk/Id/ods-organization-code",
                                    Code = request.Organization.OdsCode
                                }
                            }
                        },
                        Value = request.Organization.OdsCode
                    }
                },
                Name = request.Organization.Name,
                Address = new List<FhirAddress>
                {
                    new FhirAddress
                    {
                        Line = request.Organization.Address.Lines,
                        City = request.Organization.Address.City,
                        District = request.Organization.Address.District,
                        PostalCode = request.Organization.Address.PostalCode
                    }
                }
            };

            bundle.Entry.Add(new BundleEntry
            {
                Resource = organization,
                FullUrl = $"urn:uuid:{organization.Id}"
            });

            ((PractitionerRole)bundle.Entry[3].Resource).Organization.Reference = $"urn:uuid:{organization.Id}";

            foreach (var medication in request.Medications)
            {
                var medicationRequest = new MedicationRequest
                {
                    Status = "active",
                    Intent = "order",
                    MedicationCodeableConcept = new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://snomed.info/sct",
                                Code = medication.MedicationCode,
                                Display = medication.MedicationDisplay
                            }
                        }
                    },
                    Subject = new References { Reference = $"urn:uuid:{patient.Id}" },
                    Requester = new References { Reference = $"urn:uuid:{practitionerRole.Id}" },
                    Groupidentifier = new References { Reference = $"urn:uuid:{prescriptionId}" },
                    AuthoredOn = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    DosageInstruction = new Dosage
                    {
                        Text = medication.DosageInstructions
                    },
                    DispenseRequest = new DispenseRequest
                    {
                        Quantity = new Quantity
                        {
                            Value = medication.Quantity,
                            System = "http://snomed.info/sct",
                            Code = "428673006", // tablet
                            Unit = medication.UnitOfMeasure
                        }
                    }
                };

                if (!string.IsNullOrEmpty(request.NominatedPharmacyCode))
                {
                    medicationRequest.Extension = new FhirExtension
                    {
                        Url = "https://fhir.nhs.uk/StructureDefinition/NHSDigital-MedicationRequest-Nomination",
                        ValueCodeableConcept = new ValueCodeableConcept
                        {
                            Coding = new Coding
                            {
                                System = "https://fhir.nhs.uk/Id/ods-organization-code",
                                Code = request.NominatedPharmacyCode
                            }
                        }
                    };
                }

                bundle.Entry.Add(new BundleEntry
                {
                    Resource = medicationRequest,
                    FullUrl = $"urn:uuid:{medicationRequest.Id}"
                });
            }

            return bundle;
        }
    }
}
