using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Autofac;
using Autofac.Core;

using Foundatio.Messaging;

namespace SpikeCore.MessageBus.Foundatio.AutofacIntergration
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterFoundatio(this ContainerBuilder self)
        {
            var messageBus = new InMemoryMessageBus() as IMessageBus;

            // Register the message bus so it's still availible as a service if needs be.
            self
                .RegisterInstance(messageBus)
                .As<IMessageBus>()
                .SingleInstance();

            var subscribeAsyncMethodInfo = typeof(IMessageSubscriber)
                .GetMethods()
                .Where(methodInfo => methodInfo.Name == "SubscribeAsync")
                .Single();

            self.RegisterBuildCallback(container =>
            {
                // Find the registerd types which impliment IMessageHandler<>
                var messageHandlerRegistrations = container
                    .ComponentRegistry
                    .Registrations
                    .Where(registration => IsMessageHandler(registration))
                    .ToList();

                foreach (var messageHandlerRegistration in messageHandlerRegistrations)
                {
                    // Find all the individual IMessageHandler<> implimentations on the type
                    var iMessageHandlerTypes = messageHandlerRegistration
                        .Activator
                        .LimitType
                        .GetInterfaces()
                        .Where(interfaceType => IsMessageHandler(interfaceType))
                        .ToList();

                    if (iMessageHandlerTypes.Count() > 0)
                    {
                        // When the instance has been created
                        messageHandlerRegistration.Activated += (sender, eventArgs) =>
                        {
                            foreach (var iMessageHandlerType in iMessageHandlerTypes)
                            {
                                // Get the message type we're currently working with
                                var messageType = iMessageHandlerType.GetGenericArguments().Single();

                                // Get the handle method for the message type
                                var handleMessageAsyncMethodInfo = iMessageHandlerType.GetMethod("HandleMessageAsync", new[] { messageType, typeof(CancellationToken) });

                                // Create a delegate which satisfies SubscribeAsync, and calls the handle method
                                var messageHandlerParameter = Expression.Parameter(iMessageHandlerType);
                                var messageParameter = Expression.Parameter(messageType);
                                var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));
                                var callHandleMessageAsync = Expression.Call(messageHandlerParameter, handleMessageAsyncMethodInfo, messageParameter, cancellationTokenParameter);
                                var subscribeHandlerLambda = Expression.Lambda(callHandleMessageAsync, messageParameter, cancellationTokenParameter);

                                // Create a delegate which creates the handler delegate, with the handler passed in via closures.
                                var createSubscribeHandlerLambda = Expression.Lambda(subscribeHandlerLambda, messageHandlerParameter);
                                var createSubscribeHandler = createSubscribeHandlerLambda.Compile();

                                // Get the subscribe handling delegate by calling the create subscribe handling delegate
                                var messageHandler = eventArgs.Instance;
                                var subscribeHandler = createSubscribeHandler.DynamicInvoke(messageHandler);

                                // Call SubscribeAsync passing in the subscribeHandler
                                var task = (Task)subscribeAsyncMethodInfo
                                    .MakeGenericMethod(messageType)
                                    // Type.Missing is used to reprisent using the default value of an optional parameter
                                    .Invoke(messageBus, new object[] { subscribeHandler, Type.Missing });

                                // This might not be needed. It might be fine to have the subscribe carry on asynchronously
                                task.Wait();
                            }
                        };
                    }
                }
            });
        }

        private static bool IsMessageHandler(IComponentRegistration registration)
            => IsMessageHandler(registration.Activator.LimitType);

        private static bool IsMessageHandler(Type type)
            => (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
            || type
                .GetInterfaces()
                .Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
    }
}