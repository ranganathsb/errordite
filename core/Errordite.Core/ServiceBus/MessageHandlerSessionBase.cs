﻿using System;
using System.Diagnostics;
using CodeTrip.Core;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Session;
using Errordite.Client;
using Errordite.Core.Domain.Organisation;
using NServiceBus;

namespace Errordite.Core.ServiceBus
{
    /// <summary>
    /// MessageHandlerSessionBase handles session management for a message, we close the session (which is per thread lifestyle)
    /// at the end of each message being handled, this disposes of the session and ensures a new session is created for
    /// each message handler regardless of whether they run on the same thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MessageHandlerSessionBase<T> : SessionAccessBase, IHandleMessages<T> where T : ErrorditeNServiceBusMessageBase
    {
        protected abstract void HandleMessage(T message);

        protected virtual string GetAuditMessage(T message, Stopwatch watch)
        {
            return Resources.CoreResources.AsyncJobProcessed.FormatWith(watch.Elapsed.Seconds, watch.Elapsed.Milliseconds);
        }

        public void Handle(T message)
        {
            Trace("Received Message of type {0}", message.GetType().FullName);
            TraceObject(message);

            var watch = Stopwatch.StartNew();

            try
            {
                //dont want a limit on session requests for offline processes
                Session.RequestLimit = int.MaxValue;

                HandleMessage(message);
                Trace("Processed message in {0}ms".FormatWith(watch.ElapsedMilliseconds));
                Session.Commit();
            }
            catch (Exception e)
            {
                e.Data.Add("MessageType", typeof(T).Name); 

                try
                {
                    e.Data.Add("Message", SerializationHelper<T>.DataContractJsonSerialize(message));
                }
                catch { }

                ErrorditeClient.ReportException(e, false);
                Error(e, message.Id);
                throw;
            }
            finally
            {
                Session.Close();
            }
        }
    }

    public abstract class MessageHandlerBase<T> : ComponentBase, IHandleMessages<T> where T : class, IMessage
    {
        protected abstract void HandleMessage(T message);

        public void Handle(T message)
        {
            Trace("Received Message of type {0}", message.GetType().FullName);
            TraceObject(message);

            var watch = Stopwatch.StartNew();

            try
            {
                HandleMessage(message);
                Trace("Processed message in {0}ms".FormatWith(watch.ElapsedMilliseconds));
            }
            catch (Exception e)
            {
                e.Data.Add("MessageType", typeof(T).Name); 

                try
                {
                    e.Data.Add("Message", SerializationHelper<T>.DataContractJsonSerialize(message));
                }
                catch{}
                
                ErrorditeClient.ReportException(e, false);
                Error(e);
                throw;
            }
        }
    }
}
