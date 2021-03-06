﻿using System;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Extensions;
using Errordite.Core.Indexing;
using Errordite.Core.Messaging;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Raven.Abstractions.Data;

namespace Errordite.Core.Issues.Commands
{
    public class MergeIssuesCommand : SessionAccessBase, IMergeIssuesCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;

        public MergeIssuesCommand(IAuthorisationManager authorisationManager, ErrorditeConfiguration configuration)
        {
            _authorisationManager = authorisationManager;
            _configuration = configuration;
        }

        public MergeIssuesResponse Invoke(MergeIssuesRequest request)
        {
            Trace("Starting...");
            TraceObject(request);

            var mergeFromIssue = Load<Issue>(Issue.GetId(request.MergeFromIssueId));
            var mergeToIssue = Load<Issue>(Issue.GetId(request.MergeToIssueId));

            MergeIssuesResponse response;
            if (!ValidateCommand(mergeFromIssue, mergeToIssue, out response))
            {
                return response;
            }

            _authorisationManager.Authorise(mergeFromIssue, request.CurrentUser);
            _authorisationManager.Authorise(mergeToIssue, request.CurrentUser);

            new SynchroniseIndexCommitAction<Indexing.Errors>().Execute(Session);

            //move all errors fron the MergeFromIssue to the MergeToIssue
            Session.AddCommitAction(new UpdateByIndexCommitAction(CoreConstants.IndexNames.Errors,
                new IndexQuery
                {
                    Query = "IssueId:{0}".FormatWith(mergeFromIssue.Id)
                },
                new[]
                {
                    new PatchRequest
                    {
                        Name = "IssueId",
                        Type = PatchCommandType.Set,
                        Value = mergeToIssue.Id
                    }
            }, true));

            Store(new IssueHistory
            {
                DateAddedUtc = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.ActiveOrganisation.TimezoneId),
                SpawningIssueId = mergeFromIssue.Id,
                SystemMessage = true,
                Type = HistoryItemType.MergedTo,
                IssueId = mergeToIssue.Id,
				ApplicationId = mergeToIssue.ApplicationId
            });

            Delete(mergeFromIssue);

            //re-sync the error counts
            Session.AddCommitAction(new SendMessageCommitAction(new SyncIssueErrorCountsMessage
            {
                IssueId = request.MergeToIssueId,
                OrganisationId = request.CurrentUser.OrganisationId,
                TriggerEventUtc = DateTime.UtcNow,
            }, _configuration.GetEventsQueueAddress(request.CurrentUser.ActiveOrganisation.RavenInstance.FriendlyId)));

            return new MergeIssuesResponse
            {
                Status = MergeIssuesStatus.Ok
            };
        }

        private bool ValidateCommand(Issue mergeFromIssue, Issue mergeToIssue, out MergeIssuesResponse response)
        {
            if (mergeFromIssue == null || mergeToIssue == null)
            {
                response = new MergeIssuesResponse
                {
                    Status = MergeIssuesStatus.IssueNotFound
                };
                return false;
            }
            
            if (mergeFromIssue.RulesHash != mergeToIssue.RulesHash)
            {
                response = new MergeIssuesResponse
                {
                    Status = MergeIssuesStatus.RulesDoNotMatch
                };
                return false;
            }

            response = null;
            return true;
        }
    }

    public interface IMergeIssuesCommand : ICommand<MergeIssuesRequest, MergeIssuesResponse>
    { }

    public class MergeIssuesResponse
    {
        public MergeIssuesStatus Status { get; set; }
    }

    public enum MergeIssuesStatus
    {
        Ok,
        IssueNotFound,
        RulesDoNotMatch
    }

    public class MergeIssuesRequest : OrganisationRequestBase
    {
        public string MergeFromIssueId { get; set; }
        public string MergeToIssueId { get; set; }
        public string MergedIssueName { get; set; }
    }
}
