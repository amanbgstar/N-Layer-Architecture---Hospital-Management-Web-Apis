using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel.PrescriptionRequest
{
    public class AcutePrescriptionRequest
    {
        [Required]
        public string PrescriptionId { get; set; }

        [Required]
        public string PrescriptionOrderNumber { get; set; }

        [Required]
        public DateTime PrescriptionDate { get; set; }

        [Required]
        public PatientDetails Patient { get; set; }

        [Required]
        public PractitionerDetails Prescriber { get; set; }

        [Required]
        public OrganizationDetails PrescriberOrganization { get; set; }

        [Required]
        public PharmacyDetails DispensingPharmacy { get; set; }

        [Required, MinLength(1)]
        public List<MedicationItem> Medications { get; set; }
    }

    public class PatientDetails
    {
        [Required]
        public string NhsNumber { get; set; }

        [Required]
        public string FamilyName { get; set; }

        public List<string>? GivenNames { get; set; }

        public string Prefix { get; set; } // Made optional

        [Required]
        public string Gender { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public Address Address { get; set; }
    }

    public class PractitionerDetails
    {
        public string SdsUserId { get; set; }
        public string SdsRoleProfileId { get; set; }
        public string GmcNumber { get; set; }
        public string DinNumber { get; set; }
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
        public string Prefix { get; set; }
        public string Phone { get; set; }
    }

    public class OrganizationDetails
    {
        public string OdsCode { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public Address Address { get; set; }
        public string ParentOdsCode { get; set; }
        public string ParentName { get; set; }
    }

    public class PharmacyDetails
    {
        public string OdsCode { get; set; }
        public string Name { get; set; }
        public string Endpoint { get; set; }
    }

    public class MedicationItem
    {
        public string SnomedCode { get; set; }
        public string Display { get; set; }
        public decimal QuantityValue { get; set; }
        public string QuantityUnit { get; set; }
        public string QuantityCode { get; set; }
        public int ExpectedSupplyDuration { get; set; }
        public string DosageText { get; set; }
        public int Frequency { get; set; }
        public int Period { get; set; }
        public string PeriodUnit { get; set; }
    }

    public class Address
    {
        public List<string>? Line { get; set; }
        public string City { get; set; } = string.Empty; 
        public string District { get; set; } = string.Empty; 
        public string PostalCode { get; set; }
    }
}