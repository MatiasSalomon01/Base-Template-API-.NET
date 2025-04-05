namespace Intrastructure.Abstractions.Models.Searches;

internal class SearchResult
{
    public int Value { get; set; }
    public string Label { get; set; } = string.Empty;

    public SearchResult(){ }
    public SearchResult(int value, string label)
    {
        Value = value;
        Label = label;
    }
}
