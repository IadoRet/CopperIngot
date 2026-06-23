using CopperIngot.Engine;

namespace CopperIngot.Tests;

public class SearchEngineTests
{
    [Fact]
    public void Where_FiltersEnumerableByStringProperty()
    {
        var values = new[]
        {
            new SearchItem("Copper"),
            new SearchItem("Iron")
        };

        var request = new Requests.StringSearchRequest(
            nameof(SearchItem.Name),
            "copper",
            Enums.SearchComparison.Equals,
            StringComparison.OrdinalIgnoreCase);

        var result = new SearchEngine()
            .From(values)
            .Where(request)
            .AsEnumerable();

        SearchItem item = Assert.Single(result);
        Assert.Equal("Copper", item.Name);
    }

    private sealed record SearchItem(string Name);
}
