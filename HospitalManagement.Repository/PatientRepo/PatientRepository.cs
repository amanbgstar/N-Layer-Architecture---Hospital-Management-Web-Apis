using HospitalManagement.Domain.Context;
using HospitalManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Repository.LabTestRepo
{
    public class PatientRepository: IPatientRepository
    {
        private readonly PatientContext _patientContext;
        public PatientRepository(PatientContext patientContext)
        {
            _patientContext = patientContext;
        }


        public string AddPatient(Patient patient)
        {
            _patientContext.Patients.Add(patient);
            _patientContext.SaveChanges();
            return "Done";
        }

        public Patient GetPatient(string username) 
        { 
            var pest = _patientContext.Patients.FirstOrDefault(p => p.UserName==username);
            if (pest == null)
                return null;
            return pest;
        }
        
        public List<Patient> AllPatient()
        {
            var data = _patientContext.Patients.ToList();
            return data;
        }
    }
}
