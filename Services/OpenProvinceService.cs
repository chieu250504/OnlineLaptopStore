using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Laptop88_3.Models;

namespace Laptop88_3.Services
{
    public class OpenProvinceService
    {
        private readonly HttpClient _httpClient;
        private readonly MemoryCache _cache = MemoryCache.Default;
        private const string CacheKey = "OpenApi_Provinces";

        public OpenProvinceService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://provinces.open-api.vn/api/v1/")
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Lấy provinces (mặc định include children để dễ lấy district/ward)
        public async Task<List<Province>> GetProvinces(bool includeChildren = true, bool forceRefresh = false)
        {
            if (!forceRefresh && _cache.Contains(CacheKey))
                return (List<Province>)_cache.Get(CacheKey);

            var depth = includeChildren ? 2 : 1;
            var url = $"?depth={depth}";
            var json = await _httpClient.GetStringAsync(url);
            var provinces = JsonConvert.DeserializeObject<List<Province>>(json) ?? new List<Province>();

            // Cache 24 giờ (tùy bạn)
            _cache.Set(CacheKey, provinces, DateTimeOffset.UtcNow.AddHours(24));
            return provinces;
        }

        public async Task<List<District>> GetDistrictsByProvinceCode(int provinceCode)
        {
            var provinces = await GetProvinces(true);
            var p = provinces.FirstOrDefault(x => x.Code == provinceCode);
            return p?.Districts ?? new List<District>();
        }

        public async Task<List<Ward>> GetWardsByDistrictCode(int districtCode)
        {
            // gọi trực tiếp district API, depth=2 để lấy wards
            var url = $"https://provinces.open-api.vn/api/d/{districtCode}?depth=2";
            var json = await _httpClient.GetStringAsync(url);
            var district = JsonConvert.DeserializeObject<District>(json);

            return district?.Wards ?? new List<Ward>();
        }

    }
}
