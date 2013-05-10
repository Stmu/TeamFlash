﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;

namespace TeamFlash
{
    class Program
    {
        static void Main(string[] args)
        {
            var teamFlashConfig = LoadConfig();

            teamFlashConfig.ServerUrl = ReadConfig("TeamCity URL", teamFlashConfig.ServerUrl);
            teamFlashConfig.Username = ReadConfig("Username", teamFlashConfig.Username);
            var password = ReadConfig("Password", "");
            teamFlashConfig.BuildTypeIds = ReadConfig("Comma separated build type ids (eg, \"bt64,bt12\"), or * for all", teamFlashConfig.BuildTypeIds);

            SaveConfig(teamFlashConfig);

            Console.Clear();

            var buildTypeIds = teamFlashConfig.BuildTypeIds == "*"
                ? new string[0]
                : teamFlashConfig.BuildTypeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            IBuildLight buildLight;
            if (args.Length == 0)
            {
#if __MonoCS__
                buildLight = new Linux.BuildLight();
#else
                buildLight = new BuildLight();
#endif
            }
            else
            {
                var type = Assembly.GetExecutingAssembly().GetType(args[0]);
                var constructor = type.GetConstructor(new Type[0]);
                buildLight = constructor.Invoke(new object[0]) as IBuildLight;
            }

            buildLight.Off();

            while (!Console.KeyAvailable)
            {
                var lastBuildStatus = RetrieveBuildStatus(
                    teamFlashConfig.ServerUrl,
                    teamFlashConfig.Username,
                    password,
                    buildTypeIds);
                switch (lastBuildStatus)
                {
                    case BuildStatus.Unavailable:
                        buildLight.Off();
                        Console.WriteLine(DateTime.Now.ToShortTimeString() + " Server unavailable");
                        break;
                    case BuildStatus.Passed:
                        buildLight.Success();
                        Console.WriteLine(DateTime.Now.ToShortTimeString() + " Passed");
                        break;
                    case BuildStatus.Investigating:
                        buildLight.Warning();
                        Console.WriteLine(DateTime.Now.ToShortTimeString() + " Investigating");
                        break;
                    case BuildStatus.Failed:
                        buildLight.Fail();
                        Console.WriteLine(DateTime.Now.ToShortTimeString() + " Failed");
                        break;
                }

                Wait();
            }

            buildLight.Off();
        }

        static TeamFlashConfig LoadConfig()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var configFilePath = Path.Combine(appDataPath, @"TeamFlash\config.json");
            try
            {
                if (!File.Exists(configFilePath))
                    return new TeamFlashConfig();

                var serializer = new XmlSerializer(typeof(TeamFlashConfig));
                using (var stream = File.OpenRead(configFilePath))
                {
                    return (TeamFlashConfig)serializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following exception occurred loading the config file \"{0}\":{2}Message: {1}{2}Feel free to quit and fix it or re-enter your server details...", configFilePath, ex.Message, Environment.NewLine);
            }
            return new TeamFlashConfig();
        }

        static void SaveConfig(TeamFlashConfig config)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var teamFlashPath = Path.Combine(appDataPath, @"TeamFlash\");
            if (!Directory.Exists(teamFlashPath))
                Directory.CreateDirectory(teamFlashPath);

            var configFilePath = Path.Combine(teamFlashPath, @"config.json");

            var serializer = new XmlSerializer(typeof(TeamFlashConfig));
            using (var stream = File.OpenWrite(configFilePath))
            {
                serializer.Serialize(stream, config);
            }
        }

        static string ReadConfig(string name, string previousValue)
        {
            string input = null;
            while (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("{0}?", name);
                if (!string.IsNullOrEmpty(previousValue))
                {
                    Console.WriteLine("(press enter for previous value: {0})", previousValue);
                }
                input = Console.ReadLine();
                if (!string.IsNullOrEmpty(previousValue) &&
                    string.IsNullOrEmpty(input))
                {
                    input = previousValue;
                }
                Console.WriteLine();
            }
            return input;
        }

        static void Wait()
        {
            var delayCount = 0;
            while (delayCount < 30 &&
                !Console.KeyAvailable)
            {
                delayCount++;
                Thread.Sleep(1000);
            }
        }

        static BuildStatus RetrieveBuildStatus(
            string serverUrl, string username, string password,
            IEnumerable<string> buildTypeIds)
        {
            buildTypeIds = buildTypeIds.ToArray();

            dynamic query = new Query(serverUrl, username, password);

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
                        if (buildTypeIds.Any() &&
                            buildTypeIds.All(id => id != buildType.Id))
                        {
                            continue;
                        }
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
                            dynamic runningBuild = new Query(serverUrl, username, password) { RestBasePath = string.Format("/httpAuth/app/rest/buildTypes/id:{0}/builds/running:any", buildType.Id) };

                            runningBuild.Load();
                            if ("success".Equals(runningBuild.Status, StringComparison.CurrentCultureIgnoreCase))
                            {
                                continue;
                            }
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
