using System;
using System.Collections.Generic;

namespace hr_crm.Entities;

public partial class TodoTask
{
    public int TaskId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int AssignedTo { get; set; }

    public DateOnly DueDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Employee AssignedToNavigation { get; set; } = null!;
}
