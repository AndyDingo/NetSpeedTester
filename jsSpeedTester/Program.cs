﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSpeedTest;
using NSpeedTest.Models;

namespace jsSpeedTester
{
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

    class Program
    {
        private static SpeedTestClient client;
        private static Settings settings;
        private const string DefaultCountry = "Australia";

        public static string logfile = Environment.CurrentDirectory + @"\jsSpeedTester.log";

        static void Main()
        {
            Console.Title = "Jessica's Speed Tester";

            Console.WriteLine("Getting speedtest.net settings and server list...");
            client = new SpeedTestClient();
            settings = client.GetSettings();

            var servers = SelectServers();
            var bestServer = SelectBestServer(servers);

            Console.WriteLine("Testing speed...");
            var downloadSpeed = client.TestDownloadSpeed(bestServer, settings.Download.ThreadsPerUrl);
            PrintSpeed("Download", downloadSpeed);
            var uploadSpeed = client.TestUploadSpeed(bestServer, settings.Upload.ThreadsPerUrl);
            PrintSpeed("Upload", uploadSpeed);

            DateTime dt = new DateTime(2018, 11, 12);
            dt = DateTime.Now;

            using (StreamWriter sw = new StreamWriter(logfile, true))
            {
                sw.WriteLine("[" + dt.ToString("ddMMyyy HH:mm") + "] Download speed: "+ downloadSpeed);
                sw.WriteLine("[" + dt.ToString("ddMMyyy HH:mm") + "] Upload speed: " + uploadSpeed);

            }

            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();
        }

        private static Server SelectBestServer(IEnumerable<Server> servers)
        {
            Console.WriteLine();
            Console.WriteLine("Best server by latency:");
            var bestServer = servers.OrderBy(x => x.Latency).First();
            PrintServerDetails(bestServer);
            Console.WriteLine();
            return bestServer;
        }

        private static IEnumerable<Server> SelectServers()
        {
            Console.WriteLine();
            Console.WriteLine("Selecting best server by distance...");
            var servers = settings.Servers.Where(s => s.Country.Equals(DefaultCountry)).Take(10).ToList();

            foreach (var server in servers)
            {
                server.Latency = client.TestServerLatency(server);
                PrintServerDetails(server);
            }
            return servers;
        }

        private static void PrintServerDetails(Server server)
        {
            Console.WriteLine("Hosted by {0} ({1}/{2}), distance: {3}km, latency: {4}ms", server.Sponsor, server.Name,
                server.Country, (int)server.Distance / 1000, server.Latency);
        }

        private static void PrintSpeed(string type, double speed)
        {
            if (speed > 1024)
            {
                Console.WriteLine("{0} speed: {1} Mbps", type, Math.Round(speed / 1024, 2));
            }
            else
            {
                Console.WriteLine("{0} speed: {1} Kbps", type, Math.Round(speed, 2));
            }
        }
    }
}
