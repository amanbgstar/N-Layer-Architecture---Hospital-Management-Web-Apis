using Hl7.Fhir.Model;
using HospitalManagement.ViewModel.PrescriptionRequest;

namespace HospitalManagement.Service
{
    public class PrescriptionService : IPrescriptionService
    {
        public Bundle CreateAcutePrescriptionBundle(AcutePrescriptionRequest request)
        {
            var bundle = new Bundle
            {
                Id = Guid.NewGuid().ToString(),
                Identifier = new Identifier
                {
                    System = "https://tools.ietf.org/html/rfc4122",
                    Value = Guid.NewGuid().ToString()
                },
                Type = Bundle.BundleType.Message,
                Entry = new List<Bundle.EntryComponent>()
            };

            // Generate UUIDs for all references
            var messageHeaderId = $"urn:uuid:{Guid.NewGuid()}";
            var patientId = $"urn:uuid:{Guid.NewGuid()}";
            var practitionerRoleId = $"urn:uuid:{Guid.NewGuid()}";
            var practitionerId = $"urn:uuid:{Guid.NewGuid()}";
            var organizationId = $"urn:uuid:{Guid.NewGuid()}";

            var medicationReferences = new List<ResourceReference>();

            // Add MessageHeader
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = messageHeaderId,
                Resource = CreateMessageHeader(request, medicationReferences, messageHeaderId)
            });

            foreach (var medication in request.Medications)
            {
                var medicationId = $"urn:uuid:{Guid.NewGuid()}";
                medicationReferences.Add(new ResourceReference(medicationId));

                var medicationRequest = CreateMedicationRequest(
                    medication,
                    request,
                    patientId,
                    practitionerRoleId,
                    medicationId);

                bundle.Entry.Add(new Bundle.EntryComponent
                {
                    FullUrl = medicationId,
                    Resource = medicationRequest
                });
            }

            // Add Patient
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = patientId,
                Resource = CreatePatient(request.Patient, patientId, request.PrescriberOrganization.OdsCode)
            });

            // Add PractitionerRole
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = practitionerRoleId,
                Resource = CreatePractitionerRole(request.Prescriber, practitionerRoleId, practitionerId, organizationId)
            });

            // Add Practitioner
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = practitionerId,
                Resource = CreatePractitioner(request.Prescriber, practitionerId)
            });

            // Add Organization
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = organizationId,
                Resource = CreateOrganization(request.PrescriberOrganization, organizationId)
            });

            return bundle;
        }

        private MessageHeader CreateMessageHeader(
            AcutePrescriptionRequest request,
            List<ResourceReference> medicationReferences,
            string messageHeaderId)
        {
            return new MessageHeader
            {
                Id = messageHeaderId,
                Event = new Coding
                {
                    System = "https://fhir.nhs.uk/CodeSystem/message-event",
                    Code = "prescription-order",
                    Display = "Prescription Order"
                },
                Destination = new List<MessageHeader.MessageDestinationComponent>
            {
                new MessageHeader.MessageDestinationComponent
                {
                    Endpoint = request.DispensingPharmacy.Endpoint,
                    Receiver = new ResourceReference
                    {
                        Identifier = new Identifier
                        {
                            System = "https://fhir.nhs.uk/Id/ods-organization-code",
                            Value = request.DispensingPharmacy.OdsCode
                        },
                        Display = request.DispensingPharmacy.Name
                    }
                }
            },
                Sender = new ResourceReference
                {
                    Identifier = new Identifier
                    {
                        System = "https://fhir.nhs.uk/Id/ods-organization-code",
                        Value = request.PrescriberOrganization.OdsCode
                    },
                    Display = request.PrescriberOrganization.Name
                },
                Source = new MessageHeader.MessageSourceComponent
                {
                    Endpoint = $"https://directory.spineservices.nhs.uk/STU3/Organization/{request.PrescriberOrganization.OdsCode}"
                },
                Focus = medicationReferences
            };
        }

        private MedicationRequest CreateMedicationRequest(
            MedicationItem medication,
            AcutePrescriptionRequest request,
            string patientId,
            string practitionerRoleId,
            string medicationId)
        {
            return new MedicationRequest
            {
               /* Id = medicationId,*/
                Extension = new List<Extension>
            {
                new Extension
                {
                    Url = "https://fhir.nhs.uk/StructureDefinition/Extension-DM-PrescriptionType",
                    Value = new Coding
                    {
                        System = "https://fhir.nhs.uk/CodeSystem/prescription-type",
                        Code = "0101",
                        Display = "Primary Care Prescriber - Medical Prescriber"
                    }
                }
            },
                Identifier = new List<Identifier>
            {
                new Identifier
                {
                    System = "https://fhir.nhs.uk/Id/prescription-order-item-number",
                    Value = Guid.NewGuid().ToString()
                }
            },
                Status = MedicationRequest.MedicationrequestStatus.Active,
                Intent = MedicationRequest.MedicationRequestIntent.Order,
                Category = new List<CodeableConcept>
            {
                new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://terminology.hl7.org/CodeSystem/medicationrequest-category",
                            Code = "community",
                            Display = "Community"
                        }
                    }
                }
            },
                Medication = new CodeableConcept
                {
                    Coding = new List<Coding>
                {
                    new Coding
                    {
                        System = "http://snomed.info/sct",
                        Code = medication.SnomedCode,
                        Display = medication.Display
                    }
                }
                },
                Subject = new ResourceReference(patientId),
                Requester = new ResourceReference(practitionerRoleId),
                GroupIdentifier = new Identifier
                {
                    System = "https://fhir.nhs.uk/Id/prescription-order-number",
                    Value = request.PrescriptionOrderNumber,
                    Extension = new List<Extension>
                {
                    new Extension
                    {
                        Url = "https://fhir.nhs.uk/StructureDefinition/Extension-DM-PrescriptionId",
                        Value = new Identifier
                        {
                            System = "https://fhir.nhs.uk/Id/prescription",
                            Value = request.PrescriptionId
                        }
                    }
                }
                },
                CourseOfTherapyType = new CodeableConcept
                {
                    Coding = new List<Coding>
                {
                    new Coding
                    {
                        System = "http://terminology.hl7.org/CodeSystem/medicationrequest-course-of-therapy",
                        Code = "acute",
                        Display = "Short course (acute) therapy"
                    }
                }
                },
                DosageInstruction = new List<Dosage>
            {
                new Dosage
                {
                    Text = medication.DosageText,
                    Timing = new Timing
                    {
                        Repeat = new Timing.RepeatComponent
                        {
                            Frequency = medication.Frequency,
                            Period = medication.Period,
                            PeriodUnit = Timing.UnitsOfTime.D
                        }
                    },
                    Route = new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://snomed.info/sct",
                                Code = "26643006",
                                Display = "Oral"
                            }
                        }
                    }
                }
            },
                DispenseRequest = new MedicationRequest.DispenseRequestComponent
                {
                    Extension = new List<Extension>
                {
                    new Extension
                    {
                        Url = "https://fhir.nhs.uk/StructureDefinition/Extension-DM-PerformerSiteType",
                        Value = new Coding
                        {
                            System = "https://fhir.nhs.uk/CodeSystem/dispensing-site-preference",
                            Code = "P1"
                        }
                    }
                },
                    ValidityPeriod = new Period
                    {
                        Start = request.PrescriptionDate.ToString("yyyy-MM-dd")
                    },
                    Quantity = new()
                    {
                        Value = medication.QuantityValue,
                        Unit = medication.QuantityUnit,
                        System = "http://snomed.info/sct",
                        Code = medication.QuantityCode
                    },
                    ExpectedSupplyDuration = new Duration
                    {
                        Value = medication.ExpectedSupplyDuration,
                        Unit = "day",
                        System = "http://unitsofmeasure.org",
                        Code = "d"
                    },
                    Performer = new ResourceReference
                    {
                        Identifier = new Identifier
                        {
                            System = "https://fhir.nhs.uk/Id/ods-organization-code",
                            Value = request.DispensingPharmacy.OdsCode
                        }
                    }
                },
                Substitution = new MedicationRequest.SubstitutionComponent
                {
                    Allowed = new FhirBoolean(false),
                    Reason = new CodeableConcept
                    {
                        Coding = new List<Coding>
        {
            new Coding
            {
                System = "https://fhir.nhs.uk/CodeSystem/EPS-substitution-reason",
                Code = "N",
                Display = "No substitution"
            }
        }
                    }
                }

            };
        }

        private Patient CreatePatient(PatientDetails patient, string patientId, string gpOdsCode)
        {
            var patientResource = new Patient
            {
                /*Id = patientId,*/
                Identifier = new List<Identifier>
        {
            new Identifier
            {
                System = "https://fhir.nhs.uk/Id/nhs-number",
                Value = patient.NhsNumber
            }
        },
                Name = new List<HumanName>
        {
            new HumanName
            {
                Use = HumanName.NameUse.Usual,
                Family = patient.FamilyName,
                Given = patient.GivenNames,
                Prefix = !string.IsNullOrEmpty(patient.Prefix) ?
                    new List<string> { patient.Prefix } : null
            }
        },
                Gender = patient.Gender.ToLower() switch
                {
                    "male" => AdministrativeGender.Male,
                    "female" => AdministrativeGender.Female,
                    "other" => AdministrativeGender.Other,
                    "unknown" => AdministrativeGender.Unknown,
                    _ => AdministrativeGender.Unknown
                },
                BirthDate = patient.BirthDate.ToString("yyyy-MM-dd"),

                Address = new List<Hl7.Fhir.Model.Address>
        {
            new Hl7.Fhir.Model.Address
            {
                Use = Hl7.Fhir.Model.Address.AddressUse.Home,
                Line = patient.Address.Line,
                City = !string.IsNullOrEmpty(patient.Address.City) ?
                    patient.Address.City : null,
                District = !string.IsNullOrEmpty(patient.Address.District) ?
                    patient.Address.District : null,
                PostalCode = patient.Address.PostalCode
            }
        },
                GeneralPractitioner = new List<ResourceReference>
        {
            new ResourceReference
            {
                Identifier = new Identifier
                {
                    System = "https://fhir.nhs.uk/Id/ods-organization-code",
                    Value = gpOdsCode
                }
            }
        }
            };

            return patientResource;
        }

        private PractitionerRole CreatePractitionerRole(
            PractitionerDetails practitioner,
            string practitionerRoleId,
            string practitionerId,
            string organizationId)
        {
            return new PractitionerRole
            {
                /*Id = practitionerRoleId,*/
                Identifier = new List<Identifier>
            {
                new Identifier
                {
                    System = "https://fhir.nhs.uk/Id/sds-role-profile-id",
                    Value = practitioner.SdsRoleProfileId
                }
            },
                Practitioner = new ResourceReference(practitionerId),
                Organization = new ResourceReference(organizationId),
                Code = new List<CodeableConcept>
            {
                new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "https://fhir.nhs.uk/CodeSystem/NHSDigital-SDS-JobRoleCode",
                            Code = "R8000",
                            Display = "Clinical Practitioner Access Role"
                        },
                        new Coding
                        {
                            System = "https://fhir.hl7.org.uk/CodeSystem/UKCore-SDSJobRoleName",
                            Code = "R8000",
                            Display = "Clinical Practitioner Access Role"
                        }
                    }
                }
            },
                Telecom = new List<ContactPoint>
            {
                new ContactPoint
                {
                    System = ContactPoint.ContactPointSystem.Phone,
                    Use = ContactPoint.ContactPointUse.Work,
                    Value = practitioner.Phone
                }
            }
            };
        }

        private Practitioner CreatePractitioner(PractitionerDetails practitioner, string practitionerId)
        {
            var identifiers = new List<Identifier>
        {
            new Identifier
            {
                System = "https://fhir.nhs.uk/Id/sds-user-id",
                Value = practitioner.SdsUserId
            }
        };

            if (!string.IsNullOrEmpty(practitioner.GmcNumber))
            {
                identifiers.Add(new Identifier
                {
                    System = "https://fhir.hl7.org.uk/Id/gmc-number",
                    Value = practitioner.GmcNumber
                });
            }

            if (!string.IsNullOrEmpty(practitioner.DinNumber))
            {
                identifiers.Add(new Identifier
                {
                    System = "https://fhir.hl7.org.uk/Id/din-number",
                    Value = practitioner.DinNumber
                });
            }

            return new Practitioner
            {
                /*Id = practitionerId,*/
                Identifier = identifiers,
                Name = new List<HumanName>
            {
                new HumanName
                {
                    Family = practitioner.FamilyName,
                    Given = new List<string> { practitioner.GivenName },
                    Prefix = new List<string> { practitioner.Prefix }
                }
            }
            };
        }

        private Organization CreateOrganization(OrganizationDetails organization, string organizationId)
        {
            return new Organization
            {
                /*Id = organizationId,*/
                Identifier = new List<Identifier>
            {
                new Identifier
                {
                    System = "https://fhir.nhs.uk/Id/ods-organization-code",
                    Value = organization.OdsCode
                }
            },
                Type = new List<CodeableConcept>
            {
                new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "https://fhir.nhs.uk/CodeSystem/organisation-role",
                            Code = "76",
                            Display = "GP PRACTICE"
                        }
                    }
                }
            },
                Name = organization.Name,
                Telecom = new List<ContactPoint>
            {
                new ContactPoint
                {
                    System = ContactPoint.ContactPointSystem.Phone,
                    Use = ContactPoint.ContactPointUse.Work,
                    Value = organization.Phone
                }
            },
                Address = new List<Hl7.Fhir.Model.Address>
            {
                new Hl7.Fhir.Model.Address
                {
                    Use = Hl7.Fhir.Model.Address.AddressUse.Work,
                    Type = Hl7.Fhir.Model.Address.AddressType.Both,
                    Line = organization.Address.Line,
                    City = organization.Address.City,
                    District = organization.Address.District,
                    PostalCode = organization.Address.PostalCode
                }
            },
                PartOf = new ResourceReference
                {
                    Identifier = new Identifier
                    {
                        System = "https://fhir.nhs.uk/Id/ods-organization-code",
                        Value = organization.ParentOdsCode
                    },
                    Display = organization.ParentName
                }
            };
        }
    }
}
