using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.domain.DTO
{
   public record DocumentDTO(IFormFile? File);
}
