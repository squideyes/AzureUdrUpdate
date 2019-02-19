// Copyright 2019 Louis S. Berman
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Flurl.Http;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AzureUdrUpdate
{
    public static class RouteUpdater
    {
        [FunctionName("RouteUpdater")]
        public static async Task Run(ILogger log, [TimerTrigger("0 0 2 * * *",
            RunOnStartup = true, UseMonitor = true)]TimerInfo timerInfo,
            [Blob("azure-udr-update")]CloudBlobContainer container)
        {
            var uri = await ScrapeUriAsync();

            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference("LastUdrUpdate.json");

            UpdateInfo info;

            if (await blob.ExistsAsync())
            {
                info = JsonConvert.DeserializeObject<UpdateInfo>(
                    await blob.DownloadTextAsync());

                if (info.Uri.AbsoluteUri.Equals(
                    uri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
                {
                    log.LogDebug($"Skipped UDR Update (from {info.Uri})");

                    return;
                }
            }

            info = new UpdateInfo { Uri = uri, UpdatedOn = DateTime.UtcNow };

            var xml = await uri.AbsoluteUri.GetStringAsync();

            var doc = XDocument.Parse(xml);

            var routes = doc.Element("AzurePublicIpAddresses")
                .Elements("Region")
                .Where(r => Regions.IsValid(r.Attribute("Name").Value))
                .Elements("IpRange")
                .Attributes("Subnet")
                .Select(s => GetRoutes(s.Value))
                .Distinct()
                .OrderBy(o => o)
                .ToList();

            // emit ARM

            var json = JsonConvert.SerializeObject(info);

            await blob.UploadTextAsync(json);

            log.LogInformation($"Updated UDRs (from {info.Uri})");
        }

        private static string GetRoutes(string value)
        {
            var fields = value.Split('.');

            var octet1 = int.Parse(fields[0]);
            var octet2 = int.Parse(fields[0]);

            return $"{octet1:000}.{octet2:000}.0.0/16";
        }

        private static async Task<Uri> ScrapeUriAsync()
        {
            var html = await WellKnown.LandingPage
                .GetStringAsync();

            var doc = new HtmlDocument();

            doc.LoadHtml(html);

            var url = doc.DocumentNode
                .SelectSingleNode("//a[@data-bi-id='downloadretry']")
                .Attributes["href"].Value;

            return new Uri(url);
        }
    }
}
