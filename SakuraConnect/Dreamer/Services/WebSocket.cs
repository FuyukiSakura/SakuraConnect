using System.Net.WebSockets;
using System.Text;

namespace Sakura.Live.Connect.Dreamer.Services
{
    /// <summary>
    /// A event based implementation of <see cref="ClientWebSocket"/>
    /// </summary>
    public class WebSocket
    {
        readonly string _socketUri;
        readonly bool _keepAlive;

        CancellationTokenSource _cancellationSource = new ();
        ClientWebSocket _ws = new ();

        public event EventHandler<string>? MessageReceived;
        public event EventHandler<string?>? Closed;

        /// <summary>
        /// Gets the connected status of the socket
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="WebSocket"/>
        /// </summary>
        public WebSocket(string uri, bool keepAlive = true)
        {
            _socketUri = uri;
            _keepAlive = keepAlive;
        }

        /// <summary>
        /// Starts the socket
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            // Cancel existing request
            _cancellationSource.Cancel();
            _cancellationSource = new CancellationTokenSource();

            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(_socketUri), _cancellationSource.Token);

            IsConnected = true;
            _ = ListenAsync();
            if (_keepAlive)
            {
                _ = KeepAliveAsync();
            }
        }

        /// <summary>
        /// Keeps pinging the server to keep the connection alive
        /// </summary>
        /// <returns></returns>
        async Task KeepAliveAsync()
        {
            while (!_cancellationSource.IsCancellationRequested)
            {
                var buffer = Encoding.ASCII.GetBytes("ping");
                await _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                await Task.Delay (5000);
            }
        }

        /// <summary>
        /// Sends text to the socket
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task SendTextAsync(string text)
        {
            var encoded = Encoding.UTF8.GetBytes(text);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
            await _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Listens to incoming event returned by http-status
        /// </summary>
        /// <returns></returns>
        public async Task ListenAsync()
        {
            while(!_cancellationSource.IsCancellationRequested)
            {
                if (_ws.State != WebSocketState.Open)
                {
                    // Reconnects if the socket has errors
                    Closed?.Invoke(this, _ws.CloseStatusDescription);
                }

                var message = await ReceiveAsync();
                MessageReceived?.Invoke(this, message);
            }
        }

        /// <summary>
        /// Slices the message into smaller chunks and write them into buffer
        /// return when the full message is received
        /// </summary>
        /// <remarks>
        /// This is due to limitation of <see cref="System.Net.WebSockets.WebSocket"/> not receiving
        /// the full message when it is too long
        /// </remarks>
        /// <returns>Socket message</returns>
        async Task<string> ReceiveAsync()
        {
            var buffer = new byte[1024];
			var message = new StringBuilder();
			while (true)
			{
				var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
				if (result.EndOfMessage)
				{
					break;
				}
			}
			return message.ToString();
        }

        /// <summary>
        /// Tries to reconnect to the socket
        /// </summary>
        /// <returns></returns>
        public async Task ReconnectAsync()
        {
            while (!_cancellationSource.IsCancellationRequested)
            {
                try
                {
                    await ConnectAsync();
                    break;
                }
                catch (WebSocketException)
                {
                    // Try reconnects every 2 seconds
                    await Task.Delay(2000);
                }
            }
        }

        /// <summary>
        /// Stops listening to the http-status socket
        /// </summary>
        public void Close()
        {
            IsConnected = false;
            _cancellationSource.Cancel();
        }
    }
}
