using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASA_TENANT_BE.CustomAttribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class RequireFeatureAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly HashSet<int> _requiredFeatureIds;

        public RequireFeatureAttribute(params int[] featureIds)
        {
            _requiredFeatureIds = featureIds is { Length: > 0 }
                ? new HashSet<int>(featureIds)
                : new HashSet<int>();
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            if (httpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    Success = false,
                    Status = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                });
                return Task.CompletedTask;
            }

            if (_requiredFeatureIds.Count == 0)
            {
                return Task.CompletedTask; // no specific feature required
            }

            var featureClaim = httpContext.User.FindFirst("FeatureIds");
            if (featureClaim == null || string.IsNullOrWhiteSpace(featureClaim.Value))
            {
                context.Result = new ForbidResult();
                return Task.CompletedTask;
            }

            var userFeatures = ParseFeatureIds(featureClaim.Value);
            var hasAny = userFeatures.Overlaps(_requiredFeatureIds);

            if (!hasAny)
            {
                context.Result = new ObjectResult(new
                {
                    Success = false,
                    Status = StatusCodes.Status403Forbidden,
                    Message = "Forbidden: Feature not permitted"
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            return Task.CompletedTask;
        }

        private static HashSet<int> ParseFeatureIds(string raw)
        {
            // Token stores FeatureIds like "[1,2,3]" (string). Handle both JSON array and comma-separated.
            try
            {
                if (raw.StartsWith("[") && raw.EndsWith("]"))
                {
                    var arr = JsonSerializer.Deserialize<int[]>(raw);
                    return arr != null ? new HashSet<int>(arr) : new HashSet<int>();
                }

                var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return new HashSet<int>(parts.Select(p => int.TryParse(p, out var i) ? i : -1).Where(i => i >= 0));
            }
            catch
            {
                return new HashSet<int>();
            }
        }
    }
}


