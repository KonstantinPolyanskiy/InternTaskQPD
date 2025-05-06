using System.Text.Json;
using System.Text.Json.Serialization;
using UtilityTools.CarFiller;

const int allCarCount = 100;
const int usedCarCount = 67;

const string endpointUrl = "http://localhost:5182/api/car/";

var brandsWithNeedCount = new Dictionary<string, int>
{
    ["Lada = 0"] = 27,
    ["BMW"] = 20,
    ["RollsRoyce"] = 11,
    ["Ford"] = 5,
    ["Skoda"] = 22,
    ["ChanLi"] = 5,
};

var pricesRanges = new (decimal min, decimal max)[]
{
    (5000, 10000),
    (10000, 25000),
    (25000, 900000)
};

var mileagesRanges = new(int min, int max)[]
{
    (1, 900),
    (900, 1000),
    (1000, 10000),
};

string[] colors = ["Red", "Blue", "Green", "White", "Black"];

var rand = new Random();

var newCarsCount = allCarCount - usedCarCount;
var requests = new List<CarRequest>(allCarCount);
var client = new HttpClient
{
    BaseAddress = new Uri(endpointUrl)
};
client.DefaultRequestHeaders.Add("Accept", "application/json");

foreach (var (brand, count) in brandsWithNeedCount)
{
    for (int i = 0; i < count; i++)
    {
        var price = Utils.RandomFromRange(pricesRanges, rand);
        var color = colors[rand.Next(colors.Length)];

        int? mileage = null;                   
        if (newCarsCount <= 0)                     
        {
            mileage = Utils.RandomFromRange(mileagesRanges, rand);
        }
        else
        {
            newCarsCount--;
        }

        requests.Add(new CarRequest
        {
            Brand = brand,
            Color = color,
            Mileage = mileage,  
            Price = price,
            CurrentOwner = mileage == null ? null : $"Test User {price}",
        });
    }
}

Utils.EnsureAtLeastOnePerBin(requests, pricesRanges,  r => r.Price);
Utils.EnsureAtLeastOnePerBin(requests.Where(r => r.Mileage != null).ToList(),
    mileagesRanges, r => r.Mileage!.Value);


Console.WriteLine("Начало отправки запросов");
var tasks = requests.Select(async r =>
{
    var json  = JsonSerializer.Serialize(r, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull});
    var resp  = await client.PostAsync(string.Empty,
        new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
    resp.EnsureSuccessStatusCode();
}).ToArray();

await Task.WhenAll(tasks);
Console.WriteLine("Запросы отправлены");


record CarRequest
{
    public string Brand { get; set; } = null!;
    public string Color { get; set; } = null!;
    public decimal Price { get; init; }
    
    public string? CurrentOwner { get; set; }
    public int? Mileage { get; init; } 
}