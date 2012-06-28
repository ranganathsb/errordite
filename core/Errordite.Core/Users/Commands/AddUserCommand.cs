﻿using System;
using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using System.Linq;
using CodeTrip.Core.Extensions;
using Raven.Client.Linq;

namespace Errordite.Core.Users.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class AddUserCommand : SessionAccessBase, IAddUserCommand
    {
        private readonly ISendNotificationCommand _sendNotificationCommand;
        private readonly IEncryptor _encryptor;

        public AddUserCommand(IEncryptor encryptor, 
            ISendNotificationCommand sendNotificationCommand)
        {
            _encryptor = encryptor;
            _sendNotificationCommand = sendNotificationCommand;
        }

        public AddUserResponse Invoke(AddUserRequest request)
        {
            Trace("Starting...");
            
            var existingUser = Session.Raven.Query<User, Users_Search>().FirstOrDefault(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return new AddUserResponse(true)
                {
                    Status = AddUserStatus.EmailExists
                };
            }

            RavenQueryStatistics stats;
            var users = Session.Raven.Query<User, Users_Search>()
                .Statistics(out stats)
                .Where(u => u.OrganisationId == request.Organisation.Id)
                .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                .Take(0);

            if(stats.TotalResults >= request.Organisation.PaymentPlan.MaximumUsers)
            {
                return new AddUserResponse(true)
                {
                    Status = AddUserStatus.PlanThresholdReached
                };
            }

            var user = new User
            {
                Email = request.Email.ToLowerInvariant(), //need to do this so email is not case sensitive when signing in
                FirstName = request.FirstName,
                LastName = request.LastName,
                Status = UserStatus.Inactive,
                Role = request.Administrator ? UserRole.Administrator : UserRole.User,
                OrganisationId = request.Organisation.Id,
                GroupIds = request.GroupIds.Select(Group.GetId).ToList(),
                PasswordToken = Guid.NewGuid()
            };

            Store(user);

            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                EmailInfo = new NewUserEmailInfo
                {
                    To = user.Email,
                    Token = _encryptor.Encrypt(user.PasswordToken.ToString()).Base64Encode(),
                    UserName = user.FirstName
                },
                OrganisationId = request.Organisation.Id
            });

            Session.SynchroniseIndexes<Users_Search, Groups_Search>();

            return new AddUserResponse(false, request.Organisation.Id)
            {
                Status = AddUserStatus.Ok
            };
        }
    }

    public interface IAddUserCommand : ICommand<AddUserRequest, AddUserResponse>
    { }

    public class AddUserResponse : CacheInvalidationResponseBase
    {
        private readonly string _organisationId;
        public AddUserStatus Status { get; set; }

        public AddUserResponse(bool ignoreCache, string organisationId = "")
            : base(ignoreCache)
        {
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return Caching.CacheInvalidation.GetUserInvalidationItems(_organisationId);
        }
    }

    public class AddUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Organisation Organisation { get; set; }
        public IList<string> GroupIds { get; set; }
        public bool Administrator { get; set; }
    }

    public enum AddUserStatus
    {
        Ok,
        EmailExists,
        PlanThresholdReached
    }
}
