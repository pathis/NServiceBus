namespace NServiceBus
{
    using NServiceBus.StaticHeaders;

    /// <summary>
    /// Extensions to the public configuration api
    /// </summary>
    public static class StaticHeadersConfigExtensions
    {
        /// <summary>
        /// Allows static outgoing headers to be set
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="key">The header key</param>
        /// <param name="value">The header value</param>
        public static void SetOutgoingHeader(this BusConfiguration config, string key, string value)
        {
            Guard.AgainstNullAndEmpty(key, "key");

            StaticOutgoingHeaders headers;

            if (!config.Settings.TryGet(out headers))
            {
                headers = new StaticOutgoingHeaders();

                config.Settings.Set<StaticOutgoingHeaders>(headers);
            }


            headers[key] =  value;
        }
    }
}