using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventOrganizerModel
{
    public interface IUser
    {
        string Login { get; set; }
        string FullName { get; set; }
    }
}