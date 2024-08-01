using HospitalManagement.Domain.Models;
using HospitalManagement.Repository.LabTestRepo;
using HospitalManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Service
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepo;

        public PatientService(IPatientRepository patientRepo)
        {
            _patientRepo = patientRepo;
        }
        public string CreateUserName(PatientVM patient)
        {
            var FName = patient.PatientFirstName;
            var InLastName = patient.PatientLastName.Substring(0, 1);
            var Date = patient.DateOfBirth.ToString("ddMMyy");
            var UserName = $"PT_{FName}{InLastName}{Date}";
            return UserName;
        }

        public PatientVM AddPatient( PatientVM patient )
        {
            var patientObj = new Patient()
            {
                PatientFirstName = patient.PatientFirstName,
                PatientLastName = patient.PatientLastName,
                DateOfBirth = patient.DateOfBirth,
                UserName = CreateUserName( patient ),
                Gender = patient.Gender,
                BloodGroup = patient.BloodGroup,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                PinCode = patient.PinCode,
                LabTests = patient.LabTests 
            };
            var result = _patientRepo.AddPatient(patientObj);

            

            var showResult = new PatientVM()
            {
                PatientId = patientObj.PatientId,
                UserName = CreateUserName(patient),
                DateOfBirth = patient.DateOfBirth
            };
            if (result != "Done")
                return null;
            return showResult;
        }

        public string Login(string username, string bg)
        {
            var pest = _patientRepo.GetPatient(username);
            if (pest != null) {
                if (pest.BloodGroup == bg)
                    return "login successfully";
                return "Incorrect Date Of Birth";
            }
            return "Please SignUp First";
            
        }

        public List<PatientVM> AllData()
        {
            var patientList = new List<PatientVM>();
            var data = _patientRepo.AllPatient();
            foreach (var vm in data) 
            {
                var newData = new PatientVM
                {
                    PatientId =vm.PatientId,
                    PatientFirstName=vm.PatientFirstName,
                    PatientLastName=vm.PatientLastName,
                    DateOfBirth=vm.DateOfBirth,
                    UserName=vm.UserName,
                    Gender=vm.Gender,
                    BloodGroup=vm.BloodGroup,
                    Address=vm.Address
                };
                patientList.Add(newData);
            }
            return patientList;
            
        }
    }
     
}
