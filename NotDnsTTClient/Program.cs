using Ae.Dns.Protocol;
using Ae.Dns.Protocol.Records;
using DnsMessengerEncryption;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace NotDnsTTClient
{
    internal class Program
    {
        public class UDPSocket
        {
            private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            private const int bufSize = 8 * 1024;
            private State state = new State();
            private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
            private AsyncCallback? recv = null;
            private static readonly object udplock = new object();
            public class State
            {
                public byte[] buffer = new byte[bufSize];
            }

            public void Server(string address, int port)
            {
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 128);
                _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
                Receive();
            }

            public void Client(string address, int port)
            {
                if (_socket.Connected)
                {
                    //_socket.Disconnect(false);
                }
                _socket.ReceiveTimeout = 5000;
                _socket.SendTimeout = 5000;
                _socket.Connect(IPAddress.Parse(address), port);
                //Receive();
            }
            public void Client(IPEndPoint endpoint)
            {
                if (_socket.Connected)
                {
                    //_socket.Disconnect(false);
                }
                _socket.ReceiveTimeout = 5000;
                _socket.SendTimeout = 5000;
                _socket.Connect(endpoint);
                //Receive();
            }

            public byte[] SendWithRecieve(byte[] data)
            {
                lock(udplock)
                {
                    _socket.Send(data, 0, data.Length, SocketFlags.None);
                    //_socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
                    //{
                    //    State so = (State)ar.AsyncState;
                    //    int bytes = _socket.EndSend(ar);
                    //}, state);
                    var buffer = new byte[bufSize];
                    var read = _socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    return buffer.Take(read).ToArray();
                }
            }
            private void Receive()
            {
                _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
                {
                    Task.Run(() => {
                        try
                        {
                            State? so = (State?)ar.AsyncState;
                            int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                            _socket.BeginReceiveFrom(so!.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                            //_socket.SendTo(Encoding.ASCII.GetBytes("GOTIT!"), epFrom);
                            int offset = 0;
                            var DnsMessage = new DnsMessage();
                            DnsMessage.ReadBytes(so.buffer, ref offset);

                            var newanswer = new List<DnsResourceRecord>();
                            var hostname = DnsMessage.Header.Host.ToString().ToLower();

                            if (hostname.Contains("." + Program.srcdomainaddress))
                            {
                                var data = hostname.Replace("." + Program.srcdomainaddress, "").Replace(".", "");
                                var decoded = DnsMessengerEncryption.Base32Lower.Decode(data);
                                if (DnsMessage.Header.QueryType == Ae.Dns.Protocol.Enums.DnsQueryType.A)
                                {
                                    DnsMessage.Header.AnswerRecordCount++;
                                    DnsMessage.Header.IsQueryResponse = true;
                                    DnsMessage.Header.AuthoritativeAnswer = true;
                                    newanswer.Add(new Ae.Dns.Protocol.Records.DnsResourceRecord() { Class = Ae.Dns.Protocol.Enums.DnsQueryClass.IN, Resource = new Ae.Dns.Protocol.Records.DnsIpAddressResource() { IPAddress = IPAddress.Parse("9.18.1.1") }, Type = Ae.Dns.Protocol.Enums.DnsQueryType.A, TimeToLive = 0, Host = DnsMessage.Header.Host });
                                    new Thread(() =>
                                    {
                                        Program.readpacket(decoded);
                                    }).Start();
                                }
                                else if (DnsMessage.Header.QueryType == Ae.Dns.Protocol.Enums.DnsQueryType.TEXT)
                                {
                                    var connid = BitConverter.ToInt32(decoded.Skip(4).Take(4).ToArray());
                                    DnsMessage.Header.AnswerRecordCount++;
                                    DnsMessage.Header.IsQueryResponse = true;
                                    DnsMessage.Header.AuthoritativeAnswer = true;
                                    if (conns.ContainsKey(connid) && conns[connid].ContainsKey(Convert.ToHexString(decoded)))
                                    {
                                        newanswer.Add(new Ae.Dns.Protocol.Records.DnsResourceRecord() { Class = Ae.Dns.Protocol.Enums.DnsQueryClass.IN, Resource = new Ae.Dns.Protocol.Records.DnsTextResource() { Entries = new DnsLabels(conns[connid][Convert.ToHexString(decoded)]) }, Type = Ae.Dns.Protocol.Enums.DnsQueryType.TEXT, TimeToLive = 0, Host = DnsMessage.Header.Host });
                                    }
                                    else
                                    {
                                        newanswer.Add(new Ae.Dns.Protocol.Records.DnsResourceRecord() { Class = Ae.Dns.Protocol.Enums.DnsQueryClass.IN, Resource = new Ae.Dns.Protocol.Records.DnsTextResource() { Entries = new DnsLabels("XXXX") }, Type = Ae.Dns.Protocol.Enums.DnsQueryType.TEXT, TimeToLive = 0, Host = DnsMessage.Header.Host });
                                    }
                                }
                                DnsMessage.Answers = newanswer;
                            }
                            var responsebuffer = new byte[4096];
                            offset = 0;
                            DnsMessage.WriteBytes(responsebuffer, ref offset);
                            IPEndPoint? ipfrom = epFrom as IPEndPoint;
                            _socket.SendTo(responsebuffer, 0, offset, SocketFlags.None, ipfrom!);
                        }
                        catch
                        {

                        }
                    });
                }, state);
            }
        }
        static void readpacket(byte[] decoded)
        {
            try
            {
                var packetnum = BitConverter.ToInt32(decoded.Take(4).ToArray());
                var connid = BitConverter.ToInt32(decoded.Skip(4).Take(4).ToArray());
                var chunks = BitConverter.ToInt32(decoded.Skip(8).Take(4).ToArray());
                var iv = decoded.Skip(12).Take(12).ToArray();
                var readchunks = new List<string>();
                for (int i = 0; i < chunks; i++)
                {
                    while (true)
                    {
                        try
                        {
                            byte[] dnsdata = new byte[4096];
                            int offset = 0;
                            var stringdata = Base32Lower.Encode((BitConverter.GetBytes(packetnum).Concat(BitConverter.GetBytes(connid)).Concat(BitConverter.GetBytes(i))).ToArray());
                            var dnsmessage = DnsQueryFactory.CreateQuery(stringdata + "." + dstdomainaddress, Ae.Dns.Protocol.Enums.DnsQueryType.TEXT);
                            dnsmessage.WriteBytes(dnsdata, ref offset);
                            var udpclient = new UDPSocket();
                            udpclient.Client(dnsendpoint);
                            var responsedns = udpclient.SendWithRecieve(dnsdata.Take(offset).ToArray());
                            offset = 0;
                            dnsmessage.ReadBytes(responsedns, ref offset);
                            var resource = (DnsTextResource)dnsmessage.Answers.FirstOrDefault().Resource;
                            var data = resource.Entries.ToString().ToLower();
                            if (data == "XXXX")
                            {
                                try
                                {
                                    lock (thislock)
                                    {
                                        conninput.Remove(connid);
                                    }
                                }
                                catch
                                {

                                }
                                try
                                {
                                    lock (thislock)
                                    {
                                        conns.Remove(connid);
                                    }
                                }
                                catch
                                {

                                }
                                return;
                            }
                            else
                            {
                                readchunks.Add(data);
                            }
                            break;

                        }
                        catch
                        {

                        }
                    }
                }
                var decrypted = Encryption.decryptmsg(readchunks.ToArray(), iv);
                lock (thislock)
                {
                    conninput[connid].Add(new IncommingPacket() { packetnum = packetnum, packet = decrypted });
                }
            }
            catch
            {

            }
        }
        static void sendmessageA(string message)
        {
            while (true)
            {
                try
                {
                    byte[] dnsdata = new byte[4096];
                    int offset = 0;
                    var dnsmessage = Ae.Dns.Protocol.DnsQueryFactory.CreateQuery(message + "." + dstdomainaddress, Ae.Dns.Protocol.Enums.DnsQueryType.A);
                    dnsmessage.WriteBytes(dnsdata, ref offset);
                    var udpclient = new UDPSocket();
                    udpclient.Client(dnsendpoint);
                    var read=udpclient.SendWithRecieve(dnsdata.Take(offset).ToArray());
                    offset = 0;
                    dnsmessage.ReadBytes(read, ref offset);
                    if(dnsmessage.Answers.Count>0)
                    {
                        break;
                    }
                }
                catch
                {

                }
            }
        }
        static readonly object thislock = new object();
        static Random rnd = new Random();
        static Dictionary<int, Dictionary<string, string>> conns = new Dictionary<int, Dictionary<string, string>>();
        static Dictionary<int, List<IncommingPacket>> conninput = new Dictionary<int, List<IncommingPacket>>();
        static void tcphandlerthread(TcpClient client)
        {
            client.SendTimeout = 30000;
            client.ReceiveTimeout = 30000;
            var connid = rnd.Next();
            lock (thislock)
            {
                while (conns.ContainsKey(connid))
                {
                    connid = rnd.Next();
                }
                conns.Add(connid, new Dictionary<string, string>());
                conninput.Add(connid, new List<IncommingPacket>());
            }
            var stream = client.GetStream();
            var buffer = new byte[8192];
            int packetnum = 0;
            int expectedpacket = 0;
            while (true)
            {
                try
                {
                    while (true)
                    {
                        try
                        {
                            while (stream.DataAvailable)
                            {
                                var readbytes = stream.Read(buffer, 0, buffer.Length);
                                var datatosend = Encryption.encryptmsg(connid, packetnum, buffer.Take(readbytes).ToArray(), 80);
                                var datatoserve = datatosend.Skip(1).ToArray();
                                for (int i = 0; i < datatoserve.Length; i++)
                                {
                                    conns[connid].Add(Convert.ToHexString((BitConverter.GetBytes(packetnum).Concat(BitConverter.GetBytes(connid)).Concat(BitConverter.GetBytes(i))).ToArray()), datatoserve[i]);
                                }
                                sendmessageA(datatosend[0]);
                                packetnum++;
                            }
                            while (conninput[connid].Count(x => x.packetnum == expectedpacket) > 0)
                            {
                                byte[] data = null;
                                lock (thislock)
                                {
                                    var found = conninput[connid].FirstOrDefault(x => x.packetnum == expectedpacket);
                                    data = found.packet;
                                    conninput[connid].Remove(found);
                                }
                                if (data != null)
                                {
                                    stream.Write(data);
                                    expectedpacket++;
                                }
                            }
                            stream.Flush();
                        }
                        catch
                        {
                            if (!client.Connected || !conninput.ContainsKey(connid) || !conns.ContainsKey(connid))
                            {
                                try
                                {
                                    client.Close();
                                }
                                catch
                                {

                                }
                                try
                                {
                                    lock (thislock)
                                    {
                                        conninput.Remove(connid);
                                    }
                                }
                                catch
                                {

                                }
                                try
                                {
                                    lock (thislock)
                                    {
                                        conns.Remove(connid);
                                    }
                                }
                                catch
                                {

                                }
                                return;
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
                catch
                {

                }
            }
        }
        class IncommingPacket
        {
            public int packetnum;
            public byte[] packet;
        }
        public static string srcdomainaddress = "";
        public static string dstdomainaddress = "";
        public static IPEndPoint dnsendpoint;
        public static IPEndPoint listeningaddress;
        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: DnsMessengerClient <server_address> <base64_key> <src_domain_address> <dst_domain_address> <listening_address>");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }
            DnsMessengerEncryption.Encryption.key = Convert.FromBase64String(args[1]);
            dnsendpoint = IPEndPoint.Parse(args[0]);
            srcdomainaddress = args[2];
            dstdomainaddress = args[3];
            listeningaddress = IPEndPoint.Parse(args[4]);
            var udpserver = new UDPSocket();
            udpserver.Server("0.0.0.0", 53);
            var listener = new TcpListener(listeningaddress);
            listener.Start();
            while (true)
            {
                if (listener.Pending())
                {
                    new Thread(() =>
                    {
                        tcphandlerthread(listener.AcceptTcpClient());
                    }).Start();
                }
                Thread.Sleep(100);
            }
        }
    }
}
