using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSocketClient
{
	class Program
	{
		private static readonly HttpClient client = new HttpClient();
		private static readonly string URL = "URL_HERE";
		public static void Main(string[] args)
		{
			try
			{

				RunWebSockets().GetAwaiter().GetResult();

			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception: " + ex.Message);
			}

		}

		private static async Task RunWebSockets()
		{
			var client = new ClientWebSocket();
			await client.ConnectAsync(new Uri(URL), CancellationToken.None);

			Console.WriteLine("Connected!");

			var sending = Task.Run(async () =>
			{
				string line;
				while ((line = Console.ReadLine()) != null && line != String.Empty)
				{
					var bytes = Encoding.UTF8.GetBytes(line);
					await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
				}

				await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
			});

			var receiving = Receiving(client);

			await Task.WhenAll(sending, receiving);
		}

		private static async Task Receiving(ClientWebSocket client)
		{
			var buffer = new byte[1024 * 4];

			while (true)
			{
				var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

				if (result.MessageType == WebSocketMessageType.Text)
					Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, result.Count));

				else if (result.MessageType == WebSocketMessageType.Close)
				{
					await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
					break;
				}
			}
		}
	}
}
