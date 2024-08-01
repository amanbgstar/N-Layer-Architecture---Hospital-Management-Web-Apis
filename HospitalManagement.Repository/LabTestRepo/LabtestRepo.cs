using HospitalManagement.Domain.Context;
using HospitalManagement.Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Repository.LabTestRepo
{
    public class LabtestRepo: ILabtestRepo
    {
        private readonly PatientContext _patientContext;
        public LabtestRepo(PatientContext patientContext)
        {
            _patientContext = patientContext;
        }

        public List<LabTest> GetLabTestListFromSP(int currentPage = 0, int recordPerPage = 0)
        {
            var parameters = new SqlParameter[] {
            new SqlParameter()
            {
                ParameterName = "@pageNo",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Input,
                Value = currentPage
            },
            new SqlParameter()
            {
                ParameterName = "@pageCount",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Input,
                Value = recordPerPage
            },
             new SqlParameter()
             {
                 ParameterName = "@ ",
                 SqlDbType = System.Data.SqlDbType.Int,
                 Direction = System.Data.ParameterDirection.Output,
                 Size = 50
             }};
            return _patientContext.LabTests.FromSqlRaw("exec sp_GetPatientLabTestList @pageNo, @pageCount, @total", parameters).ToList();


        }
    }
}
