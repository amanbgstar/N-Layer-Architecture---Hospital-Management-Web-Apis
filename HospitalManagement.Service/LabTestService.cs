using HospitalManagement.Repository.LabTestRepo;
using HospitalManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Service
{
    public class LabTestService: ILabTestService
    {
        private readonly ILabtestRepo _labTestRepo;
        public LabTestService(ILabtestRepo labTestRepo)
        {
            _labTestRepo = labTestRepo;
        }

        public List<LabTestVM> GetLabtestList()
        {
            var listOFLB=_labTestRepo.GetLabTestListFromSP(0, 2);
            var labList = new List<LabTestVM>();
            {

            }
            return labList;
        }
    }
}
