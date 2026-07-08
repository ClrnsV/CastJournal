using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Notifications
{
    public class NotificationFilterDto
    {
        public bool? IsRead { get; set; }   // null = all, true = read only, false = unread only
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
    }
}
