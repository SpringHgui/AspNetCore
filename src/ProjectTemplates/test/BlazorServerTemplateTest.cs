// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.E2ETesting;
using Templates.Test.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Templates.Test
{
    public class BlazorServerTemplateTest : BlazorTemplateTestBase
    {
        public BlazorServerTemplateTest(ProjectFactoryFixture projectFactory, BrowserFixture browserFixture, ITestOutputHelper output)
            : base(projectFactory, browserFixture, output)
        {
        }

        [Fact]
        public async Task BlazorServerTemplateWorks_NoAuth()
        {
            var project = await ProjectFactory.GetOrCreateProject("blazorservernoauth", Output);

            var createResult = await project.RunDotNetNewAsync("blazorserver");
            Assert.True(0 == createResult.ExitCode, ErrorMessages.GetFailedProcessMessage("create/restore", project, createResult));

            var publishResult = await project.RunDotNetPublishAsync();
            Assert.True(0 == publishResult.ExitCode, ErrorMessages.GetFailedProcessMessage("publish", project, publishResult));

            // Run dotnet build after publish. The reason is that one uses Config = Debug and the other uses Config = Release
            // The output from publish will go into bin/Release/netcoreappX.Y/publish and won't be affected by calling build
            // later, while the opposite is not true.

            var buildResult = await project.RunDotNetBuildAsync();
            Assert.True(0 == buildResult.ExitCode, ErrorMessages.GetFailedProcessMessage("build", project, buildResult));

            await BuildAndRunTest(project);
            await PublishAndRunTest(project);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task BlazorServerTemplateWorks_IndividualAuth(bool useLocalDB)
        {
            var project = await ProjectFactory.GetOrCreateProject("blazorserverindividual" + (useLocalDB ? "uld" : ""), Output);

            var createResult = await project.RunDotNetNewAsync("blazorserver", auth: "Individual", useLocalDB: useLocalDB);
            Assert.True(0 == createResult.ExitCode, ErrorMessages.GetFailedProcessMessage("create/restore", project, createResult));

            var publishResult = await project.RunDotNetPublishAsync();
            Assert.True(0 == publishResult.ExitCode, ErrorMessages.GetFailedProcessMessage("publish", project, publishResult));

            // Run dotnet build after publish. The reason is that one uses Config = Debug and the other uses Config = Release
            // The output from publish will go into bin/Release/netcoreappX.Y/publish and won't be affected by calling build
            // later, while the opposite is not true.

            var buildResult = await project.RunDotNetBuildAsync();
            Assert.True(0 == buildResult.ExitCode, ErrorMessages.GetFailedProcessMessage("build", project, buildResult));

            await BuildAndRunTest(project);
            await PublishAndRunTest(project);
        }
    }
}
