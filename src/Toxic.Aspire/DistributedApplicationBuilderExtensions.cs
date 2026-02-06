using Toxic.Aspire.Codespaces;
using Toxic.Aspire.Health;
using Toxic.Aspire.Trust;

namespace Toxic.Aspire;

/// <summary>
/// Extensions for the App Host application builder.
/// </summary>
public static class DistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        /// Adds opinionated services and options provided by the Toxic Aspire toolkit.
        /// </summary>
        public IDistributedApplicationBuilder WithToxicDefaults()
        {
            builder
                .WithCustomHttpsCertificates()
                .WithSecureHealthChecks()
                .WithCodespacesSupport();

            return builder;
        }
    }
}
