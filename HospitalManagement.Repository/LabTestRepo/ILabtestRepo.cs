using HospitalManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Repository.LabTestRepo
{
    public interface ILabtestRepo
    {
        List<LabTest> GetLabTestListFromSP(int currentPage = 0, int recordPerPage = 0);
    }
}

