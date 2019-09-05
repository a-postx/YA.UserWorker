using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Application.Interfaces
{
    public enum Countries
    {
        UN = 0,
        RU = 1,
        CN = 2,
        US = 4,
        DE = 8,
        FR = 16,
        IE = 32,
        GB = 64,
        SG = 128,
        NL = 256
    }
    /// <summary>
    /// Retrieves geodata for the current application.
    /// </summary>
    interface IGeoDataService
    {
        /// <summary>
        /// Get country code of the application location (ISO 3166).
        /// </summary>
        Task<Countries> GetCountryCodeAsync();
    }
}
