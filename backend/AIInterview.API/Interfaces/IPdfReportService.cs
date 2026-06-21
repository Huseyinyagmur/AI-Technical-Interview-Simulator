using AIInterview.API.DTOs;
namespace AIInterview.API.Interfaces;
public interface IPdfReportService { byte[] Generate(InterviewReportDto report, string userName); }
