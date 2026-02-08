using Azure.Provisioning.Primitives;

namespace Toxic.Aspire.NamingConventions.NameResolvers;

/// <summary>
/// The default resolver for Azure resource names according to the defined standard naming convention.
/// This is used by default for all resources but can be overridden by registering a type-specific singleton for a particular resource.
/// Produces something like "ca-hgfse-cms-prod-swc" (container app, hgfse project, cms workload, prod environment, sweden central region).
/// </summary>
public class DefaultResourceNameResolver<T> : IResourceNameResolver<T> where T : ProvisionableResource
{
    public Type ResourceType => typeof(T);

    private readonly IEnvironmentNameResolver _environmentNameResolver;

    public DefaultResourceNameResolver(IEnvironmentNameResolver environmentNameResolver)
    {
        _environmentNameResolver = environmentNameResolver;
    }

    public string ResolveName(ProvisionableResource resource, NameResolutionContext context) =>
        ResolveName((T)resource, context);

    public virtual string ResolveName(T resource, NameResolutionContext context)
    {
        // e.g. "ca-hgfse-cms-prod-swc"
        var parts = new List<string?> {
            ResourcePrefixes.GetResourcePrefix(resource),
            context.ProjectName,
            context.AzureWorkloadName,
            _environmentNameResolver.ResolveEnvironmentName(context.EnvironmentName),
            context.SupportsRegion ?
                RegionNames.GetRegionName(context.ResourceRegion ?? context.DefaultRegion) :
                null
        };

        return string.Join(context.Separator, parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
