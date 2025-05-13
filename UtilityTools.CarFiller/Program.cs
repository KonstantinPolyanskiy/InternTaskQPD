// PopulateService.cs — v2
// Добавлены: camelCase‑сериализация, подробный лог исходящего JSON и флаг
// для быстрой проверки single‑request через Swagger‑UI.

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

const string BASE_URL = "http://localhost:5117"; // ← твой host
const bool SINGLE_TEST = false;                  // true → создаётся одна Lada

var http = new HttpClient { BaseAddress = new Uri(BASE_URL) };
var rng  = new Random();
JsonSerializerOptions s_json = new()
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,   // <— для ответа {"data":{...}}
    WriteIndented = false
};

// 1. Логинимся админом --------------------------------------------------------------------------
string adminToken = await LoginAsync("admin", "Password1!");
Console.WriteLine("Admin OK\n");

// 2. Создаём менеджеров -------------------------------------------------------------------------
var managers = new[]
{
    new Manager("LadaManager",  "Manager1!", "Lada"),
    new Manager("BmwManager",   "Manager1!", "Bmw"),
    new Manager("SkodaManager", "Manager1!", "Skoda")
};

foreach (var m in managers)
{
    Console.WriteLine($"→ Создаём пользователя {m.Login}");
    await PostJsonAsync("/api/admin/user", new DataForCreateUser(
        FirstName:  m.Brand + " First",
        LastName:   m.Brand + " Last",
        Login:      m.Login,
        Email:      $"{m.Login.ToLower()}@example.com",
        Password:   m.Password,
        InitialRoles: new[] { "Manager" }
    ), adminToken);
}

if (SINGLE_TEST)
{
    await FillCars(managers[0], adminToken, onlyOne:true);
    return;
}

// 3. Заполняем по 33 авто -----------------------------------------------------------------------
foreach (var m in managers)
    await FillCars(m, adminToken);

Console.WriteLine("\nВсе данные созданы!");
return;

// ────────────────────────────────────────────────────────────────────────────────

async Task FillCars(Manager m, string adminJwt, bool onlyOne = false)
{
    Console.WriteLine($"\n== {m.Login} ({m.Brand}) ==");
    string token = await LoginAsync(m.Login, m.Password);

    int total = onlyOne ? 1 : 33;
    for (int i = 0; i < total; i++)
    {
        var car = new AddCarRequest
        {
            Brand  = m.Brand,
            Color  = RandomFrom("Red","Blue","Green","Black","White","Silver","Yellow","Orange"),
            Price  = rng.Next(5_000, 50_000),
            Mileage       = rng.NextDouble() < 0.5 ? null : rng.Next(300, 30_001),
            CurrentOwner  = rng.NextDouble() < 0.5 ? null : RandomFrom("Ivan","Maria","John","Alice","Olga","Peter")
        };

        await PostCarMultipartAsync(car, token);
        Console.Write(".");
    }
    Console.WriteLine(" done");
}

string RandomFrom(params string[] arr) => arr[rng.Next(arr.Length)];

// ─── Http helpers ──────────────────────────────────────────────────────────────

async Task<string> LoginAsync(string login, string password)
{
    var resp = await http.PostAsync($"/api/auth/login?Login={Uri.EscapeDataString(login)}&Password={Uri.EscapeDataString(password)}", null);
    resp.EnsureSuccessStatusCode();

    var env = await resp.Content.ReadFromJsonAsync<LoginEnvelope>(s_json);
    return env?.Data?.AccessToken ?? throw new("accessToken missing");
}

async Task PostJsonAsync<T>(string url, T body, string? bearer = null)
{
    // Логируем исходящий JSON
    string json = JsonSerializer.Serialize(body, s_json);
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"\nPOST {url}\n{json}\n");
    Console.ResetColor();

    using var req = new HttpRequestMessage(HttpMethod.Post, url)
    {
        Content = JsonContent.Create(body, options: s_json) // JSON с PascalCase
    };
    if (!string.IsNullOrEmpty(bearer))
        req.Headers.Authorization = new("Bearer", bearer);

    var resp = await http.SendAsync(req);
    if (!resp.IsSuccessStatusCode)
    {
        var err = await resp.Content.ReadAsStringAsync();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"HTTP {(int)resp.StatusCode}: {resp.ReasonPhrase}\n{err}\n");
        Console.ResetColor();
        resp.EnsureSuccessStatusCode();
    }
}

async Task PostCarMultipartAsync(AddCarRequest car, string token)
{
    using var form = new MultipartFormDataContent();

    form.Add(new StringContent(car.Brand),  nameof(car.Brand));
    form.Add(new StringContent(car.Color),  nameof(car.Color));
    form.Add(new StringContent(car.Price.ToString()), nameof(car.Price));

    if (car.Mileage is not null)
        form.Add(new StringContent(car.Mileage.ToString()), nameof(car.Mileage));

    if (car.CurrentOwner is not null)
        form.Add(new StringContent(car.CurrentOwner), nameof(car.CurrentOwner));

    // Photo не добавляем → сервер получит null и пропустит

    using var req = new HttpRequestMessage(HttpMethod.Post, "/api/car") { Content = form };
    req.Headers.Authorization = new("Bearer", token);

    var resp = await http.SendAsync(req);
    if (!resp.IsSuccessStatusCode)
    {
        var err = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"HTTP {(int)resp.StatusCode}: {resp.ReasonPhrase}\n{err}\n");
        resp.EnsureSuccessStatusCode();
    }
}

// Унифицированные Json‑настройки (camelCase как на бэке)
// ─── Models (локальные дубликаты DTO) ──────────────────────────────────────────

record Manager(string Login, string Password, string Brand);

record DataForCreateUser(
    string FirstName,
    string LastName,
    string Login,
    string Email,
    string Password,
    IEnumerable<string> InitialRoles);

class AddCarRequest
{
    public required string Brand  { get; init; }
    public required string Color  { get; init; }
    public required decimal Price { get; init; }
    public string? CurrentOwner   { get; init; }
    public int?    Mileage        { get; init; }
    [JsonIgnore]   public object? Photo => null; // заглушка
}

class LoginEnvelope { public LoginData? Data { get; init; } }
class LoginData { public string? AccessToken { get; init; } public string? RefreshToken { get; init; } }
