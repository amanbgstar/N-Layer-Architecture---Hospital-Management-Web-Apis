using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel.PrescriptionRequest
{
    public class ProvenanceRequest
    {
        public ResourceReferenceDto? TargetResource { get; set; }
        public string? SignatureValue { get; set; }
        public string? Certificate { get; set; }
        public string? SignatureAlgorithm { get; set; }
        public string? TargetReference { get; set; }
        public string? AgentReference { get; set; }
    }

    public class ResourceReferenceDto
    {
        public string? Reference { get; set; }
    }
}
