using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Media;

public class UploadMediaDto
{
    public Guid CatchId { get; set; }
    public IFormFile File { get; set; } = null!;
}