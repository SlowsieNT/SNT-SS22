using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SNTSS22 {
    
    public delegate void HttpSilverSparkException(Exception aException);
    public delegate void HttpSilverSparkReceiveHandler(object aSender, byte[] aBytes, HttpSilverSparkHandle aHSSH);
    public delegate void HttpSilverSparkSentHandler(object aSender, int aValue, HttpSilverSparkHandle aHSSH);
    public delegate void HttpSilverSparkException2(object aSender, Exception aException);
    public class HttpSilverSparkHandle {
        public TcpClient Socket;
        public NetworkStream Stream;
        public string SentData = "";
        public HttpSilverSparkHandle() { }
    }
    public class HttpSilverSpark {
        public string Host, Method = "GET", Pathname = "/", Query = "", Hash = "", Body = "";
        public ushort Port = 80;
        /// <summary>Values above 2 cause long connection.</summary>
        public ushort WriteByteDelayMin = 0;
        /// <summary>Values above 2 cause long connection.</summary>
        public ushort WriteByteDelayMax = 1;
        /// <summary>2 = 50% chance, 3 = 33%...</summary>
        public ushort WriteByteDelayMaxRolls = 3;
        public ushort HeaderDelayMin = 111;
        public ushort HeaderDelayMax = 555;
        /// <summary>2 = 50% chance, 3 = 33%...</summary>
        public ushort TrollingCookieMaxRolls = 2;
        /// <summary>2 = 50% chance, 3 = 33%...</summary>
        public ushort TrollingProxyMaxRolls = 3;
        /// <summary>2 = 50% chance, 3 = 33%...</summary>
        public ushort GoogleRefererMaxRolls = 3;
        /// <summary>2 = 50% chance, 3 = 33%...</summary>
        public ushort CacheControlMaxRolls = 2;
        /// <summary>
        /// Transform format #NN (eg: #00) into random string.
        /// </summary>
        public bool UseMagickalQuery = true;
        /// <summary>
        /// Transform format #NN (eg: #00) into random string.
        /// </summary>
        public bool UseMagickalHash = true;
        /// <summary>
        /// Transform format #NN (eg: #00) into random string.
        /// </summary>
        public bool UseMagickalBody = true;
        public bool UseCacheControl = true;
        public bool UseTrollingCookie = true;
        public bool UseTrollingProxy = true;
        public bool UseGoogleReferer = true;
        public bool ShuffleGetRandomHVAccept = true;
        public bool ShuffleGetRandomHVEncoding = true;
        /// <summary>Looks similar to this:
        /// <br>GET / HTTP/1.1</br>
        /// </summary>
        public bool IgnoreMainHeaderDelay = false;
        /// <summary>
        /// Used if needless delay is possible.
        /// </summary>
        public bool UseHeaderDelay = false;
        /// <summary>
        /// Use 0 if no receive.
        /// </summary>
        public ushort ReceiveDataLength = 0;
        // Definitions
        readonly string[] m_PostMethods = "post,put,patch".Split(',');
        readonly Random m_R = new Random();
        readonly static List<string> m_UserAgents = new List<string>();
        public readonly List<string> Headers = new List<string>();
        // Events
        /// <summary>NOTE: Called as background thread.</summary>
        public event HttpSilverSparkReceiveHandler OnReceive;
        /// <summary>NOTE: Called as background thread.</summary>
        public event HttpSilverSparkSentHandler OnSent;
        /// <summary>NOTE: Called as background thread.</summary>
        public event HttpSilverSparkException2 OnError;
        /// <summary>NOTE: Called synchronously.</summary>
        public static event HttpSilverSparkException OnLoadError;
        // All other is below
        /// <summary>
        /// Load string separated by lines.
        /// </summary>
        /// <param name="aString">String separated by lines.</param>
        public static void LoadString(string aString) {
            m_UserAgents.AddRange(aString.Split('\n'));
        }
        /// <summary>
        /// Loads File that contains string separated by lines.
        /// </summary>
        /// <param name="aFileName">File that contains string separated by lines.</param>
        public static void Load(string aFileName) {
            try {
                if (File.Exists(aFileName))
                    m_UserAgents.AddRange(File.ReadAllLines(aFileName));
            } catch (Exception vEx) {
                OnLoadError?.Invoke(vEx);
            }
        }
        string GetHVGoogleSearchReferer(string aHost) {
            return string.Join("&", new string[] {
                "http://www.google.com/search?q=" + aHost,
                "source=hp",
                "ei=" + RHelper.RandBase64(23,"-_"),
                "iflsig=" + RHelper.RandBase64(44,"-_"),
                "ved=" + RHelper.RandBase64(40,"-_"),
                "uact=5",
                "oq=b",
                "gs_lcp="+RHelper.RandBase64(m_R.Next(56, 216),"")
            });
        }
        string GetUserAgent() {
            if (0 > m_UserAgents.Count)
                return default;
            return RHelper.Choice(m_UserAgents);
        }
        HttpSilverSparkHandle GetConnectionStream() {
            var vSocket = new TcpClient();
            NetworkStream vNS = default;
            try {
                vSocket.Connect(Host, Port);
                vNS = vSocket.GetStream();
            } catch (Exception vEx) { OnError?.Invoke(this, vEx); }
            return new HttpSilverSparkHandle() {
                Socket = vSocket,
                Stream = vNS
            };
        }
        /// <summary>
        /// If port not 443 nor 80, yields:
        /// <br>Host: hostname:port</br>
        /// </summary>
        string HeaderHP(string aHost, ushort aPort) {
            string vStr = "Host: ";
            if (80 != aPort && 443 != aPort)
                return vStr + aHost + ":" + aPort;
            return vStr + aHost;
        }
        int SendLine(HttpSilverSparkHandle vHSSH, Encoding aEncoding = null, bool aIgnoreDelay = false) {
            return SendString(vHSSH, "\r\n", aEncoding, aIgnoreDelay);
        }
        int SendLine(HttpSilverSparkHandle vHSSH, string aString, Encoding aEncoding = null, bool aIgnoreDelay = false) {
            return SendString(vHSSH, aString + "\r\n", aEncoding, aIgnoreDelay);
        }
        int SendString(HttpSilverSparkHandle vHSSH, string aString, Encoding aEncoding = null, bool aIgnoreDelay = false) {
            // Set UTF8 as default Encoding
            if (null == aEncoding)
                aEncoding = Encoding.UTF8;
            // Retrieve packet bytes
            var vBytes = aEncoding.GetBytes(aString);
            try {
                for (int i = 0; i < vBytes.Length; i++) {
                    vHSSH.SentData += aString[i];
                    vHSSH.Stream.WriteByte(vBytes[i]);
                    if (WriteByteDelayMax > WriteByteDelayMin && !aIgnoreDelay)
                        if (RHelper.Roll(WriteByteDelayMaxRolls))
                            Thread.Sleep(m_R.Next(WriteByteDelayMin, WriteByteDelayMax));
                }
                // Apply delay if set
                if (UseHeaderDelay && !aIgnoreDelay) Thread.Sleep(GetHDelay());
                // Return true if success
                return vBytes.Length;
            } catch (Exception vEx){
                OnError?.Invoke(this, vEx);
            }
            // Apply delay if set
            if (UseHeaderDelay && !aIgnoreDelay) Thread.Sleep(GetHDelay());
            // Return true if failed
            return 0;
        }
        
        /// <summary>How much time until next header is sent.</summary>
        ushort GetHDelay() {
            if (!UseHeaderDelay) return 0;
            return (ushort)m_R.Next(HeaderDelayMin, HeaderDelayMax);
        }
        string GetHVCacheControl() {
            var vList = new List<string>(new string[] {
                "{0}max-age={1}, must-revalidate",
                "{0}must-understand, no-store",
                "{0}max-age={1}, immutable",
                "{0}max-age={1}, stale-if-error={2}",
                "{0}max-age={1}"
            });
            int[] aRanges = new int[] { 0, 86400 };
            // As much randomness as possible
            string vPublic = RHelper.Roll(2) ? "public, " : "",
                   vItem = vList[m_R.Next(0, -1 + vList.Count)];
            // Handling if more formatting required
            if (vItem.Contains("{2}"))
                vItem = string.Format(vItem, vPublic, aRanges[m_R.Next(0, 1)], 86400);
            else vItem = string.Format(vItem, vPublic, aRanges[m_R.Next(0, 1)]);
            return vItem;
        }
        string GetHVRandomAccept() {
            var vMainMT = "*/*;{0}q={1}";
            var vList = new List<string>(new string[] {
                "text/html",
                "image/avif",
                "image/apng",
                "application/signed-exchange;{0}v=b3;{0}q={1}",
                "application/xml;{0}q={1}",
                "application/xhtml+xml;{0}q={1}",
                "image/webp;{0}q={1}",
                "video/webm;{0}q={1}",
                "audio/webm;{0}q={1}",
                "application/x-shockwave-flash;{0}q={1}",
                "image/svg+xml;{0}q={1}",
                "application/pdf;{0}q={1}",
                "application/json;{0}q={1}",
                "application/ld+json;{0}q={1}",
                "image/jpeg;{0}q={1}",
                "audio/mpeg;{0}q={1}",
                "video/mpeg;{0}q={1}",
                "video/mp4;{0}q={1}",
                "video/3gpp2;{0}q={1}",
                "audio/3gpp2;{0}q={1}",
                "video/3gpp;{0}q={1}",
                "audio/3gpp;{0}q={1}",
                "audio/3gpp2;{0}q={1}"
            });
            if (ShuffleGetRandomHVAccept)
                RHelper.Shuffle(vList);
            // Future: May need to set vSpace to (char)32
            string vSpace = RHelper.Roll(2) ? " " : "";
            var vMTC = m_R.Next(m_R.Next(4, 8), vList.Count);
            var vList2 = new List<string>();
            for (var vI = 0; vI < vMTC; vI++) {
                var vItem = vList[vI];
                if (vItem.Contains("{0}")) {
                    vItem = string.Format(vItem, vSpace, "0." + m_R.Next(5, 9));
                    vList2.Add(vItem);
                }
            }
            vList2.Add(string.Format(vMainMT, vSpace, "0." + m_R.Next(5, 9)));
            return string.Join("," + vSpace, vList2.ToArray());
        }
        
        
        string GetRandomHVForwarded() {
            var vIP = RHelper.GetIP();
            var vBy = RHelper.Roll(3) ? ";by=" + RHelper.GetIP() : "";
            var vStr = "for={0};proto=http" + vBy;
            return string.Format(vStr, vIP);
        }
        /// <summary>
        /// Discharge http request.
        /// </summary>
        public void Spark() {
            // Get TcpClient instance
            // Then attempt to get NetworkStream
            var vCS = GetConnectionStream();
            // Cannot spark if server refuses, or is unavailable
            if (default != vCS) {
                string vUA = GetUserAgent();
                int vSent = 0;

                // This represents first line of http packet
                vSent += SendLine(vCS, GetMainHeader(), null, RHelper.Roll(m_R.Next(2, 3)));
                // This is host header
                vSent += SendLine(vCS, HeaderHP(Host, Port));
                // Etc...
                vSent += SendLine(vCS, "Connection: keep-alive");
                // Send useragent if present
                if (default != vUA)
                    vSent += SendLine(vCS, "UserAgent: " + vUA);
                if (RHelper.Roll(CacheControlMaxRolls) && UseCacheControl)
                    vSent += SendLine(vCS, "Cache-Control: " + GetHVCacheControl());
                if (RHelper.Roll(2))
                    vSent += SendLine(vCS, "Upgrade-Insecure-Requests: 0");
                vSent += SendLine(vCS, "Sec-Fetch-User: ?1");
                vSent += SendLine(vCS, "Sec-Fetch-Site: none");
                vSent += SendLine(vCS, "Sec-Fetch-Mode: navigate");
                vSent += SendLine(vCS, "Sec-Fetch-Dest: document");
                vSent += SendLine(vCS, "Sec-CH-UA-Mobile: ?" + m_R.Next(0, 1));
                // vUA.Contains kekw
                vSent += SendLine(vCS, "Sec-CH-UA-Bitness: " + (vUA.Contains("64") ? "32" : "64"));
                // Future: remove arch?
                vSent += SendLine(vCS, "Sec-CH-UA-Arch: " + GetRandomHVUAArch());
                foreach (var vHeader in Headers)
                    vSent += SendLine(vCS, vHeader);
                vSent += SendLine(vCS, "Accept-Encoding: " + GetRandomHVEncoding());
                if (RHelper.Roll(4))
                    vSent += SendLine(vCS, string.Format("Accept-Language: en; q=0.{0}, *; q=0.{1}", m_R.Next(6, 9), m_R.Next(5, 7)));
                else vSent += SendLine(vCS, "Accept-Language: en; q=0.8, *; q=0.5");
                vSent += SendLine(vCS, "Accept: " + GetHVRandomAccept());
                // randoms:
                if (RHelper.Roll(GoogleRefererMaxRolls) && UseGoogleReferer) {
                    var vReferer = GetHVGoogleSearchReferer(Host);
                    vSent += SendLine(vCS, "Origin: http://www.google.com:80");
                    vSent += SendLine(vCS, "Referer: " + vReferer);
                }
                // Proxy IPs
                if (RHelper.Roll(TrollingProxyMaxRolls) && UseTrollingProxy) {
                    string vIP1 = RHelper.GetIP(), vIP2 = RHelper.GetIP(),
                           vIP3 = RHelper.GetIP();
                    if (RHelper.Roll(2)) {
                        var vC = m_R.Next(1, m_R.Next(2, 4));
                        List<string> vR = new List<string>();
                        for (int vI = 0; vI < vC; vI++)
                            vR.Add(GetRandomHVForwarded());
                        vSent += SendLine(vCS, "Forwarded: " + string.Join(", ", vR.ToArray()));
                    } else {
                        if (RHelper.Roll(3))
                            vSent += SendLine(vCS, string.Format("X-Forwarded-For: {0}, {1}, {2}", vIP1, vIP2, vIP3));
                        else vSent += SendLine(vCS, "X-Forwarded-For: " + vIP1);
                    }
                }
                // Trolling headers
                if (RHelper.Roll(TrollingCookieMaxRolls) && UseTrollingCookie)
                    vSent += SendLine(vCS, "Cookie: PHPSESSID=" + RHelper.RandBase64(m_R.Next(56,384)));
                // Confirm request by sending newline again
                if (m_PostMethods.Contains(Method.ToLower())) {
                    // Handle post type of request
                    vSent += SendLine(vCS, "Content-Length: " + Body.Length, null);
                    vSent += SendLine(vCS);
                    vSent += SendLine(vCS, Body, null);
                } else vSent += SendLine(vCS);
                InvokeOnSent(vSent, vCS);
                // Handle receive if not 0
                if (ReceiveDataLength > 0) {
                    byte[] vBuf = new byte[ReceiveDataLength];
                    vCS.Stream.Read(vBuf, 0, vBuf.Length);
                    InvokeOnReceive(vBuf, vCS);
                }
            }
        }
        void InvokeOnReceive(byte[] vBuf, HttpSilverSparkHandle aHSSH) {
            ThreadPool.QueueUserWorkItem(delegate (object aState) {
                OnReceive?.Invoke(this, vBuf, aHSSH);
            });
        }
        void InvokeOnSent(int vSent, HttpSilverSparkHandle aHSSH) {
            ThreadPool.QueueUserWorkItem(delegate (object aState) {
                OnSent?.Invoke(this, vSent, aHSSH);
            });
        }
        string GetRandomHVEncoding() {
            var vTypes = new List<string>(new string[] {
                "gzip; q=1.0",
                "deflate; q=0.{0}",
                "br; q=0.{0}",
                "compress; q=0.{0}"
            });
            if (ShuffleGetRandomHVEncoding)
                RHelper.Shuffle(vTypes);
            vTypes.Add("*; q=0.{0}");
            for (int i = 0; i < vTypes.Count; i++) {
                var vType = vTypes[i];
                if (vType.Contains("{0}"))
                    vTypes[i] = string.Format(vTypes[i], m_R.Next(5, 9));
            }
            return string.Join(", ", vTypes.ToArray());
        }
        string GetRandomHVUAArch() {
            var vArchs = "x64,x86,Win64; x64".Split(',');
            return RHelper.Choice(vArchs);
        }
        /// <summary>
        /// Get main header of http packet.
        /// </summary>
        public string GetMainHeader() {
            // Allow if Query seems valid enough
            string vQuery = 0 == Query.IndexOf("?") ? Query : "";
            // Allow if Pathname seems valid enough
            string vPathname = 0 == Pathname.IndexOf("/") ? Pathname : "/";
            // Allow if Hash seems valid enough
            string vHash = 0 == Hash.IndexOf("#") ? Hash : "";
            // Handle Magickals
            if (UseMagickalHash)
                vHash = RHelper.GetMagickalString(vHash);
            if (UseMagickalBody)
                Body = RHelper.GetMagickalQuery(Body);
            if (UseMagickalQuery && vQuery.Length > 1)
                vQuery = "?" + RHelper.GetMagickalQuery(vQuery.Substring(1));
            // Create string accordingly
            return Method.Trim().ToUpper() + " " + vPathname + vQuery + vHash + " HTTP/1.1";
        }
        /// <summary>
        /// Http Silver Spark is supposed to be the cutest, and tiny spark.
        /// <br>As it allows many customizations!</br>
        /// </summary>
        public HttpSilverSpark(string aHost, int aPort) {
            Host = aHost;
            if (aPort < ushort.MaxValue)
                Port = (ushort)aPort;
        }
        /// <summary>
        /// Http Silver Spark is supposed to be the cutest, and tiny spark.
        /// <br>As it allows many customizations!</br>
        /// </summary>
        public HttpSilverSpark(string aHost) {
            Host = aHost;
        }
        /// <summary>
        /// Http Silver Spark is supposed to be the cutest, and tiny spark.
        /// <br>As it allows many customizations!</br>
        /// </summary>
        public HttpSilverSpark() {
            
        }
    }
}
