using System;
using System.Configuration;
using System.Collections.Generic;

namespace TeamFlash
{
    public class BuildServer
    {
        private readonly string serverUrl;
        private readonly string username;
        private readonly string password;

        public BuildServer()
        {
            serverUrl = ConfigurationManager.AppSettings["serverUrl"];
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];
        }

        public BuildStatus GetLastBuildStatus(out List<string> buildTypeNames)
        {
            dynamic query = new Query(serverUrl, username, password);
            buildTypeNames = null;

            var buildStatus = BuildStatus.Passed;

            try
            {
                foreach (var project in query.Projects)
                {
                    if (!project.BuildTypesExists)
                    {
                        continue;
                    }
                    foreach (var buildType in project.BuildTypes)
                    {
                        if ("true".Equals(buildType.Paused, StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }
                        var builds = buildType.Builds;
                        var latestBuild = builds.First;
                        if (latestBuild == null)
                        {
                            continue;
                        }

                        if ("success".Equals(latestBuild.Status, StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }

                        var isUnstableBuild = false;
                        foreach (var property in latestBuild.Properties)
                        {
                            if ("system.BuildState".Equals(property.Name, StringComparison.CurrentCultureIgnoreCase) &&
                            "unstable".Equals(property.Value, StringComparison.CurrentCultureIgnoreCase))
                            {
                                isUnstableBuild = true;
                            }

                            if ("BuildState".Equals(property.Name, StringComparison.CurrentCultureIgnoreCase) &&
                            "unstable".Equals(property.Value, StringComparison.CurrentCultureIgnoreCase))
                            {
                                isUnstableBuild = true;

                            }
                        }

                        if (isUnstableBuild)
                        {
                            continue;
                        }

                        var buildId = buildType.Id;
                        dynamic investigationQuery = new Query(serverUrl, username, password);
                        investigationQuery.RestBasePath = @"/httpAuth/app/rest/buildTypes/id:" + buildId + @"/";
                        buildStatus = BuildStatus.Failed;

                        foreach (var investigation in investigationQuery.Investigations)
                        {
                            var investigationState = investigation.State;
                            if ("taken".Equals(investigationState, StringComparison.CurrentCultureIgnoreCase) ||
                                "fixed".Equals(investigationState, StringComparison.CurrentCultureIgnoreCase))
                            {
                                buildStatus = BuildStatus.Investigating;
                            }
                        }

                        if (buildStatus == BuildStatus.Failed)
                        {
                            return buildStatus;
                        }
                    }

                }
            }
            catch (Exception)
            {
                return BuildStatus.Unavailable;
            }

            return buildStatus;
        }
    }
}

