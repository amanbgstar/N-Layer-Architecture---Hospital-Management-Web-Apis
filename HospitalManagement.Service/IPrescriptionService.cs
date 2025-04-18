using Hl7.Fhir.Model;
using HospitalManagement.ViewModel.PrescriptionRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Service
{
    public interface IPrescriptionService
    {
        Bundle CreateAcutePrescriptionBundle(AcutePrescriptionRequest request);
    }
}
