using HospitalManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Service
{
    public interface IPatientService
    {
        PatientVM AddPatient(PatientVM patient);
        string Login(string username, string bg);
        List<PatientVM> AllData();
    }
}
