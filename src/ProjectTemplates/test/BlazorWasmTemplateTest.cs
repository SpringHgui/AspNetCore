// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.E2ETesting;
using Templates.Test.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Templates.Test
{
    public class BlazorWasmTemplateTest : BlazorTemplateTestBase
    {
        public BlazorWasmTemplateTest(ProjectFactoryFixture projectFactory, BrowserFixture browserFixture, ITestOutputHelper output)
            : base(projectFactory, browserFixture, output)
        {
        }

        [Fact]
        public async Task BlazorWasmStandaloneTemplate_Works()
        {
            var project = await ProjectFactory.GetOrCreateProject("blazorstandalone", Output);

            var createResult = await project.RunDotNetNewAsync("blazorwasm");
            Assert.True(0 == createResult.ExitCode, ErrorMessages.GetFailedProcessMessage("create/restore", project, createResult));

            // We can't run a published standalone app, but let's just make sure it publishes fine
            var publishResult = await project.RunDotNetPublishAsync();
            Assert.True(0 == publishResult.ExitCode, ErrorMessages.GetFailedProcessMessage("publish", project, publishResult));

            var buildResult = await project.RunDotNetBuildAsync();
            Assert.True(0 == buildResult.ExitCode, ErrorMessages.GetFailedProcessMessage("build", project, buildResult));

            await BuildAndRunTest(project);
        }

        [Fact]
        public async Task BlazorWasmHostedTemplate_Works()
        {
            var project = await ProjectFactory.GetOrCreateProject("blazorhosted", Output);

            var createResult = await project.RunDotNetNewAsync("blazorwasm", args: new[] { "--hosted" });
            Assert.True(0 == createResult.ExitCode, ErrorMessages.GetFailedProcessMessage("create/restore", project, createResult));

            var serverProject = project.GetSubProject("Server", $"{project.ProjectName}.Server");

            var publishResult = await serverProject.RunDotNetPublishAsync(additionalArgs: "/p:DisableImplicitAspNetCoreAnalyzers=true");
            Assert.True(0 == publishResult.ExitCode, ErrorMessages.GetFailedProcessMessage("publish", serverProject, publishResult));

            var buildResult = await serverProject.RunDotNetBuildAsync(additionalArgs: "/p:DisableImplicitAspNetCoreAnalyzers=true");
            Assert.True(0 == buildResult.ExitCode, ErrorMessages.GetFailedProcessMessage("build", serverProject, buildResult));

            await BuildAndRunTest(serverProject);
            await PublishAndRunTest(serverProject);
        }
    }
}
