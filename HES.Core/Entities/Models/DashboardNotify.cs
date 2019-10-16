﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HES.Core.Entities.Models
{
    public class DashboardNotify
    {
        public string Message { get; set; }
        public int Count { get; set; }
        public IQueryable Query { get; set; }
    }
}