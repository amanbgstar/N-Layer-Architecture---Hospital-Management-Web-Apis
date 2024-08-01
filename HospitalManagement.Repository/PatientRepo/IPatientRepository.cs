using HospitalManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Repository.LabTestRepo
{
    public interface IPatientRepository
    {
        string AddPatient(Patient patient);
        Patient GetPatient(string username);
        List<Patient> AllPatient();
    }
}