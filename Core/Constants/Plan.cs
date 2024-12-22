namespace Service.Core.Constants;

public class Feature
{
  public string Description { get; set; }
  public string Icon { get; set; }
  public string Color { get; set; }
}

public class Plan
{
  public string Title { get; set; }
  public string Description { get; set; }
  public string MonthlyPrice { get; set; }
  public string AnnualPrice { get; set; }
  public string Price { get; set; }
  public string ButtonText { get; set; }
  public string ButtonColor { get; set; }
  public string BackgroundColor { get; set; }
  public string TitleColor { get; set; }
  public string DescriptionColor { get; set; }
  public string PriceColor { get; set; }
  public List<Feature> Features { get; set; }
}
