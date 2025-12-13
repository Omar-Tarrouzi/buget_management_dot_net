using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace App_de_gestion_de_buget_version2.Data
{
    // Fix for MongoDB: Expression not supported: i.ToClaim()
    public class CustomUserStore : UserStore<IdentityUser>
    {
        public CustomUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null) 
            : base(context, describer)
        {
        }

        public override async Task<IList<Claim>> GetClaimsAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            var connection = Context.Set<IdentityUserClaim<string>>();
            var userClaims = await connection
                .Where(uc => uc.UserId == user.Id)
                .ToListAsync(cancellationToken);

            return userClaims.Select(c => c.ToClaim()).ToList();
        }

        // Fix for MongoDB: Joins are not fully supported for Many-to-Many via EF Core provider
        public override async Task<IList<string>> GetRolesAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            var userRolesQuery = Context.Set<IdentityUserRole<string>>().Where(u => u.UserId == user.Id);
            var roleIds = await userRolesQuery.Select(u => u.RoleId).ToListAsync(cancellationToken);

            var rolesQuery = Context.Set<IdentityRole>().Where(r => roleIds.Contains(r.Id));
            var roles = await rolesQuery.Select(r => r.Name).ToListAsync(cancellationToken);
            return roles.Where(r => r != null).Cast<string>().ToList();
        }

        public override async Task AddToRoleAsync(IdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(normalizedRoleName)) throw new ArgumentNullException(nameof(normalizedRoleName));

            var role = await Context.Set<IdentityRole>().FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
            if (role == null)
            {
                 throw new InvalidOperationException($"Role {normalizedRoleName} not found.");
            }

            var userRole = new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id };
            await Context.Set<IdentityUserRole<string>>().AddAsync(userRole, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        }

        public override async Task RemoveFromRoleAsync(IdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
             ThrowIfDisposed();
             if (user == null) throw new ArgumentNullException(nameof(user));
             if (string.IsNullOrWhiteSpace(normalizedRoleName)) throw new ArgumentNullException(nameof(normalizedRoleName));

             var role = await Context.Set<IdentityRole>().FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
             if (role != null)
             {
                 var userRole = await Context.Set<IdentityUserRole<string>>().FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
                 if (userRole != null)
                 {
                     Context.Set<IdentityUserRole<string>>().Remove(userRole);
                     await Context.SaveChangesAsync(cancellationToken);
                 }
             }
        }

        public override async Task<bool> IsInRoleAsync(IdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
             ThrowIfDisposed();
             if (user == null) throw new ArgumentNullException(nameof(user));
             if (string.IsNullOrWhiteSpace(normalizedRoleName)) throw new ArgumentNullException(nameof(normalizedRoleName));

             var role = await Context.Set<IdentityRole>().FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
             if (role == null) return false;

             var userRole = await Context.Set<IdentityUserRole<string>>().FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
             return userRole != null;
        }
    }
}
