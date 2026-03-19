using Application.Helpers;
using FluentAssertions;

namespace PorfolioApi.Tests;

public class HelperTests
{
  [Fact]
  public void ConvertMonthYearStringToDateAndBack_ShouldRoundTrip()
  {
    var date = DateHelper.ConvertMonthYearStringToDate("Jan 2024");
    var formatted = DateHelper.ConvertDateToMonthYearString(date);

    date.Should().Be(new DateTime(2024, 1, 16));
    formatted.Should().Be("Jan 2024");
  }

  [Fact]
  public void BuildUpdateObject_ShouldExcludeNullsAndDefaultValueTypes()
  {
    var source = new TestUpdateModel
    {
      Name = "Updated",
      Count = 0,
      Tags = ["api"]
    };

    var result = UpdateObjectBuilderHelper.BuildUpdateObject(source);

    result.Keys.Should().BeEquivalentTo(["Name", "Tags"]);
    result["Name"].Should().Be("Updated");
    ((List<string>)result["Tags"]).Should().BeEquivalentTo(["api"]);
  }

  private sealed class TestUpdateModel
  {
    public string? Name { get; set; }

    public int Count { get; set; }

    public List<string>? Tags { get; set; }

    public string? Missing { get; set; }
  }
}
