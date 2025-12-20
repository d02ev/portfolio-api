using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Domain.Entities.Postgres;

[Table("resume_jobs")]
public class ResumeJob : BaseModel
{
  [PrimaryKey("id", false)]
  public long Id { get; set; }

  [Column("status")]
  public string Status { get; set; } = "pending";

  [Column("pdf_url")]
  public string? PdfUrl { get; set; } = null;

  [Column("error")]
  public string? Error { get; set; } = null;

  [Column("created_at")]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  [Column("updated_at")]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}