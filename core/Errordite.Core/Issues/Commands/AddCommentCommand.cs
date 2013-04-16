﻿using System;
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Extensions;

namespace Errordite.Core.Issues.Commands
{
    public class AddCommentCommand : SessionAccessBase, IAddCommentCommand
    {
        public AddCommentResponse Invoke(AddCommentRequest request)
        {
            Trace("Starting...");

            if (request.Comment.IsNotNullOrEmpty())
            {
                var issue = Load<Issue>(request.IssueId);

                if (issue != null)
                {
                    if (issue.Comments == null)
                        issue.Comments = new List<IssueComment>();

                    issue.Comments.Add(new IssueComment
                    {
                        UserId = request.CurrentUser.Id,
                        DateAdded = DateTime.UtcNow.ToDateTimeOffset(request.CurrentUser.Organisation.TimezoneId),
                        Comment = request.Comment
                    });
                }
			}

            return new AddCommentResponse
            {
                Status = AddCommentStatus.Ok
            };
        }
    }

    public interface IAddCommentCommand : ICommand<AddCommentRequest, AddCommentResponse>
    { }

    public class AddCommentResponse
    {
        public AddCommentStatus Status { get; set; }
    }

    public class AddCommentRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public string ApplicationId { get; set; }
        public string Comment { get; set; }
    }

    public enum AddCommentStatus
    {
        Ok,
        IssueNotFound
    }
}
