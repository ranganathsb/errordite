﻿using System.Linq;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Issues;
using Errordite.Core.Session;

namespace Errordite.Core.Reception.Commands
{
    public class ReceiveErrorCommand : SessionAccessBase, IReceiveErrorCommand
    {
        private readonly IReceptionServiceIssueCache _receptionServiceIssueCache;
        private readonly IGetApplicationQuery _getApplicationQuery;
        private readonly IGetApplicationByTokenQuery _getApplicationByTokenQuery;
		private readonly IAttachToNewIssueCommand _attachToNewIssueCommand;
		private readonly IAttachToExistingIssueCommand _attachToExistingIssueCommand;

        public ReceiveErrorCommand(IGetApplicationQuery getApplicationQuery, 
            IReceptionServiceIssueCache receptionServiceIssueCache, 
            IGetApplicationByTokenQuery getApplicationByTokenQuery,
			IAttachToNewIssueCommand attachToNewIssueCommand,
			IAttachToExistingIssueCommand attachToExistingIssueCommand)
        {
            _getApplicationQuery = getApplicationQuery;
            _receptionServiceIssueCache = receptionServiceIssueCache;
            _getApplicationByTokenQuery = getApplicationByTokenQuery;
			_attachToNewIssueCommand = attachToNewIssueCommand;
			_attachToExistingIssueCommand = attachToExistingIssueCommand;
        }

        public ReceiveErrorResponse Invoke(ReceiveErrorRequest request)
        {
            Trace("Starting...");

            var application = GetApplication(request);

            if(application == null)
            {
                Trace("Application could not be found");
                return new ReceiveErrorResponse();
            }

            Trace("ApplicationId:={0}, OrganisationId:={1}", application.Id, application.OrganisationId);

            var error = request.Error;
            var issues = _receptionServiceIssueCache.GetIssues(application.Id, application.OrganisationId);
            var existingIssue = request.ExistingIssueId.IfPoss(i => Load<Issue>(request.ExistingIssueId));
            var matchingIssue = existingIssue == null 
                ? issues.FirstOrDefault(i => i.RulesMatch(error)) 
                : issues.FirstOrDefault(i => i.Id != existingIssue.Id && i.RulesMatch(error));

            Trace("Matching issue: {0}", existingIssue == null ? "NONE" : existingIssue.Id);

            //if we are re-ingesting an issues errors and we cant find another match, do nothing so the error remains attached to the existing issue
            if (matchingIssue == null && existingIssue != null)
            {
                Trace("No issues matched, error remains attached to the existing issue Id:={0}", existingIssue.Id);

				return new ReceiveErrorResponse
				{
                    IssueId = existingIssue.Id
				};
            }

            var issue = matchingIssue == null
				? _attachToNewIssueCommand.Invoke(new AttachToNewIssueRequest { Application = application, Error = error }).Issue
				: _attachToExistingIssueCommand.Invoke(new AttachToExistingIssueRequest { Application = application, Error = error, IssueId = matchingIssue.Id }).Issue;
            
            Trace("Complete");

            return new ReceiveErrorResponse
            {
                IssueId = issue.Id
            };
        }

        private Application GetApplication(ReceiveErrorRequest request)
        {
            GetApplicationResponse getApplicationResponse;

            if (request.ApplicationId.IsNullOrEmpty())
            {
                getApplicationResponse = _getApplicationByTokenQuery.Invoke(new GetApplicationByTokenRequest
                {
                    Token = request.Token,
                    CurrentUser = User.System()
                });
            }
            else
            {
                getApplicationResponse = _getApplicationQuery.Invoke(new GetApplicationRequest
                {
                    Id = request.Error.ApplicationId,
                    OrganisationId = request.Error.OrganisationId,
                    CurrentUser = User.System()
                });
            }

            var application = getApplicationResponse.Application;

            //dont process if we cant find the application or if the application is inactive
            if (application == null || !application.IsActive)
            {
                Trace("Failed to locate application {0}.", application == null ? "application is null" : "application is inactive");
                return null;
            }

            Session.SetOrganisation(getApplicationResponse.Organisation);

            request.Error.ApplicationId = application.Id;
            request.Error.OrganisationId = application.OrganisationId;
            request.Error.Version = application.Version;

            return application;
        }
    }

    public interface IReceiveErrorCommand : ICommand<ReceiveErrorRequest, ReceiveErrorResponse>
    { }

    public class ReceiveErrorRequest
    {
        public Error Error { get; set; }
        public string ApplicationId { get; set; }
        public string OrganisationId { get; set; }
        public string Token { get; set; }
        public string ExistingIssueId { get; set; }
    }

    public class ReceiveErrorResponse
    {
        public string IssueId { get; set; }
    }
}