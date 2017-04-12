using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using aspnet_resource_manifest_sample.Contracts;

namespace aspnet_resource_manifest_sample.Cache {
    public class ResourceManifestCache : IResourceManifestCache {
        private readonly IFileProvider _fileProvider;
        private Dictionary<string, string> _manifest;
        private bool _initialized;

        public ResourceManifestCache(IHostingEnvironment hostingEnvironment) {
            _fileProvider = hostingEnvironment.WebRootFileProvider;
        }

        private void Initialize() {
            if (_initialized)
                return;
            _initialized = true;

            var fi = _fileProvider.GetFileInfo("rev-manifest.json");
            if (!fi.Exists) {
                _manifest = new Dictionary<string, string>();
                return;
            }

            string content;
            using (var sr = new StreamReader(fi.CreateReadStream()))
                content = sr.ReadToEnd();

            try {
                _manifest = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            } catch {
                _manifest = new Dictionary<string, string>();
            }
        }


        public Dictionary<string, string> GetManifest() {
            Initialize();
            return _manifest;
        }
    }
}