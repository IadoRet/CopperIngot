using CopperIngot.Engine;
using CopperIngot.Enums;
using CopperIngot.Models;
using CopperIngot.Requests;

namespace CopperIngot.Tests;

public class SearchEngineBehaviorTests
{
    [Fact]
    public void Where_FiltersEnumerableByExactStringValue()
    {
        var result = new SearchEngine()
            .From(CreateCatalog())
            .Where(new StringSearchRequest(
                nameof(CatalogItem.Name),
                "Copper Ingot",
                SearchComparison.Equals))
            .AsEnumerable();

        CatalogItem item = Assert.Single(result);
        Assert.Equal("Copper Ingot", item.Name);
    }

    [Fact]
    public void Where_FiltersEnumerableByStringValueIgnoringCase()
    {
        var result = new SearchEngine()
            .From(CreateCatalog())
            .Where(new StringSearchRequest(
                nameof(CatalogItem.Name),
                "copper ingot",
                SearchComparison.Equals,
                StringComparison.OrdinalIgnoreCase))
            .AsEnumerable();

        CatalogItem item = Assert.Single(result);
        Assert.Equal("Copper Ingot", item.Name);
    }

    [Fact]
    public void Where_FiltersEnumerableByStringContainsIgnoringCase()
    {
        var result = new SearchEngine()
            .From(CreateCatalog())
            .Where(new StringSearchRequest(
                nameof(CatalogItem.Name),
                "copper",
                SearchComparison.Contains,
                StringComparison.OrdinalIgnoreCase))
            .AsEnumerable()
            .Select(item => item.Name);

        Assert.Equal(["Copper Ingot", "Copper Wire"], result);
    }

    [Fact]
    public void Where_FiltersEnumerableByPrimitiveComparison()
    {
        var result = new SearchEngine()
            .From(CreateCatalog())
            .Where(new ObjectSearchRequest(
                nameof(CatalogItem.Stock),
                50,
                SearchComparison.GreaterOrEqual))
            .AsEnumerable()
            .Select(item => item.Name);

        Assert.Equal(["Copper Ingot", "Iron Ingot"], result);
    }

    [Fact]
    public void Where_ChainsMultipleFilters()
    {
        var result = new SearchEngine()
            .From(CreateCatalog())
            .Where(new StringSearchRequest(
                nameof(CatalogItem.Name),
                "ingot",
                SearchComparison.Contains,
                StringComparison.OrdinalIgnoreCase))
            .Where(new ObjectSearchRequest(
                nameof(CatalogItem.Stock),
                50,
                SearchComparison.GreaterOrEqual))
            .AsEnumerable()
            .Select(item => item.Name);

        Assert.Equal(["Copper Ingot", "Iron Ingot"], result);
    }

    [Fact]
    public void FirstOrDefault_ReturnsFirstMatchingObject()
    {
        CatalogItem? result = new SearchEngine()
            .From(CreateCatalog())
            .FirstOrDefault(new ObjectSearchRequest(
                nameof(CatalogItem.Stock),
                10,
                SearchComparison.Less));

        Assert.NotNull(result);
        Assert.Equal("Tin Nugget", result.Name);
    }

    [Fact]
    public void Where_FiltersEnumerableByNestedPropertyPath()
    {
        var result = new SearchEngine()
            .From(CreateCatalog())
            .Where(new StringSearchRequest(
                $"{nameof(CatalogItem.Category)}.{nameof(Category.Name)}",
                "Metal",
                SearchComparison.Equals))
            .AsEnumerable()
            .Select(item => item.Name);

        Assert.Equal(["Copper Ingot", "Iron Ingot", "Copper Wire"], result);
    }

    [Fact]
    public void Where_NestedPropertyPathIgnoresNullIntermediateObjects()
    {
        var result = new SearchEngine()
            .From(CreateCatalog())
            .Where(new StringSearchRequest(
                $"{nameof(CatalogItem.Category)}.{nameof(Category.Name)}",
                "Unknown",
                SearchComparison.Equals))
            .AsEnumerable();

        Assert.Empty(result);
    }

    [Fact]
    public void Where_FiltersEnumerableByConfiguredAlias()
    {
        var categoryAlias = new SearchAlias("category")
            .WithPropertyOf<CatalogItem>(nameof(CatalogItem.Category))
            .WithPropertyOf<Category>(nameof(Category.Name));

        var searchEngine = new SearchEngine(new SearchEngineConfiguration()
            .WithAlias(categoryAlias)
            .UseAliases());

        var result = searchEngine
            .From(CreateCatalog())
            .Where(new StringSearchRequest(
                "category",
                "Metal",
                SearchComparison.Equals))
            .AsEnumerable()
            .Select(item => item.Name);

        Assert.Equal(["Copper Ingot", "Iron Ingot", "Copper Wire"], result);
    }

    [Fact]
    public void Where_FiltersGuidPropertyWithDefaultBuilders()
    {
        Guid id = Guid.Parse("c97be8de-d933-4bc6-a722-2e7719258867");

        var result = new SearchEngine(new SearchEngineConfiguration().WithDefaultBuilders())
            .From(CreateCatalog())
            .Where(new StringSearchRequest(
                nameof(CatalogItem.Id),
                id.ToString(),
                SearchComparison.Equals))
            .AsEnumerable();

        CatalogItem item = Assert.Single(result);
        Assert.Equal("Copper Ingot", item.Name);
    }

    [Fact]
    public void Where_FiltersDateOnlyPropertyWithDefaultBuilders()
    {
        var result = new SearchEngine(new SearchEngineConfiguration().WithDefaultBuilders())
            .From(CreateCatalog())
            .Where(new StringSearchRequest(
                nameof(CatalogItem.AvailableFrom),
                "2024-01-01",
                SearchComparison.GreaterOrEqual))
            .AsEnumerable()
            .Select(item => item.Name);

        Assert.Equal(["Copper Ingot", "Copper Wire", "Tin Nugget"], result);
    }

    [Fact]
    public void QueryableWhere_HonorsStringComparisonLikeEnumerable()
    {
        var result = new SearchEngine()
            .From(CreateCatalog().AsQueryable())
            .Where(new StringSearchRequest(
                nameof(CatalogItem.Name),
                "copper ingot",
                SearchComparison.Equals,
                StringComparison.OrdinalIgnoreCase))
            .AsQueryable()
            .Select(item => item.Name);

        Assert.Equal(["Copper Ingot"], result);
    }

    [Fact]
    public void QueryableWhere_HonorsStringContainsComparisonLikeEnumerable()
    {
        var result = new SearchEngine()
            .From(CreateCatalog().AsQueryable())
            .Where(new StringSearchRequest(
                nameof(CatalogItem.Name),
                "copper",
                SearchComparison.Contains,
                StringComparison.OrdinalIgnoreCase))
            .AsQueryable()
            .Select(item => item.Name);

        Assert.Equal(["Copper Ingot", "Copper Wire"], result);
    }

    [Fact]
    public void QueryableWhere_HonorsStringNotEqualsComparisonLikeEnumerable()
    {
        var result = new SearchEngine()
            .From(CreateCatalog().AsQueryable())
            .Where(new StringSearchRequest(
                nameof(CatalogItem.Name),
                "copper ingot",
                SearchComparison.NotEquals,
                StringComparison.OrdinalIgnoreCase))
            .AsQueryable()
            .Select(item => item.Name);

        Assert.Equal(["Iron Ingot", "Copper Wire", "Tin Nugget"], result);
    }

    private static CatalogItem[] CreateCatalog()
    {
        return
        [
            new CatalogItem(
                "Copper Ingot",
                100,
                new Category("Metal"),
                Guid.Parse("c97be8de-d933-4bc6-a722-2e7719258867"),
                new DateOnly(2024, 1, 1)),
            new CatalogItem(
                "Iron Ingot",
                50,
                new Category("Metal"),
                Guid.Parse("e3cf3257-7d8d-4444-b604-72914803076a"),
                new DateOnly(2023, 6, 15)),
            new CatalogItem(
                "Copper Wire",
                25,
                new Category("Metal"),
                Guid.Parse("686f33c1-5139-4baa-86e2-e25b7f9539c4"),
                new DateOnly(2024, 3, 10)),
            new CatalogItem(
                "Tin Nugget",
                5,
                null,
                Guid.Parse("c9dfcf5f-06fe-439d-a6d0-538bc759ca4a"),
                new DateOnly(2025, 5, 20))
        ];
    }

    private sealed record CatalogItem(
        string Name,
        int Stock,
        Category? Category,
        Guid Id,
        DateOnly AvailableFrom);

    private sealed record Category(string Name);
}
