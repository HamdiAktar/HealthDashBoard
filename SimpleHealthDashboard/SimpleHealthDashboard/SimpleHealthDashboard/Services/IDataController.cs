using SimpleHealthDashboard.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHealthDashboard.Services
{
    public interface IDataController
    {
        Task<List<HealthData>> GetHealthData();
    }
}
