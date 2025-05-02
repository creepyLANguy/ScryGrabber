class CardInfo
{
  public string name { get; set; }
  public string mana_cost { get; set; }
  public float cmc { get; set; }
  public List<string> colors { get; set; }
  public List<string> color_identity { get; set; }
  public string type_line { get; set; }
  public string oracle_text { get; set; }
  public List<string> keywords { get; set; }
  public string power { get; set; }
  public string toughness { get; set; }
  public string loyalty { get; set; }
  public string quantity { get; set; }
}