using FluentAssertions;
using SocialHub.Application.Common.Specifications;
using Xunit;
 
namespace SocialHub.Application.Tests.Common.Specifications;
 
public class SpecificationEvaluatorTests
{
    private sealed record Widget(int Id, string Name, bool IsActive);
 
    private sealed class ActiveWidgetsSpecification : BaseSpecification<Widget>
    {
        public ActiveWidgetsSpecification()
            : base(w => w.IsActive)
        {
            ApplyOrderBy(w => w.Name);
        }
    }
 
    private sealed class PagedWidgetsSpecification : BaseSpecification<Widget>
    {
        public PagedWidgetsSpecification(int skip, int take)
        {
            ApplyOrderBy(w => w.Id);
            ApplyPaging(skip, take);
        }
    }
 
    private static IQueryable<Widget> SampleData() => new List<Widget>
    {
        new(1, "Charlie", true),
        new(2, "Alpha", true),
        new(3, "Bravo", false),
        new(4, "Delta", true)
    }.AsQueryable();
 
    [Fact]
    public void GetQuery_Should_ApplyCriteriaAndOrdering()
    {
        var spec = new ActiveWidgetsSpecification();
 
        var result = SpecificationEvaluator<Widget>.GetQuery(SampleData(), spec).ToList();
 
        result.Should().HaveCount(3);
        result.Select(w => w.Name).Should().ContainInOrder("Alpha", "Charlie", "Delta");
        result.Should().OnlyContain(w => w.IsActive);
    }
 
    [Fact]
    public void GetQuery_Should_ApplyPaging()
    {
        var spec = new PagedWidgetsSpecification(skip: 1, take: 2);
 
        var result = SpecificationEvaluator<Widget>.GetQuery(SampleData(), spec).ToList();
 
        result.Should().HaveCount(2);
        result.Select(w => w.Id).Should().ContainInOrder(2, 3);
    }
 
    [Fact]
    public void GetQuery_WithoutPaging_Should_ReturnAllMatches()
    {
        var spec = new ActiveWidgetsSpecification();
 
        var result = SpecificationEvaluator<Widget>.GetQuery(SampleData(), spec).ToList();
 
        result.Should().HaveCount(3);
    }
}