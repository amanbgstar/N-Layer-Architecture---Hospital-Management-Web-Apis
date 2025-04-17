namespace HospitalManagement.ViewModel.EPS
{
    public class FhirBundle
    {
        public string ResourceType { get; set; } = "Bundle";
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = "message";
        public List<BundleEntry> Entry { get; set; } = new List<BundleEntry>();
        public Meta Meta { get; set; } = new Meta();
    }

    public class Meta
    {
        public List<string> Profile { get; set; } = new List<string>();
    }

    public class BundleEntry
    {
        public FhirResource Resource { get; set; }
        public string FullUrl { get; set; }
    }

    public class FhirResource
    {
        public string ResourceType { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }

    public class MessageHeader : FhirResource
    {
        public MessageHeader()
        {
            ResourceType = "MessageHeader";
        }

        public string EventCoding { get; set; }
        public Sender Sender { get; set; }
        public Source Source { get; set; }
        public List<Destination> Destination { get; set; } = new List<Destination>();
        public Focus Focus { get; set; }
    }

    public class Sender
    {
        public References Reference { get; set; }
    }

    public class Source
    {
        public string Name { get; set; }
        public string Software { get; set; }
        public string Version { get; set; }
        public string Endpoint { get; set; }
    }

    public class Destination
    {
        public string Endpoint { get; set; }
    }

    public class Focus
    {
        public References Reference { get; set; }
    }

    // Patient resource
    public class FhirPatient : FhirResource
    {
        public FhirPatient()
        {
            ResourceType = "Patient";
        }

        public List<Identifier> Identifier { get; set; } = new List<Identifier>();
        public List<HumanName> Name { get; set; } = new List<HumanName>();
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public List<FhirAddress> Address { get; set; } = new List<FhirAddress>();
    }

    public class Identifier
    {
        public CodeableConcept System { get; set; }
        public string Value { get; set; }
    }

    public class HumanName
    {
        public string Family { get; set; }
        public List<string> Given { get; set; } = new List<string>();
        public List<string> Prefix { get; set; } = new List<string>();
    }

    public class FhirAddress
    {
        public List<string> Line { get; set; } = new List<string>();
        public string City { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
    }

    // Practitioner resource
    public class FhirPractitioner : FhirResource
    {
        public FhirPractitioner()
        {
            ResourceType = "Practitioner";
        }

        public List<Identifier> Identifier { get; set; } = new List<Identifier>();
        public List<HumanName> Name { get; set; } = new List<HumanName>();
    }

    // PractitionerRole resource
    public class PractitionerRole : FhirResource
    {
        public PractitionerRole()
        {
            ResourceType = "PractitionerRole";
        }

        public References Practitioner { get; set; }
        public References Organization { get; set; }
        public List<CodeableConcept> Code { get; set; } = new List<CodeableConcept>();
        public List<Identifier> Identifier { get; set; } = new List<Identifier>();
    }

    // Organization resource
    public class FhirOrganization : FhirResource
    {
        public FhirOrganization()
        {
            ResourceType = "Organization";
        }

        public List<Identifier> Identifier { get; set; } = new List<Identifier>();
        public string Name { get; set; }
        public List<FhirAddress> Address { get; set; } = new List<FhirAddress>();
    }

    // MedicationRequest resource
    public class MedicationRequest : FhirResource
    {
        public MedicationRequest()
        {
            ResourceType = "MedicationRequest";
        }

        public string Status { get; set; } = "active";
        public string Intent { get; set; } = "order";
        public CodeableConcept MedicationCodeableConcept { get; set; }
        public References Subject { get; set; }
        public References Requester { get; set; }
        public References Groupidentifier { get; set; }
        public string AuthoredOn { get; set; }
        public Dosage DosageInstruction { get; set; }
        public DispenseRequest DispenseRequest { get; set; }
        public FhirExtension Extension { get; set; }
    }

    public class Dosage
    {
        public string Text { get; set; }
        public Timing Timing { get; set; }
        public CodeableConcept Route { get; set; }
        public CodeableConcept Method { get; set; }
        public DoseAndRate DoseAndRate { get; set; }
    }

    public class Timing
    {
        public CodeableConcept Code { get; set; }
    }

    public class DoseAndRate
    {
        public Quantity Dose { get; set; }
    }

    public class Quantity
    {
        public decimal Value { get; set; }
        public string System { get; set; }
        public string Code { get; set; }
        public string Unit { get; set; }
    }

    public class DispenseRequest
    {
        public Quantity Quantity { get; set; }
        public FhirExtension Extension { get; set; }
        public CodeableConcept PerformerType { get; set; }
    }

    public class FhirExtension
    {
        public string Url { get; set; }
        public ValueCodeableConcept ValueCodeableConcept { get; set; }
    }

    public class ValueCodeableConcept
    {
        public Coding Coding { get; set; }
    }

    public class Coding
    {
        public string System { get; set; }
        public string Code { get; set; }
        public string Display { get; set; }
    }

    public class CodeableConcept
    {
        public List<Coding> Coding { get; set; } = new List<Coding>();
    }

    public class References
    {
        public string Reference { get; set; }
    }

    // Provenance resource for signatures
    public class Provenance : FhirResource
    {
        public Provenance()
        {
            ResourceType = "Provenance";
        }

        public List<References> Target { get; set; } = new List<References>();
        public string Recorded { get; set; }
        public List<ProvenanceAgent> Agent { get; set; } = new List<ProvenanceAgent>();
        public Signature Signature { get; set; }
    }

    public class ProvenanceAgent
    {
        public CodeableConcept Role { get; set; }
        public References Who { get; set; }
    }

    public class Signature
    {
        public string Type { get; set; }
        public string When { get; set; }
        public References Who { get; set; }
        public string Data { get; set; }
    }

}
