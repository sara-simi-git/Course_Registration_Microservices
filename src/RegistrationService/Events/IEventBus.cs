using System.Threading.Tasks;

namespace RegistrationService.Events
{
    public interface IEventBus
    {
        Task PublishAsync(object @event);
    }
}
