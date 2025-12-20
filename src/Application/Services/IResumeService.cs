using Application.Dto;
using Application.Responses;

namespace Application.Services;

public interface IResumeService
{
  Task<CreateResourceResponse> CreateResume(ResumeDto resumeDto);

  Task<FetchResourceResponse<FetchResumeDto>> FetchResume();

  Task<UpdateResourceResponse<IDictionary<string, object>>> UpdateResume(string resumeId, UpdateResumeDto updateResumeDto);

  Task<CreateResourceResponse<IDictionary<string, long>>> GenerateResume(GenerateResumeDto generateResumeDto);

  Task<ResumeJobRunResponse> FetchResumeJobRunStatus(long jobId);

  Task<FetchResourceResponse<IDictionary<string, string>>> FetchLatestResumePdfUrl();
}