using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Group
{
    public int GroupId { get; set; }

    public int GroupNumber { get; set; }

    public int DepartmentId { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<GroupCourse> GroupCourses { get; set; } = new List<GroupCourse>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
