using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel
{
    public class LabTestVM
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public DateTime TakenDate { get; set; }
        public DateTime ReportGenerationTime { get; set; }
        public string TechnicianName { get; set; }
        public string UserName { get; set; }
        public int PatientId { get; set; }
    }
}
