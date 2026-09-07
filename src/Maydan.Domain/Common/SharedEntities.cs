namespace Maydan.Domain.Common;

public abstract class SharedEntities
{
    public int Id { get; set; }
    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; }

}
