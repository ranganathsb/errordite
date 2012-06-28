﻿
using CodeTrip.Core.Paging;
using Errordite.Web.Models.Users;

namespace Errordite.Web.Models.Organisation
{
    public class OrganisationsViewModel
    {
        public Page<Core.Domain.Organisation.Organisation> Organisations { get; set; }
        public PagingViewModel Paging { get; set; }
    }

    public class OrganisationUsersViewModel : UsersViewModel
    {
        public string OrganisationId { get; set; }
    }
}