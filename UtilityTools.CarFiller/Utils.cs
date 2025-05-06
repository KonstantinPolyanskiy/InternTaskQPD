namespace UtilityTools.CarFiller;

internal static class Utils
{
    public static decimal RandomFromRange((decimal min, decimal max)[] range, Random rand)
    {
        var temp = range[rand.Next(range.Length)];
        return Math.Round(temp.min + (decimal)rand.NextDouble() * (temp.max - temp.min), 2);
    }

    public static int RandomFromRange((int min, int max)[] range, Random rand)
    {
         var temp = range[rand.Next(range.Length)];
         return rand.Next(temp.min, temp.max + 1);
    }
    
    public static void EnsureAtLeastOnePerBin(
        List<CarRequest> list,
        (decimal min, decimal max)[] range,
        Func<CarRequest, decimal > selector)
    {
        foreach (var (min, max) in range)
        {
            if (!list.Any(r => selector(r).CompareTo(min) >= 0 &&
                               selector(r).CompareTo(max) < 0))
            {
                var idx = new Random().Next(list.Count);
                var req = list[idx];

                if (selector == (Func<CarRequest, decimal>)(o => o.Price))
                {
                    decimal v = min is var dMin && max is var dMax
                        ? dMin + (decimal)new Random().NextDouble() * (dMax - dMin)
                        : min!;
                    list[idx] = req with { Price = Math.Round(v, 2) };
                }
            }
        }
    }

    public static void EnsureAtLeastOnePerBin(
        List<CarRequest> list,
        (int min, int max)[] range,
        Func<CarRequest, int> selector)
    {
        foreach (var (min, max) in range)
        {
            if (!list.Any(r => selector(r).CompareTo(min) >= 0 &&
                               selector(r).CompareTo(max) < 0))
            {
                var idx = new Random().Next(list.Count);
                var req = list[idx];

                int v = min is var iMin && max is var iMax
                    ? new Random().Next(iMin, iMax + 1)
                    : min!;
                list[idx] = req with { Mileage = v };
            }
        }
        
    }
}