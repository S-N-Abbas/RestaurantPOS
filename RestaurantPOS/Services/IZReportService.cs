using RestaurantPOS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public interface IZReportService
    {
        Task<ZReportData> GenerateAsync(DateTime from, DateTime to);
    }
}
