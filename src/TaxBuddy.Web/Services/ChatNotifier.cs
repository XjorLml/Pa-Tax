using System.Collections.Concurrent;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Services;

/// <summary>
/// In-process live updates (SDD live updates; DI and lifecycle: a thread-safe singleton). Open chat pages subscribe for
/// their account and reload from the database when told. Assumes one server instance; more than one needs a shared
/// notifier and session affinity.
/// </summary>
public sealed class ChatNotifier(ILogger<ChatNotifier> logger) : IChatNotifier
{
    private static readonly EventId CallbackFailed = new(1201, "ChatNotifyFailed");

    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Subscription, byte>> subscribers = new();

    public IDisposable Subscribe(Guid accountId, Func<Guid, Task> changed)
    {
        var subscription = new Subscription(this, accountId, changed);
        subscribers.GetOrAdd(accountId, _ => new()).TryAdd(subscription, 0);
        return subscription;
    }

    public void Changed(Conversation conversation)
    {
        Notify(conversation.ClientAccountId, conversation.Id);
        Notify(conversation.AccountantAccountId, conversation.Id);
    }

    private void Notify(Guid accountId, Guid conversationId)
    {
        if (!subscribers.TryGetValue(accountId, out var set))
        {
            return;
        }

        foreach (var subscription in set.Keys)
        {
            // Never wait for a page: each one reloads on its own dispatcher, and a failure stays with that page.
            _ = Task.Run(async () =>
            {
                try
                {
                    await subscription.Changed(conversationId);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(CallbackFailed, "Chat page update failed ({ErrorType})", ex.GetType().Name);
                }
            });
        }
    }

    private void Remove(Subscription subscription)
    {
        if (subscribers.TryGetValue(subscription.AccountId, out var set))
        {
            set.TryRemove(subscription, out _);
        }
    }

    private sealed class Subscription(ChatNotifier owner, Guid accountId, Func<Guid, Task> changed) : IDisposable
    {
        public Guid AccountId => accountId;

        public Func<Guid, Task> Changed => changed;

        public void Dispose() => owner.Remove(this);
    }
}
