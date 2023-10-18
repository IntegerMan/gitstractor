using Microsoft.Extensions.Hosting;

namespace GitStractor.Workers; 

public interface ICanStopEarly : IHostedService {
    string Name { get; }

    void InvokeOnCompleted(Action<ICanStopEarly, bool> listener);
}