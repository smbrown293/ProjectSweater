using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectSweater.Configuration;
using ProjectSweater.Dtos;

namespace ProjectSweater.Services
{
    public class ApplicationService : BackgroundService
    {
        private readonly OpenWeatherMapApiService _weatherSvc;
        private readonly IOptionsMonitor<List<Recommendation>> _recommendationsOptions;

        public ApplicationService(OpenWeatherMapApiService weatherSvc, IOptionsMonitor<List<Recommendation>> recommendationsOptions)
        {
            _weatherSvc = weatherSvc;
            _recommendationsOptions = recommendationsOptions;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Welcome to project S.W.E.A.T.E.R.!");
            Console.WriteLine("Please provide a US city and state to be given clothing recommendations for the next 24 hours.");
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("Where are you headed?");

                Console.Write("Enter a city for lookup: ");
                var city = Console.ReadLine();
                Console.Write("Enter a state code for lookup: ");
                var state = Console.ReadLine();
                //Perform the API call
                var resp = await _weatherSvc.GetForecasts(city, state, stoppingToken);
                //Do stuff; all logic for determining output goes here
                var finalRecommendations = GetRecommendationsForForecasts(resp.list);
                //Format based on results.
                WriteRecommendations(finalRecommendations, resp.city.name);
            }
        }

        private static void WriteRecommendations(List<string> finalRecommendations, string cityName)
        {
            switch (finalRecommendations.Count)
            {
                case 0:
                    Console.WriteLine("What kind of weather is this? I have no recommendations for you!");
                    break;
                case 1:
                    Console.WriteLine($"You should bring {finalRecommendations.First()} to {cityName}.");
                    break;
                default:
                    Console.WriteLine("You should bring " +
                                      $"{string.Join(", ", finalRecommendations.ToArray(), 0, finalRecommendations.Count - 1)} " +
                                      $", and {finalRecommendations.Last()} if you're headed to {cityName}!");
                    break;
            }
        }

        private List<string> GetRecommendationsForForecasts(IList<Forecast> forecasts)
        {
            //Supports update of config for recommendations at run-time
            var recommendations = _recommendationsOptions.CurrentValue;

            //First get a rough average temp of all of the forecasts; get average temp for each forecast and then average these estimates together.
            //Definitely some degree of error here. Better statistical analysis would yield more accurate results.
            var avgTemp = forecasts.Average(o => (o.main.temp_max + o.main.temp_min) / 2);
            
            //Also check if "waterproof" is necessary; arbitrarily pick >25% of forecasts predict rain.
            var willBeRainy = forecasts.Count(o => o.weather.Any(w => w.main.Contains("Rain"))) >
                              (forecasts.Count / 4.0);

            //Use the above to filter our starting list of possible recommendations. Want our average temp to be in the valid range of temperatures for the recommendation.
            var validRecommendations = recommendations.Where(o => o.MaxTemp > avgTemp && o.MinTemp < avgTemp && o.Waterproof == willBeRainy);

            //Only care about the names of the items being recommended at this point
            return validRecommendations.Select(o => o.Name).ToList();
        }
    }
}
