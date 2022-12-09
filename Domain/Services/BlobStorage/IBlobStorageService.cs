using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.BlobStorage;
public interface IBlobStorageService
{
	Task<Uri> SaveImage(int projectId, byte[] file, string name);
}
