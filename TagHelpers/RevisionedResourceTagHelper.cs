using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using aspnet_resource_manifest_sample.Contracts;

namespace aspnet_resource_manifest_sample.TagHelpers {
    [HtmlTargetElement("script", Attributes = ScriptSrcAttributeName)]
    [HtmlTargetElement("link", Attributes = LinkHrefAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class RevisionedResourceTagHelper : UrlResolutionTagHelper {
        private readonly IResourceManifestCache _manifestCache;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IFileProvider _fileProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RevisionedResourceTagHelper(IUrlHelperFactory urlHelper, HtmlEncoder htmlEncoder, IResourceManifestCache manifestCache, IHostingEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor) : base(urlHelper, htmlEncoder) {
            _manifestCache = manifestCache;
            _hostingEnvironment = hostingEnvironment;
            _fileProvider = hostingEnvironment.WebRootFileProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        private const string ScriptSrcAttributeName = "src";
        private const string LinkHrefAttributeName = "href";

        public override void Process(TagHelperContext context, TagHelperOutput output) {
            base.Process(context, output);

            var httpContext = _httpContextAccessor.HttpContext;

            foreach (var attrName in new[] { ScriptSrcAttributeName, LinkHrefAttributeName }) {
                try {
                    var manifest = _manifestCache.GetManifest();
                    var attr = output.Attributes.FirstOrDefault(attribute => attribute.Name == attrName);
                    if (!(attr?.Value is HtmlString))
                        continue;

                    var attrValue = ((HtmlString)attr.Value).ToString();

                    if (attrValue.Contains("://"))
                        continue;

                    var fi = new FileInfo(attrValue);
                    string manifestEntry;
                    if (!manifest.TryGetValue(fi.Name, out manifestEntry))
                        return;

                    attrValue = attrValue.Replace(fi.Name, manifestEntry);

                    // TODO We should cache which files exist so we don't have to call GetFileInfo all the time
                    var file = _fileProvider.GetFileInfo(attrValue + ".br");
                    if(file.Exists && httpContext.Request.Headers["Accept-Encoding"].Any(header => header.Contains("br")))
                        attrValue += ".br";

                    file = _fileProvider.GetFileInfo(attrValue + ".gz");
                    if(file.Exists && httpContext.Request.Headers["Accept-Encoding"].Any(header => header.Contains("gzip")))
                        attrValue += ".gz";

                    output.Attributes.SetAttribute(attr.Name, new TagHelperAttribute(attrValue));
                } catch {
                    // Let the dragons roam free...
                }
            }
        }
    }
}
