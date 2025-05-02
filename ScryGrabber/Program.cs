using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace ScryGrabber;

internal class Program
{
  public static string FetchCardInfoEndpoint = "https://api.scryfall.com/cards/named?fuzzy=";

  private static readonly HttpClient Client = new()
  {
    BaseAddress = new Uri("https://api.scryfall.com")
  };

  private static async Task Main()
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

  private static async Task DoTheThings()
  {
    Client.DefaultRequestHeaders.UserAgent.ParseAdd("ScryGrabber/1.0 (+https://yourdomain.com)");
    Client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

    Console.WriteLine("Enter path of MTGO deck .txt file: ");
    var input = string.Empty;
    while (input == string.Empty)
    {
      input = Console.ReadLine() ?? string.Empty;
    }
    input += input.Contains(".txt") ? string.Empty : ".txt";
    

    var lines = await File.ReadAllLinesAsync(input);
    var cardNames = ParseCardNames(lines.ToList());
    var mainboard = new List<CardInfo>();
    var sideboard = new List<CardInfo>();
    var commanders = new List<CardInfo>();

    foreach(var(cardName, section, quantity) in cardNames)
    {
      var cardInfo = await FetchCardInfo(cardName);
      if (cardInfo != null)
      {
        cardInfo.quantity = quantity.ToString();

        switch (section)
        {
          case "mainboard":
            mainboard.Add(cardInfo);
            break;
          case "sideboard":
            sideboard.Add(cardInfo);
            break;
          case "commanders":
            commanders.Add(cardInfo);
            break;
        }
      }
    }

    var outputObject = new
    {
      commanders,
      mainboard,
      sideboard
    };

    var json = JsonConvert.SerializeObject(outputObject, Formatting.Indented);
    var output = input[..input.LastIndexOf(".", StringComparison.Ordinal)] + ".json";
    await File.WriteAllTextAsync(output, json);

    Console.WriteLine("Done! Cards details saved to " + output);
  }

  //TODO - Find a much better way of breaking this down. Not robust and will fail on 60 card decks.
  private static List<(string name, string section, int quantity)> ParseCardNames(List<string> lines)
  {
    var cardNames = new List<(string name, string section, int quantity)>();

    AddCommanders(ref lines, ref cardNames);

    var inSideboard = false;

    foreach (var line in lines)
    {
      var trimmed = line.Trim();

      if (string.IsNullOrEmpty(trimmed))
        continue;

      if (trimmed.StartsWith("SIDEBOARD", StringComparison.OrdinalIgnoreCase))
      {
        inSideboard = true;
        continue;
      }

      var parts = trimmed.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length != 2 || !int.TryParse(parts[0], out var quantity))
      {
        continue;
      }

      var section = inSideboard ? "sideboard" : "mainboard";

      cardNames.Add((parts[1], section, quantity));
    }

    return cardNames;
  }

  private static void AddCommanders(ref List<string> lines, ref List<(string name, string section, int quantity)> cardNames)
  {
    lines.Reverse();
    foreach (var line in lines)
    {
      if (line.Length == 0)
      {
        break;
      }

      var parts = line.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length != 2 || !int.TryParse(parts[0], out _))
      {
        continue;
      }

      cardNames.Add((parts[1], "commanders", 1));
    }

    lines.RemoveRange(0, cardNames.Count);
    lines.Reverse();
  }

  private static async Task<CardInfo?> FetchCardInfo(string name)
  {
    var url = FetchCardInfoEndpoint + Uri.EscapeDataString(name);

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
        toughness = json.Value<string>("toughness") ?? string.Empty,
        loyalty = json.Value<string>("loyalty") ?? string.Empty,
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine($"⚠️ Error fetching {name}: {ex.Message}");
      return null;
    }
  }
}