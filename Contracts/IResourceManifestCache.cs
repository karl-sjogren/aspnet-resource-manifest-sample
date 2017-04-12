using System.Collections.Generic;

namespace aspnet_resource_manifest_sample.Contracts {
    public interface IResourceManifestCache {
        Dictionary<string, string> GetManifest();
    }
}