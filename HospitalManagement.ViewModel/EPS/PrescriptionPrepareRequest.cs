namespace HospitalManagement.ViewModel.EPS
{
    public class PrescriptionPrepareRequest
    {
        public PatientInfo Patient { get; set; }
        public PractitionerInfo Practitioner { get; set; }
        public OrganizationInfo Organization { get; set; }
        public List<MedicationRequestItem> Medications { get; set; } = new List<MedicationRequestItem>();
        public string NominatedPharmacyCode { get; set; }
    }

    public class MedicationRequestItem
    {
        public string MedicationCode { get; set; } // SNOMED CT code
        public string MedicationDisplay { get; set; } // Medication name
        public string DosageInstructions { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
    }

    public class PatientInfo
    {
        public string NhsNumber { get; set; }
        public string FamilyName { get; set; }
        public List<string> GivenNames { get; set; } = new List<string>();
        public string Title { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public AddressInfo Address { get; set; }
    }

    public class AddressInfo
    {
        public List<string> Lines { get; set; } = new List<string>();
        public string City { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
    }

    public class PractitionerInfo
    {
        public string ProfessionalCode { get; set; } // SDS User ID
        public string FamilyName { get; set; }
        public List<string> GivenNames { get; set; } = new List<string>();
        public string Title { get; set; }
    }

    public class OrganizationInfo
    {
        public string OdsCode { get; set; }
        public string Name { get; set; }
        public AddressInfo Address { get; set; }
    }
}