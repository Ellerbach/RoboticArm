using System;
using System.Threading;
using System.Diagnostics;
using Windows.Devices.WiFi;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using System.Net;

namespace RoboticArm
{
    public class Program
    {
        private static string MySsid = "YourWifi";
        private static string MyPassword = "YourWifiPassword";
        private static bool _isConnected = false;

        public static void Main()
        {
            Debug.WriteLine("Hello from robotic arm :-)!");

            try
            {
                int connectRetry = 0;

                // Get the first WiFI Adapter
                WiFiAdapter wifi = WiFiAdapter.FindAllAdapters()[0];
                Debug.WriteLine("Getting wifi adaptor");

                wifi.AvailableNetworksChanged += WifiAvailableNetworksChanged;

            rescan:
                wifi.ScanAsync();
                Debug.WriteLine("Scanning...");

                var timeout = DateTime.UtcNow.AddSeconds(10);
                while (!_isConnected)
                {
                    Thread.Sleep(100);
                    if (DateTime.UtcNow > timeout)
                    {
                        goto rescan;
                    }
                }

                NetworkHelpers.SetupAndConnectNetwork(true);

                Debug.WriteLine("Waiting for network up and IP address...");

                //NetworkHelpers.IpAddressAvailable.WaitOne();                
                //NetworkHelpers.DateTimeAvailable.WaitOne();

                while (!NetworkHelpers.CheckIP())
                {
                    Thread.Sleep(500);
                    connectRetry++;
                    if (connectRetry == 5)
                    {
                        connectRetry = 0;
                        goto rescan;
                    }
                }

                ControllerApi.Initialize();

#if USE_TLS
                X509Certificate _myWebServerCertificate509 = new X509Certificate2(_myWebServerCrt, _myWebServerPrivateKey, "1234");

                // Instantiate a new web server on port 443.
                using (WebServer server = new WebServer(443, HttpProtocol.Https, new Type[] { typeof(ControllerPerson), typeof(ControllerTest), typeof(ControllerAuth) }))
#else
                // Instantiate a new web server on port 80.
                // The Web pages controller must be the last one and the / route must be the last of all attributes
                using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(ControllerApi), typeof(ControllerWebpages) }))
#endif                
                {
                    // To test authentication with various scenarios
                    server.Credential = new NetworkCredential("user", "password");
                    // Add a handler for commands that are received by the server.

#if USE_TLS
                    server.HttpsCert = _myWebServerCertificate509;

                    server.SslProtocols = System.Net.Security.SslProtocols.Tls11 | System.Net.Security.SslProtocols.Tls12;
#endif

                    // Start the server.
                    server.Start();

                    Thread.Sleep(Timeout.Infinite);
                }

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"{ex}");
            }
        }

        private static void WifiAvailableNetworksChanged(WiFiAdapter sender, object e)
        {
            var wifiNetworks = sender.NetworkReport.AvailableNetworks;
            foreach (var net in wifiNetworks)
            {
                Debug.WriteLine($"SSID: {net.Ssid}, strength: {net.SignalBars}");
                if (net.Ssid == MySsid)
                {
                    if (_isConnected)
                    {
                        sender.Disconnect();
                        _isConnected = false;
                        Thread.Sleep(3000);
                    }
                    // Connect to network
                    WiFiConnectionResult result = sender.Connect(net, WiFiReconnectionKind.Automatic, MyPassword);

                    // Display status
                    if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                    {
                        Debug.WriteLine($"Connected to Wifi network {net.Ssid}");
                        _isConnected = true;
                        break;
                    }
                    else
                    {
                        Debug.WriteLine($"Error {result.ConnectionStatus} connecting to Wifi network");
                    }
                }

            }
        }

        #region certificates & keys

        // X509 RSA key PEM format 2048 bytes
        // generate with openssl:
        // > openssl req -newkey rsa:2048 -nodes -keyout selfcert.key -x509 -days 365 -out selfcert.crt
        // and paste selfcert.crt content below:
        private const string _myWebServerCrt =
@"-----BEGIN CERTIFICATE-----
MIIDNzCCAh+gAwIBAgIBAjANBgkqhkiG9w0BAQsFADA7MQswCQYDVQQGEwJOTDER
MA8GA1UECgwIUG9sYXJTU0wxGTAXBgNVBAMMEFBvbGFyU1NMIFRlc3QgQ0EwHhcN
MTkwMjEwMTQ0NDA2WhcNMjkwMjEwMTQ0NDA2WjA0MQswCQYDVQQGEwJOTDERMA8G
A1UECgwIUG9sYXJTU0wxEjAQBgNVBAMMCWxvY2FsaG9zdDCCASIwDQYJKoZIhvcN
AQEBBQADggEPADCCAQoCggEBAMFNo93nzR3RBNdJcriZrA545Do8Ss86ExbQWuTN
owCIp+4ea5anUrSQ7y1yej4kmvy2NKwk9XfgJmSMnLAofaHa6ozmyRyWvP7BBFKz
NtSj+uGxdtiQwWG0ZlI2oiZTqqt0Xgd9GYLbKtgfoNkNHC1JZvdbJXNG6AuKT2kM
tQCQ4dqCEGZ9rlQri2V5kaHiYcPNQEkI7mgM8YuG0ka/0LiqEQMef1aoGh5EGA8P
hYvai0Re4hjGYi/HZo36Xdh98yeJKQHFkA4/J/EwyEoO79bex8cna8cFPXrEAjya
HT4P6DSYW8tzS1KW2BGiLICIaTla0w+w3lkvEcf36hIBMJcCAwEAAaNNMEswCQYD
VR0TBAIwADAdBgNVHQ4EFgQUpQXoZLjc32APUBJNYKhkr02LQ5MwHwYDVR0jBBgw
FoAUtFrkpbPe0lL2udWmlQ/rPrzH/f8wDQYJKoZIhvcNAQELBQADggEBAC465FJh
Pqel7zJngHIHJrqj/wVAxGAFOTF396XKATGAp+HRCqJ81Ry60CNK1jDzk8dv6M6U
HoS7RIFiM/9rXQCbJfiPD5xMTejZp5n5UYHAmxsxDaazfA5FuBhkfokKK6jD4Eq9
1C94xGKb6X4/VkaPF7cqoBBw/bHxawXc0UEPjqayiBpCYU/rJoVZgLqFVP7Px3sv
a1nOrNx8rPPI1hJ+ZOg8maiPTxHZnBVLakSSLQy/sWeWyazO1RnrbxjrbgQtYKz0
e3nwGpu1w13vfckFmUSBhHXH7AAS/HpKC4IH7G2GAk3+n8iSSN71sZzpxonQwVbo
pMZqLmbBm/7WPLc=
-----END CERTIFICATE-----";

        // this one is generated with the command below. We need a password.
        // > openssl rsa -des3 -in selfcert.key -out selfcertenc.key
        // the one below was encoded with '1234' as the password.
        private const string _myWebServerPrivateKey =
@"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAwU2j3efNHdEE10lyuJmsDnjkOjxKzzoTFtBa5M2jAIin7h5r
lqdStJDvLXJ6PiSa/LY0rCT1d+AmZIycsCh9odrqjObJHJa8/sEEUrM21KP64bF2
2JDBYbRmUjaiJlOqq3ReB30Zgtsq2B+g2Q0cLUlm91slc0boC4pPaQy1AJDh2oIQ
Zn2uVCuLZXmRoeJhw81ASQjuaAzxi4bSRr/QuKoRAx5/VqgaHkQYDw+Fi9qLRF7i
GMZiL8dmjfpd2H3zJ4kpAcWQDj8n8TDISg7v1t7HxydrxwU9esQCPJodPg/oNJhb
y3NLUpbYEaIsgIhpOVrTD7DeWS8Rx/fqEgEwlwIDAQABAoIBAQCXR0S8EIHFGORZ
++AtOg6eENxD+xVs0f1IeGz57Tjo3QnXX7VBZNdj+p1ECvhCE/G7XnkgU5hLZX+G
Z0jkz/tqJOI0vRSdLBbipHnWouyBQ4e/A1yIJdlBtqXxJ1KE/ituHRbNc4j4kL8Z
/r6pvwnTI0PSx2Eqs048YdS92LT6qAv4flbNDxMn2uY7s4ycS4Q8w1JXnCeaAnYm
WYI5wxO+bvRELR2Mcz5DmVnL8jRyml6l6582bSv5oufReFIbyPZbQWlXgYnpu6He
GTc7E1zKYQGG/9+DQUl/1vQuCPqQwny0tQoX2w5tdYpdMdVm+zkLtbajzdTviJJa
TWzL6lt5AoGBAN86+SVeJDcmQJcv4Eq6UhtRr4QGMiQMz0Sod6ettYxYzMgxtw28
CIrgpozCc+UaZJLo7UxvC6an85r1b2nKPCLQFaggJ0H4Q0J/sZOhBIXaoBzWxveK
nupceKdVxGsFi8CDy86DBfiyFivfBj+47BbaQzPBj7C4rK7UlLjab2rDAoGBAN2u
AM2gchoFiu4v1HFL8D7lweEpi6ZnMJjnEu/dEgGQJFjwdpLnPbsj4c75odQ4Gz8g
sw9lao9VVzbusoRE/JGI4aTdO0pATXyG7eG1Qu+5Yc1YGXcCrliA2xM9xx+d7f+s
mPzN+WIEg5GJDYZDjAzHG5BNvi/FfM1C9dOtjv2dAoGAF0t5KmwbjWHBhcVqO4Ic
BVvN3BIlc1ue2YRXEDlxY5b0r8N4XceMgKmW18OHApZxfl8uPDauWZLXOgl4uepv
whZC3EuWrSyyICNhLY21Ah7hbIEBPF3L3ZsOwC+UErL+dXWLdB56Jgy3gZaBeW7b
vDrEnocJbqCm7IukhXHOBK8CgYEAwqdHB0hqyNSzIOGY7v9abzB6pUdA3BZiQvEs
3LjHVd4HPJ2x0N8CgrBIWOE0q8+0hSMmeE96WW/7jD3fPWwCR5zlXknxBQsfv0gP
3BC5PR0Qdypz+d+9zfMf625kyit4T/hzwhDveZUzHnk1Cf+IG7Q+TOEnLnWAWBED
ISOWmrUCgYAFEmRxgwAc/u+D6t0syCwAYh6POtscq9Y0i9GyWk89NzgC4NdwwbBH
4AgahOxIxXx2gxJnq3yfkJfIjwf0s2DyP0kY2y6Ua1OeomPeY9mrIS4tCuDQ6LrE
TB6l9VGoxJL4fyHnZb8L5gGvnB1bbD8cL6YPaDiOhcRseC9vBiEuVg==
-----END RSA PRIVATE KEY-----";

        #endregion
    }
}
