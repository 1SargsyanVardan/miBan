using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public int DepartmentYear { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string ProgramName { get; set; } = null!;

    public string ModeName { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}
