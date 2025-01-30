using System;
using System.Collections.Generic;

namespace CRUDDatabaseFirst.Model;

public partial class StudentInfo
{
    public int StudentId { get; set; }

    public string? StudentName { get; set; }

    public string? StudentRollNumber { get; set; }

    public string? StudentSection { get; set; }

    public string? StudentEmail { get; set; }
}
