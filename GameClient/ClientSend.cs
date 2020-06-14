using TOH.Common.ServerData;

namespace GameClient
{
    public class ClientSend
    {
        private static void SendTcpData(Packet packet)
        {
            packet.WriteLength();
            ClientWrapper.Instance.Tcp.SendData(packet);
        }

        public static void WelcomeReceived()
        {
            using(Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(ClientWrapper.Instance.Id);
                packet.Write("Client 1");

                SendTcpData(packet);
            }
        }

    }
}
