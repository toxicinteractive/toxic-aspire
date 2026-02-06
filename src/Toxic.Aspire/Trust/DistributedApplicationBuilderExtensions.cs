#pragma warning disable ASPIRECERTIFICATES001

using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace Toxic.Aspire.Trust;

public static class DistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        /// Looks for a custom HTTPS certificate and custom CA certificate and adds them to the following resources in run mode: projects, containers, JavaScript apps.
        /// Both custom certificates are optional and this method will do nothing if neither exists.
        /// 
        /// The custom HTTPS certificate should be PFX and be specified with CUSTOM_CERT_FILE or ASPNETCORE_Kestrel__Certificates__Default__Path.
        /// The custom HTTPS certificate password should be specified with CUSTOM_CERT_PASSWORD or ASPNETCORE_Kestrel__Certificates__Default__Password.
        /// The custom CA certificate should be PEM and be specified with CUSTOM_CA_FILE.
        /// </summary>
        internal IDistributedApplicationBuilder WithCustomHttpsCertificates()
        {
            X509Certificate2? customCert = null;
            IResourceBuilder<CertificateAuthorityCollection>? caCollection = null;

            var customCertFileName = builder.GetFirstValidEnvironmentValue([
                "CUSTOM_CERT_FILE", 
                "ASPNETCORE_Kestrel__Certificates__Default__Path"
            ]);

            var customCertPassword = builder.GetFirstValidEnvironmentValue([
                "CUSTOM_CERT_PASSWORD", 
                "ASPNETCORE_Kestrel__Certificates__Default__Password"
            ]);

            var customCaFileName = builder.Configuration.GetValue<string>("CUSTOM_CA_FILE");
        
            if (!string.IsNullOrWhiteSpace(customCertFileName))
            {
                customCert = X509CertificateLoader.LoadPkcs12FromFile(customCertFileName, customCertPassword);
            }

            if (!string.IsNullOrWhiteSpace(customCaFileName))
            {
                caCollection = builder
                    .AddCertificateAuthorityCollection("custom-ca")
                    .WithCertificatesFromFile(customCaFileName);
            }

            // projects (when running in a devcontainer)
            foreach (var project in builder.Resources.OfType<ProjectResource>())
            {
                builder.ConfigureResourceCerts(project, customCert, caCollection);
            }

            // containers
            foreach (var container in builder.Resources.OfType<ContainerResource>())
            {
                builder.ConfigureResourceCerts(container, customCert, caCollection);
            }

            // executables (javascript apps for example)
            foreach (var app in builder.Resources.OfType<ExecutableResource>())
            {
                builder.ConfigureResourceCerts(app, customCert, caCollection);
            }

            return builder;
        }

        private void ConfigureResourceCerts<TResource>(
            TResource resource,
            X509Certificate2? customCert,
            IResourceBuilder<CertificateAuthorityCollection>? caCollection) 
            where TResource : IResourceWithArgs, IResourceWithEnvironment
        {
            var resourceBuilder = builder.CreateResourceBuilder(resource);

            if (customCert != null)
            {
                resourceBuilder.WithHttpsCertificate(customCert);
            }

            if (caCollection != null)
            {
                resourceBuilder.WithCertificateAuthorityCollection(caCollection);
            }
        }

        // TODO: https://github.com/dotnet/roslyn/issues/80024
        // private string? GetFirstValidConfigurationValue(params string[] keys)
        private string? GetFirstValidEnvironmentValue(List<string> keys)
        {
            foreach (var key in keys.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var value = Environment.GetEnvironmentVariable(key!);

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }
    }
}
