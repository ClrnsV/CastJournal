using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Backup;
public class CleanupResultDto
{
    public int DeletedCatchesPurged { get; set; }
    public int ExpiredTokensPurged { get; set; }
    public int OldAuditLogsPurged { get; set; }
}