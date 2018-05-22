using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SandboxCrash.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private static Lazy<CloudBlobContainer> _containerLazy = new Lazy<CloudBlobContainer>(() =>
        {
            var account = CloudStorageAccount.Parse("PUT STORAGE CONNECTION STRING HERE");
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("sandboxcrash");
            container.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            return container;
        });

        private static CloudBlobContainer _container = _containerLazy.Value;

        [HttpPost("{count}/{delay}")]
        public async Task Post(int count, int delay)
        {
            var cts = new CancellationTokenSource();
            var tasks = new List<Task>();

            for (int i = 0; i < count; i++)
            {
                var blob = _container.GetBlockBlobReference(Guid.NewGuid().ToString());
                tasks.Add(blob.UploadTextAsync(i.ToString(), null, null, null, null, cts.Token));
            }

            await Task.Delay(delay);
            cts.Cancel();

            try
            {
                await Task.WhenAll(tasks);
            }
            catch
            {
            }
        }
    }
}
