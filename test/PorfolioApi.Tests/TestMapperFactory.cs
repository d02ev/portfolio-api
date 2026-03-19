using Application.Mapping;
using AutoMapper;

namespace PorfolioApi.Tests;

internal static class TestMapperFactory
{
  private static readonly Lazy<IMapper> Mapper = new(() =>
  {
    var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
    configuration.AssertConfigurationIsValid();
    return configuration.CreateMapper();
  });

  public static IMapper Create() => Mapper.Value;
}
