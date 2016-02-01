using System;

namespace Net.DDP.Client
{
    public static class Validations
    {
        public static void ValidateUrl (string url)
        {
            // TODO Strengthen this validation. The default Uri.IsWellFormedUriString won't handle ws/wss schemas.
            if (string.IsNullOrWhiteSpace (url)
                || (!url.StartsWith ("ws://") && !url.StartsWith ("wss://"))) {
                throw new ArgumentException ("Invalid url: " + url, "url");
            }
        }
    }
}

