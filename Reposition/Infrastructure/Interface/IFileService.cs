using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposition.Infrastructure.Interface
{
    public interface IFileService
    {
        public Task<IStorageFile?> OpenFileAsync();
        public Task<IStorageFile?> SaveFileAsync();
    }
}
