using Application.Dto;
using Application.Helpers;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public class MappingProfile : Profile
{
  public MappingProfile()
  {
    #region Project
    CreateMap<Project, ProjectDto>()
      .ReverseMap();
    CreateMap<Project, FetchProjectDto>()
      .ReverseMap();
    #endregion

    #region Experience
    CreateMap<ExperienceDto, Experience>()
      .ForMember(e => e.StartDate, opts => opts.MapFrom(_ => DateHelper.ConvertMonthYearStringToDate(_.StartDate)))
      .ForMember(e => e.EndDate, opts =>
      {
        opts.PreCondition(e => !string.IsNullOrEmpty(e.EndDate));
        opts.MapFrom(_ => DateHelper.ConvertMonthYearStringToDate(_.EndDate!));
      });
    CreateMap<Experience, FetchExperienceDto>()
      .ForMember(e => e.StartDate, opts => opts.MapFrom(_ => DateHelper.ConvertDateToMonthYearString(_.StartDate)))
      .ForMember(e => e.EndDate, opts =>
      {
        opts.PreCondition(e => e.EndDate is not null);
        opts.MapFrom(_ => DateHelper.ConvertDateToMonthYearString((DateTime)_.EndDate!));
      });
    CreateMap<UpdateExperienceDto, Experience>()
      .ForMember(e => e.StartDate, opts =>
      {
        opts.PreCondition(e => !string.IsNullOrEmpty(e.StartDate));
        opts.MapFrom(_ => DateHelper.ConvertMonthYearStringToDate(_.StartDate));
      })
      .ForMember(e => e.EndDate, opts =>
      {
        opts.PreCondition(e => !string.IsNullOrEmpty(e.EndDate));
        opts.MapFrom(_ => DateHelper.ConvertMonthYearStringToDate(_.EndDate!));
      })
      .ReverseMap();
    #endregion

    #region About
    CreateMap<About, AboutDto>()
      .ReverseMap();
    CreateMap<Bio, BioDto>()
      .ReverseMap();
    CreateMap<BioHighlight, BioHighlightDto>()
      .ReverseMap();
    CreateMap<About, FetchAboutDto>();
    #endregion

    #region TechStack
    CreateMap<TechStack, TechStackDto>()
      .ReverseMap();
    CreateMap<TechStack, FetchTechStackDto>();
    #endregion

    #region Contact
    CreateMap<Contact, ContactDto>()
      .ReverseMap();
    CreateMap<Contact, FetchContactDto>();
    #endregion

    #region Education
    CreateMap<Education, EducationDto>()
      .ReverseMap();
    CreateMap<Education, FetchEducationDto>();
    #endregion

    #region Resume
    CreateMap<Resume, ResumeDto>()
      .ReverseMap();
    CreateMap<Resume, FetchResumeDto>();
    #endregion
  }
}