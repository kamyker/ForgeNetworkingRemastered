using BeardedManStudios;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.TCP;

namespace UnitTests.Source
{
	[TestClass]
	public class FrameTestTCP : BaseTCPTests
	{
		private const int PORT = 17931;
		private const int BINARY_GROUP_ID = MessageGroupIds.START_OF_GENERIC_IDS + 1;
		private const string MESSAGE = "THIS IS A TEST TO MAKE SURE BINARY IS WORKING!";

		private static NetworkingPlayer responsePlayer;
		private static string response;

		[ClassInitialize]
		public static void CreateServer(TestContext context)
		{
			ConnectSetup();

			server.binaryMessageReceived += BinaryMessageRead;
			client.binaryMessageReceived += BinaryMessageRead;
			server.textMessageReceived += TextMessageRead;
			client.textMessageReceived += TextMessageRead;
		}

		private static void BinaryMessageRead(NetworkingPlayer player, Binary frame, NetWorker sender)
		{
			responsePlayer = player;
			response = ObjectMapper.Instance.Map<string>(frame.StreamData);
		}

		private static void TextMessageRead(NetworkingPlayer player, Text frame, NetWorker sender)
		{
			responsePlayer = player;
			response = frame.ToString();
		}

		[ClassCleanup]
		public static void DisposeServer()
		{
			server.binaryMessageReceived -= BinaryMessageRead;
			client.binaryMessageReceived -= BinaryMessageRead;

			ConnectTeardown();
		}

		private Binary SendBinary(NetWorker networker, bool useMask)
		{
			BMSByte data = new BMSByte();
			ObjectMapper.Instance.MapBytes(data, MESSAGE);

			ulong timestep = (ulong)(DateTime.UtcNow - start).TotalMilliseconds;
			return new Binary(timestep, useMask, data, Receivers.Target, 17931, networker is BaseTCP);
		}

		private Text SendText(NetWorker networker, bool useMask)
		{
			ulong timestep = (ulong)(DateTime.UtcNow - start).TotalMilliseconds;
			return Text.CreateFromString(timestep, MESSAGE, useMask, Receivers.Target, 17932, networker is BaseTCP);
		}

		[TestMethod]
		public void SendBinaryTestTCP()
		{
			WaitFor(() => { return client.IsConnected; });

			ResetVars();

#region UseMask = true is working

			//client.Send(SendBinary(client, true));
			//WaitForResponse();
			//Assert.AreEqual(server.Players.Last(), responsePlayer);
			//CompareMessages();

			//ResetVars();

			//server.Send(client.client, SendBinary(server, true));
			//WaitForResponse();
			////Assert.AreEqual(client.Server as NetworkingPlayer, responsePlayer);
			//CompareMessages();

			//ResetVars();
#endregion

			client.Send(SendBinary(client, false));
			WaitForResponse();
			Assert.AreEqual(server.Players.Last(), responsePlayer);
			CompareMessages();

			ResetVars();

			server.Send(client.client, SendBinary(server, false));
			WaitForResponse();
			//Assert.AreEqual(client.Server, responsePlayer);
			CompareMessages();

			response = null;
		}

		private void ResetVars()
		{
			responsePlayer = null;
			response = null;
		}

		private void WaitForResponse()
		{
			WaitFor(() => { return !string.IsNullOrEmpty(response); });
		}

		private void CompareMessages()
		{
			Assert.AreEqual(MESSAGE, response);
		}

		[TestMethod]
		public void SendTextTestTCP()
		{
			WaitFor(() => { return client.IsConnected; });
			
			ResetVars();

			client.Send(SendText(client, false));
			WaitForResponse();
			Assert.AreEqual(server.Players.Last(), responsePlayer);
			CompareMessages();

			ResetVars();

			server.Send(client.client, SendText(server, false));
			WaitForResponse();
			Assert.AreEqual(client.Server, responsePlayer);
			CompareMessages();

			ResetVars();

			client.Send(SendText(client, true));
			WaitForResponse();
			Assert.AreEqual(server.Players.Last(), responsePlayer);
			CompareMessages();

			ResetVars();

			server.Send(client.client, SendText(server, true));
			WaitForResponse();
			Assert.AreEqual(client.Server, responsePlayer);
			CompareMessages();

			response = null;
		}
	}
}
