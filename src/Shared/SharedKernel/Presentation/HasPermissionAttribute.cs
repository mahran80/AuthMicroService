using Microsoft.AspNetCore.Authorization;

namespace SharedKernel.Presentation;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base(policy: permission)
    {
    }
}
