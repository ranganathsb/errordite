﻿
using System;
using System.Net.Http;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;

namespace Errordite.Core.Session.Actions
{
    public class FlushApplicationCacheCommitAction : SessionCommitAction
    {
		private readonly string _applicationId;
		private readonly Organisation _organisation;
        private readonly ErrorditeConfiguration _configration;

		public FlushApplicationCacheCommitAction(ErrorditeConfiguration configration, Organisation organisation, string applicationId)
        {
            _applicationId = applicationId.GetFriendlyId();
            _configration = configration;
			_organisation = organisation;
        }

        public override void Execute(IAppSession session)
        {
            session.ReceiveHttpClient.DeleteAsync("cache?applicationId={0}".FormatWith(_applicationId));

            foreach (var endpoint in _configration.ReceiveWebEndpoints.Split(new []{'|'}, StringSplitOptions.RemoveEmptyEntries))
            {
                using (var client = new HttpClient {BaseAddress = new Uri(endpoint)})
                {
                    var t = client.DeleteAsync("cache/flush?organisationId={0}&applicationId={1}".FormatWith(_organisation.FriendlyId, _applicationId));
                    t.Wait(5000);
                }
			}

            using (var eventsClient = new HttpClient
                {
                    BaseAddress = new Uri("{0}:802/api/{1}/".FormatWith(_organisation.RavenInstance.ServicesBaseUrl, _organisation.FriendlyId))
                })
            {
                var task = eventsClient.DeleteAsync("cache?applicationId={0}".FormatWith(_applicationId));
                task.Wait(5000);
            }
        }
    }
}
