﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; }
    public string FullName { get; set; }

    public List<Event> Events { get; set; } = new List<Event>();
}