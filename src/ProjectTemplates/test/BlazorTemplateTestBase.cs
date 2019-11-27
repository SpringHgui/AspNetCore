// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.E2ETesting;
using OpenQA.Selenium;
using Templates.Test.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Templates.Test
{
    public abstract class BlazorTemplateTestBase : BrowserTestBase
    {
        public BlazorTemplateTestBase(ProjectFactoryFixture projectFactory, BrowserFixture browserFixture, ITestOutputHelper output)
            : base(browserFixture, output)
        {
            ProjectFactory = projectFactory;
        }

        public ProjectFactoryFixture ProjectFactory { get; set; }

        protected async Task BuildAndRunTest(Project project)
        {
            using var aspNetProcess = project.StartBuiltProjectAsync();

            Assert.False(
                aspNetProcess.Process.HasExited,
                ErrorMessages.GetFailedProcessMessageOrEmpty("Run built project", project, aspNetProcess.Process));

            await aspNetProcess.AssertStatusCode("/", HttpStatusCode.OK, "text/html");
            if (BrowserFixture.IsHostAutomationSupported())
            {
                aspNetProcess.VisitInBrowser(Browser);
                TestBasicNavigation(project);
            }
            else
            {
                BrowserFixture.EnforceSupportedConfigurations();
            }
        }

        protected async Task PublishAndRunTest(Project project)
        {
            using var aspNetProcess = project.StartPublishedProjectAsync();

            Assert.False(
                aspNetProcess.Process.HasExited,
                ErrorMessages.GetFailedProcessMessageOrEmpty("Run published project", project, aspNetProcess.Process));

            await aspNetProcess.AssertStatusCode("/", HttpStatusCode.OK, "text/html");
            if (BrowserFixture.IsHostAutomationSupported())
            {
                aspNetProcess.VisitInBrowser(Browser);
                TestBasicNavigation(project);
            }
            else
            {
                BrowserFixture.EnforceSupportedConfigurations();
            }
        }

        private void TestBasicNavigation(Project project)
        {
            // Give components.server enough time to load so that it can replace
            // the prerendered content before we start making assertions.
            Browser.Exists(By.TagName("ul"));

            // <title> element gets project ID injected into it during template execution
            Browser.Equal(project.AppName.Trim(), () => Browser.Title.Trim());

            // Initially displays the home page
            Browser.Equal("Hello, world!", () => Browser.FindElement(By.TagName("h1")).Text);

            // Can navigate to the counter page
            Browser.FindElement(By.PartialLinkText("Counter")).Click();
            Browser.Contains("counter", () => Browser.Url);
            Browser.Equal("Counter", () => Browser.FindElement(By.TagName("h1")).Text);

            // Clicking the counter button works
            Browser.Equal("Current count: 0", () => Browser.FindElement(By.CssSelector("h1 + p")).Text);
            Browser.FindElement(By.CssSelector("p+button")).Click();
            Browser.Equal("Current count: 1", () => Browser.FindElement(By.CssSelector("h1 + p")).Text);

            // Can navigate to the 'fetch data' page
            Browser.FindElement(By.PartialLinkText("Fetch data")).Click();
            Browser.Contains("fetchdata", () => Browser.Url);
            Browser.Equal("Weather forecast", () => Browser.FindElement(By.TagName("h1")).Text);

            // Asynchronously loads and displays the table of weather forecasts
            Browser.Exists(By.CssSelector("table>tbody>tr"));
            Browser.Equal(5, () => Browser.FindElements(By.CssSelector("p+table>tbody>tr")).Count);
        }
    }
}
