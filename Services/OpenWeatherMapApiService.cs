using Microsoft.Extensions.Options;
using ProjectSweater.Configuration;
using ProjectSweater.Dtos;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSweater.Services
{
    public class OpenWeatherMapApiService
    {
        private readonly HttpClient _client;
        private readonly IOptionsMonitor<ServiceConfig> _cfg;

        public OpenWeatherMapApiService(HttpClient client, IOptionsMonitor<ServiceConfig> cfg)
        {
            if (cfg == null || string.IsNullOrEmpty(cfg.CurrentValue.ApiKey))
                throw new ArgumentNullException(nameof(cfg),
                    "No value provided for API key, verify that the application is configured properly before continuing.");
            _client = client;
            _cfg = cfg;
        }

        public async Task<OpenWeatherResponseDto> GetForecasts(string city, string state, CancellationToken cancellationToken = default)
        {
            var cfg = _cfg.CurrentValue;
            //notes on api call:
            //defaulting country to us 
            //grabbing units/# of forecasts to analyze from config
            //# of forecasts intended to be additional knob for testing analysis
            var resp = await _client.GetAsync($"https://api.openweathermap.org/data/2.5/forecast?q={city},{state},us&appid={cfg.ApiKey}&units={cfg.Units}&cnt={cfg.ForecastCount}", cancellationToken);
            var model = JsonSerializer.Deserialize<OpenWeatherResponseDto>(await resp.Content.ReadAsStringAsync());
            return model;
        }
    }
}
