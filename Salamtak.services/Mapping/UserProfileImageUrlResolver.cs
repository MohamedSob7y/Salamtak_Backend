using AutoMapper;
using Microsoft.Extensions.Configuration;
using Salamtak.Domain.Models;
using Salamtak.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Mapping
{
    public class UserProfileImageUrlResolver: IValueResolver<User, UserDto, string?>
    {
        private readonly IConfiguration _configuration;

        public UserProfileImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string? Resolve(User source,UserDto destination,string? destMember,ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(
                source.ProfileImagePath))
            {
                return null;
            }

            if (source.ProfileImagePath.StartsWith(
                    "http",
                    StringComparison.OrdinalIgnoreCase))
            {
                return source.ProfileImagePath;
            }

            var baseUrl =
                _configuration["URLs:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return source.ProfileImagePath;
            }

            return
                $"{baseUrl.TrimEnd('/')}/" +
                $"{source.ProfileImagePath.TrimStart('/')}";
        }
    }
}
