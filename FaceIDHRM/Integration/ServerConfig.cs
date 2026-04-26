using System;

namespace FaceIDHRM.Integration
{
    public static class ServerConfig
    {
        public static string ApprovalServerUrl =>
            Environment.GetEnvironmentVariable("FACEID_SERVER_URL")?.TrimEnd('/')
            ?? "http://localhost:5055";

        public static bool UseServerSync =>
            (Environment.GetEnvironmentVariable("FACEID_USE_SERVER_SYNC") ?? "true").Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
