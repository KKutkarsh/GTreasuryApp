using GTreasury.Api.Functions.Services.Interface;
using GTreasury.Api.Utilities.Dtos;
using GTreasury.Api.Utilities.Records;
using System.Collections.Concurrent;

namespace GTreasury.Api.Functions.Services
{

    /// <summary>
    /// slight difference is possible because of data type decimal and double 
    /// </summary>
    public class NpvCalculator : INpvCalculator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public IEnumerable<NpvResult> Calculate(NpvInput input)
        {
            var results = new List<NpvResult>();

            int baseYear = input.EvaluationYear;

            for (double rate = input.LowerRate; rate <= input.UpperRate; rate += input.Increment)
            {
                double convertedRate = rate / 100.0;

                double npv = 0;

                foreach (var cashFlow in input.CashFlows)
                {
                    int t = cashFlow.Year - baseYear;
                    npv += cashFlow.Amount / Math.Pow(1 + convertedRate, t);
                }

                results.Add(new NpvResult
                {
                    Rate = rate,
                    Value = Math.Round(npv, 5)
                });
            }

            return results;
        }

        /// <summary>
        /// pros: simple and fast
        /// good for moderate data
        /// in memory processing
        /// 
        /// cons:
        /// not good for very large processing
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IEnumerable<NpvResult> CalculateParallel(NpvInput input)
        {
            var results = new ConcurrentBag<NpvResult>();

            int baseYear = input.EvaluationYear; ;

            var rates = GetRates(input.LowerRate, input.UpperRate, input.Increment);

            var c0 = input.CashFlows.FirstOrDefault(cf => cf.Year == baseYear)?.Amount ?? 0;

            Parallel.ForEach(rates, rate =>
            {
                double convertedRate = rate / 100.0;
                double npv = 0.0;
                foreach (var cashFlow in input.CashFlows)
                {
                    int t = cashFlow.Year - baseYear;
                    npv += cashFlow.Amount / Math.Pow(1 + convertedRate, t);
                }

                results.Add(new NpvResult
                {
                    Rate = rate,
                    Value = Math.Round(npv, 5)
                });
            });

            return results.OrderBy(r => r.Rate);
        }

        private IEnumerable<double> GetRates(double lower, double upper, double increment)
        {
            for (var rate = lower; rate <= upper; rate += increment)
            {
                yield return rate;
            }
        }

        public IEnumerable<NpvResult> CalculateBatch(NpvInput input, IEnumerable<double> rates)
        {
            var results = new List<NpvResult>();

            // IMPORTANT: Match the base year logic from Approach 1
            int baseYear = input.EvaluationYear;

            foreach (var rate in rates)
            {
                double convertedRate = rate / 100.0;
                double npv = 0;

                // Process all cash flows exactly as in Approach 1
                foreach (var cashFlow in input.CashFlows)
                {
                    int t = cashFlow.Year - baseYear;
                    npv += cashFlow.Amount / Math.Pow(1 + convertedRate, t);
                }

                results.Add(new NpvResult
                {
                    Rate = rate,
                    // Match the 5-decimal precision from Approach 1
                    Value = Math.Round(npv, 5)
                });
            }

            return results;
        }

        public IEnumerable<double> GetRateRange(double lower, double upper, double increment)
        {
            for (var rate = lower; rate <= upper; rate += increment)
            {
                yield return rate;
            }
        }

    }
}
