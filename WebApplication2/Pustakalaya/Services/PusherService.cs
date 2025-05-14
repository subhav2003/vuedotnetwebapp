using PusherServer;

namespace Pustakalaya.Services
{
    public class PusherService
    {
        private readonly Pusher _pusher;

        public PusherService(Microsoft.Extensions.Options.IOptions<Pustakalaya.Helpers.PusherOptions> options)
        {
            var config = options.Value;

            _pusher = new Pusher(
                config.AppId,
                config.Key,
                config.Secret,
                new PusherOptions
                {
                    Cluster = config.Cluster,
                    Encrypted = config.UseTLS
                });
        }

        public async Task TriggerAsync(string channel, string eventName, object payload)
        {
            await _pusher.TriggerAsync(channel, eventName, payload);
        }
    }
}