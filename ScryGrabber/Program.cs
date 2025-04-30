using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace ScryGrabber;

class Program
{
  private static readonly HttpClient Client = new()
  {
    BaseAddress = new Uri("https://api.scryfall.com")
  };

  static async Task Main()
  {
    try
    {
      await DoTheThings();
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }
  }

  static async Task DoTheThings()
  {
    Client.DefaultRequestHeaders.UserAgent.ParseAdd("DeckCardFetcher/1.0 (+https://yourdomain.com)");
    Client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

    Console.WriteLine("Enter path of MTGO deck .txt file: ");
    var input = string.Empty;
    while (input == string.Empty)
    {
      input = Console.ReadLine() ?? string.Empty;
    }
    if (input.Contains(".txt") == false)
    {
      input += ".txt";
    }

    var lines = await File.ReadAllLinesAsync(input);
    var cardNames = ParseCardNames(lines);
    var mainboard = new List<CardInfo>();
    var sideboard = new List<CardInfo>();
    var commander = new List<CardInfo>();

    foreach(var(cardName, section) in cardNames)
    {
      var cardInfo = await FetchSingleCard(cardName);
      if (cardInfo != null)
      {
        switch (section)
        {
          case "mainboard":
            mainboard.Add(cardInfo);
            break;
          case "sideboard":
            sideboard.Add(cardInfo);
            break;
          case "commander":
            commander.Add(cardInfo);
            break;
        }
      }
    }

    var outputObject = new
    {
      commander,
      mainboard,
      sideboard
    };

    var json = JsonConvert.SerializeObject(outputObject, Formatting.Indented);
    var output = input[..input.LastIndexOf(".", StringComparison.Ordinal)] + ".json";
    await File.WriteAllTextAsync(output, json);

    Console.WriteLine("Done! Cards details saved to " + output);
  }

  static List<(string name, string section)> ParseCardNames(string[] lines)
  {
    var cardNames = new List<(string name, string section)>();
    var inSideboard = false;

    for (var i = 0; i < lines.Length; i++)
    {
      var trimmed = lines[i].Trim();

      if (string.IsNullOrEmpty(trimmed))
        continue;

      if (trimmed.StartsWith("SIDEBOARD", StringComparison.OrdinalIgnoreCase))
      {
        inSideboard = true;
        continue;
      }

      var parts = trimmed.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length != 2 || !int.TryParse(parts[0], out _))
      {
        continue;
      }

      // If it's the last valid line, treat it as the commander
      var isLastValidLine = i == lines.Length - 1 ||
                            lines.Skip(i + 1).All(string.IsNullOrWhiteSpace);

      var section = isLastValidLine ? "commander" : (inSideboard ? "sideboard" : "mainboard");

      cardNames.Add((parts[1], section));
    }

    return cardNames;
  }

  static async Task<CardInfo?> FetchSingleCard(string name)
  {
    var url = $"https://api.scryfall.com/cards/named?fuzzy={Uri.EscapeDataString(name)}";

    try
    {
      var response = await Client.GetAsync(url);
      var body = await response.Content.ReadAsStringAsync();

      if (!response.IsSuccessStatusCode)
      {
        Console.WriteLine($"❌ Failed: {name} ({response.StatusCode})\n{body}");
        return null;
      }

      var json = JObject.Parse(body);

      return new CardInfo
      {
        name = json.Value<string>("name") ?? string.Empty,
        mana_cost = json.Value<string>("mana_cost") ?? string.Empty,
        cmc = json.Value<float>("cmc"),
        colors = json["colors"]?.ToObject<List<string>>() ?? [],
        color_identity = json["color_identity"]?.ToObject<List<string>>() ?? [],
        type_line = json.Value<string>("type_line") ?? string.Empty,
        oracle_text = json.Value<string>("oracle_text") ?? string.Empty,
        keywords = json["keywords"]?.ToObject<List<string>>() ?? [],
        power = json.Value<string>("power") ?? string.Empty,
        toughness = json.Value<string>("toughness") ?? string.Empty
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine($"⚠️ Error fetching {name}: {ex.Message}");
      return null;
    }
  }
}